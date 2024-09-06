using AO;

public partial class OverseerPromoNPC : Component
{
    [Serialized] Seat fighter1Seat;
    [Serialized] Seat fighter2Seat;

    public SyncVar<bool> BattleActive = new(false);
    public Interactable interactable;
    public static OverseerPromoNPC Instance;

    public SyncVar<Entity> Fighter1 = new();
    public SyncVar<Entity> Fighter2 = new();
    public SyncVar<bool> Fighter1Died = new();
    public SyncVar<bool> Fighter2Died = new();
    public SyncVar<float> BattleStartTime = new();
    private Sprite_Renderer spriteRenderer;

    public override void Awake()
    {
        Instance = this;
        spriteRenderer = Entity.GetComponent<Sprite_Renderer>();

        interactable = Entity.AddComponent<Interactable>();
        interactable.CanUseCallback = (Player p) =>
        {
            var op = (OfficePlayer)p;
            return op.CurrentRole != Role.JANITOR && op.CurrentRole != Role.OVERSEER;
        };

        interactable.OnInteract = (Player p) =>
        {
            if (!Network.IsServer) return;
            var op = (OfficePlayer)p;

            if (op.CurrentRole <= Role.EMPLOYEE)
            {
                op.CallClient_ShowNotification("Talk to me again when you're a manager or higher...");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (DayNightManager.Instance.CurrentState == DayState.NIGHT)
            {
                op.CallClient_ShowNotification("You must wait till night has passed...");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (op.Experience < op.RequiredExperience)
            {
                op.CallClient_ShowNotification("You need more experience to be promoted...");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (BattleActive)
            {
                op.CallClient_ShowNotification("You cannot be promoted during a fight");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (Time.TimeSinceStartup - op.LastRequestedCEOPromoAt < 120f)
            {
                op.CallClient_ShowNotification("You must wait 2 minutes before requesting another promotion...");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            op.LastRequestedCEOPromoAt.Set(Time.TimeSinceStartup);

            var OverseerPlayers = GameManager.Instance.GetPlayersByRole(Role.OVERSEER);
            var OverseerPlayer = OverseerPlayers.Length > 0 ? (OfficePlayer)OverseerPlayers[0] : null;
            if (OverseerPlayer.Alive())
            {
                Fighter1.Set(OverseerPlayer.Entity);
                Fighter2.Set(op.Entity);

                var seats = new List<Seat>();
                foreach (var seat in Scene.Components<Seat>())
                {
                    if (seat.Type == "Board")
                    {
                        seats.Add(seat);
                        Log.Info("Seat info " + seat.Entity.Name);
                    }
                }

                var backupSeat = seats.GetRandom();

                foreach (Player player in Player.AllPlayers)
                {
                    var op2 = (OfficePlayer)player;
                    Seat seat = backupSeat;

                    if (seats.Count > 0)
                    {
                        seat = seats.Pop();
                    }

                    op2.AssignedMeetingSeat.Set(seat.Entity);
                }

                OverseerPlayer.AssignedMeetingSeat.Set(fighter1Seat.Entity);
                op.AssignedMeetingSeat.Set(fighter2Seat.Entity);

                CallClient_StartBattle();
            }
        };
    }

    [ClientRpc]
    public void StartBattle()
    {
        if (Network.IsClient) {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/clue_found2.wav"), new());
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/suspense.wav"), new());
        }

        if (Network.IsServer) {
            BattleActive.Set(true);
            BattleStartTime.Set(Time.TimeSinceStartup);
        }

        foreach (Player player in Player.AllPlayers)
        {
            player.AddEffect<BoardMeetingEffect>();
        }
    }

    public override void Update()
    {
        if (Network.IsServer)
        {
            if (BattleActive && (Time.TimeSinceStartup - BattleStartTime >= 17f || Fighter1Died || Fighter2Died))
            {
                BattleActive.Set(false);
                Fighter1Died.Set(false);
                Fighter2Died.Set(false);

                foreach (Player player in Player.AllPlayers)
                {
                    player.Teleport(Vector2.Zero);
                }

                if (!Fighter1.Value.Alive() || !Fighter2.Value.Alive())
                {
                    Log.Error("Fighter was null");
                    GameManager.Instance.CallClient_ShowNotification("The trial has been cancelled due to a candidate being unavailable.");
                    return;
                }

                var fighter1 = (OfficePlayer)Fighter1.Value.GetComponent<Player>();
                var fighter2 = (OfficePlayer)Fighter2.Value.GetComponent<Player>();

                if (Fighter1Died && Fighter2Died)
                {
                    GameManager.Instance.CallClient_ShowNotification("The trial has ended in a draw.");
                    return;
                }

                if (Fighter1Died)
                {
                    fighter2.CallClient_ShowNotification("Welcome... overseer.");
                    fighter2.CurrentRole = Role.OVERSEER;
                    fighter2.Experience.Set(0);
                    return;
                }

                if (Fighter2Died)
                {
                    fighter1.CallClient_ShowNotification("Welcome... overseer.");
                    fighter1.CurrentRole = Role.OVERSEER;
                    fighter1.Experience.Set(0);
                    return;
                }
            }
        }


        if (Network.IsServer) return;
        var op = (OfficePlayer)Network.LocalPlayer;
        if (op.CurrentRole == Role.JANITOR)
        {
            spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
        } else {
            spriteRenderer.Tint = new Vector4(1, 0.5f, 0.5f, 1);
        }

        var interactible = Entity.GetComponent<Interactable>();
        if (!op.Alive()) return;
        if (op.CurrentRole == Role.JANITOR)
        {
            interactible.Text = "Request Promotion to Employee... (100XP)";
            return;
        } else if (op.CurrentRole == Role.EMPLOYEE)
        {
            interactible.Text = "Request Promotion to Manager... (100XP)";
            return;
        } else if (op.CurrentRole == Role.MANAGER)
        {
            interactible.Text = "Request Promotion to CEO... (100XP) (Requires Conference Speech Given)";
            return;
        } else if (op.CurrentRole == Role.CEO)
        {
            interactible.Text = "Request Promotion... (100XP)";
            return;
        }
    }
}

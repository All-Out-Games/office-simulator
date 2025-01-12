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
            return op.CurrentRole != Role.JANITOR;
        };

        interactable.OnInteract = (Player p) =>
        {
            if (!Network.IsServer) return;
            var op = (OfficePlayer)p;

            if (op.CurrentRole == Role.OVERSEER)
            {
                op.CallClient_ShowNotification("Nice to see you again... overseer.");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

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

            // if (Time.TimeSinceStartup - op.LastRequestedOverseerPromoAt < 120f)
            // {
            //     op.CallClient_ShowNotification("You must wait 2 minutes before requesting another promotion...");
            //     op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
            //     return;
            // }

            if (op.Cash < 500)
            {
                op.CallClient_ShowNotification("You do not have enough cash to request a promotion...");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            op.LastRequestedOverseerPromoAt.Set(Time.TimeSinceStartup);

            var OverseerPlayers = GameManager.Instance.GetPlayersByRole(Role.OVERSEER);
            var OverseerPlayer = OverseerPlayers.Length > 0 ? (OfficePlayer)OverseerPlayers[0] : null;

            // If there isn't a current overseer they get set to it immediately
            if (!OverseerPlayer.Alive())
            {
                GameManager.Instance.CallClient_ShowNotification("A dark force sweeps over the office...");
                op.CurrentRole = Role.OVERSEER;
                op.Experience.Set(0);
                // Quest Progress
                if (!Game.LaunchedFromEditor)
                {
                    Battlepass.IncrementProgress(op, "6772ede693ce6b94e1eee7cc", 1);
                }
            }

            // Fight to the death
            if (OverseerPlayer.Alive())
            {
                // Quest Progress
                if (!Game.LaunchedFromEditor)
                {
                    Battlepass.IncrementProgress(op, "6772ede693ce6b94e1eee7cc", 1);
                }
                Fighter1.Set(OverseerPlayer.Entity);
                Fighter2.Set(op.Entity);

                OverseerPlayer.AssignedMeetingSeat.Set(fighter1Seat.Entity);
                OverseerPlayer.WasKilledInOverseerBattle.Set(false);
                op.AssignedMeetingSeat.Set(fighter2Seat.Entity);
                op.WasKilledInOverseerBattle.Set(false);

                CallClient_StartBattle();
            }

            op.Cash.Set(op.Cash - 500);
        };
    }

    [ClientRpc]
    public void StartBattle()
    {
        var fighter1 = (OfficePlayer)Fighter1.Value.GetComponent<Player>();
        var fighter2 = (OfficePlayer)Fighter2.Value.GetComponent<Player>();

        if (Network.IsClient)
        {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/clue_found2.wav"), new());
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/suspense.wav"), new());
        }

        if (Network.IsServer)
        {
            BattleActive.Set(true);
            BattleStartTime.Set(Time.TimeSinceStartup);
            fighter1.CallClient_ShowNotification("A challenger has appeared... fight for your life.");
            fighter2.CallClient_ShowNotification("Eliminate the overseer to take their role...");
        }

        fighter1.SetLightOn(true);
        fighter2.SetLightOn(true);

        fighter1.Teleport(Fighter1.Value.GetComponent<OfficePlayer>().AssignedMeetingSeat.Value.GetComponent<Seat>().Position);
        fighter2.Teleport(Fighter2.Value.GetComponent<OfficePlayer>().AssignedMeetingSeat.Value.GetComponent<Seat>().Position);
    }

    [ClientRpc]
    public void EndFight1(Entity fighter1)
    {
        var fighter1Op = fighter1.GetComponent<OfficePlayer>();

        if (fighter1Op != null)
        {
            fighter1Op.SetLightOn(false);
            fighter1Op.Teleport(Vector2.Zero);
            fighter1Op.RemoveEffect<SpectatorEffect>(false);
        }
    }

    [ClientRpc]
    public void EndFight2(Entity fighter2)
    {
        var fighter2Op = fighter2.GetComponent<OfficePlayer>();

        fighter2Op.SetLightOn(false);
        fighter2Op.Teleport(Vector2.Zero);
        fighter2Op.RemoveEffect<SpectatorEffect>(false);
    }


    public override void Update()
    {
        if (Network.IsServer)
        {
            OfficePlayer fighter1 = null;
            OfficePlayer fighter2 = null;

            if (Fighter1.Value.Alive())
            {
                fighter1 = Fighter1.Value.GetComponent<OfficePlayer>();
            }

            if (Fighter2.Value.Alive())
            {
                fighter2 = Fighter2.Value.GetComponent<OfficePlayer>();
            }

            if (BattleActive && (fighter1 == null || fighter2 == null))
            {
                BattleActive.Set(false);
                GameManager.Instance.CallClient_ShowNotification("The trial has been cancelled due to a candidate being unavailable.");
                if (fighter1.Alive())
                {
                    CallClient_EndFight1(fighter1.Entity);
                }

                if (fighter2.Alive())
                {
                    CallClient_EndFight2(fighter2.Entity);
                }

                Fighter1.Set(null);
                Fighter2.Set(null);
                return;
            }

            if (BattleActive && (Time.TimeSinceStartup - BattleStartTime >= 17f || fighter1?.WasKilledInOverseerBattle || fighter2?.WasKilledInOverseerBattle))
            {
                BattleActive.Set(false);
                // so the guns reset
                Fighter1.Set(null);
                Fighter2.Set(null);

                CallClient_EndFight1(fighter1.Entity);
                CallClient_EndFight2(fighter2.Entity);

                if ((fighter1.WasKilledInOverseerBattle && fighter2.WasKilledInOverseerBattle) || (!fighter1.WasKilledInOverseerBattle && !fighter2.WasKilledInOverseerBattle))
                {
                    GameManager.Instance.CallClient_ShowNotification("The trial has ended in a draw.");
                    return;
                }

                if (fighter1.WasKilledInOverseerBattle)
                {
                    fighter1.CallClient_ShowNotification("The trial has ended... you have been eliminated.");
                    fighter2.CallClient_ShowNotification("Welcome... overseer.");
                    fighter2.CurrentRole = Role.OVERSEER;
                    fighter2.Experience.Set(0);
                    return;
                }
                else if (fighter2.WasKilledInOverseerBattle)
                {
                    fighter1.CallClient_ShowNotification("You keep your throne... for now.");
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
        }
        else
        {
            spriteRenderer.Tint = new Vector4(1, 0.5f, 0.5f, 1);
        }

        var interactible = Entity.GetComponent<Interactable>();
        if (op.CurrentRole == Role.OVERSEER)
        {
            interactible.Text = "Keep my janitors safe.";
            return;
        }

        interactible.Text = "Commence the Overseer's Trial ($500)";
    }
}

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

        interactable = Entity.Unsafe_AddComponent<Interactable>();
        interactable.CanUseCallback = (Player p) =>
        {
            var op = p as OfficePlayer;
            return op.Alive() && op.CurrentRole != Role.JANITOR;
        };

        interactable.OnInteract = (Player p) =>
        {
            if (!Network.IsServer) return;
            var op = p as OfficePlayer;
            if (!op.Alive()) return;

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
                if (!fighter1Seat.Alive() || !fighter2Seat.Alive())
                {
                    op.CallClient_ShowNotification("The trial cannot begin right now...");
                    op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                    return;
                }

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
        if (!Fighter1.Value.Alive() || !Fighter2.Value.Alive()) return;

        var fighter1 = Fighter1.Value.GetComponent<OfficePlayer>();
        var fighter2 = Fighter2.Value.GetComponent<OfficePlayer>();
        if (!fighter1.Alive() || !fighter2.Alive()) return;

        var fighter1SeatEntity = fighter1.AssignedMeetingSeat.Value;
        var fighter2SeatEntity = fighter2.AssignedMeetingSeat.Value;
        if (!fighter1SeatEntity.Alive() || !fighter2SeatEntity.Alive()) return;

        var fighter1BattleSeat = fighter1SeatEntity.GetComponent<Seat>();
        var fighter2BattleSeat = fighter2SeatEntity.GetComponent<Seat>();
        if (!fighter1BattleSeat.Alive() || !fighter2BattleSeat.Alive()) return;

        var fighter1Revolver = fighter1.GetAbility<Revolver>();
        var fighter2Revolver = fighter2.GetAbility<Revolver>();
        if (fighter1Revolver != null)
        {
            fighter1Revolver.CooldownRemaining = 3f;
        }

        if (fighter2Revolver != null)
        {
            fighter2Revolver.CooldownRemaining = 3f;
        }

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

        fighter1.Teleport(fighter1BattleSeat.Position);
        fighter2.Teleport(fighter2BattleSeat.Position);
    }

    [ClientRpc]
    public void EndFight1(Entity fighter1)
    {
        if (!fighter1.Alive()) return;

        var fighter1Op = fighter1.GetComponent<OfficePlayer>();

        if (!fighter1Op.Alive()) return;

        fighter1Op.SetLightOn(false);
        fighter1Op.Teleport(Vector2.Zero);
        fighter1Op.RemoveEffect<SpectatorEffect>(false);
        fighter1Op.RemoveEffect<AimingRevolver>(false);
    }

    [ClientRpc]
    public void EndFight2(Entity fighter2)
    {
        if (!fighter2.Alive()) return;

        var fighter2Op = fighter2.GetComponent<OfficePlayer>();

        if (!fighter2Op.Alive()) return;

        fighter2Op.SetLightOn(false);
        fighter2Op.Teleport(Vector2.Zero);
        fighter2Op.RemoveEffect<SpectatorEffect>(false);
        fighter2Op.RemoveEffect<AimingRevolver>(false);
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

            if (BattleActive && (!fighter1.Alive() || !fighter2.Alive()))
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

            if (BattleActive && (Time.TimeSinceStartup - BattleStartTime >= 17f || fighter1.WasKilledInOverseerBattle || fighter2.WasKilledInOverseerBattle))
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
        var op = Network.LocalPlayer as OfficePlayer;
        if (!op.Alive()) return;
        if (!spriteRenderer.Alive()) return;

        if (op.CurrentRole == Role.JANITOR)
        {
            spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
        }
        else
        {
            spriteRenderer.Tint = new Vector4(1, 0.5f, 0.5f, 1);
        }

        var interactible = Entity.GetComponent<Interactable>();
        if (!interactible.Alive()) return;

        if (op.CurrentRole == Role.OVERSEER)
        {
            interactible.Text = "Keep my janitors safe.";
            return;
        }

        interactible.Text = "Commence the Overseer's Trial ($500)";
    }
}

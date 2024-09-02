using System.Runtime.InteropServices;
using AO;

public partial class PromoNPC : Component
{
    [Serialized] Seat candidateSeat1;
    [Serialized] Seat candidateSeat2;

    public SyncVar<bool> BoardMeetingActive = new(false);
    public Interactable interactable;
    public static PromoNPC Instance;

    public override void Awake()
    {
        Instance = this;

        interactable = Entity.AddComponent<Interactable>();
        interactable.OnInteract = (Player p) =>
        {
            if (!Network.IsServer) return;
            var op = (OfficePlayer)p;

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

            if (BoardMeetingActive)
            {
                op.CallClient_ShowNotification("You cannot be promoted during a board meeting");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
            }

            switch (op.CurrentRole)
            {
                case Role.JANITOR:
                    op.CurrentRole = Role.EMPLOYEE;
                    op.Experience.Set(0);
                    op.CallClient_PlaySFX("sfx/rank-up.wav");
                    op.CallClient_ShowNotification("New tasks unlocked...");
                    op.CallClient_ShowNotification("Office purchasing unlocked...");
                    op.CallClient_ShowNotification("Vending machines unlocked...");
                    break;
                case Role.EMPLOYEE:
                    op.CurrentRole = Role.MANAGER;
                    op.Experience.Set(0);

                    op.CallClient_ShowNotification("New tasks unlocked...");
                    op.CallClient_ShowNotification("Luxury offices unlocked");
                    
                    op.CallClient_PlaySFX("sfx/rank-up.wav");
                    break;
                case Role.MANAGER:
                    if (!op.HasGivenSpeech)
                    {
                        op.CallClient_ShowNotification("You must give a speech in the conference room first...");
                        op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                        break;
                    }

                    var CEOPlayers = GameManager.Instance.GetPlayersByRole(Role.CEO);
                    var CEOPlayer = CEOPlayers.Length > 0 ? (OfficePlayer)CEOPlayers[0] : null;
                    if (CEOPlayer.Alive())
                    {
                        foreach (Player player in Player.AllPlayers)
                        {
                            var op2 = (OfficePlayer)player;
                            op2.IsBoardElectionCandidate.Set(false);
                        }

                        CEOPlayer.IsBoardElectionCandidate.Set(true);
                        op.IsBoardElectionCandidate.Set(true);

                        var seats = new List<Seat>();
                        foreach (var seat in Scene.Components<Seat>())
                        {
                            if (seat.Type == "Board")
                            {
                                seats.Add(seat);
                                Log.Info("Seat info " + seat.Entity.Name);
                            }
                        }

                        foreach (Player player in Player.AllPlayers)
                        {
                            var op2 = (OfficePlayer)player;
                            op2.AssignedMeetingSeat.Set(seats.Pop().Entity);
                        }

                        CEOPlayer.AssignedMeetingSeat.Set(candidateSeat1.Entity);
                        op.AssignedMeetingSeat.Set(candidateSeat2.Entity);

                        CallClient_StartBoardMeeting();
                    } else {
                        op.CurrentRole = Role.CEO;
                        op.Experience.Set(0);
                        op.CallClient_PlaySFX("sfx/rank-up.wav");
                    }

                    break;
                case Role.CEO:
                    op.CallClient_ShowNotification("The board knows what you're trying to do... to be continued...");
                    op.CallClient_PlaySFX("sfx/clue_found2.wav");
                    op.CallClient_PlaySFX("sfx/creepy-phone.wav");
                    break;
            }
        };
    }

    [ClientRpc]
    public void StartBoardMeeting()
    {
        if (Network.IsClient) {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/clue_found2.wav"), new());
        }

        if (Network.IsServer) {
            BoardMeetingActive.Set(true);
        }

        foreach (Player player in Player.AllPlayers)
        {
            player.AddEffect<BoardMeetingEffect>();
        }
    }

    public override void Update()
    {
        if (Network.IsServer) return;
        var interactible = Entity.GetComponent<Interactable>();
        var workerPlayer = (OfficePlayer)Network.LocalPlayer;
        if (!workerPlayer.Alive()) return;
        if (workerPlayer.CurrentRole == Role.JANITOR)
        {
            interactible.Text = "Request Promotion to Employee... (100XP)";
            return;
        } else if (workerPlayer.CurrentRole == Role.EMPLOYEE)
        {
            interactible.Text = "Request Promotion to Manager... (100XP)";
            return;
        } else if (workerPlayer.CurrentRole == Role.MANAGER)
        {
            interactible.Text = "Request Promotion to CEO... (100XP) (Requires Conference Speech Given)";
            return;
        } else if (workerPlayer.CurrentRole == Role.CEO)
        {
            interactible.Text = "Request Promotion... (100XP)";
            return;
        }
    }
}

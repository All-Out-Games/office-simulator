using System.Runtime.InteropServices;
using AO;

public partial class PromoNPC : Component
{
    [Serialized] Seat candidateSeat1;
    [Serialized] Seat candidateSeat2;

    public SyncVar<bool> BoardMeetingActive = new(false);
    public Interactable interactable;
    public static PromoNPC Instance;

    public SyncVar<Entity> Candidate1 = new();
    public SyncVar<Entity> Candidate2 = new();
    public SyncVar<int> Candidate1Votes = new();
    public SyncVar<int> Candidate2Votes = new();
    public SyncVar<float> BoardMeetingStartTime = new();

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
                        Candidate1.Set(CEOPlayer.Entity);
                        Candidate2.Set(op.Entity);

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
                        GameManager.Instance.CallClient_ShowNotification(op.Name + " is the first to reach CEO!");
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

    [ServerRpc]
    public void CountVote(int cadidate)
    {
        if (!Candidate1.Value.Alive() || !Candidate2.Value.Alive())
        {
            Log.Error("Candidates are not alive");
            return;
        }

        if (cadidate == 1)
        {
            Candidate1Votes.Set(Candidate1Votes + 1);
            Log.Info("Candidate 1 votes: " + Candidate1Votes);
        } else if (cadidate == 2) {
            Candidate2Votes.Set(Candidate2Votes + 1);
            Log.Info("Candidate 2 votes: " + Candidate2Votes);
        } else {
            Log.Error("Invalid candidate vote");
        }
    }

    [ClientRpc]
    public void StartBoardMeeting()
    {
        if (Network.IsClient) {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/clue_found2.wav"), new());
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/suspense.wav"), new());
        }

        if (Network.IsServer) {
            Candidate1Votes.Set(0);
            Candidate2Votes.Set(0);

            BoardMeetingActive.Set(true);
            BoardMeetingStartTime.Set(Time.TimeSinceStartup);

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
            if (BoardMeetingActive && Time.TimeSinceStartup - BoardMeetingStartTime >= 17f)
            {
                BoardMeetingActive.Set(false);

                foreach (Player player in Player.AllPlayers)
                {
                    player.RemoveEffect<BoardMeetingEffect>(false);
                }

                if (Candidate1.Value == null || Candidate2.Value == null)
                {
                    Log.Error("Candidate was null");
                    GameManager.Instance.CallClient_ShowNotification("The board meeting has been cancelled due to a candidate being unavailable.");
                    return;
                }

                var candidate1 = Candidate1.Value.GetComponent<OfficePlayer>();
                var candidate2 = Candidate2.Value.GetComponent<OfficePlayer>();

                if (!candidate1.Alive() || !candidate2.Alive())
                {
                    Log.Error("Candidate was not alive");
                    GameManager.Instance.CallClient_ShowNotification("The board meeting has been cancelled due to a candidate being unavailable.");
                    return;
                }

                // Right candidate is the newcomer, the old candidate will lose their stuff if this person wins
                if (Candidate2Votes.Value >= Candidate1Votes.Value)
                {
                    candidate2.CurrentRole = Role.CEO;
                    candidate1.CurrentRole = Role.MANAGER;
                    candidate1.OfficeController?.Value?.GetComponent<OfficeController>().Reset();
                    candidate1.Experience.Set(0);
                    GameManager.Instance.CallClient_ShowNotification($"{candidate2.Name} has been elected as the new CEO.");
                }
                else
                {
                    // Incumbent keeps their role
                    candidate1.CurrentRole = Role.CEO;
                    GameManager.Instance.CallClient_ShowNotification($"{candidate1.Name} will remain as the CEO.");
                }
            }
        }


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

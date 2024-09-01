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
                GameManager.Instance.CallClient_ShowNotification("You must wait till night has passed...");
                GameManager.Instance.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (op.Experience < op.RequiredExperience)
            {
                GameManager.Instance.CallClient_ShowNotification("You need more experience to be promoted...");
                GameManager.Instance.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                return;
            }

            if (BoardMeetingActive)
            {
                GameManager.Instance.CallClient_ShowNotification("You cannot be promoted during a board meeting");
                GameManager.Instance.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
            }

            switch (op.CurrentRole)
            {
                case Role.JANITOR:
                    op.CurrentRole = Role.EMPLOYEE;
                    op.Experience.Set(0);
                    GameManager.Instance.CallClient_PlaySFX("sfx/rank-up.wav");
                    break;
                case Role.EMPLOYEE:
                    op.CurrentRole = Role.MANAGER;
                    op.Experience.Set(0);
                    GameManager.Instance.CallClient_PlaySFX("sfx/rank-up.wav");
                    break;
                case Role.MANAGER:
                    // if (!op.HasGivenSpeech)
                    // {
                    //     GameManager.Instance.CallClient_ShowNotification("You must give a speech in the conference room first...");
                    //     GameManager.Instance.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                    //     break;
                    // }

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
                            }
                        }

                        foreach (Player player in Player.AllPlayers)
                        {
                            var op2 = (OfficePlayer)player;
                            op2.AssignedMeetingSeat.Set(seats.PopFront().Entity);
                        }

                        CEOPlayer.AssignedMeetingSeat.Set(candidateSeat1.Entity);
                        op.AssignedMeetingSeat.Set(candidateSeat2.Entity);

                        CallClient_StartBoardMeeting();
                    } else {
                        op.CurrentRole = Role.CEO;
                        op.Experience.Set(0);
                        GameManager.Instance.CallClient_PlaySFX("sfx/rank-up.wav");
                    }

                    break;
                case Role.CEO:
                    GameManager.Instance.CallClient_ShowNotification("The board knows what you're trying to do... to be continued...");
                    GameManager.Instance.CallClient_PlaySFX("sfx/night-hit.wav");
                    GameManager.Instance.CallClient_PlaySFX("sfx/creepy-phone.wav");
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
        interactible.Text = "Request Promotion...";
    }
}

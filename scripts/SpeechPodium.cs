using AO;

public class SpeechPodium : Activity
{

  public override void Awake()
  {
    base.Awake();

    interactable.OnInteract = (player) => {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)player;
      var confPlayers = RoomBounds.GetPlayersInRoom(Room.CONFERENCE);

      if (confPlayers.Count < 2)
      {
        GameManager.Instance.CallClient_ShowNotification("Head to the conference room to listen to " + op.Name + "'s speech!");
        op.CallClient_ShowNotification("There must be at least 2 players in the audience!");
        return;
      }

      op.HasGivenSpeech.Set(true);
      base.OnInteract(player);
    };
  }
} 
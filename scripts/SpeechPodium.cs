using AO;

public class SpeechPodium : Activity
{

  public override void Start()
  {
    base.Start();

    interactable.OnInteract = (player) => {
      if (!Network.IsServer) return;

      var confPlayers = RoomBounds.GetPlayersInRoom(Room.CONFERENCE);

      if (confPlayers.Count < 2)
      {
        GameManager.Instance.CallClient_ShowNotification("There must be at least 2 players in the audience!");
        return;
      }

      base.OnInteract(player);
    };
  }
}
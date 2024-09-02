using AO;

public class SpeechPodium : Activity
{

  public override void Start()
  {
    base.Start();

    interactable.OnInteract = (player) => {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)player;
      var confPlayers = RoomBounds.GetPlayersInRoom(Room.CONFERENCE);

      if (confPlayers.Count < 2)
      {
        GameManager.Instance.CallClient_ShowNotification(op.Name + " tried to give a speech to become CEO, but there must be at least 2 players in the audience!");
        op.CallClient_ShowNotification("There must be at least 2 players in the audience!");
        return;
      }

      op.HasGivenSpeech.Set(true);
      base.OnInteract(player);
    };
  }
} 
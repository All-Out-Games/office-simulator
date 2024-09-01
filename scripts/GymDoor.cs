using AO;

public class GymDoor : TwoWayDoor
{
  public override void Awake()
  {
    base.Awake();
    
    interactable.OnInteract = (Player player) =>
    {
      if (!Network.IsServer) return;

      var op = (OfficePlayer)player;

      if (!op.HasGymPass)
      {
        GameManager.Instance.CallClient_ShowNotification("You don't have a gym pass!");
        return;
      }

      base.OnInteract(player);
    };
  }

  public override void Update()
  {
    if (Network.IsServer) return;

    var op = (OfficePlayer)Network.LocalPlayer;
    if (!op.Alive()) return;

    if (op.CurrentRoom != RoomName)
    {
      interactable.Text = $"Enter {RoomName}";
    }
    else
    {
      interactable.Text = $"Leave {RoomName}";
    }
  }
}
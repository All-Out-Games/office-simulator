using AO;

public class TwoWayDoor : Component
{
  [Serialized]
  public Entity Inside;

  [Serialized]
  public Entity Outside;

  [Serialized]
  public Room RoomName;

  protected Interactable interactable;


  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.OnInteract = OnInteract;
  }

  public void OnInteract(Player player)
  {
    if (!Network.IsServer) return;

    var op = (OfficePlayer)player;

    if (op.CurrentRoom != RoomName)
    {
      op.Teleport(Inside.Position);
    }
    else
    {
      op.Teleport(Outside.Position);
      op.CurrentRoom = Room.HALLS;
    } 
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
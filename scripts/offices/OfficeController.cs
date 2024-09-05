using AO;

public class OfficeController : Component
{
  [Serialized] public Role RequiredRole;
  [Serialized] public int BreachCost;
  [Serialized] public int Cost;
  public SyncVar<bool> Unlocked = new(false);

  public SyncVar<Entity> Owner = new();
  
  

  public bool IsOwned => Owner.Value != null && Owner.Value.Alive();
  public bool IsOwnedByMyClient => IsOwned && Owner.Value == Network.LocalPlayer.Entity;

  public override void Awake()
  {

  }

  public override void Update()
  {

  }

  public void Reset()
  {
    var room = Entity.Parent.TryGetChildByName("Door").GetComponent<OfficeDoor>().RoomName;
    var playersInOffice = RoomBounds.GetPlayersInRoom(room);


    foreach (var player in playersInOffice)
    {
      var op = (OfficePlayer)player;
      op.Teleport(Entity.Parent.TryGetChildByName("Door").GetComponent<OfficeDoor>().Outside.Position);
    }


    if (!Network.IsServer) return;
    if (Owner.Value.Alive())
    {
      var op = Owner.Value.GetComponent<OfficePlayer>();
      op.OfficeController.Set(null);
    }


    foreach (var player in playersInOffice)
    {
      var op = (OfficePlayer)player;
      op.CurrentRoom = Room.HALLS;
      op.CallClient_ShowNotification("The office has been reset...");
    }

    var buyables = Entity.Parent.TryGetChildByName("Buyables");
    if (buyables != null)
    {
      foreach (var child in buyables.Children)
      {
        child.GetComponent<Buyable>().Bought.Set(false);
      }
    }

    Owner.Set(null);
    Unlocked.Set(false);
  }
}
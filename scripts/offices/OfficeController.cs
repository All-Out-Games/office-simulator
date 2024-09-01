using AO;

public class OfficeController : Component
{
  [Serialized] public Role RequiredRole;
  [Serialized] public int Cost;

  public SyncVar<Entity> Owner = new();
  

  public bool IsOwned => Owner.Value != null && Owner.Value.Alive();
  public bool IsOwnedByMyClient => IsOwned && Owner.Value == Network.LocalPlayer.Entity;

  public override void Awake()
  {

  }

  public override void Update()
  {

  }
}
using AO;

public class OfficeController : Component
{
  [Serialized] public Role RequiredRole;
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
    Owner.Set(null);
    Unlocked.Set(false);

    var buyables = Entity.Parent.TryGetChildByName("Buyables");
    if (buyables != null)
    {
      foreach (var child in buyables.Children)
      {
        child.GetComponent<Buyable>().Bought.Set(false);
      }
    }
  }
}
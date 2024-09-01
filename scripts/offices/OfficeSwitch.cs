using AO;

public class OfficeSwitch : Component
{
  public OfficeController Controller;
  public Interactable interactable;

  public override void Awake()
  {
    Controller = Entity.Parent.TryGetChildByName("Controller").GetComponent<OfficeController>();

    interactable = AddComponent<Interactable>();
    interactable.CanUseCallback = (Player p) => {
      var op = (OfficePlayer)p;

      if (Controller.Owner != op.Entity) {
        return false;
      }

      return true;
    };

    interactable.OnInteract = (Player p) => {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)p;

      Controller.Unlocked.Set(!Controller.Unlocked);
    };
  }
}
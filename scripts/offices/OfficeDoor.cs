using AO;

public class OfficeDoor : TwoWayDoor
{
  OfficeController Controller;

  public override void Start()
  {
    base.Start();
    Controller = Entity.Parent.TryGetChildByName("Controller").GetComponent<OfficeController>();
    
    interactable.OnInteract = (Player player) =>
    {
      if (!Network.IsServer) return;

      var op = (OfficePlayer)player;

      if (!Controller.IsOwned)
      {
        if (op.Cash < Controller.Cost)
        {
          GameManager.Instance.ShowNotification("You don't have enough cash");
          GameManager.Instance.PlaySFX("sfx/error.wav");
          return;
        }
      }

      Controller.Owner.Set(op.Entity);

      base.OnInteract(player);
    };
  }

  private string GetOfficeName()
  {
    if (Controller.IsOwnedByMyClient) return "Your Office";
    return Controller.Entity.Parent.Name;
  }

  public override void Update()
  {
    if (Network.IsServer) return;

    var op = (OfficePlayer)Network.LocalPlayer;
    if (!op.Alive()) return;

    if (op.CurrentRoom != RoomName)
    {
      if (Controller.IsOwned) {
        interactable.Text = $"Enter {GetOfficeName()}";
      }
      else 
      {
        interactable.Text = $"Buy {GetOfficeName()} - ${Controller.Cost}";
      }
    }
    else
    {
      interactable.Text = $"Leave {GetOfficeName()}";
    }
  }
}
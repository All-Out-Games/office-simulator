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

      if (Controller.IsOwned && Controller.Owner != player.Entity && !Controller.Unlocked)
      {
        if (op.CurrentRoom != RoomName)
        {
          if (op.Cash < 333)
          {
            op.CallClient_ShowNotification("This office is locked... not enough $ to breach");
            op.CallClient_PlaySFX("sfx/error.wav");
            return;
          } else {
            // Breach for money
            op.CallClient_PlaySFX("sfx/invisibility_off.wav");
            op.Cash.Set(op.Cash - 333);
          }
        }
      }

      if (!Controller.IsOwned)
      {
        if (op.CurrentRole < Controller.RequiredRole)
        {
          op.CallClient_ShowNotification("You must reach the " + Controller.RequiredRole + " role to buy this office.");
          op.CallClient_PlaySFX("sfx/error.wav");
          return;
        }

        if (op.Cash < Controller.Cost)
        {
          op.CallClient_ShowNotification("You don't have enough cash");
          op.CallClient_PlaySFX("sfx/error.wav");
          return;
        }

        op.OfficeController?.Value?.GetComponent<OfficeController>().Reset();
        op.CallClient_PlaySFX("sfx/rank-up.wav");
        op.Cash.Set(op.Cash - Controller.Cost);
        Controller.Owner.Set(op.Entity);
        op.OfficeController.Set(Controller.Entity);
      }

      base.OnInteract(player);
    };
  }

  private string GetOfficeName()
  {
    if (Controller.IsOwnedByMyClient) return "Your Office";
    if (Controller.IsOwned) return Controller.Owner.Value.Name + "'s Office";
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
        if (!Controller.Unlocked && !Controller.IsOwnedByMyClient)
        {
          interactable.Text = $"{GetOfficeName()} (Locked - Breach $333)";
          return;
        }

        interactable.Text = $"Enter {GetOfficeName()}";
      }
      else 
      {
        if (op.OfficeController.Value != null)
        {
          interactable.Text = $"Buy {GetOfficeName()} - ${Controller.Cost} (DESTROYS OLD OFFICE)";
        } else {
          interactable.Text = $"Buy {GetOfficeName()} - ${Controller.Cost}";
        }
      }
    }
    else
    {
      interactable.Text = $"Leave {GetOfficeName()}";
    }
  }
}
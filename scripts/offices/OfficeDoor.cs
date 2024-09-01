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
          GameManager.Instance.CallClient_ShowNotification("This office is locked...");
          GameManager.Instance.CallClient_PlaySFX("sfx/error.wav");
          return;
      }

      if (!Controller.IsOwned)
      {
        if (op.Cash < Controller.Cost)
        {
          GameManager.Instance.CallClient_ShowNotification("You don't have enough cash");
          GameManager.Instance.CallClient_PlaySFX("sfx/error.wav");
          return;
        }

        GameManager.Instance.CallClient_PlaySFX("sfx/rank-up.wav");
        op.Cash.Set(op.Cash - Controller.Cost);
        Controller.Owner.Set(op.Entity);
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
          interactable.Text = $"{GetOfficeName} (Locked)";
        }

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
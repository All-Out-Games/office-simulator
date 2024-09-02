using AO;

public class OfficeSwitch : Component
{
  public OfficeController Controller;
  public Interactable interactable;

  public override void Awake()
  {
    Controller = Entity.Parent.TryGetChildByName("Controller")?.GetComponent<OfficeController>();

    // Log null
    if (!Controller.Alive())
    {
      Log.Error("OfficeSwitch " + Entity.Name + " doesn't have a parent office controller");
    }

    interactable = AddComponent<Interactable>();
    interactable.CanUseCallback = (Player p) => {
      var op = (OfficePlayer)p;

      if (Controller.IsOwned && Controller.Owner != op.Entity) {
        return false;
      }

      return true;
    };

    interactable.OnInteract = (Player p) => {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)p;

      if (Controller.Owner != op.Entity) {
        op.CallClient_ShowNotification("You must own this office to lock/unlock it");
        op.CallClient_PlaySFX("sfx/error.wav");
        return;
      }

      var newUnlocked = !Controller.Unlocked;

      if (!newUnlocked)
      {
        var inOfficePlayers = Controller.GetComponent<RoomBounds>().GetPlayersInRoom();
        // Kick all non owners out of the office when it gets locked
        foreach (OfficePlayer player in inOfficePlayers)
        {
          if (Controller.Owner.Value == player.Entity || player.CurrentRole != Role.JANITOR) continue;

          player.CallClient_ShowNotification("This office has been locked");
          player.CallClient_PlaySFX("sfx/warp.wav");
          player.Teleport(Controller.Entity.Parent.TryGetChildByName("Door").GetComponent<OfficeDoor>().Outside.Position);
          player.CurrentRoom = Room.HALLS;
        }
      }

      Controller.Unlocked.Set(!Controller.Unlocked);
    };
  }

  public override void Update()
  {
    interactable.Text = Controller.Unlocked ? "Lock" : "Unlock";
  }
}
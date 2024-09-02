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

      var newUnlocked = !Controller.Unlocked;

      if (!newUnlocked)
      {
        var inOfficePlayers = Controller.GetComponent<RoomBounds>().GetPlayersInRoom();
        // Kick all non owners out of the office when it gets locked
        foreach (OfficePlayer player in inOfficePlayers)
        {
          if (Controller.Owner.Value == player.Entity) continue;
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
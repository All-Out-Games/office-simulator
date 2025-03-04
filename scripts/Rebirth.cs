using AO;

public class Rebirth : Component
{
  private Interactable interactable;
  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.Text = "Rebirth as Janitor (you keep your cash)";
    interactable.OnInteract += (Player p) => {
      var op = (OfficePlayer)p;
      op.Teleport(new Vector2(0, 0));

      if (!Network.IsServer) return;
      
      op.CurrentRole = Role.JANITOR;
      op.Experience.Set(0);
      op.OfficeController?.Value?.GetComponent<OfficeController>().Reset();
      op.CurrentRoom = Room.HALLS;
      op.CallClient_PlaySFX("sfx/rank-up.wav");
      op.CallClient_ShowNotification("You have been reborn as a Janitor...");
    };
  }
}
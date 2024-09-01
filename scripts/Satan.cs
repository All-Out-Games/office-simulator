using AO;

public class Satan : Component
{
  private Interactable interactable;
  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.Text = "Repent";
    interactable.OnInteract += (Player p) => {
      p.RemoveEffect<SpectatorEffect>(false);
    };
  }
}
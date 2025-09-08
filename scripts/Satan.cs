using AO;

public class Satan : Component
{
  private Interactable interactable;
  public override void Awake()
  {
    interactable = Entity.Unsafe_AddComponent<Interactable>();
    interactable.Text = "Repent";
    interactable.OnInteract += (Player p) =>
    {
      p.RemoveEffect<SpectatorEffect>(false);
    };
  }
}
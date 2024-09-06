using AO;

public class StolenBag : Component
{
  public SyncVar<bool> Fixed = new(false);
  private Interactable interactable;
  private Sprite_Renderer spriteRenderer;
  private SpriteFlasher spriteFlasher;
  public override void Awake()
  {
    spriteFlasher = Entity.AddComponent<SpriteFlasher>();
    spriteFlasher.Flash = false;

    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    interactable = Entity.AddComponent<Interactable>();
    interactable.Text = "Recover Stolen Cash";
    interactable.CanUseCallback = (Player p) =>
    {
      return !Fixed;
    };
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;
      Fix(op);
    };
  }

  public override void Update()
  {
    if (Fixed)
    {
      spriteFlasher.Flash = false;
      spriteRenderer.Tint = new Vector4(0, 0, 0, 0);

    }
    else
    {
      spriteFlasher.Flash = true;
    }
  }

  public override void Start()
  {
    StopEvent();
  }

  private void Fix(OfficePlayer op)
  {
    if (!Network.IsServer) return;
    Fixed.Set(true);
    op?.CallClient_PlaySFX("sfx/money.wav");
  }

  public void StartEvent()
  {
    if (Network.IsServer)
    {
      Fixed.Set(false);
    }
  }

  public void StopEvent()
  {
    spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
    Fix(null);
  }

}
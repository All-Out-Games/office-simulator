using AO;

public class Mop : Component
{
  public SyncVar<bool> Fixed = new(false);
  private Interactable interactable;
  private Sprite_Renderer spriteRenderer;
  private SpriteFlasher spriteFlasher;
  public override void Awake()
  {
    spriteFlasher = Entity.Unsafe_AddComponent<SpriteFlasher>();
    spriteFlasher.Flash = false;

    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    interactable = Entity.Unsafe_AddComponent<Interactable>();
    interactable.Text = "Mop Water";
    interactable.RequiredHoldTime = 0.25f;
    interactable.CanUseCallback = (Player p) =>
    {
      return !Fixed;
    };
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;
      Fix(op);
    };

    StopEvent();
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

  private void Fix(OfficePlayer op)
  {
    if (!Network.IsServer) return;
    Fixed.Set(true);
    op?.CallClient_PlaySFX("anomalies/flood/mop.wav");
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
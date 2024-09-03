using AO;

// Can be added to an entity for visual emphasis (e.g. can be interacted with)
public class SpriteFlasher : Component
{
  public bool Flash = false;
  private Sprite_Renderer spriteRenderer;
  private float curTintOffset;
  public float FlashDepth = 0.625f;
  public float FlashSpeed = 1.6f;

  public override void Awake()
  {
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();

    if (!spriteRenderer.Alive())
    {
      Log.Error($"Attempted to add SpriteFlasher to entity ({Entity.Name}) but no sprite renderer was found on it.");
      this.LocalEnabled = false;
    }
  }

  public override void Update()
  {
    if (!Flash)
    {
      Entity.GetComponent<Sprite_Renderer>().Tint = new Vector4(1f, 1f, 1f, 1f);
      return;
    }
    
    curTintOffset += Time.DeltaTime * FlashSpeed;
    float tintValue = MathF.Sin(curTintOffset) * FlashDepth + 0.8f;
    Entity.GetComponent<Sprite_Renderer>().Tint = new Vector4(tintValue, tintValue, tintValue, 1f);
  }

    public override void OnDestroy()
    {
      spriteRenderer.Tint = new Vector4(1f, 1f, 1f, 1f);
    }
}
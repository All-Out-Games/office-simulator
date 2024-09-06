using AO;

public class Flame : Component
{
  private Sprite_Renderer spriteRenderer;
  [Serialized] Texture frame1;
  [Serialized] Texture frame2;
  [Serialized] Texture frame3;

  public override void Awake()
  {
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    spriteRenderer.Texture = frame1;
  }

  public override void Update()
  {
    if (Time.TimeSinceStartup % 0.25f < Time.DeltaTime)
    {
      if (spriteRenderer.Texture == frame1)
      {
        spriteRenderer.Texture = frame2;
      }
      else if (spriteRenderer.Texture == frame2)
      {
        spriteRenderer.Texture = frame3;
      }
      else
      {
        spriteRenderer.Texture = frame1;
      }
    }
  }
}
using AO;

public class SpookyLight : Component
{
  private Light light;
  public bool On;
  Random random;

  public override void Awake()
  {
    light = Entity.AddComponent<Light>();
    light.Color = new Vector4(1, 0.6f, 0.4f, 0);
    light.Intensity = 1f;
    light.Radi = new Vector2(0, 8.5f);
    light.ShadowCaster = true;
    random = new Random();
  }

  public override void Update()
  {
    // TODO: the closer a zombie is the more your light flickers out
  }
}

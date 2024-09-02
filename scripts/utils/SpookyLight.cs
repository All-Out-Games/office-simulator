using AO;

public class SpookyLight : Component
{
  public Light Light;
  public bool On;
  Random random;

  public override void Awake()
  {
    Light = Entity.AddComponent<Light>();
    Light.Color = new Vector4(1, 0.6f, 0.4f, 0);
    Light.Intensity = 1f;
    Light.Radi = new Vector2(0, 8.5f);
    Light.ShadowCaster = true;
    Light.Softness = 0.35f;
    random = new Random();
  }

  public override void Update()
  {

  }
}

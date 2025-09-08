using AO;

public partial class MoneyCollectUI : Component
{
  private static float animationDuration = 0.6f;
  private static Vector2 startScale = new Vector2(3f, 3f);
  private static Vector2 endScale = new Vector2(1f, 1f);
  private static float startRotation = -20f;
  private static float endRotation = 20f;

  private class MoneyAnimation
  {
    public Vector2 StartPosition;
    public float StartTime;
    public float Amount;
  }

  private static List<MoneyAnimation> activeAnimations = new();

  [ClientRpc]
  public void PlayMoneyCollectAnimation(Vector2 startPosition, float amount)
  {
    if (!Network.IsClient) return;

    activeAnimations.Add(new MoneyAnimation
    {
      StartPosition = startPosition,
      StartTime = Time.TimeSinceStartup,
      Amount = amount
    });
  }

  public override void Update()
  {
    if (!Network.IsClient) return;

    for (int i = activeAnimations.Count - 1; i >= 0; i--)
    {
      var anim = activeAnimations[i];
      var elapsed = Time.TimeSinceStartup - anim.StartTime;
      var progress = elapsed / animationDuration;

      if (progress >= 1f)
      {
        activeAnimations.RemoveAt(i);
        continue;
      }

      var moneyUI = References.Instance.MoneyStatText;
      if (!moneyUI.Alive()) continue;

      var endPosition = moneyUI.Entity.Position;
      var currentPos = Vector2.Lerp(anim.StartPosition, endPosition, progress);
      var currentScale = Vector2.Lerp(startScale, endScale, progress);
      var currentRotation = AOMath.Lerp(startRotation, endRotation, progress);
      var alpha = 1f - progress;

      var cashIcon = Assets.GetAsset<Texture>("ui/Cash Outline 64.png");
      var baseSize = 128f;
      var rect = UI.ScreenRect.CenterRect()
        .Scale(baseSize * currentScale.X / UI.ScreenRect.Width, baseSize * currentScale.Y / UI.ScreenRect.Height)
        .Offset(currentPos.X - baseSize / 2, currentPos.Y - baseSize / 2);

      using var _ = UI.PUSH_ROTATE_ABOUT_POINT(currentRotation, rect.Center);
      UI.Image(rect, cashIcon, new Vector4(1, 1, 1, 1));

      var ts = new UI.TextSettings()
      {
        Size = 32 * currentScale.X,
        Color = new Vector4(0, 1, 0, alpha),
        Font = UI.Fonts.BarlowBold,
        VerticalAlignment = UI.VerticalAlignment.Center,
        HorizontalAlignment = UI.HorizontalAlignment.Center,
        Outline = true,
        OutlineThickness = 3,
        OutlineColor = new Vector4(0, 0, 0, alpha)
      };

      UI.TextAsync(rect.Offset(0, 60 * currentScale.Y), $"+${anim.Amount}", ts);
    }
  }
}
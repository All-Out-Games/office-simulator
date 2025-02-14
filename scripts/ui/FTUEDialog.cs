using AO;

public class FTUEDialog : Component
{
  private static bool isShowing;
  private static float showStartTime;
  private const float SECTION_TRANSITION_TIME = 0.5f;
  private const float ICON_PULSE_SPEED = 2f; // Speed of icon pulsing
  private const float ICON_PULSE_AMOUNT = 0.1f; // How much the icons scale
  private const float BUTTON_PULSE_SPEED = 1.5f; // Speed of button pulsing
  private const float BUTTON_PULSE_AMOUNT = 0.05f; // How much the button scales

  private static readonly (string Title, string Content, string ImagePath)[] sections =
  {
        (
            "Work your way up",
            "Start as a janitor, fix things around the office to work your way up the corporate ladder.",
            "ui/Upgrade Outline 64.png" // Placeholder image
        ),
        (
            "Buy & Upgrade your Office",
            "Buy and upgrade your office to make it more comfortable... and safe...",
            "table.png" // Placeholder image
        ),

        (
            "Make friends... or enemies",
            "Something spooky happens in the office at night... ",
            "ui/Cash Outline 64.png" // Placeholder image
        )
    };

  public override void Awake()
  {
    if (Network.IsClient)
    {
      Show();
    }
  }

  public static void Show()
  {
    isShowing = true;
    showStartTime = Time.TimeSinceStartup;
  }

  public static void Hide()
  {
    isShowing = false;
  }

  public static void PreviewFTUE()
  {
    Show();
    Draw(Time.TimeSinceStartup);
  }

  public override void Update()
  {
    if (Network.IsClient && isShowing)
    {
      Draw(Time.TimeSinceStartup);
    }
  }

  private static void Draw(float time)
  {
    // Semi-transparent background overlay
    UI.Image(UI.ScreenRect, null, new Vector4(0, 0, 0, 0.75f));

    // Main dialog container using button sprite
    var dialogRect = UI.ScreenRect.CenterRect().Grow(350, 325, 350, 325);
    UI.Image(dialogRect, Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_9.png"), Vector4.White);


    var contentRect = dialogRect.Inset(20);

    // Title text settings
    var titleTs = new UI.TextSettings()
    {
      Font = UI.Fonts.BarlowBold,
      Size = 36,
      Color = Vector4.White,
      DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
      DropShadowOffset = new Vector2(0f, -3f),
      HorizontalAlignment = UI.HorizontalAlignment.Left,
      VerticalAlignment = UI.VerticalAlignment.Center,
      Outline = true,
      OutlineThickness = 3,
    };

    // Content text settings
    var contentTs = new UI.TextSettings()
    {
      Font = UI.Fonts.BarlowBold,
      Size = 24,
      Color = new Vector4(0.7f, 0.7f, 0.7f, 1f),
      DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
      DropShadowOffset = new Vector2(0f, -2f),
      HorizontalAlignment = UI.HorizontalAlignment.Left,
      VerticalAlignment = UI.VerticalAlignment.Center,
      WordWrap = true,
      Outline = true,
      OutlineThickness = 2,
    };

    // Main title
    var mainTitleTs = titleTs;
    mainTitleTs.Size = 48;
    mainTitleTs.HorizontalAlignment = UI.HorizontalAlignment.Center;
    UI.Text(contentRect.CutTop(60), "Five Nights at The Office", mainTitleTs);
    contentRect.CutTop(20); // Spacing after main title

    // Animate entrance
    float slideOffset = Ease.OutQuart(MathF.Min(1, time - showStartTime)) * 1080f;

    // Draw each section
    for (int i = 0; i < sections.Length; i++)
    {
      var section = sections[i];
      var sectionRect = contentRect.CutTop(120);
      if (i < sections.Length - 1) contentRect.CutTop(20); // Spacing between sections

      // Section container with slight tint
      UI.Image(sectionRect, null, new Vector4(1, 1, 1, 0.05f));
      sectionRect = sectionRect.Inset(10);

      // Image on the left with pulsing animation
      var imageRect = sectionRect.CutLeft(100).Inset(5);
      var image = Assets.GetAsset<Texture>(section.ImagePath);

      // Calculate pulse scale based on time
      float pulseScale = 1f + (MathF.Sin(time * ICON_PULSE_SPEED + i) * ICON_PULSE_AMOUNT);
      var animatedImageRect = imageRect.FitAspect(image.Aspect).Scale(pulseScale);
      UI.Image(animatedImageRect, image, Vector4.White);

      // Title and content on the right
      var textArea = sectionRect.Inset(0, 0, 0, 10);
      UI.Text(textArea.CutTop(30), section.Title, titleTs);
      UI.Text(textArea, section.Content, contentTs);
    }

    // Close button at the bottom with pulsing animation
    var buttonRect = contentRect.BottomRect().CutBottom(50).Inset(10);
    var buttonSettings = new UI.ButtonSettings()
    {
      Sprite = Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_1.png"),
      BackgroundColorMultiplier = Vector4.White,
      PressScaling = 0.25f,
    };

    var buttonTs = new UI.TextSettings()
    {
      Font = UI.Fonts.BarlowBold,
      Size = 32,
      Color = Vector4.White,
      HorizontalAlignment = UI.HorizontalAlignment.Center,
      VerticalAlignment = UI.VerticalAlignment.Center,
      Outline = true,
      OutlineThickness = 2,
    };

    using var _ = UI.PUSH_ID("close");

    // Calculate button pulse scale
    float buttonPulseScale = 1f + (MathF.Sin(time * BUTTON_PULSE_SPEED) * BUTTON_PULSE_AMOUNT);
    var animatedButtonRect = buttonRect.CenterRect().Offset(0, 35).Grow(35, 135, 35, 135).Scale(buttonPulseScale);

    if (UI.Button(animatedButtonRect, "Become a Janitor", buttonSettings, buttonTs).Clicked)
    {
      Hide();
    }
  }
}
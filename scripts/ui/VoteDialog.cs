using AO;

public static class LogButton
{
  private static float Player1Votes = 0;
  private static float Player2Votes = 0;
  private static float LastVoteTime = 0;
  private static float IntroStartTime = -1;
  private static readonly float VoteDecayRate = 0.5f;
  private static readonly float MaxShake = 25f;
  private static readonly float MaxScale = 1.5f;
  private static readonly float IntroDuration = 3.0f;

  // Add new fields for player names
  private static string Player1Name = "";
  private static string Player2Name = "";
  private static Action OnPlayer1Vote;
  private static Action OnPlayer2Vote;

  private static float Clamp(float value, float min, float max) => MathF.Min(MathF.Max(value, min), max);

  private static readonly UI.ButtonSettings ButtonSettings = new UI.ButtonSettings()
  {
    Sprite = Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_2.png"),
    BackgroundColorMultiplier = new Vector4(1, 1, 1, 1),
    PressScaling = 0.25f,
  };

  private static readonly UI.TextSettings VoteCountSettings = new UI.TextSettings()
  {
    Font = UI.Fonts.BarlowBold,
    Size = 96,
    Color = Vector4.White,
    DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
    DropShadowOffset = new Vector2(0f, -3f),
    HorizontalAlignment = UI.HorizontalAlignment.Center,
    VerticalAlignment = UI.VerticalAlignment.Center,
    WordWrap = false,
    Outline = true,
    OutlineThickness = 3,
  };

  private static readonly UI.TextSettings PlayerNameSettings = new UI.TextSettings()
  {
    Font = UI.Fonts.BarlowBold,
    Size = 56,
    Color = new Vector4(1f, 0.8f, 0.2f, 1f),
    DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
    DropShadowOffset = new Vector2(0f, -3f),
    HorizontalAlignment = UI.HorizontalAlignment.Center,
    VerticalAlignment = UI.VerticalAlignment.Center,
    WordWrap = false,
    Outline = true,
    OutlineThickness = 3,
  };

  private static Rect GetShakeRect(Rect baseRect, float intensity)
  {
    var shake = MathF.Sin(Time.TimeSinceStartup * 20f) * intensity;
    var verticalShake = MathF.Cos(Time.TimeSinceStartup * 15f) * intensity;
    return baseRect.Offset(shake, verticalShake);
  }

  private static void DrawVoteSection(Rect baseRect, string playerName, ref float votes, bool isLeftSide)
  {
    if (IntroStartTime < 0) IntroStartTime = Time.TimeSinceStartup;
    var timeSinceIntro = Time.TimeSinceStartup - IntroStartTime;

    // Intro animation timing
    var introDelay = isLeftSide ? 0.5f : 0.8f;
    var introProgress = Clamp((timeSinceIntro - introDelay) / 1.0f, 0f, 1f);
    var rotationProgress = Clamp((timeSinceIntro - introDelay) / 0.5f, 0f, 1f);

    // Calculate entrance animation
    var startOffsetX = isLeftSide ? -1500f : 1500f;
    var offsetX = startOffsetX * (1f - Ease.OutBack(introProgress));
    var rotation = (isLeftSide ? -720f : 720f) * (1f - Ease.OutBack(rotationProgress));

    var intensity = votes / 100f;
    var scale = 1f + (intensity * (MaxScale - 1f));
    var shake = intensity * MaxShake;

    // Background effect with rotation
    var bgRect = baseRect.Grow(150 * scale, 150 * scale, 150 * scale, 150 * scale).Offset(offsetX, 0);
    using var _ = UI.PUSH_ROTATE_ABOUT_POINT(rotation * MathF.PI / 180f, bgRect.Center);
    UI.Image(GetShakeRect(bgRect, shake * 0.5f), Assets.GetAsset<Texture>("$AO/circle.png"),
            new Vector4(1f, 0.5f, 0f, 0.2f * intensity));

    // Player name with glow effect
    var nameRect = baseRect.Offset(offsetX, 120);
    var glowIntensity = 0.5f + 0.5f * MathF.Sin(Time.TimeSinceStartup * 3f);
    var nameSettings = PlayerNameSettings;
    nameSettings.Color = new Vector4(1f, 0.8f + (0.2f * glowIntensity), 0.2f + (0.8f * glowIntensity), 1f);
    UI.TextAsync(GetShakeRect(nameRect, shake * 0.3f), playerName, nameSettings);

    // Vote count with scaling effect
    var countSettings = VoteCountSettings;
    countSettings.Size = 48 + (int)(48 * intensity);
    var voteRect = baseRect.Offset(offsetX, 0);
    UI.TextAsync(GetShakeRect(voteRect, shake), ((int)votes).ToString(), countSettings);

    // Vote button with pulsing effect
    var buttonRect = baseRect.Offset(offsetX, -140).Grow(60 + (30 * intensity), 100 + (50 * intensity), 60 + (30 * intensity), 100 + (50 * intensity));
    var buttonSettings = ButtonSettings;
    var pulseScale = 1f + 0.1f * MathF.Sin(Time.TimeSinceStartup * 5f);
    buttonSettings.BackgroundColorMultiplier = new Vector4(1f, 0.5f + (0.5f * intensity), 0.5f + (0.5f * intensity), 1);

    using var _2 = UI.PUSH_SCALE_FACTOR(MathF.Max(pulseScale, 0.01f));
    using var _id = UI.PUSH_ID(isLeftSide ? "player1_vote" : "player2_vote");
    if (UI.Button(GetShakeRect(buttonRect, shake), "VOTE!", buttonSettings, PlayerNameSettings).Clicked)
    {
      votes += 1;
      LastVoteTime = Time.TimeSinceStartup;

      var volume = 0.5f + (intensity * 0.5f);
      SFX.Play(Assets.GetAsset<AudioAsset>("sfx/camera.wav"),
              new SFX.PlaySoundDesc() { Volume = volume, RangeMultiplier = 1f + intensity });

      // Call the appropriate vote callback
      if (isLeftSide)
      {
        OnPlayer1Vote?.Invoke();
      }
      else
      {
        OnPlayer2Vote?.Invoke();
      }
    }
  }

  // Modify DrawVotingUI to accept player names and vote callbacks
  public static void DrawVotingUI(Rect rect, string player1Name, string player2Name, Action onPlayer1Vote, Action onPlayer2Vote)
  {
    if (Network.IsServer)
    {
      return;
    }

    // Store the names and callbacks
    Player1Name = player1Name;
    Player2Name = player2Name;
    OnPlayer1Vote = onPlayer1Vote;
    OnPlayer2Vote = onPlayer2Vote;

    // Decay votes over time
    var timeSinceLastVote = Time.TimeSinceStartup - LastVoteTime;
    if (timeSinceLastVote > 1f)
    {
      Player1Votes = AOMath.Lerp(Player1Votes, 0, VoteDecayRate * Time.DeltaTime);
      Player2Votes = AOMath.Lerp(Player2Votes, 0, VoteDecayRate * Time.DeltaTime);
    }

    // Left side for Player 1
    var leftRect = rect.CenterRect().Offset(-300, 0);
    DrawVoteSection(leftRect, Player1Name, ref Player1Votes, true);

    // Right side for Player 2
    var rightRect = rect.CenterRect().Offset(300, 0);
    DrawVoteSection(rightRect, Player2Name, ref Player2Votes, false);

    // Draw VS text in the middle with epic animation
    if (IntroStartTime >= 0)
    {
      var timeSinceIntro = Time.TimeSinceStartup - IntroStartTime;
      var vsProgress = Clamp((timeSinceIntro - 1.2f) / 0.5f, 0f, 1f);
      var vsScale = Ease.OutBack(vsProgress) * (1.5f + 0.2f * MathF.Sin(Time.TimeSinceStartup * 8f));

      var vsSettings = PlayerNameSettings;
      vsSettings.Size = 64;
      vsSettings.Color = new Vector4(1f, 0.2f + 0.3f * MathF.Sin(Time.TimeSinceStartup * 10f), 0.2f, 1f);
      var vsRect = rect.CenterRect();

      using var _vs = UI.PUSH_SCALE_FACTOR(MathF.Max(vsScale, 0.01f));
      using var _vsRotate = UI.PUSH_ROTATE_ABOUT_POINT(MathF.Sin(Time.TimeSinceStartup * 3f) * 0.1f, vsRect.Center);

      var vsShake = MathF.Max(Player1Votes, Player2Votes) / 100f * MaxShake;
      UI.TextAsync(GetShakeRect(vsRect, vsShake), "VS", vsSettings);

      // Add dramatic flash effect
      if (vsProgress > 0 && vsProgress < 0.3f)
      {
        var flashAlpha = (1f - (vsProgress / 0.3f)) * 0.5f;
        UI.Image(rect, Assets.GetAsset<Texture>("$AO/white.png"), new Vector4(1f, 1f, 1f, flashAlpha));
      }
    }
  }
}
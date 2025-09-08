using AO;
using System.Collections.Generic;

public class SecurityCameraControl : Component
{
  public Interactable interactable;
  public CameraControl cameraControl;
  public bool isInCameraMode;
  public int currentCameraIndex;
  public Entity[] cameraPositions;
  public Entity interactingPlayer;
  public static readonly Vector4 BUTTON_COLOR = new(0.2f, 0.7f, 0.2f, 1f);

  public static readonly UI.ButtonSettings ButtonSettings = new UI.ButtonSettings()
  {
    Sprite = Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_8.png"),
    BackgroundColorMultiplier = new Vector4(1f, 0.5f, 0.5f, 1),
    PressScaling = 0.5f,
  };

  public static readonly UI.TextSettings TextSettings = new UI.TextSettings()
  {
    Font = UI.Fonts.BarlowBold,
    Size = 48,
    Color = Vector4.White,
    DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
    DropShadowOffset = new Vector2(0f, -3f),
    HorizontalAlignment = UI.HorizontalAlignment.Center,
    VerticalAlignment = UI.VerticalAlignment.Center,
    WordWrap = false,
    Outline = true,
    OutlineThickness = 2
  };

  public override void Awake()
  {
    interactable = Entity.Unsafe_AddComponent<Interactable>();
    interactable.Text = "Enter Camera Mode";

    // Cache camera positions
    var camerasEntity = Entity.FindByName("Cameras");
    if (camerasEntity != null && camerasEntity.Alive())
    {
      var tempList = new List<Entity>();
      foreach (var child in camerasEntity.Children)
      {
        tempList.Add(child);
      }
      cameraPositions = tempList.ToArray();
    }

    interactable.OnInteract = (Player p) =>
    {
      if (cameraPositions == null || cameraPositions.Length == 0) return;
      if (!p.IsLocal) return;

      isInCameraMode = !isInCameraMode;
      if (isInCameraMode)
      {
        interactingPlayer = p.Entity;
        currentCameraIndex = 0;
        EnterCameraMode();
      }
      else
      {
        interactingPlayer = null;
        ExitCameraMode();
      }
    };
  }

  public void EnterCameraMode()
  {
    if (!Network.IsClient) return;
    if (!interactingPlayer.Alive()) return;

    if (cameraControl == null)
    {
      cameraControl = CameraControl.Create(15);
      cameraControl.Zoom = 1.2f;
      UpdateCameraPosition();
    }
  }

  public void ExitCameraMode()
  {
    if (!Network.IsClient) return;

    if (cameraControl != null)
    {
      cameraControl.Destroy();
      cameraControl = null;
    }
  }

  public void UpdateCameraPosition()
  {
    if (!Network.IsClient || cameraControl == null) return;
    if (!interactingPlayer.Alive()) return;

    if (cameraPositions != null && cameraPositions.Length > 0 && currentCameraIndex < cameraPositions.Length)
    {
      var targetCamera = cameraPositions[currentCameraIndex];
      if (targetCamera.Alive())
      {
        cameraControl.Position = targetCamera.Position;
      }
    }
  }

  public override void Update()
  {
    if (!Network.IsClient) return;
    if (!interactingPlayer.Alive())
    {
      if (isInCameraMode)
      {
        isInCameraMode = false;
        ExitCameraMode();
      }
      return;
    }

    if (isInCameraMode && cameraControl != null && interactingPlayer == Network.LocalPlayer.Entity)
    {
      DrawCameraUI();
    }
  }

  public void DrawCameraUI()
  {
    var baseRect = UI.ScreenRect.CenterRect();
    var containerRect = baseRect.Offset(0, -300);

    var buttonWidth = 200;
    var buttonHeight = buttonWidth * (16f / 9f); // Maintain 9:16 aspect ratio
    var nextButtonRect = containerRect.OffsetUnscaled(-buttonWidth - 100, 0).GrowUnscaled(buttonWidth / 2, buttonHeight / 2, buttonWidth / 2, buttonHeight / 2);
    var exitButtonRect = containerRect.OffsetUnscaled(100, 0).GrowUnscaled(buttonWidth / 2, buttonHeight / 2, buttonWidth / 2, buttonHeight / 2);


    var buttonSettings = ButtonSettings;
    var intensity = 0.5f + 0.5f * MathF.Sin(Time.TimeSinceStartup * 3f);
    buttonSettings.BackgroundColorMultiplier = new Vector4(1f, 0.5f + (0.5f * intensity), 0.5f + (0.5f * intensity), 1);

    var cameraText = $"Camera {currentCameraIndex + 1}/{cameraPositions?.Length ?? 0}";
    var textRect = containerRect.Offset(0, 125);
    UI.TextAsync(textRect, cameraText, TextSettings);

    using var _scale = UI.PUSH_SCALE_FACTOR(MathF.Max(1f, 0.01f));

    // Next Camera Button
    using var _nextId = UI.PUSH_ID("security_camera_next");
    if (UI.Button(nextButtonRect, "Next Camera", buttonSettings, TextSettings).Clicked)
    {
      if (cameraPositions != null && cameraPositions.Length > 0)
      {
        currentCameraIndex = (currentCameraIndex + 1) % cameraPositions.Length;
        UpdateCameraPosition();

        // Play click sound
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/camera.wav"),
                new SFX.PlaySoundDesc() { Volume = 0.5f + (intensity * 0.5f), RangeMultiplier = 1f + intensity });
      }
    }

    // Exit Button
    var exitButtonSettings = ButtonSettings;
    exitButtonSettings.BackgroundColorMultiplier = new Vector4(0.7f, 0.2f, 0.2f, 1);
    using var _exitId = UI.PUSH_ID("security_camera_exit");
    if (UI.Button(exitButtonRect, "Exit", exitButtonSettings, TextSettings).Clicked)
    {
      isInCameraMode = false;
      ExitCameraMode();

      // Play exit sound
      SFX.Play(Assets.GetAsset<AudioAsset>("sfx/camera.wav"),
              new SFX.PlaySoundDesc() { Volume = 0.3f, RangeMultiplier = 1f });
    }
  }

  public override void OnDestroy()
  {
    if (Network.IsClient && cameraControl != null)
    {
      cameraControl.Destroy();
      cameraControl = null;
    }
  }
}
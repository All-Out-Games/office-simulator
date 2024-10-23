using System;
using AO;

public enum DayState
{
  DAY,
  DUSK,
  NIGHT,
  DAWN
}

public partial class DayNightManager : Component
{
  [Serialized]
  public UIText ClockUIRef;

  public static DayNightManager Instance;
  public float DayLength = 180;
  public float NightLength = 45;
  public SyncVar<float> Darkness = new(0f);
  private SyncVar<float> transitionStartTime = new(0f);

  private SyncVar<int> currentState = new((int)DayState.DAY);
  public DayState CurrentState
  {
    get => (DayState)currentState.Value;
    set => currentState.Set((int)value);
  }

  private ulong nightSfxHandle1;
  private ulong nightSfxHandle2;
  public SyncVar<bool> Paused = new(false);

  private int curNightfallMessageIndex = 0;
  private readonly string[] nightfallMessages = new string[]
  {
        "It's getting dark...",
        "Better get back to your office!",
        "[WARNING] Return to your office immediately or face... consequences.",
        "... your choice :)"
  };

  private readonly string[] janitorNightfallMessages = new string[]
  {
        "It's getting dark...",
        "You feel... strange...",
        "Get ready to hunt the employees...",
        "You are a killer now..."
  };

  public override void Awake()
  {
    Instance = this;
  }

  public override void Update()
  {
    // Don't progress time during board meetings
    if (PromoNPC.Instance.BoardMeetingActive) return;
    if (Paused) return;

    if (Network.IsClient)
    {
      var op = (OfficePlayer)Network.LocalPlayer;
      if (op.CurrentRoom != Room.HR)
      {
        var clampedAmbiant = Math.Clamp(1 - Darkness, 0f, 0.2f);
        op.CameraControl.AmbientColour = new Vector3(clampedAmbiant, clampedAmbiant, clampedAmbiant);
      }

      UpdateClockUI();
    }
    else if (Network.IsServer)
    {
      switch (CurrentState)
      {
        case DayState.DAY:
          UpdateDay();
          break;
        case DayState.DUSK:
          UpdateDusk();
          break;
        case DayState.NIGHT:
          UpdateNight();
          break;
        case DayState.DAWN:
          UpdateDawn();
          break;
      }
    }
  }

  private void UpdateClockUI()
  {
    if (ClockUIRef != null && (CurrentState == DayState.DAY))
    {
      float timeToNightfall = DayLength * (1 - Darkness);
      ClockUIRef.Text = $"Nightfall In: {timeToNightfall:F0}s";
    }
    else if (CurrentState == DayState.DAWN)
    {
      ClockUIRef.Text = "Back to work survivors!";
    }
    else if (ClockUIRef != null)
    {
      ClockUIRef.Text = "Survive...";
    }
  }

  // Server update fns that actually progress time
  private void UpdateDay()
  {
    Darkness.Set(Darkness + Time.DeltaTime / DayLength);
    if (Darkness >= 0.85f)
    {
      // Advance to dusk
      Darkness.Set(0.85f);
      transitionStartTime.Set(Time.TimeSinceStartup);
      CurrentState = DayState.DUSK;
      CallClient_StartClientDusk();

      foreach (var player in Scene.Components<OfficePlayer>())
      {
        var op = (OfficePlayer)player;

        if (op.Alive())
        {
          op.CallClient_PlaySFX("sfx/pre-night.wav");
        }
      }
    }
  }

  private void UpdateDusk()
  {
    float interval = 15f / nightfallMessages.Length;
    if (Time.TimeSinceStartup - transitionStartTime >= interval * curNightfallMessageIndex && curNightfallMessageIndex < nightfallMessages.Length)
    {
      foreach (var player in Scene.Components<OfficePlayer>())
      {
        var op = (OfficePlayer)player;
        if (op.CurrentRole == Role.JANITOR) {
          op.CallClient_ShowNotification(janitorNightfallMessages[curNightfallMessageIndex]);
        } else {
          op.CallClient_ShowNotification(nightfallMessages[curNightfallMessageIndex]);
        }
      }

      curNightfallMessageIndex++;
    }

    if (Time.TimeSinceStartup - transitionStartTime >= 14.5f)
    {
      // Advance to night time
      Darkness.Set(0.993f);
      curNightfallMessageIndex = 0;
      transitionStartTime.Set(Time.TimeSinceStartup);
      CurrentState = DayState.NIGHT;

      CallClient_StartClientNight();
    }
  }

  private void UpdateNight()
  {
    if (Time.TimeSinceStartup - transitionStartTime >= NightLength)
    {
      // Advance to Dawn
      transitionStartTime.Set(Time.TimeSinceStartup);
      CurrentState = DayState.DAWN;
      CallClient_StopClientNight();
    }
  }

  private void UpdateDawn()
  {
    Darkness.Set(Darkness - Time.DeltaTime / 20f);
    if (Darkness <= 0f)
    {
      // Advance to Day
      Darkness.Set(0f);
      CurrentState = DayState.DAY;
    }
  }

  [ClientRpc]
  public void StartClientDusk()
  {
    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.Stop();
    }
  }

  [ClientRpc]
  public void StartClientNight()
  {

    foreach (var player in Scene.Components<OfficePlayer>())
    {
      var op = (OfficePlayer)player;
      if (op.CurrentRole != Role.JANITOR) continue;
      player.AddEffect<KillerEffect>();
    }

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(true);
    }

    // CLIENT ONLY FROM HERE
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;

    playerRef.SetLightOn(true);

    nightSfxHandle1 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
    nightSfxHandle2 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-music.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
  }

  [ClientRpc]
  public void StopClientNight()
  {
    foreach (var player in Scene.Components<OfficePlayer>())
    {
      player.RemoveEffect<KillerEffect>(false);
    }

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(false);
    }

    // CLIENT ONLY FROM HERE
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;


    playerRef.SetLightOn(false);

    // Assuming these are no-op if the handle is null?
    SFX.Stop(nightSfxHandle1);
    SFX.Stop(nightSfxHandle2);
  }
}

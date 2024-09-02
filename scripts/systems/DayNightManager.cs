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
  public float DayLength = 100;
  public float NightLength = 55;
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

  private int curNightfallMessageIndex = 0;
  private readonly string[] nightfallMessages = new string[]
  {
        "It's getting dark...",
        "Better get back to your office!",
        "[WARNING] Return to your office immediately or face... consequences.",
        "... your choice :)"
  };

  public override void Awake()
  {
    Instance = this;
  }

  public override void Update()
  {
    // Don't progress time during board meetings
    if (PromoNPC.Instance.BoardMeetingActive) return;

    if (Network.IsClient)
    {
      var op = (OfficePlayer)Network.LocalPlayer;
      if (op.CurrentRoom != Room.HR)
      {
        var clampedAmbiant = Math.Clamp(1 - Darkness, 0f, 0.75f);
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

      var playerRef = (OfficePlayer)Player.AllPlayers[0];
      if (playerRef.Alive())
      {
        playerRef.CallClient_PlaySFX("sfx/pre-night.wav");
      }
    }
  }

  private void UpdateDusk()
  {
    float interval = 15f / nightfallMessages.Length;
    if (Time.TimeSinceStartup - transitionStartTime >= interval * curNightfallMessageIndex && curNightfallMessageIndex < nightfallMessages.Length)
    {
      GameManager.Instance.CallClient_ShowNotification(nightfallMessages[curNightfallMessageIndex]);

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

    foreach (Player player in Player.AllPlayers)
    {
      var op = (OfficePlayer)player;
      if (op.CurrentRole != Role.JANITOR) continue;
      player.AddEffect<KillerEffect>();
    }

    // CLIENT ONLY FROM HERE
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;

    playerRef.SetLightOn(true);

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(true);
    }

    nightSfxHandle1 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
    nightSfxHandle2 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-music.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
  }

  [ClientRpc]
  public void StopClientNight()
  {
    foreach (Player player in Player.AllPlayers)
    {
      player.RemoveEffect<KillerEffect>(false);
    }

    // CLIENT ONLY FROM HERE
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(false);
    }

    playerRef.SetLightOn(false);

    // Assuming these are no-op if the handle is null?
    SFX.Stop(nightSfxHandle1);
    SFX.Stop(nightSfxHandle2);
  }
}

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
  public static DayNightManager Instance;
  public float DayLength = 10;
  public float NightLength = 15;
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
    if (Network.IsClient)
    {
      var op = (OfficePlayer)Network.LocalPlayer;
      var clampedAmbiant = Math.Clamp(1 - Darkness, 0f, 0.3f);
      op.CameraControl.AmbientColour = new Vector3(clampedAmbiant, clampedAmbiant, clampedAmbiant);
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

  public void SendDuskMessages()
  {


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

      var playerRef = (OfficePlayer)Player.AllPlayers[0];
      if (playerRef.Alive())
      {
        GameManager.Instance.CallClient_PlaySFX("sfx/alarm.wav");
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

    if (Time.TimeSinceStartup - transitionStartTime >= 15f)
    {
      // Advance to night time
      Darkness.Set(0.986f);
      curNightfallMessageIndex = 0;
      transitionStartTime.Set(Time.TimeSinceStartup);
      CurrentState = DayState.NIGHT;

      foreach (Player player in Player.AllPlayers)
      {
        player.AddEffect<TransformToKillerEffect>();
      }

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
  public void StartClientNight()
  {
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(true);
    }

    playerRef.SetLightOn(true);

    nightSfxHandle1 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
    nightSfxHandle2 = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-music.wav"), new SFX.PlaySoundDesc() { Volume = 1f });
  }

  [ClientRpc]
  public void StopClientNight()
  {
    var playerRef = (OfficePlayer)Network.LocalPlayer;
    if (!playerRef.Alive()) return;

    playerRef.SetLightOn(false);

    foreach (Jukebox jukkebox in Scene.Components<Jukebox>()) {
      jukkebox.SetNightVersion(false);
    }

    // Assuming these are no-op if the handle is null?
    SFX.Stop(nightSfxHandle1);
    SFX.Stop(nightSfxHandle2);
  }
}

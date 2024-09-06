using AO;

public partial class PowerEvent : Event
{
  private List<PowerSwitch> switches = new List<PowerSwitch>();
  private ulong sfxHandle;

  public override void Awake()
  {
    base.Awake();
  }

  public override void Start()
  {
    var switchIterator = Entity.Children;
    foreach (var powerSwitch in switchIterator)
    {
      switches.Add(powerSwitch.GetComponent<PowerSwitch>());
    }
  }

  private int GetUnfixedSwitchCount()
  {
    int count = 0;
    foreach (var powerSwitch in switches)
    {
      if (!powerSwitch.Fixed)
      {
        count++;
      }
    }
    return count;
  }

  public override void Update()
  {
    if (!IsActive) return;


    if (Network.IsClient)
    {
      var op = (OfficePlayer)Network.LocalPlayer;
      op.CameraControl.AmbientColour = new Vector3(0f, 0f, 0f);
    }

    References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = $"Power Outage (Time Remaining: {TimeRemaining:F0})";
    References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Breakers to restore: " + GetUnfixedSwitchCount() + " / " + switches.Count;

    if (IsCompleted() && IsActive && Network.IsServer)
    {
      CallClient_ReceiveServerStopEvent(false);
    }

    if (TimeRemaining <= 0 && Network.IsServer)
    {
      CallClient_ReceiveServerStopEvent(true);
    }
  }

  // Needed to be an RPC because the server sees the completed status, sets isactive to false, and update stops running
  [ClientRpc]
  public void ReceiveServerStopEvent(bool failed)
  {
    StopEvent(failed);
  }

  public override bool IsCompleted()
  {
    return GetUnfixedSwitchCount() == 0;
  }

  public override void StartEvent()
  {
    base.StartEvent();

    SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/power/power-out.wav"), new SFX.PlaySoundDesc() { Volume=0.8f });

    foreach (Player player in Player.AllPlayers)
    {
      var op = (OfficePlayer)player;
      op.SetLightOn(true);
    }


    foreach (Jukebox jukebox in Scene.Components<Jukebox>()) {
      jukebox.Stop();
    }

    foreach (var powerSwitch in switches)
    {
      powerSwitch.StartEvent();
    }
  }

  public override void StopEvent(bool failed)
  {
    base.StopEvent(failed);

    if (failed && Network.IsServer)
    {
      GameManager.Instance.CallClient_ShowNotification("Power Outage anomaly failed");
      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().LoseEvent();
      }
    }

    if (!failed && Network.IsServer)
    {
      GameManager.Instance.CallClient_PlaySFX("anomalies/power/power-on.wav");
      GameManager.Instance.CallClient_ShowNotification("Power has been restored");

      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().WinEvent();
      }
    }
    

    foreach (Player player in Player.AllPlayers)
    {
      var op = (OfficePlayer)player;
      op.SetLightOn(false);
    }

    foreach (Jukebox jukebox in Scene.Components<Jukebox>()) {
      jukebox.SafePlay();
    }

    foreach (var powerSwitch in switches)
    {
      powerSwitch.StopEvent();
    }
  }
}
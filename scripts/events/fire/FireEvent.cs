using AO;

public partial class FireEvent : Event
{
  private List<FireSwitch> switches = new List<FireSwitch>();
  private Sprite_Renderer fireOverlay;
  private ulong sfxHandle;

  public override void Awake()
  {
    base.Awake();
  }

  public override void Start()
  {
    fireOverlay = Entity.TryGetChildByName("FireOverlay").GetComponent<Sprite_Renderer>();
    foreach (var child in Entity.Children)
    {
      if (child.Name == "Flames")
      {
        switches.Add(child.TryGetChildByName("Lever").GetComponent<FireSwitch>());
      }

    }
  }

  private int GetUnfixedCount()
  {
    int count = 0;
    foreach (var bucket in switches)
    {
      if (!bucket.Fixed)
      {
        count++;
      }
    }
    return count;
  }

  public override void Update()
  {
      if (!IsActive) return;
      base.Tick();
  
      var progression = (Duration - TimeRemaining) / (Duration + TimeRemaining);

      int totalFires = switches.Count;
      int fixedBuckets = totalFires - GetUnfixedCount();
      float fixedPercentage = fixedBuckets / (float)totalFires;
  
      fireOverlay.Tint = new Vector4(1, 0.25f, 0.25f, 0.1f + (0.9f * progression) * (1 - fixedPercentage));
  
      References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = $"Fires are enveloping the office (Time Remaining: {TimeRemaining.Value:F0})";
      References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Extinguish: " + GetUnfixedCount() + " / " + totalFires;
  
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
    return GetUnfixedCount() == 0;
  }

  public override void StartEvent()
  {
    base.StartEvent();
    sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/fire/fire.wav"), new SFX.PlaySoundDesc() { Volume=0.4f, Loop = true });

    foreach (var bucket in switches)
    {
      bucket.StartEvent();
    }
  }

  public override void StopEvent(bool failed)
  {
    base.StopEvent(failed);

    SFX.Stop(sfxHandle);

    fireOverlay.Tint = new Vector4(0, 0, 0, 0);

    if (failed && Network.IsServer)
    {
      GameManager.Instance.CallClient_ShowNotification("The office has burned down...");
      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().LoseEvent();
      }
    }

    if (!failed)
    {
      if (Network.IsServer)
      {
        GameManager.Instance.CallClient_ShowNotification("The fires have been extinguished!");
      }
      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().WinEvent();
      }
    }

    foreach (var bucket in switches)
    {
      bucket.StopEvent();
    }
  }
}
using AO;

public partial class FloodEvent : Event
{
  private List<Mop> buckets = new List<Mop>();
  private Sprite_Renderer floodWater;
  private ulong sfxHandle;

  public override void Awake()
  {
    base.Awake();

    floodWater = Entity.TryGetChildByName("Blue").GetComponent<Sprite_Renderer>();
    var switchIterator = Entity.TryGetChildByName("Mops").Children;
    foreach (var bucket in switchIterator)
    {
      buckets.Add(bucket.GetComponent<Mop>());
    }
  }

  private int GetUnfixedMopCount()
  {
    int count = 0;
    foreach (var bucket in buckets)
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

    // buckets slightly reduce the tinting 
    int totalBuckets = buckets.Count;
    int fixedBuckets = totalBuckets - GetUnfixedMopCount();
    float fixedPercentage = fixedBuckets / (float)totalBuckets;

    floodWater.Tint = new Vector4(0, 0.25f, 1, 0.1f + (0.9f * progression) * (1 - fixedPercentage));

    References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = $"Flooding (Time Remaining: {TimeRemaining.Value:F0})";
    References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Buckets to Mop: " + GetUnfixedMopCount() + " / " + totalBuckets;

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
    return GetUnfixedMopCount() == 0;
  }

  public override void StartEvent()
  {
    base.StartEvent();
    sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/flood/flood.wav"), new SFX.PlaySoundDesc() { Volume = 0.4f, Loop = true });

    foreach (var bucket in buckets)
    {
      bucket.StartEvent();
    }
  }

  public override void StopEvent(bool failed)
  {
    base.StopEvent(failed);

    SFX.Stop(sfxHandle);

    floodWater.Tint = new Vector4(0, 0, 0, 0);

    if (failed && Network.IsServer)
    {
      GameManager.Instance.CallClient_ShowNotification("The flooding couldn't be stopped...");
      foreach (var player in Scene.Components<OfficePlayer>())
      {
        player.Entity.GetComponent<OfficePlayer>().LoseEvent();
      }
    }

    if (!failed)
    {
      if (Network.IsServer)
      {
        GameManager.Instance.CallClient_ShowNotification("The flooding has been stopped!");
      }
      foreach (var player in Scene.Components<OfficePlayer>())
      {
        player.Entity.GetComponent<OfficePlayer>().WinEvent();
      }
    }

    foreach (var bucket in buckets)
    {
      bucket.StopEvent();
    }
  }
}
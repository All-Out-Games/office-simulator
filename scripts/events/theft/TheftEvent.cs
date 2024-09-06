using AO;

public partial class TheftEvent : Event
{
  private List<StolenBag> bags = new List<StolenBag>();

  public override void Awake()
  {
    base.Awake();
  }

  public override void Start()
  {
    var bagLocations = Entity.TryGetChildByName("BagLocations").Children;
    var locations = new List<StolenBag>();
    foreach (var bag in bagLocations)
    {
      locations.Add(bag.GetComponent<StolenBag>());
    }

    // Pick one bag at random
    var Random = new Random();
    var randomBag = locations[Random.Next(0, locations.Count)];
    bags.Add(randomBag);
  }

  private int GetUnfixedMopCount()
  {
    int count = 0;
    foreach (var bucket in bags)
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
  
      var Progression = (Time.TimeSinceStartup - startTime) / Duration;
  
      References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = $"The Overseer Has Stolen Cash (Time Remaining: {TimeRemaining:F0})";
      References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Locate the stash and recover the stolen cash";
  
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
    SFX.Play(Assets.GetAsset<AudioAsset>("sfx/alarm.wav"), new SFX.PlaySoundDesc() { Volume=0.4f });

    foreach (var bucket in bags)
    {
      bucket.StartEvent();
    }
  }

  public override void StopEvent(bool failed)
  {
    base.StopEvent(failed);

    if (failed && Network.IsServer)
    {
      GameManager.Instance.CallClient_ShowNotification("The money has been lost...");
      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().LoseEvent();
      }
    }

    if (!failed)
    {
      GameManager.Instance.CallClient_ShowNotification("The money has been recovered!");
      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().WinEvent();
      }
    }

    foreach (var bucket in bags)
    {
      bucket.StopEvent();
    }
  }
}
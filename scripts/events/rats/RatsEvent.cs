using AO;

public partial class RatsEvent : Event
{
  private List<MovingRat> rats = new List<MovingRat>();

  public override void Awake()
  {
    base.Awake();
  }

  public override void Start()
  {
    var ratIterator = Entity.Children;
    foreach (var rat in ratIterator)
    {
      rats.Add(rat.GetComponent<MovingRat>());
    }
  }

  private int GetAliveRatCount()
  {
    int count = 0;
    foreach (var rat in rats)
    {
      if (!rat.Squashed)
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

    References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = $"Rat Infestation (Time Remaining: {TimeRemaining.Value:F0})";
    References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Rats Left: " + GetAliveRatCount() + " / " + rats.Count;

    if (IsCompleted() && IsActive && Network.IsServer)
    {
      CallClient_ReceiveServerStopEvent(false);
    }

    if (TimeRemaining <= 0 && Network.IsServer)
    {
      CallClient_ReceiveServerStopEvent(true);

    }
  }

  [ClientRpc]
  public void ReceiveServerStopEvent(bool failed)
  {
    StopEvent(failed);
  }

  public override bool IsCompleted()
  {
    return GetAliveRatCount() == 0;
  }

  public override void StartEvent()
  {
    base.StartEvent();

    foreach (var rat in rats)
    {
      rat.StartEvent();
    }
  }

  public override void StopEvent(bool failed)
  {
    base.StopEvent(failed);

    foreach (var rat in rats)
    {
      rat.StopEvent();
    }

    if (failed)
    {
      if (!Network.IsServer) return;
      GameManager.Instance.CallClient_ShowNotification("You failed to terminate the rats...");

      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().LoseEvent();
      }
    }

    if (!failed)
    {
      if (!Network.IsServer) return;
      GameManager.Instance.CallClient_ShowNotification("The rats have been terminated");

      foreach (var player in Player.AllPlayers)
      {
        player.Entity.GetComponent<OfficePlayer>().WinEvent();
      }
    }
  }
}
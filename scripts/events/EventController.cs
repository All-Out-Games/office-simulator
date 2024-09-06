using AO;

public class EventController : Component
{
  public EventController Instance;
  public UICanvas EventUI;
  public SyncVar<float> EventCooldownTrackerTime = new();
  public float MinTimeBetweenEvents = 50f;

  public override void Awake()
  {
    Instance = this;
    References.Instance.EventUI.Tint = new Vector4(0, 0, 0, 0);
  }

  public float GetCooldownTimeRemaining()
  {
    return MinTimeBetweenEvents - (Time.TimeSinceStartup - EventCooldownTrackerTime);
  }

  public void StartEvent(Event eventToStart)
  {
    if (Time.TimeSinceStartup - EventCooldownTrackerTime < MinTimeBetweenEvents)
    {
      Log.Info("Tried to start event " + eventToStart.Entity.Name + " before cooldown");
      return;

    }
    eventToStart.StartEvent();

    if (Network.IsServer)
    {
      EventCooldownTrackerTime.Set(Time.TimeSinceStartup);
    }
  }
}
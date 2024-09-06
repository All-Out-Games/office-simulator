using AO;

public class EventController : Component
{
  public UICanvas EventUI;
  public SyncVar<float> EventCooldownTrackerTime = new();
  public float MinTimeBetweenEvents = 10f;
  public EventController Instance;

  public override void Awake()
  {
    Instance = this;
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
    References.Instance.EventUI.Tint = new Vector4(1, 1, 1, 1);
    eventToStart.StartEvent();
    EventCooldownTrackerTime.Set(Time.TimeSinceStartup);
  }

  public void StopEvent(Event eventToStop)
  {
    References.Instance.EventUI.Tint = new Vector4(0, 0, 0, 0);
    eventToStop.StopEvent();
  }
}
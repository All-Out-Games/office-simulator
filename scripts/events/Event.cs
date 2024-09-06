using AO;

public class Event : Component
{
  [Serialized] public float Duration = 10f;
  public SyncVar<float> startTime = new();
  public float TimeRemaining => Duration - (Time.TimeSinceStartup - startTime);
  protected EventController eventController;
  public SyncVar<bool> IsActive = new(false);

  public virtual void StartEvent()
  {
    Log.Info("Event has started");
    References.Instance.EventUI.Tint = new Vector4(1f, 1f, 1f, 1f);

    if (!Network.IsServer) return;
    GameManager.Instance.CallClient_ShowNotification("An anomaly has been detected...");
    DayNightManager.Instance.Paused.Set(true);
    startTime.Set(Time.TimeSinceStartup);
    IsActive.Set(true);
  }

  public virtual void StopEvent(bool failed)
  {
    Log.Info("Event has ended");
    References.Instance.EventUI.Tint = new Vector4(0, 0, 0, 0);

    if (!Network.IsServer) return;
    DayNightManager.Instance.Paused.Set(false);
    IsActive.Set(false);
  }

  public virtual bool IsCompleted()
  {
    return false;
  }
}
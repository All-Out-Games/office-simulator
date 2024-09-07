using AO;

public class Event : Component
{
  [Serialized] public float Duration = 10f;
  public SyncVar<float> startTime = new();
  public SyncVar<float> TimeRemaining = new(100000f);
  protected EventController eventController;
  public SyncVar<bool> IsActive = new(false);

  public virtual void StartEvent()
  {
    Log.Info("Event has started");
    References.Instance.EventUI.Tint = new Vector4(1f, 1f, 1f, 1f);

    if (!Network.IsServer) return;
    TimeRemaining.Set(Duration);
    GameManager.Instance.CallClient_ShowNotification("An anomaly has been detected...");
    GameManager.Instance.CallClient_PlaySFX("sfx/clue_found2.wav");
    DayNightManager.Instance.Paused.Set(true);
    IsActive.Set(true);
  }

  public virtual void Tick()
  {
    if (!Network.IsServer) return;
    if (!IsActive) return;
    References.Instance.EventUI.Tint = new Vector4(1f, 1f, 1f, 1f);
    TimeRemaining.Set(TimeRemaining - Time.DeltaTime);
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
using AO;

public class EventController : Component
{
  public EventController Instance;
  public UICanvas EventUI;
  public SyncVar<float> CooldownTimeRemaining = new();
  public float MinTimeBetweenEvents = 110f;

  public override void Awake()
  {
    Instance = this;
    References.Instance.EventUI.Tint = new Vector4(0, 0, 0, 0);
  }

  public float GetCooldownTimeRemaining()
  {
    return CooldownTimeRemaining.Value;
  }

  public void StartEvent(Event eventToStart)
  {
    if (CooldownTimeRemaining >= 0f)
    {
      Log.Info("Tried to start event " + eventToStart.Entity.Name + " before cooldown");
      return;

    }
    eventToStart.StartEvent();

    if (Network.IsServer)
    {
      CooldownTimeRemaining.Set(MinTimeBetweenEvents);
    }
  }

  public override void Update()
  {
    if (CooldownTimeRemaining >= 0f && Network.IsServer)
    {
      CooldownTimeRemaining.Set(CooldownTimeRemaining - Time.DeltaTime);
    } 
  }
}
using AO;

public class EventSwitch : Component
{
  private Interactable interactable;
  private EventController eventController;
  [Serialized] public string EventName;
  [Serialized] public Event Event;

  public override void Awake()
  {
    eventController = Entity.Parent.GetComponent<EventController>();

    interactable = Entity.AddComponent<Interactable>();
    interactable.Text = EventName;
    interactable.CanUseCallback = (Player p) =>
    {
      var op = (OfficePlayer)p;
      return op.CurrentRole == Role.OVERSEER;
    };
    
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;

      if (DayNightManager.Instance.CurrentState == DayState.NIGHT || DayNightManager.Instance.CurrentState == DayState.DUSK)
      {
        op.CallClient_ShowNotification("You must wait till night has passed...");
        op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
        return;
      }

      if (eventController.GetCooldownTimeRemaining() > 0)
      {
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/error.wav"), new() { Volume = 0.5f });
        return;
      }

      eventController.StartEvent(Event);
    };
  }

  public override void Update()
  {
    if (eventController.GetCooldownTimeRemaining() > 0)
    {
      interactable.Text = "Cooldown: " + eventController.GetCooldownTimeRemaining().ToString("0.0");
    }
    else
    {
      interactable.Text = EventName;
    }
  }
}
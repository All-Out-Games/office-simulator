using AO;

public class Event : Component
{
  [Serialized] public float Duration = 10f;
  protected EventController eventController;

  public virtual void StartEvent()
  {

  }

  public virtual void StopEvent()
  {

  }
}
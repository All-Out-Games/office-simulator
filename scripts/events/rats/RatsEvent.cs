using AO;

public class RatsEvent : Event
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

    StartEvent();
  }

  public override void Update()
  {
    References.Instance.EventUI.Entity.TryGetChildByName("Title").GetComponent<UIText>().Text = "Rat Infestation";
    References.Instance.EventUI.Entity.TryGetChildByName("Subtitle").GetComponent<UIText>().Text = "Rats: " + rats.Count;
  }

  public override void StartEvent()
  {
    foreach (var rat in rats)
    {
      rat.StartEvent();
    }
  }

  public override void StopEvent()
  {
    foreach (var rat in rats)
    {
      rat.StopEvent();
    }
  }
}
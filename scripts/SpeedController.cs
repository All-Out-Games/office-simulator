using AO;

public class SpeedController : Component
{
  private Interactable interactable;
  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.CanUseCallback = (Player p) => {
      return ((OfficePlayer)p).CurrentRole == Role.OVERSEER;
    };

    interactable.OnInteract += (Player p) => {
      var op = (OfficePlayer)p;

      var newFastJanitors = !GameManager.Instance.FastJanitors;
      if (newFastJanitors)
      {
        GameManager.Instance.CallClient_ShowNotification("The overseer has empowered their minions...");
      }
      else {
        GameManager.Instance.CallClient_ShowNotification("The overseer has reduced their minions power...");
      }

      if (!Network.IsServer) return;
      GameManager.Instance.FastJanitors.Set(newFastJanitors);
    };
  }

  public override void Update()
  {
    if (!GameManager.Instance.FastJanitors)
    {
      interactable.Text = "Increase Minion Speed";
    }
    else
    {
      interactable.Text = "Reduce Minion Speed";
    }
  }
}
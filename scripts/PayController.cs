using AO;

public class PayController : Component
{
  private Interactable interactable;
  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.CanUseCallback = (Player p) => {
      return ((OfficePlayer)p).CurrentRole == Role.CEO;
    };

    interactable.OnInteract += (Player p) => {
      var op = (OfficePlayer)p;

      var newReducedPay = !GameManager.Instance.ReducedPay;
      if (newReducedPay)
      {
        GameManager.Instance.CallClient_ShowNotification("The CEO has decided to cut the budget. Salaries have been reduced.");
      }
      else {
        GameManager.Instance.CallClient_ShowNotification("The CEO has decided to increase salaries.");
      }

      GameManager.Instance.ReducedPay.Set(newReducedPay);
    };
  }

  public override void Update()
  {
    if (GameManager.Instance.ReducedPay)
    {
      interactable.Text = "Increase Salaries";
    }
    else
    {
      interactable.Text = "Cut Budget (Reduce Salaries)";
    }
  }
}
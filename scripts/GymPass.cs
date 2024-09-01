using AO;

public class GymPass : Component
{
  public int Cost = 100;
  private Interactable interactable;

  public override void Awake()
  {
    interactable = Entity.GetComponent<Interactable>();
    interactable.OnInteract = (Player p) => 
    {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)p;

      if (op.Cash < Cost)
      {
        GameManager.Instance.CallClient_ShowNotification("You don't have enough money to buy a gym pass!");
        return;
      }

      if (op.HasGymPass)
      {
        GameManager.Instance.CallClient_ShowNotification("You already have a gym pass!");
        return;
      }

      op.Cash.Set(op.Cash - Cost);
      op.HasGymPass.Set(true);
      GameManager.Instance.CallClient_PlaySFX("sfx/coin.wav");
    };
  }

  public override void Update()
  {
    if (Network.IsServer) return;
    interactable.Text = $"Buy a gym pass for ${Cost}";
  }
}
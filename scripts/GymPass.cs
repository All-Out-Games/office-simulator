using AO;

public class GymPass : Component
{
  public int Cost = 100;
  private Interactable interactable;

  public override void Awake()
  {
    interactable = Entity.GetComponent<Interactable>();
    interactable.CanUseCallback += (Player p) => 
    {
      var op = (OfficePlayer)p;
      return !op.HasGymPass;
    };

    interactable.OnInteract = (Player p) => 
    {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)p;

      if (op.Cash < Cost)
      {
        op.CallClient_ShowNotification("You don't have enough money to buy a gym pass!");
        return;
      }

      op.Cash.Set(op.Cash - Cost);
      op.HasGymPass.Set(true);
      op.CallClient_PlaySFX("sfx/money.wav");
      op.CallClient_ShowNotification("You now have access to the gym...");
    };
  }

  public override void Update()
  {
    if (Network.IsServer) return;
    interactable.Text = $"Buy a gym pass for ${Cost}";
  }
}
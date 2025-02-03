using AO;

public class AdPlayer : Component
{
  public Interactable Interactable;

  public override void Awake()
  {
    Interactable = Entity.GetComponent<Interactable>();
    Interactable.OnInteract += OnInteract;

    // if (!Game.IsMobile)
    // {
    //   Entity.RemoveComponent<Interactable>();
    // }
  }

  public void OnInteract(Player player)
  {
    if (!player.Alive()) return;
    if (!player.IsLocal) return;

    if (Ads.IsRewardedAdLoaded())
    {
      Ads.PromptRewardedAd("5n_xp_ad", "+75XP", "Our corporate overlords have a message for you...", Assets.GetAsset<Texture>("$AO/new/icons/Video.png"));
    }
    else
    {
      Notifications.Show("No ad available");
    }
  }
}

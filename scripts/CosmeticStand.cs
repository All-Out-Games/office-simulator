using AO;

public class CosmeticStand : Component
{
  [Serialized]
  public string cosmeticId;

  public Interactable interactable;
  public override void Awake()
  {
    interactable = Entity.Unsafe_AddComponent<Interactable>();
    interactable.OnInteract += OnInteract;
  }

  private void OnInteract(Player player)
  {
    if (Network.LocalPlayer != player) return;
    if (!Network.IsClient) return;

    if (!Cosmetics.GetAllCosmetics().Any(c => c.Id == cosmeticId))
    {
      return;
    }

    if (Cosmetics.OwnsCosmetic(player, cosmeticId))
    {
      Cosmetics.EquipCosmetic(cosmeticId);
    }
    else
    {
      Cosmetics.PromptPurchase(cosmeticId);
    }
  }

  public override void Update()
  {
    if (!Network.LocalPlayer.Alive()) return;

    if (!Cosmetics.GetAllCosmetics().Any(c => c.Id == cosmeticId))
    {
      interactable.Text = "Loading...";
      return;
    }

    if (Cosmetics.OwnsCosmetic(Network.LocalPlayer, cosmeticId))
    {
      interactable.Text = "Equip Outfit";
    }
    else
    {
      interactable.Text = "Buy This Outfit";
    }
  }
}
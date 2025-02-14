using AO;

public class CosmeticStand : Component
{
  [Serialized]
  public string cosmeticId;

  public Interactable interactable;
  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.OnInteract += OnInteract;
  }

  private void OnInteract(Player player)
  {
    if (!Network.IsClient) return;
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
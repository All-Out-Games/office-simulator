using AO;

public class Buyable : Component
{
  OfficeController Controller;
  [Serialized] public int Cost;
  private Interactable interactable;
  public SyncVar<bool> Bought = new(false);
  private Sprite_Renderer spriteRenderer;

  public override void Awake()
  {
    Controller = Entity.Parent.Parent.TryGetChildByName("Controller").GetComponent<OfficeController>();

    if (!Controller.Alive())
    {
      Log.Error("Buyable " + Entity.Name + " doesn't have a parent office controller");
    }

    interactable = AddComponent<Interactable>();
    interactable.OnInteract = OnInteract;
    interactable.PromptOffset = new Vector2(-0.6f, -0.5f);
    interactable.CanUseCallback = (Player p) =>
    {
      if (!Controller.IsOwned || Bought) return false;
      return Controller.Owner.Value == p.Entity;
    };

    spriteRenderer = GetComponent<Sprite_Renderer>();
  }

  public void OnInteract(Player p)
  {
    if (!Network.IsServer) return;
    var op = (OfficePlayer)p;

    if (op.Cash < Cost)
    {
      op.CallClient_ShowNotification("You don't have enough cash");
      op.CallClient_PlaySFX("sfx/error.wav");
      return;
    }

    op.CallClient_PlaySFX("sfx/money.wav");
    op.Cash.Set(op.Cash - Cost);
    Bought.Set(true);
  }

  public override void Update()
  {
    if (!Network.IsClient) return;

    if (Bought)
    {
      spriteRenderer.Tint = new Vector4(1, 1, 1, 1f);
    }
    else
    {
      if (Controller.IsOwnedByMyClient)
      {
        interactable.Text = $"Buy {Entity.Name} - ${Cost}";
        spriteRenderer.Tint = new Vector4(0, 0, 0, 0.35f);
      }
      else
      {
        spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
      }
    }
  }
}
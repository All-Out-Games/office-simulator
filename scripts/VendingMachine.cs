using AO;

public class VendingMachine : Component
{
    public SyncVar<bool> InStock = new(true);
    public int Cost = 25;

    private Interactable interactable;

    public override void Awake()
    {
        interactable = Entity.AddComponent<Interactable>();
        interactable.OnInteract += OnInteract;
    }

    public void OnInteract(Player p)
    {
        if (!Network.IsServer) return;

        var op = (OfficePlayer)p;

        if (op.CurrentRole == Role.JANITOR)
        {
            if (!InStock)
            {
                op.Experience.Set(op.Experience + 10);
                InStock.Set(true);
            }
            else
            {
                op.CallClient_ShowNotification("Vending machines are strictly for full time employees!");
                op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
            }
        }
        else
        {
            if (InStock)
            {
                if (op.Cash >= Cost)
                {
                    InStock.Set(false);
                    op.Cash.Set(op.Cash - Cost);
                    op.CallClient_ShowNotification("Caffiene fills you... (+25% movement speed)");
                    op.CallClient_PlaySFX("sfx/vending.wav");
                    op.Caffeinated.Set(true);
                    op.CaffinatedAt.Set(Time.TimeSinceStartup);
                }
                else
                {
                    op.CallClient_ShowNotification("I need more money");
                    op.CallClient_PlaySFX(References.Instance.ErrorSfx.Name);
                }
            }
        }
    }

    public override void Update()
    {
        if (InStock)
        {
            Entity.GetComponent<Sprite_Renderer>().Texture = Assets.GetAsset<Texture>(
                "48x48/Modern_Office_Singles_48x48_175.png"
            );
        }
        else
        {
            Entity.GetComponent<Sprite_Renderer>().Texture = Assets.GetAsset<Texture>(
                "48x48/Modern_Office_Singles_48x48_176.png"
            );
        }

        var interactable = Entity.GetComponent<Interactable>();
        var op = (OfficePlayer)Network.LocalPlayer;
        if (!op.Alive()) return;
        if (op.CurrentRole == Role.JANITOR)
        {
            interactable.Text = InStock
                ? "Employee's only..."
                : "Restock with Plastic Straws (+10% Experience)";
        }
        else
        {
            interactable.Text = InStock
                ? $"Buy snack (${Cost})"
                : "Out of Stock. Please contact a janitor...";
        }
    }
}

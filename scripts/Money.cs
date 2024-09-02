using AO;

public class Money : Component
{
    public float RespawnInterval = 10f;
    public float lastInteractedTime = 0f;
    public bool MoneyEnabled = true;

    public override void Awake()
    {
        var interactible = Entity.GetComponent<Interactable>();
        interactible.OnInteract = (Player p) =>
        {
            var op = (OfficePlayer)p;
            if (Network.IsServer)
            {
                // 5 pieces of cash per salary
                op.Cash.Set(op.Cash + op.Salary / 7);
                Chat.SendMessage(op, "Thank you for your hard work!");
            }
            if (Network.LocalPlayer == p)
            {
                SFX.Play(Assets.GetAsset<AudioAsset>("sfx/money.wav"), new SFX.PlaySoundDesc());
            }
            MoneyEnabled = false;
            lastInteractedTime = Time.TimeSinceStartup;
        };

        interactible.CanUseCallback = (Player p) =>
        {
            return MoneyEnabled;
        };
    }

    public override void Update()
    {
        if (Time.TimeSinceStartup - lastInteractedTime > RespawnInterval)
        {
            MoneyEnabled = true;
        }

        if (Network.IsServer) return;

        if (MoneyEnabled)
        {
            Entity.GetComponent<Sprite_Renderer>().Tint = new Vector4(1, 1, 1, 1);
        }
        else
        {
            Entity.GetComponent<Sprite_Renderer>().Tint = new Vector4(0, 0, 0, 0);
        }

        var interactible = Entity.GetComponent<Interactable>();
        var op = (OfficePlayer)Network.LocalPlayer;
        if (!op.Alive()) return;
        interactible.Text = $"Collect a payment (+${op.Salary/7})";
    }
}

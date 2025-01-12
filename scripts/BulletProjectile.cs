using AO;

public partial class MurderProjectile : Component
{
    [Serialized] public bool IsBullet;
    public bool IsFromCEOGun;

    public float Lifetime;
    public const float MaxLife = 0.7f;
    public bool AlreadyHitSomething;

    public override void Awake()
    {
        Entity.GetComponent<Projectile>().OnHit += OnHit;
    }

    public override void Update()
    {
        Lifetime += Time.DeltaTime;
        if (Lifetime > MaxLife)
        {
            this.Entity.Destroy();
        }
    }

    private void OnHit(Entity other, bool predicted)
    {
        if (AlreadyHitSomething) return;
        // if (other.GetComponent<ProjectileIgnore>() != null) return;

        OfficePlayer player = null;
        var collisionChild = other.GetComponent<PlayerCollisionChild>();
        if (collisionChild != null)
        {
            player = collisionChild.Player;
        }

        if (player == null) return;
        if (player.HasEffect<SpectatorEffect>()) return;
        if (player.CurrentRole == Role.OVERSEER && !OverseerPromoNPC.Instance.BattleActive)
        {
            if (Network.IsServer && IsFromCEOGun)
            {
                var ceoPlayer = (OfficePlayer)GameManager.Instance.GetPlayersByRole(Role.CEO).FirstOrDefault();

                ceoPlayer.CallClient_ShowNotification("[̲̅y][̲̅o][̲̅u] [̲̅c][̲̅a][̲̅n] [̲̅k][̲̅i][̲̅l][̲̅l] [̲̅m][̲̅e][̲̅?]");
            }

            return;
        }

        var projectile = Entity.GetComponent<Projectile>();
        if (player == projectile.Owner) return;

        // HIT CONFIRMED
        AlreadyHitSomething = true;
        if (predicted == false)
        {
            if (Network.IsServer)
            {
                CallClient_KillPlayer(player, (OfficePlayer)projectile.Owner);
            }
        }

        Entity.Destroy();
    }

    [ClientRpc]
    public static void KillPlayer(OfficePlayer player, OfficePlayer killer)
    {
        if (Network.IsServer)
        {
            player.WasKilledInOverseerBattle.Set(true);
        }

        player.AddEffect<KillEffect>(preInit: effect =>
        {

        });
    }
}
using AO;

public partial class MurderProjectile : Component
{
    [Serialized] public bool IsBullet;

    public float Lifetime;
    public const float MaxLife = 1f;
    public bool AlreadyHitSomething;

    public override void Start()
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
        if (player.IsDead) return;
        if (player.HasEffect<SpectatorEffect>()) return;

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
using AO;

public partial class Revolver : MyAbility
{
    public override TargettingMode TargettingMode => TargettingMode.Line;
    public override Texture Icon => Assets.GetAsset<Texture>("Ability_Icons/revolver_icon.png");
    public override Type Effect => typeof(ShootGunEffect);
    public override Type TargettingEffect => typeof(AimingRevolver);
    public override float MaxDistance => 8f;
    public override float Cooldown => Player.IsInOverseerBattle() ? 1f : DayNightManager.Instance.CurrentState == DayState.NIGHT ? 10f : 60f;

    public override bool CanBeginTargeting()
    {
        return true;
    }

    public override bool CanUse()
    {
        if (Player.CurrentRole == Role.CEO || Player.IsInOverseerBattle()) return true;
        return false;
    }

    public override bool CanTarget(Player p)
    {
        return true;
    }
}

public class AimingRevolver : MyEffect
{
    public override bool IsActiveEffect => true;
    public override List<Type> AbilityWhitelist { get; } = new List<Type>() { typeof(Revolver) };
    public Entity GunEntity;

    public override void OnEffectStart(bool isDropIn)
    {
        GunEntity = Entity.Instantiate(Assets.GetAsset<Prefab>("PlayerEquip.prefab"));
        Player.SetMouseIKEnabled(true);
    }

    public override void OnEffectEnd(bool interrupt)
    {
        GunEntity.Destroy();
        Player.SetMouseIKEnabled(false);
    }

    public override void OnEffectUpdate()
    {
        GunEntity.Position = Player.SpineAnimator.GetBonePosition("Hand_R");
        GunEntity.Rotation = Player.SpineAnimator.GetBoneRotation("Hand_R") * (Player.Entity.LocalScaleX < 0 ? -1 : 1) + (Player.Entity.LocalScaleX < 0 ? 180 : 0);
        GunEntity.LocalScaleY = Player.Entity.LocalScaleX < 0 ? -1 : 1;
    }
}

public partial class ShootGunEffect : MyEffect
{
    public override bool IsActiveEffect => true;

    public override void OnEffectStart(bool isDropIn)
    {
        if (!isDropIn)
        {
            Game.SpawnProjectile(Player.Entity, "Bullet.prefab", "detective_bullet", Player.Entity.Position, AbilityDirection);
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/bullet.wav"), new SFX.PlaySoundDesc() { Positional = true, Position = Player.Entity.Position });
        }
        Player.RemoveEffect(this, false);
    }

    public override void OnEffectEnd(bool interrupt)
    {
    }

    public override void OnEffectUpdate()
    {
    }
}

public class DetectiveGun : Component
{
    [Serialized] public Entity Barrell;
    [Serialized] public Entity BarrellTarget;
}

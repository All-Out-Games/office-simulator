using AO;

public class Teleporter : Component
{
    [Serialized]
    public Entity Target;

    public override void Awake()
    {
        var interactible = Entity.GetComponent<Interactable>();
        interactible.OnInteract = (Player p) =>
        {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/warp.wav"), new SFX.PlaySoundDesc() { Volume = 0.5f } );
            p.Teleport(new Vector2(Target.X, Target.Y));
        };
    }
}

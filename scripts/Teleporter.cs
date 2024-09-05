using AO;

public class Teleporter : Component
{
    [Serialized]
    public Entity Target;
    public float LastPlayedSfxAt = 0;

    public override void Awake()
    {
        var interactible = Entity.GetComponent<Interactable>();
        interactible.OnInteract = (Player p) =>
        {
            var op = (OfficePlayer)p;

            if (Network.IsServer)
            {
                if (op.CurrentRoom == Room.HR) op.CurrentRoom = Room.HALLS;
                if (op.CurrentRoom == Room.FINANCE) op.CurrentRoom = Room.HR;

            }

            if (Network.IsClient)
            {
                if (op.CurrentRoom == Room.HR)
                {
                    if (DayNightManager.Instance.CurrentState != DayState.NIGHT)
                    {
                        op.SetLightOn(false);
                    }
                } else
                {
                    op.SetLightOn(true);
                }


                // Cooldown for the global sounds
                if (Time.TimeSinceStartup - LastPlayedSfxAt > 3)
                {
                    LastPlayedSfxAt = Time.TimeSinceStartup;
                    SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 0.45f } );
                }
                else {
                    if (Network.LocalPlayer == p)
                    {
                        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 0.45f } );
                    }
                }
            }

            p.Teleport(new Vector2(Target.X, Target.Y));
        };
    }
}

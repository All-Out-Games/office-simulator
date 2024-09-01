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
                    op.SetLightOn(false);
                } else
                {
                    op.SetLightOn(true);
                    op.CameraControl.AmbientColour = new Vector3(0, 0, 0);
                }

                SFX.Play(Assets.GetAsset<AudioAsset>("sfx/night-hit.wav"), new SFX.PlaySoundDesc() { Volume = 0.5f } );
            }

            p.Teleport(new Vector2(Target.X, Target.Y));
        };
    }
}

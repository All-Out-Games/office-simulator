using AO;

public class MovingRat : Component
{
  public SyncVar<bool> Squashed = new(false);
  public Vector2 StartPosition;
  private Interactable interactable;
  private Sprite_Renderer spriteRenderer;
  private ulong sfxHandle;

  public override void Awake()
  {
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    StartPosition = Entity.Position;
    interactable = Entity.AddComponent<Interactable>();
    interactable.Text = "Squash Rat";
    interactable.CanUseCallback = (Player p) =>
    {
      return !Squashed;
    };
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;
      Squash(op);
    };

    StopEvent();
  }

  public override void Update()
  {
    if (Squashed)
    {
      spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
    }
    else
    {
      spriteRenderer.Tint = new Vector4(0.8f, 0.8f, 0.8f, 1);
      Entity.Position = StartPosition + new Vector2(MathF.Sin(Time.TimeSinceStartup) * 3, 0);
      Entity.LocalScaleX = MathF.Sign(MathF.Sin(Time.TimeSinceStartup));
    }
  }

  private void Squash(OfficePlayer op)
  {
    if (sfxHandle != 0) SFX.Stop(sfxHandle);
    spriteRenderer.Tint = new Vector4(0, 0, 0, 0);

    if (!Network.IsServer) return;
    Squashed.Set(true);
    op?.CallClient_PlaySFX("anomalies/rats/rat-die.wav");
  }

  public void StartEvent()
  {
    if (Network.IsServer)
    {
      Squashed.Set(false);
    }

    sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/rats/rat.wav"), new SFX.PlaySoundDesc() { Volume = 0.6f, Loop = true, Positional = true, Position = Entity.Position });
  }

  public void StopEvent()
  {
    Squash(null);
  }
}
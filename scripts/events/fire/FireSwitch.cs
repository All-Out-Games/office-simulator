using AO;

public class FireSwitch : Component
{
  private SyncVar<bool> eventActive = new(false);
  public SyncVar<bool> Fixed = new(false);
  private Interactable interactable;
  private Sprite_Renderer spriteRenderer;
  private SpriteFlasher spriteFlasher;
  private ulong sfxHandle;

  public override void Awake()
  {
    spriteFlasher = Entity.AddComponent<SpriteFlasher>();
    spriteFlasher.Flash = false;

    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    interactable = Entity.AddComponent<Interactable>();
    interactable.Text = "Pull Extinguisher";
    interactable.CanUseCallback = (Player p) =>
    {
      return !Fixed;
    };
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;
      Fix(op);
    };
  }

  public override void Update()
  {
    if (Fixed)
    {
      spriteFlasher.Flash = false;
    } else {
      spriteFlasher.Flash = true;
    }

    if (!eventActive)
    {
      spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
    }
  }

  public override void Start()
  {
    StopEvent();
  }

  private void Fix(OfficePlayer op)
  {
    SFX.Stop(sfxHandle);

    if (!Network.IsServer) return;
    Fixed.Set(true);
    op?.CallClient_PlaySFX("sfx/progress_check_on.wav");
  }

  public void StartEvent()
  {
    if (Network.IsServer)
    {
      eventActive.Set(true);
    }


    sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/fire/fire.wav"), new SFX.PlaySoundDesc() { Volume=0.4f, Loop = true, Positional=true, Position=Entity.Position });

    if (Network.IsServer)
    {
      Fixed.Set(false);
    }
  }

  public void StopEvent()
  {
    if (Network.IsServer)
    {
      eventActive.Set(false);
    }

    spriteRenderer.Tint = new Vector4(0, 0, 0, 0);
    Fix(null);
  }
}
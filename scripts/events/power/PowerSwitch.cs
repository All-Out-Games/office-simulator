using AO;

public partial class PowerSwitch : Component
{
  private SyncVar<bool> eventActive = new(false);
  public SyncVar<bool> Fixed = new(false);
  private Interactable interactable;
  private Spine_Animator spineAnimator;
  private ulong sfxHandle;
  private bool playingTurnOn = false;
  private bool playingSwitchOn = false;

  public override void Awake()
  {
    spineAnimator = Entity.GetComponent<Spine_Animator>();
    if (!spineAnimator.Alive())
    {
      Log.Warn("PowerSwitch has no spine animator");
      return;
    }
    interactable = Entity.AddComponent<Interactable>();
    interactable.Text = "Restore Breaker";
    interactable.CanUseCallback = (Player p) =>

    {
      return !Fixed;
    };
    interactable.OnInteract = (Player p) =>
    {
      var op = (OfficePlayer)p;
      Fix(op);
    };

    spineAnimator.OnAnimationEnd += OnAnimationEnd;
    StopEvent();
  }

  private void OnAnimationEnd(string animName)
  {
    if (Network.IsClient && animName == "turn_on")
    {
      playingTurnOn = false;
      spineAnimator.SpineInstance.SetAnimation("switch_off", false);
    }
  }

  public override void Update()
  {
    if (Network.IsClient && spineAnimator.Alive())
    {
      if (eventActive && !Fixed && !playingTurnOn && !playingSwitchOn)
      {
        playingSwitchOn = true;
        spineAnimator.SpineInstance.SetAnimation("switch_on", true);
      }
    }
  }

  private void Fix(OfficePlayer op)
  {
    SFX.Stop(sfxHandle);

    if (!Network.IsServer) return;
    Fixed.Set(true);
    CallClient_PlayFixedAnimation();
    op?.CallClient_PlaySFX("anomalies/power/breaker-switched.wav");
  }

  [ClientRpc]
  public void PlayFixedAnimation()
  {
    if (!spineAnimator.Alive()) return;
    playingTurnOn = true;
    spineAnimator.SpineInstance.SetAnimation("turn_on", false);
  }

  public void StartEvent()
  {
    if (Network.IsServer)
    {
      eventActive.Set(true);
      Fixed.Set(false);
    }

    if (Network.IsClient && spineAnimator.Alive())
    {
      playingTurnOn = false;
      playingSwitchOn = false;
      spineAnimator.SpineInstance.SetAnimation("switch_on", true);
    }

    sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("anomalies/power/breaker.wav"), new SFX.PlaySoundDesc() { Volume = 0.6f, Loop = true, Positional = true, Position = Entity.Position });
  }

  public void StopEvent()
  {
    if (Network.IsServer)
    {
      eventActive.Set(false);
    }

    Fix(null);
  }
}
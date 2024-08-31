using AO;

public partial class Jukebox : Component
{
  [Serialized] public AudioAsset Sfx;
  public ulong sfxHandle;
  private bool manuallyStopped = false;
  private bool nightVersion = false;

  private Interactable interactable;

  public override void Awake()
  {
    interactable = AddComponent<Interactable>();
    interactable.RequiredHoldTime = 0.4f;
    interactable.PromptOffset = new Vector2(-0.6f, -0.5f);
    interactable.OnInteract += (Player p) => {
      manuallyStopped = !manuallyStopped;
      if (manuallyStopped)
      {
        Stop();
      }
      else
      {
        SafePlay();
      }
    };

    SafePlay();
  }

  public override void Update()
  {
    interactable.Text = manuallyStopped ? "Play" : "Mute";
  }

  // DayNightManager will call this method to switch to the night version of the sfx
  public void SetNightVersion(bool night)
  {
    nightVersion = night;
    Stop();
    SafePlay();
  }

  // Plays unless the user has manually muted the jukebox
  public void SafePlay()
  {
    if (!manuallyStopped)
    {
      if (nightVersion)
      {
        sfxHandle = SFX.Play(Sfx, new SFX.PlaySoundDesc() { Volume = 1.1f, Speed=0.75f, Loop = true, Position = Entity.Position, Positional = true, RangeMultiplier = 2.5f });
      }
      else
      {
        sfxHandle = SFX.Play(Sfx, new SFX.PlaySoundDesc() { Volume = 1.5f, Loop = true, Position = Entity.Position, Positional = true, RangeMultiplier = 2f });
      }
    }
  }

  public void Stop()
  {
    SFX.Stop(sfxHandle);
  }
}
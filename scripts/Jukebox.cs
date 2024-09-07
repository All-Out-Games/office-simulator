using AO;

public partial class Jukebox : Component
{
  [Serialized] public AudioAsset Sfx;
  [Serialized] public float Volume;
  public ulong sfxHandle;
  private SyncVar<bool> manuallyStopped = new(false);
  private SyncVar<bool> isPlaying = new(true);
  private bool nightVersion = false;

  private Interactable interactable;

  public override void Start()
  {
    interactable = AddComponent<Interactable>();
    interactable.RequiredHoldTime = 0.4f;
    interactable.PromptOffset = new Vector2(-0.6f, -0.5f);
    interactable.OnInteract += (Player p) => {
      if (Network.IsServer)
      {
        manuallyStopped.Set(!manuallyStopped);
      }

      var newManuallyStopped = !manuallyStopped;
      
      if (newManuallyStopped)
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

  [ClientRpc]
  public void ActuallyPlay()
  {
    if (Network.IsServer) return;

    if (nightVersion)
    {
      sfxHandle = SFX.Play(Sfx, new SFX.PlaySoundDesc() { Volume = Volume, Speed=0.875f, Loop = true, Position = Entity.Position, Positional = true, RangeMultiplier = 3f });
    }
    else
    {
      sfxHandle = SFX.Play(Sfx, new SFX.PlaySoundDesc() { Volume = Volume, Loop = true, Position = Entity.Position, Positional = true, RangeMultiplier = 2.75f });
    }
  }

  // Plays unless the user has manually muted the jukebox
  public void SafePlay()
  {
    if (!manuallyStopped && !isPlaying)
    {
      if (Network.IsServer)
      {
        isPlaying.Set(true);
        CallClient_ActuallyPlay();
      }


    } else {
      Log.Info("Jukebox is muted");
    }
  }


  [ClientRpc]
  public void ActuallyStop()
  {
    if (Network.IsServer) return;

    SFX.Stop(sfxHandle);
  }

  public void Stop()
  {
    if (Network.IsServer)
    {
      isPlaying.Set(false);
      CallClient_ActuallyStop();
    }
  }
}
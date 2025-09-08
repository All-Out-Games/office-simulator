using AO;

public partial class Jukebox : Component
{
    [Serialized] public AudioAsset Sfx;
    [Serialized] public float Volume;
    public ulong sfxHandle;
    public SyncVar<bool> manuallyStopped = new(false);
    public SyncVar<bool> isPlaying = new(false);
    private bool nightVersion = false;
    private bool isLocalPlaying = false;
    private Interactable interactable;

    public override void Awake()
    {
        interactable = Entity.Unsafe_AddComponent<Interactable>();
        interactable.RequiredHoldTime = 0.8f;
        interactable.PromptOffset = new Vector2(-0.6f, -0.5f);
        interactable.OnInteract += (Player p) =>
        {
            if (Network.IsServer)
            {
                manuallyStopped.Set(!manuallyStopped);
                if (manuallyStopped)
                {
                    Stop();
                }
                else
                {
                    SafePlay();
                }
            }
        };
        SafePlay();
    }

    public override void Update()
    {
        interactable.Text = manuallyStopped ? "Play Jukebox" : "Mute Jukebox";
        if (isPlaying.Value != isLocalPlaying)
        {
            isLocalPlaying = isPlaying.Value;
            if (isLocalPlaying)
            {
                ActuallyPlay();
            }
            else
            {
                ActuallyStop();
            }
        }
    }

    public void SetNightVersion(bool night)
    {
        Log.Info("Setting night version to " + night + " current version was " + nightVersion);
        if (nightVersion != night)
        {
            Log.Info("Actually Setting night version to " + night);
            nightVersion = night;
            if (Network.IsServer)
            {
                Stop();
                SafePlay();
            }
            else
            {
                ActuallyStop();
                ActuallyPlay();
            }
        }
    }

    public void ActuallyPlay()
    {
        if (Network.IsServer) return;
        ActuallyStop();
        var soundDesc = new SFX.PlaySoundDesc
        {
            Volume = Volume,
            Loop = true,
            Position = Entity.Position,
            Positional = true,
            RangeMultiplier = nightVersion ? 3f : 2.75f,
            Speed = nightVersion ? 0.875f : 1f
        };
        sfxHandle = SFX.Play(Sfx, soundDesc);
    }

    public void SafePlay()
    {
        if (!Network.IsServer) return;
        if (!manuallyStopped.Value && !isPlaying.Value)
        {
            isPlaying.Set(true);
        }
    }

    public void ActuallyStop()
    {
        if (Network.IsServer) return;
        SFX.Stop(sfxHandle);
        sfxHandle = 0;
    }

    public void Stop()
    {
        if (!Network.IsServer) return;
        isPlaying.Set(false);
    }
}
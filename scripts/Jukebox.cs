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

    public override void Start()
    {
        interactable = AddComponent<Interactable>();
        interactable.RequiredHoldTime = 0.4f;
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
        if (nightVersion != night)
        {
            nightVersion = night;
            Stop();
            SafePlay();
        }
    }

    public void ActuallyPlay()
    {
        if (Network.IsServer) return;

        SFX.Stop(sfxHandle);

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

        // Stop the current sound on the client
        SFX.Stop(sfxHandle);
    }

    public void Stop()
    {
        if (!Network.IsServer) return;

        isPlaying.Set(false);
    }
}

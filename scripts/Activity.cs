using AO;

public enum ActivityState
{
  ACTIVE,
  COOLDOWN,
}

public partial class Activity : Component
{
  [Serialized] public string PromptText;
  [Serialized] public Role MinimumRoleRequired;
  [Serialized] public Role MaxRole = Role.CEO;

  // Textures/Resources
  [Serialized] public Texture ActiveTexture;
  [Serialized] public Texture CooldownTexture;
  [Serialized] public AudioAsset OnCompleteSfx;
  [Serialized] public AudioAsset OnActiveSfx;
  private ulong activeSfxHandle;
  [Serialized] public AudioAsset OnCooldownSfx;

  // Cooldown Settings
  [Serialized] public float CooldownSeconds;
  [Serialized] public float AvailableForSeconds; // Time the task is available before it will return to cooldown (use -1 for indefinite)
  [Serialized] public bool HideWhenOnCooldown;

  // If it's time to respawn but either of these is false and it's day/night, the respawn will wait till the next available time. 
  [Serialized] public bool SpawnsDuringDay;
  [Serialized] public bool SpawnsDuringNight;


  // Costs/Rewards
  [Serialized] public int XpReward = 0;
  // -2 can be used to lookup the player's salary
  [Serialized] public int CashReward = 0;
  [Serialized] public int CashCost = 0;

  // Refs
  protected Interactable interactable;
  private Sprite_Renderer spriteRenderer;

  // State
  private SyncVar<int> currentState = new((int)ActivityState.ACTIVE);
  public ActivityState CurrentState
  {
    get => (ActivityState)currentState.Value;
    set
    {
      timeInState.Set(0f);
      currentState.Set((int)value);
    }
  }

  // Timers
  private SyncVar<float> timeInState = new(0f);

  public override void Awake()
  {
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();
    if (!spriteRenderer.Alive())
    {
      Log.Error($"Activity on entity ({Entity.Name} is missing a sprite renderer. Disabling)");
      LocalEnabled = false;
    }

    if (ActiveTexture == null)
    {
      ActiveTexture = spriteRenderer.Texture;
    }

    interactable = Entity.AddComponent<Interactable>();

    if (Network.IsClient)
    {
      Entity.AddComponent<SpriteFlasher>();
    }

    interactable.PromptOffset = new Vector2(-0.5f, 0.25f);
    interactable.OnInteract = OnInteract;
    interactable.Radius = 2.5f;
    interactable.CanUseCallback += MeetsBasicRequirements;
    interactable.Text = GetPromptWithRewardsAndCost();
  }

  public void OnInteract(Player p)
  {
    var op = (OfficePlayer)p;

    if (Network.LocalPlayer == p)
    {
      if (Network.IsClient)
      {
        Haptics.PlayImpact(Haptics.ImpactType.Light);
      }
    }

    // Guards
    if (!Network.IsServer) return;
    var requirementsResult = CheckAllRequirements(op);
    if (!requirementsResult.Success)
    {
      op.CallClient_ShowNotification(requirementsResult.ErrorMessage);
      op.CallClient_PlaySFX("sfx/error.wav");
      return;
    }

    // Happy path
    op.Experience.Set(op.Experience + XpReward);
    op.Cash.Set(op.Cash + (CashReward - CashCost));
    CurrentState = ActivityState.COOLDOWN;
    if (OnCompleteSfx != null)
    {
      op.CallClient_PlaySFX(OnCompleteSfx.Name);
    }
  }

  public override void Update()
  {
    if (Network.IsServer)
    {
      TickState();
    }
    else if (Network.IsClient)
    {
      TickClientVisuals();
    }
  }

  private void TickState()
  {
    timeInState.Set(timeInState + Time.DeltaTime);

    switch (CurrentState)
    {
      case ActivityState.ACTIVE:
        if (SpawnsDuringNight && !SpawnsDuringDay && DayNightManager.Instance.CurrentState != DayState.NIGHT)
        {
          CurrentState = ActivityState.COOLDOWN;
          break;
        }

        if (AvailableForSeconds > 0 && timeInState.Value >= AvailableForSeconds)
        {
          CurrentState = ActivityState.COOLDOWN;
          break;
        }

        break;
      case ActivityState.COOLDOWN:
        if (timeInState.Value >= CooldownSeconds)
        {
          bool isNightTime = DayNightManager.Instance.CurrentState == DayState.NIGHT;
          if ((isNightTime && SpawnsDuringNight) || (!isNightTime && SpawnsDuringDay))
          {
            CurrentState = ActivityState.ACTIVE;
          }
        }
        break;
    }
  }

  private void TickClientVisuals()
  {
    switch (CurrentState)
    {
      case ActivityState.ACTIVE:
        spriteRenderer.Texture = ActiveTexture;

        if (activeSfxHandle == 0 && OnActiveSfx != null)
        {
          activeSfxHandle = SFX.Play(OnActiveSfx, new() { Volume = 0.5f, Positional = true, Position = Entity.Position });
        }

        // Makes the sprite flash if they meet all the requirements to bring attention to the activity
        if (CheckAllRequirements(Network.LocalPlayer).Success)
        {
          Entity.GetComponent<SpriteFlasher>().Flash = true;
        }
        break;
      case ActivityState.COOLDOWN:
        if (activeSfxHandle != 0)
        {
          SFX.Stop(activeSfxHandle);
          activeSfxHandle = 0;
        }

        spriteRenderer.Texture = HideWhenOnCooldown ? null : CooldownTexture;

        break;
    }

    // Cleanup flasher if it exists
    if (Entity.GetComponent<SpriteFlasher>().Flash && (CurrentState == ActivityState.COOLDOWN || !CheckAllRequirements(Network.LocalPlayer).Success))
    {
      Entity.GetComponent<SpriteFlasher>().Flash = false;
    }
  }

  // This is probably overengineered. Grug sad. 
  public struct RequirementsResult
  {
    public bool Success;
    public string ErrorMessage;

    public RequirementsResult(bool success, string errorMessage = null)
    {
      Success = success;
      ErrorMessage = errorMessage;
    }
  }


  // We'll show the interact option if some basic pre-reqs are met, but they may still not be able to use it (e.g. missing $$$)
  public bool MeetsBasicRequirements(Player p)
  {
    var op = (OfficePlayer)p;
    if (CurrentState == ActivityState.COOLDOWN) return false;
    if (op.CurrentRole < MinimumRoleRequired) return false;
    if (op.CurrentRole > MaxRole) return false;

    return true;
  }

  // If all checks out and they can actually complete the activity
  public RequirementsResult CheckAllRequirements(Player p)
  {
    var op = (OfficePlayer)p;

    if (CurrentState == ActivityState.COOLDOWN)
    {
      return new RequirementsResult(false, "Item on cooldown (H4x?)");
    }

    if (op.CurrentRole < MinimumRoleRequired)
    {
      return new RequirementsResult(false, "Insufficient role");
    }

    if (op.CurrentRole > MaxRole)
    {
      return new RequirementsResult(false, "Too high role");
    }

    if (op.Cash < CashCost)
    {
      return new RequirementsResult(false, "Insufficient cash");
    }

    return new RequirementsResult(true);
  }

  // Compiles a string including the prompt text and any costs/rewards
  private string GetPromptWithRewardsAndCost()
  {
    var decorations = new List<string>
    {
        CashCost > 0 ? $"-${CashCost}" : null,
        XpReward > 0 ? $"+{XpReward} XP" : null,
        CashReward > 0 ? $"+${CashReward}" : null,
    }.Where(reward => !string.IsNullOrEmpty(reward)); ;

    var decorationString = string.Join(" ", decorations);

    return string.IsNullOrEmpty(decorationString) ? PromptText : $"{PromptText} ({decorationString})";
  }
}

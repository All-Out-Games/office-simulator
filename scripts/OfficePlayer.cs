using AO;

public enum Role
{
  JANITOR,
  EMPLOYEE,
  MANAGER,
  CEO,
  OVERSEER
}

public partial class OfficePlayer : Player
{
  public SyncVar<bool> WasKilledInOverseerBattle = new(false);
  public SyncVar<float> LastRequestedCEOPromoAt = new(0);
  public SyncVar<float> LastRequestedOverseerPromoAt = new(0);
  private Entity lightEntity;

  // Animation timing fields
  private float experienceAnimEndTime;
  private float cashAnimEndTime;
  private float roleAnimEndTime;
  private float ANIM_DURATION = 0.2f;

  public bool ShownPromoPrompt = false;
  public CameraControl CameraControl;
  private SyncVar<int> currentRole = new((int)Role.JANITOR);
  public Role CurrentRole
  {
    get => (Role)currentRole.Value;
    set => currentRole.Set((int)value);
  }

  public SyncVar<int> currentRoom = new((int)Room.HALLS);
  public Room CurrentRoom
  {
    get => (Room)currentRoom.Value;
    set => currentRoom.Set((int)value);
  }

  public Entity StatsUI;

  public int InternalSalary => CurrentRole switch
  {
    Role.JANITOR => 60,
    Role.EMPLOYEE => 200,
    Role.MANAGER => 300,
    Role.CEO => 500,
    Role.OVERSEER => 300,
    _ => 0
  };

  public int Salary => GameManager.Instance.ReducedPay ? InternalSalary / 2 : InternalSalary;

  // EXPERIENCE //
  public SyncVar<int> Experience = new(0);
  public SyncVar<int> Cash = new(0);

  public int RequiredExperience => 100;

  public Spine_Animator KillerSpineAnimator;
  public SyncVar<bool> HasGymPass = new(false);
  public SyncVar<bool> HasGivenSpeech = new(false);

  public SyncVar<bool> Caffeinated = new(false);
  public SyncVar<float> CaffinatedAt = new();

  public SyncVar<Entity> AssignedMeetingSeat = new();

  public SyncVar<Entity> OfficeController = new();
  public SyncVar<float> MoveSpeedModifier = new(0.95f);

  public override Vector2 CalculatePlayerVelocity(Vector2 currentVelocity, Vector2 input, float deltaTime)
  {
    var multiplier = MoveSpeedModifier.Value;
    if (HasEffect<KillerEffect>())
    {
      multiplier *= 0.75f;
    }
    if (HasEffect<OverseerEffect>())
    {
      if (GameManager.Instance.FastJanitors)
      {
        multiplier *= 0.85f;
      }
      else
      {
        multiplier *= 0.725f;
      }
    }
    if (Caffeinated)
    {
      multiplier *= 1.3f;
    }

    Vector2 velocity = DefaultPlayerVelocityCalculation(currentVelocity, input, deltaTime, multiplier);
    return velocity;
  }

  public override void OnDestroy()
  {
    if (!OfficeController.Value.Alive()) return;

    var controller = OfficeController.Value?.GetComponent<OfficeController>();
    if (controller.Alive())
    {
      controller.Reset(true);
    }
  }

  public override void Awake()
  {
    Game.SetVoiceEnabled(true);
    var spawns = Scene.Components<Spawn_Point>();
    foreach (Spawn_Point spawn in spawns)
    {
      // Only teleport to the first spawn
      Teleport(spawn.Position);
      continue;
    }

    Experience.OnSync += (oldValue, newValue) =>
    {
      if (Experience.Value >= 100 && !ShownPromoPrompt)
      {
        ShownPromoPrompt = true;
        if (Network.IsServer)
        {
          if (CurrentRole == Role.MANAGER)
          {
            CallClient_ShowNotification("Give a speech to convince the office to vote for you.");
          }
          else
          {
            CallClient_ShowNotification("You can now request a promotion in the HR room.");
          }
        }
      }

      if (IsLocal)
      {
        References.Instance.ExperienceStatText.Text = "XP: " + Math.Clamp(newValue, 0, 100) + "/100";

        var settings = References.Instance.ExperienceStatText.Settings;

        // Flash green if increased, red if decreased
        if (newValue > oldValue)
        {
          settings.Color = new Vector4(0, 1, 0, 1);
          settings.Size = settings.Size * 1.2f; // Scale bump
        }
        else if (newValue < oldValue)
        {
          settings.Color = new Vector4(1, 0, 0, 1);
          settings.Size = settings.Size * 1.2f; // Scale bump
        }
        else
        {
          settings.Color = new Vector4(1, 1, 1, 1);
        }

        References.Instance.ExperienceStatText.Settings = settings;
        experienceAnimEndTime = Time.TimeSinceStartup + ANIM_DURATION;
      }
    };

    Cash.OnSync += (oldValue, newValue) =>
    {
      if (IsLocal)
      {
        References.Instance.MoneyStatText.Text = "Cash $" + newValue;

        var settings = References.Instance.MoneyStatText.Settings;

        // Flash green if increased, red if decreased
        if (newValue > oldValue)
        {
          settings.Color = new Vector4(0, 1, 0, 1);
          settings.Size = settings.Size * 1.2f; // Scale bump
        }
        else if (newValue < oldValue)
        {
          settings.Color = new Vector4(1, 0, 0, 1);
          settings.Size = settings.Size * 1.2f; // Scale bump
        }
        else
        {
          settings.Color = new Vector4(1, 1, 1, 1);
        }

        References.Instance.MoneyStatText.Settings = settings;
        cashAnimEndTime = Time.TimeSinceStartup + ANIM_DURATION;
      }
    };

    currentRole.OnSync += (oldValue, newValue) =>
    {
      if (IsLocal)
      {
        var settings = References.Instance.RoleStatText.Settings;
        settings.Color = new Vector4(0, 1, 0, 1);
        settings.Size = settings.Size * 1.2f; // Scale bump
        References.Instance.RoleStatText.Settings = settings;
        roleAnimEndTime = Time.TimeSinceStartup + ANIM_DURATION;
      }
    };

    if (IsLocal)
    {
      CameraControl = CameraControl.Create(1);
    }

    {
      var murderLayer = SpineAnimator.SpineInstance.StateMachine.CreateLayer("murder_layer", 10);
      var aoLayer = SpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("main");
      var aoIdleState = aoLayer.TryGetStateByName("Idle");
      var aoRunState = aoLayer.TryGetStateByName("Run_Fast");
      var idleState = murderLayer.CreateState("MURD_002/empty", 0, true);
      murderLayer.InitialState = idleState;

      var pointBool = SpineAnimator.SpineInstance.StateMachine.CreateVariable("point", StateMachineVariableKind.BOOLEAN);
      var pointExaggerateTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("point_exaggerate", StateMachineVariableKind.TRIGGER);
      var pointState = murderLayer.CreateState("MURD_002/point_mIK_AL", 0, true);
      var pointExaggerateState = murderLayer.CreateState("MURD_002/point_ex_mIK_AL", 0, false);
      murderLayer.CreateTransition(idleState, pointState, false).CreateBoolCondition(pointBool, true);
      murderLayer.CreateTransition(pointState, pointExaggerateState, false).CreateTriggerCondition(pointExaggerateTrigger);
      murderLayer.CreateTransition(pointExaggerateState, pointExaggerateState, false).CreateTriggerCondition(pointExaggerateTrigger);
      murderLayer.CreateTransition(pointExaggerateState, pointState, true);
      murderLayer.CreateTransition(pointState, idleState, false).CreateBoolCondition(pointBool, false);

      var attackTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("murder_attack", StateMachineVariableKind.TRIGGER);
      var attackState = murderLayer.CreateState("MURD_002/kill_swipe_mIK_AL", 0, false);
      murderLayer.CreateGlobalTransition(attackState).CreateTriggerCondition(attackTrigger);
      murderLayer.CreateTransition(attackState, idleState, true);

      var openFolderTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("open_folder", StateMachineVariableKind.TRIGGER);
      var closeFolderTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("close_folder", StateMachineVariableKind.TRIGGER);
      var takeOutFolderState = aoLayer.CreateState("MURD_002/evidence_take_out", 0, false);
      var folderLoopState = aoLayer.CreateState("MURD_002/evidence_loop", 0, true);
      var putAwayFolderState = aoLayer.CreateState("MURD_002/evidence_put_away", 0, false);
      aoLayer.CreateGlobalTransition(takeOutFolderState).CreateTriggerCondition(openFolderTrigger);
      aoLayer.CreateTransition(takeOutFolderState, folderLoopState, true);
      aoLayer.CreateTransition(folderLoopState, putAwayFolderState, false).CreateTriggerCondition(closeFolderTrigger);
      aoLayer.CreateTransition(putAwayFolderState, aoIdleState, true);

      var openCamerasTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("open_cameras", StateMachineVariableKind.TRIGGER);
      var closeCamerasTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("close_cameras", StateMachineVariableKind.TRIGGER);
      var takeOutCamerasState = aoLayer.CreateState("MURD_002/camera_tablet_take_out", 0, false);
      var camerasLoopState = aoLayer.CreateState("MURD_002/camera_tablet_loop", 0, true);
      var putAwayCamerasState = aoLayer.CreateState("MURD_002/camera_tablet_put_away", 0, false);
      aoLayer.CreateGlobalTransition(takeOutCamerasState).CreateTriggerCondition(openCamerasTrigger);
      aoLayer.CreateTransition(takeOutCamerasState, camerasLoopState, true);
      aoLayer.CreateTransition(camerasLoopState, putAwayCamerasState, false).CreateTriggerCondition(closeCamerasTrigger);
      aoLayer.CreateTransition(putAwayCamerasState, aoIdleState, true);

      var transformTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("transform_start", StateMachineVariableKind.TRIGGER);
      var transformBackTrigger = SpineAnimator.SpineInstance.StateMachine.CreateVariable("transform_back_end", StateMachineVariableKind.TRIGGER);
      var toKillerState = aoLayer.CreateState("MURD_002/transform_to_imposter_start", 0, false);
      var fromKillerState = aoLayer.CreateState("MURD_002/transform_to_normal_end", 0, false);
      aoLayer.CreateGlobalTransition(toKillerState).CreateTriggerCondition(transformTrigger);
      aoLayer.CreateTransition(toKillerState, aoIdleState, true);
      aoLayer.CreateGlobalTransition(fromKillerState).CreateTriggerCondition(transformBackTrigger);
      aoLayer.CreateTransition(fromKillerState, aoIdleState, true);

      var dragBodyBool = SpineAnimator.SpineInstance.StateMachine.CreateVariable("dragging_body", StateMachineVariableKind.BOOLEAN);
      var aoMovingBool = SpineAnimator.SpineInstance.StateMachine.TryGetVariableByName("moving");
      var dragIdle = aoLayer.CreateState("MURD_002/drag_body_idle_right", 0, true);
      var dragMove = aoLayer.CreateState("MURD_002/drag_body_walk_right", 0, true);
      aoLayer.CreateTransition(aoIdleState, dragIdle, false).CreateBoolCondition(dragBodyBool, true);
      aoLayer.CreateTransition(dragIdle, aoIdleState, false).CreateBoolCondition(dragBodyBool, false);
      aoLayer.CreateTransition(aoRunState, dragMove, false).CreateBoolCondition(dragBodyBool, true);
      aoLayer.CreateTransition(dragMove, aoRunState, false).CreateBoolCondition(dragBodyBool, false);
      aoLayer.CreateTransition(dragIdle, dragMove, false).CreateBoolCondition(aoMovingBool, true);
      aoLayer.CreateTransition(dragMove, dragIdle, false).CreateBoolCondition(aoMovingBool, false);

      var tp = aoLayer.TryGetStateByName("Teleport_Appear");
      aoLayer.CreateTransition(tp, aoIdleState, true);
    }

    {
      var killerEntity = Entity.Create();
      killerEntity.LocalScale = new Vector2(0.22f, 0.22f);
      killerEntity.SetParent(Entity, false);
      KillerSpineAnimator = killerEntity.AddComponent<Spine_Animator>();
      KillerSpineAnimator.SpineInstance.SetSkeleton(Assets.GetAsset<SpineSkeletonAsset>("animations/Impostor/imposter_monster.spine"));
      KillerSpineAnimator.MaskInShadow = true;
      KillerSpineAnimator.SpineInstance.Scale = new Vector2(2.4f, 2.4f);
      var killerStateMachine = StateMachine.Make();

      var killerLayer = killerStateMachine.CreateLayer("killer_layer", 0);

      var movingBool = killerStateMachine.CreateVariable("moving", StateMachineVariableKind.BOOLEAN);
      var idleState = killerLayer.CreateState("idle", 0, true);
      var runState = killerLayer.CreateState("run", 0, true);
      killerLayer.CreateTransition(idleState, runState, false).CreateBoolCondition(movingBool, true);
      killerLayer.CreateTransition(runState, idleState, false).CreateBoolCondition(movingBool, false);
      killerLayer.InitialState = idleState;

      var transformToTrigger = killerStateMachine.CreateVariable("transform_end", StateMachineVariableKind.TRIGGER);
      var transformToEndState = killerLayer.CreateState("teleport_appear", 0, false);
      killerLayer.CreateGlobalTransition(transformToEndState).CreateTriggerCondition(transformToTrigger);
      killerLayer.CreateTransition(transformToEndState, idleState, true);

      var transformBackTrigger = killerStateMachine.CreateVariable("transform_back_start", StateMachineVariableKind.TRIGGER);
      var transformFromStartState = killerLayer.CreateState("teleport_disapear", 0, false);
      killerLayer.CreateGlobalTransition(transformFromStartState).CreateTriggerCondition(transformBackTrigger);
      killerLayer.CreateTransition(transformFromStartState, idleState, true);

      var attackTrigger = killerStateMachine.CreateVariable("attack", StateMachineVariableKind.TRIGGER);
      var attackState = killerLayer.CreateState("convertbreath", 0, false);
      killerLayer.CreateGlobalTransition(attackState).CreateTriggerCondition(attackTrigger);
      killerLayer.CreateTransition(attackState, idleState, true);

      KillerSpineAnimator.SpineInstance.SetStateMachine(killerStateMachine, killerEntity);
      KillerSpineAnimator.LocalEnabled = false;
    }

    var collisionEntity = Assets.GetAsset<Prefab>("FatPlayerCollision.prefab").Instantiate();
    collisionEntity.GetComponent<PlayerCollisionChild>().Player = this;
    collisionEntity.LocalScale = new Vector2(1.1f, 1.1f);
    collisionEntity.SetParent(Entity, false);

    if (IsLocal)
    {
      lightEntity = Entity.Create();
      lightEntity.SetParent(this.Entity, false);
      var light = lightEntity.AddComponent<SpookyLight>();
      light.Light.Color = new Vector4(1, 1f, 1f, 0);
      light.Light.ShadowCaster = false;
      light.Light.Radi = new Vector2(0, 5000);
    }
  }

  public void SetLightOn(bool on)
  {
    if (!IsLocal) return;

    if (on)
    {
      {
        lightEntity.GetComponent<SpookyLight>().Light.Color = new Vector4(1, 0.6f, 0.4f, 0);
        lightEntity.GetComponent<SpookyLight>().Light.ShadowCaster = true;
        lightEntity.GetComponent<SpookyLight>().Light.Radi = new Vector2(0, 8.5f);
      }
    }
    else
    {
      lightEntity.GetComponent<SpookyLight>().Light.Color = new Vector4(1, 1f, 1f, 0);
      lightEntity.GetComponent<SpookyLight>().Light.Radi = new Vector2(0, 5000f);
      lightEntity.GetComponent<SpookyLight>().Light.ShadowCaster = false;
    }
  }

  public UI.TextSettings GetTextSettings(float size, Vector4 color, float offset = 1.90f, FontAsset font = null, UI.HorizontalAlignment halign = UI.HorizontalAlignment.Center)
  {
    if (font == null)
    {
      font = UI.Fonts.BarlowBold;
    }
    var ts = new UI.TextSettings()
    {
      Font = font,
      Size = size,
      Color = color,
      DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
      DropShadowOffset = new Vector2(0f, -3f),
      HorizontalAlignment = halign,
      VerticalAlignment = UI.VerticalAlignment.Center,
      WordWrap = false,
      WordWrapOffset = 0,
      Outline = true,
      OutlineThickness = 3,
      Offset = new Vector2(0, offset),
    };
    return ts;
  }

  public void LoseEvent()
  {
    if (!Network.IsServer) return;
    CallClient_ShowNotification("Damages have been deducted from your account.");
    Cash.Set(Math.Max(Cash.Value - 500, 0));
  }

  public void WinEvent()
  {
    if (!Network.IsServer) return;
    CallClient_ShowNotification("The overseer has rewarded you... (+$250, +25XP)");
    Cash.Set(Cash.Value + 250);
    Experience.Set(Experience + 25);
  }

  public bool IsInOverseerBattle()
  {
    return OverseerPromoNPC.Instance.Fighter1?.Value == this.Entity || OverseerPromoNPC.Instance.Fighter2?.Value == this.Entity;
  }

  public override void Update()
  {
    bool moving = Velocity.Length > 0.03f;
    KillerSpineAnimator.SpineInstance.StateMachine.SetBool("moving", moving);

    if (Network.IsServer)
    {
      if (Time.TimeSinceStartup - CaffinatedAt >= 90f)
      {
        Caffeinated.Set(false);
      }
    }

    if (CurrentRole == Role.OVERSEER && !HasEffect<OverseerEffect>())
    {
      AddEffect<OverseerEffect>();
    }

    if (CurrentRole != Role.OVERSEER && HasEffect<OverseerEffect>())
    {
      RemoveEffect<OverseerEffect>(false);
    }

    if (IsLocal)
    {
      // Handle Experience animation reset
      if (Time.TimeSinceStartup >= experienceAnimEndTime && experienceAnimEndTime > 0)
      {
        var resetSettings = References.Instance.ExperienceStatText.Settings;
        resetSettings.Color = Experience.Value >= 100 ? new Vector4(0, 1, 0, 1) : new Vector4(1, 1, 1, 1);
        resetSettings.Size = resetSettings.Size / 1.2f;
        References.Instance.ExperienceStatText.Settings = resetSettings;
        experienceAnimEndTime = 0;
      }

      // Handle Cash animation reset
      if (Time.TimeSinceStartup >= cashAnimEndTime && cashAnimEndTime > 0)
      {
        var resetSettings = References.Instance.MoneyStatText.Settings;
        resetSettings.Color = new Vector4(1, 1, 1, 1);
        resetSettings.Size = resetSettings.Size / 1.2f;
        References.Instance.MoneyStatText.Settings = resetSettings;
        cashAnimEndTime = 0;
      }

      // Handle Role animation reset
      if (Time.TimeSinceStartup >= roleAnimEndTime && roleAnimEndTime > 0)
      {
        var resetSettings = References.Instance.RoleStatText.Settings;
        resetSettings.Color = new Vector4(1, 1, 1, 1);
        resetSettings.Size = resetSettings.Size / 1.2f;
        References.Instance.RoleStatText.Settings = resetSettings;
        roleAnimEndTime = 0;
      }

      if (HasEffect<KillerEffect>())
      {
        DrawDefaultAbilityUI(new AbilityDrawOptions()
        {
          AbilityElementSize = 125,
          Abilities = new Ability[] { GetAbility<KillAbility>() }
        });
      }

      if (CurrentRole == Role.CEO || IsInOverseerBattle())
      {
        DrawDefaultAbilityUI(new AbilityDrawOptions()
        {
          AbilityElementSize = 125,
          Abilities = new Ability[] { GetAbility<Revolver>() }
        });
      }

      if (CurrentRoom == Room.CONFERENCE || CurrentRoom == Room.CONFERENCE_SPEAKER)
      {
        CameraControl.Zoom = 1.8f;
      }
      else
      {
        CameraControl.Zoom = 1.25f;
      }


      CameraControl.Position = Entity.Position + new Vector2(0, 0.5f);

      var roleStatTextSettings = References.Instance.RoleStatText.Settings;
      roleStatTextSettings.Color = new Vector4(1, 1, 1, 1);
      References.Instance.RoleStatText.Text = "Your Role: " + CurrentRole.ToString();
      References.Instance.RoleStatText.Settings = roleStatTextSettings;

      if (DayNightManager.Instance.CurrentState == DayState.NIGHT)
      {
        // Special display of the role for zombie janitors
        if (CurrentRole == Role.JANITOR)
        {
          roleStatTextSettings = References.Instance.RoleStatText.Settings;
          roleStatTextSettings.Color = new Vector4(1, 0, 0, 1);
          References.Instance.RoleStatText.Text = "Your Role: \"Janitor\"";
          References.Instance.RoleStatText.Settings = roleStatTextSettings;
        }
      }
    }

    // Point to promo NPC when full of XP
    // TODO: support manager role which requires giving conference talk
    if (IsLocal && Experience >= RequiredExperience && CurrentRoom != Room.HR && !(CurrentRole == Role.OVERSEER || CurrentRole == Role.CEO))
    {
      var pointToEntity = References.Instance.PromoNPC;
      if (CurrentRole == Role.MANAGER)
      {
        pointToEntity = References.Instance.Podium;

      }

      if (pointToEntity.Alive())
      {
        var worldOffset = (pointToEntity.Position - Entity.Position);

        var sellAreaScreenPos = Camera.WorldToScreen(pointToEntity.Position);
        var playerScreenPos = Camera.WorldToScreen(Entity.Position + new Vector2(0, 0.5f));
        var dir = (sellAreaScreenPos - playerScreenPos).Normalized;
        var pos = playerScreenPos;
        var distance = worldOffset.Length;

        float arrowSize = 50;
        var anim = (float)Math.Pow(Math.Abs(Math.Sin(Math.PI * Time.TimeSinceStartup)), 0.75);
        float distanceThreshold = 3;
        if (distance >= (distanceThreshold + 0.5f))
        {
          var t = 1 - Ease.T(distance - distanceThreshold, 1);
          var arrowScreenPos = new Rect(pos, pos).Offset(dir.X * 300, dir.Y * 300).Center; // note(josh): using rects to scale by screen size
          arrowScreenPos = Vector2.Lerp(arrowScreenPos, sellAreaScreenPos, t);
          var rect = new Rect(arrowScreenPos, arrowScreenPos).Grow(arrowSize);
          var rotation = Math.Atan2(dir.Y, dir.X) * (180.0 / Math.PI);
          UI.Image(rect, References.Instance.ArrowIcon, Vector4.White, default, (float)rotation);
        }
        else
        {
          var rect = new Rect(sellAreaScreenPos, sellAreaScreenPos).Grow(arrowSize);
          rect = rect.Offset(0, anim * 50);
          UI.Image(rect, References.Instance.ArrowIcon, Vector4.White, default, 270);
        }
      }
    }
  }

  public override void LateUpdate()
  {
    using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
    using var _2 = IM.PUSH_Z(GetZOffset() - 0.001f);
    using var _3 = UI.PUSH_PLAYER_MATERIAL(this);
    var rect = new Rect(Entity.Position, Entity.Position).Grow(0.125f);

    var ts = GetTextSettings(0.225f, new Vector4(1, 1, 1, 1));
    if (DayNightManager.Instance.CurrentState == DayState.NIGHT && CurrentRole == Role.JANITOR)
    {
      ts = GetTextSettings(0.225f, new Vector4(1, 0, 0, 1));
      UI.Text(rect, "JANITOR", ts);
    }
    else if (CurrentRole == Role.OVERSEER)
    {
      ts = GetTextSettings(0.40f, new Vector4(0.6f, 0.2f, 1f, 1f));
      UI.Text(rect, "OVERSEER", ts);
    }
    else if (CurrentRole == Role.CEO)
    {
      ts = GetTextSettings(0.35f, new Vector4(0, 1, 0, 1));
      UI.Text(rect, "CEO", ts);
    }
    else
    {
      UI.Text(rect, CurrentRole.ToString(), ts);
    }
  }

  [ClientRpc]
  public void ShowNotification(string text)
  {
    if (IsLocal)
    {
      Notifications.Show(text);
    }
  }

  [ClientRpc]
  public void PlaySFX(string sfxPath)
  {
    float volume = 1f;
    if (sfxPath == References.Instance.ErrorSfx.Name) volume = 0.6f;
    if (IsLocal)
    {
      SFX.Play(Assets.GetAsset<AudioAsset>(sfxPath), new SFX.PlaySoundDesc() { Volume = volume });
    }
  }
}

public abstract class MyEffect : AEffect
{
  public new OfficePlayer Player => (OfficePlayer)base.Player;
}

public class WaitForAnimEffect : MyEffect
{
  public override bool IsActiveEffect => true;
  public override bool FreezePlayer => true;

  public override void OnEffectStart(bool isDropIn)
  {
    if (!isDropIn)
    {
      DurationRemaining = Player.SpineAnimator.SpineInstance.StateMachine.TryGetLayerByIndex(0).GetCurrentStateLength();
    }
  }

  public override void OnEffectEnd(bool interrupt)
  {
  }

  public override void OnEffectUpdate()
  {
  }
}

public abstract class MyAbility : Ability
{
  public new OfficePlayer Player => (OfficePlayer)base.Player;


}
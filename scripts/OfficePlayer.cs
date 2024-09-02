using AO;

public enum Role
{
  JANITOR,
  EMPLOYEE,
  MANAGER,
  CEO
}

public partial class OfficePlayer : Player
{
  public SyncVar<bool> IsDead = new(false);
  private Entity lightEntity;

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

  public Entity UI;

  public int Salary => CurrentRole switch
  {
    Role.JANITOR => 100,
    Role.EMPLOYEE => 200,
    Role.MANAGER => 300,
    Role.CEO => 500,
    _ => 0
  };

  public SyncVar<int> Experience = new(100);
  public SyncVar<int> Cash = new(1000);

  public int RequiredExperience => CurrentRole == Role.MANAGER ? 100 : 100;

  public Spine_Animator KillerSpineAnimator;
  public SyncVar<bool> HasGymPass = new(false);
  public SyncVar<bool> HasGivenSpeech = new(false);

  public SyncVar<bool> Caffeinated = new(false);
  public SyncVar<float> CaffinatedAt = new();

  public SyncVar<int> BoardVotes = new(0);
  public SyncVar<bool> IsBoardElectionCandidate = new(false);

  public SyncVar<Entity> AssignedMeetingSeat = new();

  public SyncVar<Entity> OfficeController = new();

  public override Vector2 CalculatePlayerVelocity(Vector2 currentVelocity, Vector2 input, float deltaTime)
  {
      var multiplier = 2f;
      if (HasEffect<KillerEffect>())
      {
        multiplier *= 0.6f;
      }
      if (Caffeinated)
      {
        multiplier *= 1.25f;
      }

      Vector2 velocity = DefaultPlayerVelocityCalculation(currentVelocity, input, deltaTime, multiplier);
      return velocity;
  }

  public override void OnDestroy()
  {
    OfficeController.Value?.GetComponent<OfficeController>().Reset();
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
      if (IsLocal) {
        References.Instance.ExperienceStatText.Text = "XP: " + Math.Clamp(newValue, 0, 100) + "/100";

        if (newValue >= 100)
        {
          var settings = References.Instance.ExperienceStatText.Settings;
          settings.Color = new Vector4(0, 1, 0, 1);

          References.Instance.ExperienceStatText.Settings = settings;
        } else {
          var settings = References.Instance.ExperienceStatText.Settings;
          settings.Color = new Vector4(1, 1, 1, 1);

          References.Instance.ExperienceStatText.Settings = settings;
        }
      }

    };

    Cash.OnSync += (oldValue, newValue) =>
    {
      if (IsLocal) References.Instance.MoneyStatText.Text = "Cash $" + newValue;
    };

    if (IsLocal)
    {
      CameraControl = Camera.CreateCameraControl(1);
    }

    {
        var murderLayer = SpineAnimator.SpineInstance.StateMachine.CreateLayer("murder_layer", 10);
        var aoLayer = SpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("main");
        var aoIdleState = aoLayer.TryGetStateByName("Idle");
        var aoRunState = aoLayer.TryGetStateByName("Run_Fast");
        var idleState = murderLayer.CreateState("MURD_002/empty", 0, true);
        murderLayer.SetInitialState(idleState);

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
        killerLayer.SetInitialState(idleState);

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

    if (IsLocal)
    {
      lightEntity = Entity.Create();
      lightEntity.SetParent(this.Entity, false);
      var light = lightEntity.AddComponent<SpookyLight>();
      light.Light.Color = new Vector4(1, 1f, 1f, 0);
      light.Light.ShadowCaster = false;
      light.Light.Radi = new Vector2(0, 50);
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
      lightEntity.GetComponent<SpookyLight>().Light.Radi = new Vector2(0, 50f);
      lightEntity.GetComponent<SpookyLight>().Light.ShadowCaster = false;
    }
  }

  public override void Update()
  {
    bool moving = Velocity.Length > 0.03f;
    KillerSpineAnimator.SpineInstance.StateMachine.SetBool("moving", moving);

    if (Network.IsServer)
    {
      if (Time.TimeSinceStartup - CaffinatedAt >= 10f)
      {
        Caffeinated.Set(false);
      }
    }

    if (IsLocal)
    {
      if (HasEffect<KillerEffect>())
      {
        DrawDefaultAbilityUI(new AbilityDrawOptions() {
          Abilities = new Ability[] { GetAbility<KillAbility>() }
        });
      }

      // Make the camera follow the player
      CameraControl.Position = Entity.Position + new Vector2(0, 0.5f);
      if (CurrentRoom == Room.CONFERENCE || CurrentRoom == Room.CONFERENCE_SPEAKER)
      {
        CameraControl.Zoom = 1.8f;
      }
      else
      {
        CameraControl.Zoom = 1.25f;
      }

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
  }

  [ServerRpc]
  public void CountVote(Player candidate)
  {
      Log.Info("RPC CALLED");
      var op = (OfficePlayer)candidate;
      op.BoardVotes.Set(op.BoardVotes.Value + 1);
      Log.Info($"Player voted for {op.Name}");
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
      if (IsLocal)
      {
        SFX.Play(Assets.GetAsset<AudioAsset>(sfxPath), new SFX.PlaySoundDesc() { Volume = 1f });
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
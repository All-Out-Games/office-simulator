using AO;


public partial class BoardMeetingEffect : MyEffect, INetworkedComponent
{
    public override bool IsActiveEffect => true;
    public override bool BlockAbilityActivation => true;
    public override bool FreezePlayer => true;

    public Random random;

    public float Timer;

    public CameraControl BoardCameraControl;
    public float CameraPhase = 0;

    // public Entity UI;

    public override float DefaultDuration => 17f;

    public bool HasPlayedEndSound;

    public override void OnEffectStart(bool isDropIn)
    {
        var playerSeat = Player.AssignedMeetingSeat;
        Player.Teleport(playerSeat.Value.Position);
        Player.SetLightOn(false);

        Player.SetFacingDirection(playerSeat.Value.GetComponent<Seat>().FaceLeft ? false : true);

        if (!Player.IsLocal) return;

        // Create camera control with higher priority than player camera
        BoardCameraControl = CameraControl.Create(2);
        BoardCameraControl.Zoom = 0.8f;

        Notifications.Show("A board meeting to elect a new CEO has been called...");
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.Teleport(new Vector2(0, 0));
        Player.SetLightOn(false);
        if (Player.IsLocal)
        {
            if (BoardCameraControl != null)
            {
                BoardCameraControl.Destroy();
            }
        }
    }

    public override void OnEffectUpdate()
    {
        if (AO.Util.OneTime(DurationRemaining <= 4.25f, ref HasPlayedEndSound))
        {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/levelup.wav"), new SFX.PlaySoundDesc() { Volume = 0.55f });
        }

        if (Player.IsLocal && PromoNPC.Instance.BoardMeetingActive)
        {
            var candidate1 = PromoNPC.Instance.Candidate1.Value?.GetComponent<OfficePlayer>();
            var candidate2 = PromoNPC.Instance.Candidate2.Value?.GetComponent<OfficePlayer>();

            if (candidate1?.Alive() == true && candidate2?.Alive() == true)
            {
                // Camera control sequence
                if (CameraPhase == 0 && DurationRemaining > 15f)
                {
                    // Zoom to candidate 1
                    BoardCameraControl.Position = candidate1.Entity.Position + new Vector2(0, 0.5f);
                    BoardCameraControl.Zoom = AOMath.Lerp(BoardCameraControl.Zoom, 1.5f, 0.2f * Time.DeltaTime);

                    if (DurationRemaining <= 16f)
                    {
                        BoardCameraControl.Zoom = 0.8f;
                        CameraPhase = 1;
                    }
                }
                else if (CameraPhase == 1 && DurationRemaining > 14f)
                {
                    // Zoom to candidate 2
                    BoardCameraControl.Position = candidate2.Entity.Position + new Vector2(0, 0.5f);
                    BoardCameraControl.Zoom = AOMath.Lerp(BoardCameraControl.Zoom, 1.5f, 0.2f * Time.DeltaTime);

                    if (DurationRemaining <= 15f)
                    {
                        CameraPhase = 2;
                    }
                }
                else
                {
                    // Back out to overview position
                    BoardCameraControl.Position = new Vector2(9.575f, -50.249f);
                    BoardCameraControl.Zoom = AOMath.Lerp(BoardCameraControl.Zoom, 1.8f, 0.2f * Time.DeltaTime);
                }

                if (DurationRemaining <= 15f)
                {
                    var rect = UI.ScreenRect;
                    LogButton.DrawVotingUI(rect, candidate1.Name, candidate2.Name, () =>
                    {
                        PromoNPC.Instance.CallServer_CountVote(1);
                    }, () =>
                    {
                        PromoNPC.Instance.CallServer_CountVote(2);
                    });
                }
            }
        }
    }
}

public class OverseerEffect : MyEffect, INetworkedComponent
{
    public override bool IsActiveEffect => false;
    public override bool BlockAbilityActivation => !DoneAnim;

    public bool DoneAnim = false;
    public ulong sfxHandle;

    public void OnKillerAnimationEnd(string animationName)
    {
        if (animationName == "teleport_appear")
        {
            Player.RemoveFreezeReason("transforming");
            DoneAnim = true;
        }
    }
    public void OnKillerAnimationEvent(string eventName)
    {
        if (eventName == "footstep")
        {
            Player.PlayRandomFootstep();
        }
    }

    public override void OnEffectStart(bool isDropIn)
    {
        if (!isDropIn)
        {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/invisibility_on.wav"), new() { Volume = 0.6f, Positional = true, Position = Player.Entity.Position });


            Player.AddFreezeReason("transforming");
            Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_end");
            Player.KillerSpineAnimator.OnAnimationEnd += OnKillerAnimationEnd;
        }
        else
        {
            Player.RemoveFreezeReason("transforming");
            DoneAnim = true;
        }

        Player.AddInvisibilityReason(nameof(OverseerEffect));
        if (!Player.IsLocal)
        {
            // sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/zombie-hum.wav"), new SFX.PlaySoundDesc {Volume=0.2f, Position = Player.Entity.Position, Positional=true} );
        }
        Player.KillerSpineAnimator.SpineInstance.Scale = new Vector2(6, 6);
        Player.KillerSpineAnimator.LocalEnabled = true;
        Player.KillerSpineAnimator.OnEvent += OnKillerAnimationEvent;
    }

    public override void OnEffectEnd(bool interrupt)
    {
        SFX.Stop(sfxHandle);
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/invisibility_off.wav"), new() { Volume = 0.5f, Positional = true, Position = Player.Entity.Position });
        Player.KillerSpineAnimator.LocalEnabled = false;
        Player.RemoveInvisibilityReason(nameof(OverseerEffect));
        Player.RemoveFreezeReason("transforming");
        if (!interrupt)
        {
            Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_end");
            Player.AddEffect<WaitForAnimEffect>();
        }
        Player.KillerSpineAnimator.OnAnimationEnd -= OnKillerAnimationEnd;
        Player.KillerSpineAnimator.OnEvent -= OnKillerAnimationEvent;
    }

    public override void OnEffectUpdate()
    {
    }
}


public class KillerEffect : MyEffect, INetworkedComponent
{
    public override bool IsActiveEffect => false;
    public override bool BlockAbilityActivation => !DoneAnim;

    public bool DoneAnim = false;
    public ulong sfxHandle;

    public void OnKillerAnimationEnd(string animationName)
    {
        if (animationName == "teleport_appear")
        {
            Player.RemoveFreezeReason("transforming");
            DoneAnim = true;
        }
    }
    public void OnKillerAnimationEvent(string eventName)
    {
        if (eventName == "footstep")
        {
            Player.PlayRandomFootstep();
        }
    }

    public override void OnEffectStart(bool isDropIn)
    {
        if (!isDropIn)
        {
            SFX.Play(Assets.GetAsset<AudioAsset>("sfx/invisibility_on.wav"), new() { Volume = 0.6f, Positional = true, Position = Player.Entity.Position });
            Player.AddFreezeReason("transforming");
            Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_end");
            Player.KillerSpineAnimator.OnAnimationEnd += OnKillerAnimationEnd;
        }
        else
        {
            Player.RemoveFreezeReason("transforming");
            DoneAnim = true;
        }

        Player.AddInvisibilityReason(nameof(KillerEffect));
        Player.KillerSpineAnimator.SpineInstance.Scale = new Vector2(4, 4);
        if (!Player.IsLocal)
        {
            sfxHandle = SFX.Play(Assets.GetAsset<AudioAsset>("sfx/zombie-hum.wav"), new SFX.PlaySoundDesc { Volume = 0.3f, Position = Player.Entity.Position, Positional = true });
        }
        Player.KillerSpineAnimator.LocalEnabled = true;
        Player.KillerSpineAnimator.OnEvent += OnKillerAnimationEvent;
    }

    public override void OnEffectEnd(bool interrupt)
    {
        SFX.Stop(sfxHandle);
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/invisibility_off.wav"), new() { Volume = 0.75f, Positional = true, Position = Player.Entity.Position });
        Player.KillerSpineAnimator.LocalEnabled = false;
        Player.RemoveInvisibilityReason(nameof(KillerEffect));
        Player.RemoveFreezeReason("transforming");
        if (!interrupt)
        {
            Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_end");
            Player.AddEffect<WaitForAnimEffect>();
        }
        Player.KillerSpineAnimator.OnAnimationEnd -= OnKillerAnimationEnd;
        Player.KillerSpineAnimator.OnEvent -= OnKillerAnimationEvent;
    }

    public override void OnEffectUpdate()
    {
    }
}

public class TransformFromKillerEffect : MyEffect
{
    public override bool IsActiveEffect => true;
    public override bool FreezePlayer => true;

    public override void OnEffectStart(bool isDropIn)
    {
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/invisibility_on.wav"), new() { Positional = true, Position = Player.Entity.Position });
        Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_start");
        if (isDropIn)
        {
            DurationRemaining = Player.KillerSpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("killer_layer").GetCurrentStateLength();
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.AddEffect<FinishTransformFromKillerEffect>();
        if (interrupt)
        {
            Player.RemoveEffect<FinishTransformFromKillerEffect>(true);
        }
    }

    public override void OnEffectUpdate()
    {

    }
}

public class FinishTransformFromKillerEffect : MyEffect
{
    public override bool IsActiveEffect => true;

    public override void OnEffectStart(bool isDropIn)
    {
        Player.KillerSpineAnimator.LocalEnabled = false;
        Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_end");
        if (!isDropIn)
        {
            Player.RemoveInvisibilityReason(nameof(KillerEffect));
            Player.RemoveNameInvisibilityReason(nameof(KillerEffect));
            DurationRemaining = Player.SpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("main").GetCurrentStateLength();
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
    }

    public override void OnEffectUpdate()
    {

    }
}

public partial class KillAbility : MyAbility
{
    public override TargettingMode TargettingMode => TargettingMode.Nearest;
    public override float MaxDistance => 1.5f;
    public override int MaxTargets => 1;
    public override Texture Icon => Assets.GetAsset<Texture>("Ability_Icons/kill_cleaver_icon.png");
    public override float Cooldown => 30f;

    public override bool CanTarget(Player p)
    {
        var op = (OfficePlayer)p;
        return op.CurrentRole != Role.JANITOR && op.CurrentRole != Role.OVERSEER;
    }

    public override bool OnTryActivate(List<Player> targetPlayers, Vector2 positionOrDirection, float magnitude)
    {
        Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("attack");

        if (Network.IsServer)
        {
            Player.CallClient_ShowNotification("Kill (+20% EXP +$10)");
            Player.Experience.Set(Player.Experience + 20);
            Player.Cash.Set(Player.Cash + 10);

            CallClient_KillPlayer(targetPlayers[0], Player);
        }

        return true;
    }

    public override bool CanUse()
    {
        return true;
    }

    [ClientRpc]
    public static void KillPlayer(Player player, Player killer)
    {
        player.AddEffect<KillEffect>();
    }
}

public class KillEffect : MyEffect
{
    public override bool IsActiveEffect => true;
    public override bool FreezePlayer => true;
    public override bool IsValidTarget => false;
    public override float DefaultDuration => 1.133f;

    public SpineInstance JumpSpineInstance;


    public override void OnEffectStart(bool isDropIn)
    {
        SFX.Play(Assets.GetAsset<AudioAsset>("sfx/disguise_activate.wav"), new() { Positional = true, Position = Player.Entity.Position });

        Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("teleport_away");
        JumpSpineInstance = SpineInstance.Make();
        JumpSpineInstance.SetSkeleton(Assets.GetAsset<SpineSkeletonAsset>("animations/Jump/jumpscare_asset.spine"));
        JumpSpineInstance.SetAnimation("appear", false);
    }

    public override void OnEffectEnd(bool interrupt)
    {
        if (!interrupt)
        {
            Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("teleport_appear");
            Player.AddEffect<SpectatorEffect>();
        }

        JumpSpineInstance.Destroy();
    }

    public override void OnEffectUpdate()
    {
        JumpSpineInstance.Update(Time.DeltaTime);

        if (Player.IsLocal)
        {
            UI.Image(UI.ScreenRect, UI.WhiteSprite, Vector4.Black);
            UI.DrawSkeleton(UI.SafeRect.CenterRect(), JumpSpineInstance, Vector2.One * 100, 0);
        }
    }
}

public class SpectatorEffect : MyEffect
{
    public override bool IsActiveEffect => false;
    public override bool IsValidTarget => false;

    public bool HasNameInvisReason = false;

    public void UpdateInvis()
    {
        if (Player.IsLocal || (Network.LocalPlayer.Alive() && Network.LocalPlayer.HasEffect<SpectatorEffect>()))
        {
            Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 0.5f);
            if (HasNameInvisReason)
            {
                HasNameInvisReason = false;
                Player.RemoveNameInvisibilityReason(nameof(SpectatorEffect));
            }
        }
        else
        {
            Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 0);
            if (!HasNameInvisReason)
            {
                HasNameInvisReason = true;
                Player.AddNameInvisibilityReason(nameof(SpectatorEffect));
            }
        }
    }

    public override void OnEffectStart(bool isDropIn)
    {
        UpdateInvis();
        Player.Teleport(new Vector2(29.046f, -65.001f));
        Player.SpineAnimator.DepthOffset = -10000;
        Player.SpineAnimator.SpineInstance.StateMachine.SetBool("ghost_form", true);
        if (!isDropIn)
        {
            Player.AddEmoteBlockReason(nameof(SpectatorEffect));
        }

        if (Network.IsServer)
        {
            Player.CurrentRoom = Room.LOBBY;
            Player.CurrentRole = Role.JANITOR;
            Player.OfficeController?.Value?.GetComponent<OfficeController>().Reset();
            Player.Experience.Set(0);
            Player.Cash.Set(0);
        }
    }

    public override void OnEffectUpdate()
    {
        UpdateInvis();
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.Teleport(new Vector2(-64.494f, -20.005f));
        Player.RemoveEmoteBlockReason(nameof(SpectatorEffect));
        Player.SpineAnimator.DepthOffset = 0;
        Player.SpineAnimator.SpineInstance.StateMachine.SetBool("ghost_form", false);
        Player.SpineAnimator.SpineInstance.ColorMultiplier = new Vector4(1, 1, 1, 1);
        if (HasNameInvisReason)
        {
            Player.RemoveNameInvisibilityReason(nameof(SpectatorEffect));
        }
    }
}
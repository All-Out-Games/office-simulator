using AO;
public class TransformToKillerEffect : MyEffect
{
    public override bool IsActiveEffect => true;
    public override bool FreezePlayer => true;

    public override void OnEffectStart(bool isDropIn)
    {
        Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_start");
        if (!isDropIn)
        {
            DurationRemaining = Player.SpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("main").GetCurrentStateLength();
            // SFX.Play(Assets.GetAsset<AudioAsset>("sfx/MurderMystery Player Character/transform_to_imposter_start.wav"), new SFX.PlaySoundDesc(){Positional=true, Position=Player.Entity.Position});
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
        if (!interrupt)
        {
            Player.AddEffect<KillerEffect>();
        }
    }

    public override void OnEffectUpdate()
    {
    }
}

public class BoardMeetingEffect : MyEffect, INetworkedComponent
{
    public override bool IsActiveEffect => false;
    public override bool BlockAbilityActivation => true;

    public Random random;

    public float Timer;

    public Entity UI;

    public List<OfficePlayer> candidates;
    
    public override void OnEffectStart(bool isDropIn)
    {
        random = new Random();

        UI = Entity.Instantiate(Assets.GetAsset<Prefab>("BoardMeetingUI.prefab"));

        var seatList = new List<Seat>();
        var seats = Scene.Components<Seat>();
        foreach (Seat seat in seats)
        {
            if (seat.Occupied.Value) continue;
            seatList.Add(seat);
        }

        var boardSeats = seatList.Where(seat => seat.Type == "Board");
        var candidateSeats = seatList.Where(seat => seat.Type == "Candidate");

        Notifications.Show("A board meeting to elect a new CEO has been called...");

        if (Player.IsBoardElectionCandidate)
        {
            candidates.Add(Player);

            Player.Teleport(candidateSeats.First().Position);
            // h4x h4x h4x ;(
            candidateSeats.First().Entity.Position = new Vector2(candidateSeats.First().Position.X+2, candidateSeats.First().Position.Y);
        }

        var randomSeat = seatList[random.Next(seatList.Count)];
        if (Network.IsServer) randomSeat.Occupied.Set(true);

        Player.Teleport(randomSeat.Position);
        Player.AddFreezeReason("board_meeting");

        var voteLeft= UI.TryGetChildByName("VoteLeft");
        var voteRight= UI.TryGetChildByName("VoteRight");

        if (voteLeft.Alive() && voteRight.Alive())
        {
            var voteLeftButton = voteLeft.GetComponent<UIButton>();
            var voteRightButton = voteRight.GetComponent<UIButton>();

            var voteLeftText = voteLeft.GetComponent<UIText>();
            var voteRightText = voteRight.GetComponent<UIText>();

            voteLeftButton.OnClicked = () => {
                candidates[0].BoardVotes.Set(candidates[0].BoardVotes.Value + 1);
            };

            voteRightButton.OnClicked = () => {
                candidates[1].BoardVotes.Set(candidates[1].BoardVotes.Value + 1);
            };
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.Teleport(new Vector2(0, 0));
        Player.RemoveFreezeReason("board_meeting");
    }

    public override void OnEffectUpdate()
    {
        Timer += Time.DeltaTime;
        if (Timer >= 10)
        {
            Player.RemoveEffect<BoardMeetingEffect>(false);
        }
    }
}


public class KillerEffect : MyEffect, INetworkedComponent
{
    public override bool IsActiveEffect => false;
    public override bool BlockAbilityActivation => !DoneAnim;

    public bool DoneAnim = false;
    
    public void OnKillerAnimationEnd(string animationName)
    {
        if (animationName == "transform_to_imposter_end")
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
            Player.AddInvisibilityReason(nameof(KillerEffect));
            Player.AddFreezeReason("transforming");
            // SFX.Play(Assets.GetAsset<AudioAsset>("sfx/transform_scary.wav"), new SFX.PlaySoundDesc(){Positional=true, Position=Player.Entity.Position});
            // SFX.Play(Assets.GetAsset<AudioAsset>("sfx/v2/killer-transform_to_imposter_end_v2.wav"), new SFX.PlaySoundDesc(){Positional=true, Position=Player.Entity.Position});
            Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_end");
            Player.KillerSpineAnimator.SpineInstance.OnAnimationEnd += OnKillerAnimationEnd;
        }
        else
        {
            Player.RemoveFreezeReason("transforming");
            DoneAnim = true;
        }

        Player.KillerSpineAnimator.LocalEnabled = true;
        Player.KillerSpineAnimator.SpineInstance.OnEvent += OnKillerAnimationEvent;
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.KillerSpineAnimator.LocalEnabled = false;
        Player.RemoveInvisibilityReason(nameof(KillerEffect));
        Player.RemoveFreezeReason("transforming");
        if (!interrupt)
        {
            Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_end");
            Player.AddEffect<WaitForAnimEffect>();
        }
        Player.KillerSpineAnimator.SpineInstance.OnAnimationEnd -= OnKillerAnimationEnd;
        Player.KillerSpineAnimator.SpineInstance.OnEvent -= OnKillerAnimationEvent;
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
        Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("transform_back_start");
        if (isDropIn)
        {
            DurationRemaining = Player.KillerSpineAnimator.SpineInstance.StateMachine.TryGetLayerByName("killer_layer").GetCurrentStateLength();
            // SFX.Play(Assets.GetAsset<AudioAsset>("sfx/v2/killer-transform_to_normal_start2_v2.wav"), new SFX.PlaySoundDesc(){Positional=true, Position=Player.Entity.Position});
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
    public override float MaxDistance => 1f;
    public override int MaxTargets => 1;
    public override Texture Icon => Assets.GetAsset<Texture>("Ability_Icons/kill_cleaver_icon.png");
    public override float Cooldown => 30f;

    public override bool OnTryActivate(List<Player> targetPlayers, Vector2 positionOrDirection, float magnitude)
    {
        if (Player.HasEffect<KillerEffect>())
        {
            Player.KillerSpineAnimator.SpineInstance.StateMachine.SetTrigger("attack");
        }
        else 
        {
            Player.SpineAnimator.SpineInstance.StateMachine.SetTrigger("murder_attack");
        }

        if (Network.IsServer)
        {
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

    }
}

public class KillerKillAbility : KillAbility
{
    public override float Cooldown => 3f;
}

public class KillEffect : MyEffect
{
    public enum DeathSourceEnum
    {
        Knife,
        Bullet,
        ThrownKnife,
        LeftGame,
    }

    public override bool IsActiveEffect => true;
    public override bool FreezePlayer => true;
    public override bool IsValidTarget => false;
    public override float DefaultDuration => 1.0f;

    public DeathSourceEnum DeathSource = DeathSourceEnum.Knife;

    public override void OnEffectStart(bool isDropIn)
    {
        if (!isDropIn)
        {
            Player.RemoveEffect<KillerEffect>(true);
            Player.AddInvisibilityReason(nameof(KillEffect));
        }

        if (Network.IsServer)
        {
            Player.IsDead.Set(true);
        }
    }

    public override void OnEffectEnd(bool interrupt)
    {
        Player.RemoveInvisibilityReason(nameof(KillEffect));
    }

    public override void OnEffectUpdate()
    {
    }
}


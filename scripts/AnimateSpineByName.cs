using AO;

public class AnimateSpineByName : Component
{
  [Serialized] public string AnimationName = "Player_Shop/Idle";
  // bcuz I added it after and there are no defaults
  [Serialized] public bool DontLoop;

  public override void Awake()
  {
    var spineAnimator = Entity.GetComponent<Spine_Animator>();
    if (spineAnimator.Alive())
    {
      spineAnimator.Awaken();
      spineAnimator.Entity.Scale = new Vector2(1.25f, 1.25f);
      spineAnimator.SpineInstance.SetAnimation(AnimationName, !DontLoop);
      spineAnimator.SetCrewchsia(10);
    }
  }
}
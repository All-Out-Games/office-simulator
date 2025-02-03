using AO;

public class References : Component
{
    [Serialized] public UICanvas EventUI;
    [Serialized] public AudioAsset ErrorSfx;
    [Serialized] public AudioAsset ClickSfx;
    [Serialized] public UIText MoneyStatText;
    [Serialized] public UIText ExperienceStatText;
    [Serialized] public UIText RoleStatText;
    [Serialized] public Entity PromoNPC;
    [Serialized] public Texture ArrowIcon;
    [Serialized] public Entity DarknessOverlay;


    public static References _instance;
    public static References Instance
    {
        get
        {
            if (_instance.Alive() == false)
            {
                foreach (var c in Scene.Components<References>())
                {
                    _instance = c;
                    _instance.Awaken();
                    break;
                }
            }
            return _instance;
        }
    }
}
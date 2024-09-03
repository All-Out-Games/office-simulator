using AO;

public class GymDoor : TwoWayDoor
{
  private Sprite_Renderer spriteRenderer;

  public override void Awake()
  {
    base.Awake();
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();

    interactable.OnInteract = (Player player) =>
    {
      if (!Network.IsServer) return;

      var op = (OfficePlayer)player;

      if (!op.HasGymPass)
      {
        op.CallClient_ShowNotification("You don't have a gym pass!");
        return;
      }

      base.OnInteract(player);
    };
  }

  public override void Update()
  {
    if (Network.IsServer) return;

    var op = (OfficePlayer)Network.LocalPlayer;
    if (!op.Alive()) return;

    if (op.HasGymPass)
    {
      spriteRenderer.Tint = new Vector4(1, 1, 1, 0.7f);
    }
    else {
      spriteRenderer.Tint = new Vector4(1, 1, 1, 1f);
    }

    if (op.CurrentRoom != RoomName)
    {
      interactable.Text = $"Enter {RoomName}";
    }
    else
    {
      interactable.Text = $"Leave {RoomName}";
    }
  }
}
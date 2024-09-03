using AO;

public class OfficeDoor : TwoWayDoor
{
  OfficeController Controller;
  private Sprite_Renderer spriteRenderer;

  public override void Start()
  {
    base.Start();
    spriteRenderer = Entity.GetComponent<Sprite_Renderer>();

    Controller = Entity.Parent.TryGetChildByName("Controller").GetComponent<OfficeController>();
    
    interactable.OnInteract = (Player player) =>
    {
      if (!Network.IsServer) return;
      var op = (OfficePlayer)player;

      if (Controller.IsOwned && Controller.Owner != player.Entity && !Controller.Unlocked)
      {
        if (op.CurrentRoom != RoomName)
        {
          if (op.Cash < Controller.BreachCost)
          {
            op.CallClient_ShowNotification("This office is locked... not enough $ to breach");
            op.CallClient_PlaySFX("sfx/error.wav");
            return;
          } else {
            // Breach for money
            op.CallClient_PlaySFX("sfx/invisibility_off.wav");
            // TODO: Positional
            GameManager.Instance.CallClient_PlaySFX("sfx/ImpactDoorBreak_S08IM.288.wav");
            Controller.Owner.Value.GetComponent<OfficePlayer>().CallClient_ShowNotification("Your office was breached by " + op.Name + "!");
            op.Cash.Set(op.Cash - Controller.BreachCost);
          }
        }
      }

      if (!Controller.IsOwned)
      {
        if (op.CurrentRole < Controller.RequiredRole)
        {
          op.CallClient_ShowNotification("You must reach the " + Controller.RequiredRole + " role to buy this office.");
          op.CallClient_PlaySFX("sfx/error.wav");
          return;
        }

        if (op.Cash < Controller.Cost)
        {
          op.CallClient_ShowNotification("You don't have enough cash");
          op.CallClient_PlaySFX("sfx/error.wav");
          return;
        }

        op.OfficeController?.Value?.GetComponent<OfficeController>().Reset();
        op.CallClient_PlaySFX("sfx/rank-up.wav");
        op.Cash.Set(op.Cash - Controller.Cost);
        Controller.Owner.Set(op.Entity);
        op.OfficeController.Set(Controller.Entity);
      }

      base.OnInteract(player);
    };
  }

  private string GetOfficeName()
  {
    if (Controller.IsOwnedByMyClient) return "Your Office";
    if (Controller.IsOwned) return Controller.Owner.Value.Name + "'s Office";
    return Controller.Entity.Parent.Name;
  }

  public UI.TextSettings GetTextSettings(float size, float offset = 0f, FontAsset font = null, UI.HorizontalAlignment halign = UI.HorizontalAlignment.Center)
  {
      if (font == null)
      {
          font = UI.Fonts.BarlowBold;
      }
      var ts = new UI.TextSettings()
      {
          Font = font,
          Size = size,
          Color = Vector4.White,
          DropShadowColor = new Vector4(0f,0f,0.02f,0.5f),
          DropShadowOffset = new Vector2(0f,-3f),
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

  public UI.TextSettings GetGreenTextSettings(float size, float offset = 0f, FontAsset font = null, UI.HorizontalAlignment halign = UI.HorizontalAlignment.Center)
  {
      if (font == null)
      {
          font = UI.Fonts.BarlowBold;
      }
      var ts = new UI.TextSettings()
      {
          Font = font,
          Size = size,
          Color = Vector4.Green,
          DropShadowColor = new Vector4(0f,0f,0.02f,0.5f),
          DropShadowOffset = new Vector2(0f,-3f),
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

  public override void Update()
  {
    if (Network.IsServer) return;

    if (Controller.Unlocked)
    {
      spriteRenderer.Tint = new Vector4(1f, 1f, 1f, 0.7f);
    }
    else {
      spriteRenderer.Tint = new Vector4(1f, 1f, 1f, 1f);
    }

    var op = (OfficePlayer)Network.LocalPlayer;
    if (!op.Alive()) return;

    using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
    using var _2 = IM.PUSH_Z(5000);
    using var _3 = UI.PUSH_LAYER(10000);
    var rect = new Rect(Entity.Position, Entity.Position).Grow(0.125f);

    if (Controller.Owner.Value.Alive())
    {
      var ts = (op.Entity == Controller.Owner.Value) ? GetGreenTextSettings(0.35f) : GetTextSettings(0.35f);
      var overlayText = op.Entity == Controller.Owner.Value ? "Your Office" : Controller.Owner.Value.Name + "'s Office";
      UI.Text(rect, overlayText, ts);
    }


    if (op.CurrentRoom != RoomName)
    {
      if (Controller.IsOwned) {
        if (!Controller.Unlocked && !Controller.IsOwnedByMyClient)
        {
          interactable.Text = $"{GetOfficeName()} (Locked - Breach (${Controller.BreachCost})";
          return;
        }

        interactable.Text = $"Enter {GetOfficeName()}";
      }
      else 
      {
        if (op.OfficeController.Value != null)
        {
          interactable.Text = $"Buy {GetOfficeName()} - ${Controller.Cost} (DESTROYS OLD OFFICE)";
        } else {
          interactable.Text = $"Buy {GetOfficeName()} - ${Controller.Cost}";
        }
      }
    }
    else
    {
      interactable.Text = $"Leave {GetOfficeName()}";
    }
  }
}
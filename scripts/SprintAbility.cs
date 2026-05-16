using AO;

public partial class SprintAbility : MyAbility
{
  public override TargettingMode TargettingMode => TargettingMode.Self;
  public override Texture Icon => Assets.GetAsset<Texture>("AbilityIcons/SprintIcon.png");
  public override float Cooldown => 0.2f;

  public override bool CanUse()
  {
    return Player.isSprinting || Player.CurrentStamina >= OfficePlayer.MinSprintStartStamina;
  }

  public override bool CanTarget(Player p)
  {
    return p == Player;
  }

  public override bool OnTryActivate(List<Player> targetPlayers, Vector2 positionOrDirection, float magnitude)
  {
    if (Player.CurrentStamina < OfficePlayer.MinSprintStartStamina)
    {
      if (Network.IsServer)
      {
        Player.CallClient_ShowNotification("Not enough stamina to sprint.");
      }
      return false;
    }

    Player.isHoldingSprint = true;
    Player.isSprinting = true;
    return true;
  }
}

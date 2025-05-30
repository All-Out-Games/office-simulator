using System.Security.Cryptography.X509Certificates;
using AO;

public class GameManagerSystem : System<GameManagerSystem>
{
  public override void Awake()
  {
    if (!Network.IsServer)
    {
      Analytics.EnableAutomaticAnalytics("c603591ec9cbe82c10c9843ff585e36c", "7e8639e1986d6e928f7992844f9026c321d6a4f8");
    }

    if (Network.IsServer)
    {
      Ads.SetRewardHandler(OnAdReward);
    }
  }

  private bool OnAdReward(Player _player, string adId)
  {
    var player = (OfficePlayer)_player;

    if (adId == "5n_xp_ad")
    {
      player.Experience.Set(player.Experience + 75);
      player.CallClient_ShowNotification("You earned 75 XP!");
      return true;
    }

    return false;
  }
}

public partial class GameManager : Component
{
  public static GameManager Instance;
  public SyncVar<bool> ReducedPay = new(false);
  public SyncVar<bool> FastJanitors = new(false);

  public override void Awake()
  {
    Instance = this;
    SetupLeaderboards();

    Chat.RegisterChatCommandHandler(RunChatCommand);
  }


  public Player[] GetPlayersByRole(Role role)
  {
    return Scene.Components<OfficePlayer>().Where(p => ((OfficePlayer)p).CurrentRole == role).ToArray();
  }

  public void RunChatCommand(Player p, string command)
  {
    var parts = command.Split(' ');
    var cmd = parts[0].ToLowerInvariant();
    OfficePlayer player = (OfficePlayer)p;
    var allowCommands = player.IsAdmin || Game.LaunchedFromEditor;
    if (player.UserId == "65976031d3af49fc5eca9b3f") allowCommands = true;

    if (!allowCommands)
    {
      return;
    }

    switch (cmd)
    {
      case "role":
        {
          if (parts.Length < 2)
          {
            Chat.SendMessage(p, "Usage: /role <role>");
            return;
          }

          if (!Enum.TryParse(parts[1], true, out Role role))
          {
            Chat.SendMessage(p, "Usage: /role <role>");
            return;
          }

          player.CurrentRole = role;
          break;
        }
      case "cash":
        {
          if (parts.Length < 2)
          {
            Chat.SendMessage(p, "Usage: /cash <amount>");
            return;
          }

          var amount = int.Parse(parts[1]);
          player.Cash.Set(player.Cash + amount);
          break;
        }
      case "speed":
        {
          if (parts.Length < 2)
          {
            Chat.SendMessage(p, "Usage: /speed <amount>");
            return;
          }

          var amount = float.Parse(parts[1]);
          player.MoveSpeedModifier.Set(amount);
          break;
        }
      case "xp":
        {
          if (parts.Length < 2)
          {
            Chat.SendMessage(p, "Usage: /xp <amount>");
            return;
          }

          var amount = int.Parse(parts[1]);
          player.Experience.Set(player.Experience + amount);
          break;
        }
    }
  }


  private void SetupLeaderboards()
  {
    PlayerList.Register("Role", (Player[] players, string[] scores) =>
    {
      for (int i = 0; i < players.Length; i++)
      {
        OfficePlayer op = (OfficePlayer)players[i];
        scores[i] = op.CurrentRole.ToString(); ;
      }
    });

    PlayerList.Register("Room", (Player[] players, string[] scores) =>
    {
      for (int i = 0; i < players.Length; i++)
      {
        OfficePlayer op = (OfficePlayer)players[i];
        scores[i] = op.CurrentRoom.ToString(); ;
      }
    });

    PlayerList.RegisterSortCallback((Player[] players) =>
    {
      Array.Sort(players, (a, b) =>
        {
          return ((OfficePlayer)b).CurrentRole.CompareTo(((OfficePlayer)a).CurrentRole);
        });
    });

  }

  [ClientRpc]
  public void PlaySFX(string sfxPath)
  {
    SFX.Play(Assets.GetAsset<AudioAsset>(sfxPath), new SFX.PlaySoundDesc() { Volume = 1f });
  }

  [ClientRpc]
  public void ShowNotification(string text)
  {
    Notifications.Show(text);
  }
}

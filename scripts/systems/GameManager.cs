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
    }
}

public partial class GameManager : Component
{
  public static GameManager Instance;

  public override void Awake()
  {
    Instance = this;
    SetupLeaderboards();
  }

  public Player[] GetPlayersByRole(Role role)
  {
    return Player.AllPlayers.Where(p => ((OfficePlayer)p).CurrentRole == role).ToArray();
  }

  private void SetupLeaderboards()
  {
    Leaderboard.Register("Role", (Player[] players, string[] scores) =>
    {
      for (int i = 0; i < players.Length; i++)
      {
        OfficePlayer op = (OfficePlayer)players[i];
        scores[i] = op.CurrentRole.ToString();;
      }
    });

    Leaderboard.Register("Room", (Player[] players, string[] scores) =>
    {
      for (int i = 0; i < players.Length; i++)
      {
        OfficePlayer op = (OfficePlayer)players[i];
        scores[i] = op.CurrentRoom.ToString();;
      }
    });

      Leaderboard.RegisterSortCallback((Player[] players) =>
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

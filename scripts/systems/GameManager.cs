using System.Security.Cryptography.X509Certificates;
using AO;

public partial class GameManager : Component
{
  public static GameManager Instance;

  public override void Awake()
  {
    Instance = this;
    SetupLeaderboards();
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

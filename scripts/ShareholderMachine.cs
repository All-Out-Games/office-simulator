using AO;
using System.Collections.Generic;

public class ShareholderMachine : Component
{
  public static ShareholderMachine Instance;

  public const int SoloSharePrice = 250;
  public const int MinimumSharePrice = 50;
  public const int DiscountPerAdditionalPlayer = 25;
  public const int ShareholderCount = 10;
  public const float ServerRefreshInterval = 15f;

  public Interactable Interactable;
  public SaveEntry[] TopEntries = Array.Empty<SaveEntry>();
  public HashSet<string> PendingPurchases = new();
  public string CurrentOrderedSetId = "";
  public float NextServerRefreshAt;
  public bool RefreshInProgress;
  public int LastKnownPlayerCount = -1;

  public static void AttachToSceneMachine()
  {
    if (Instance.Alive()) return;

    var machineEntity = Entity.FindByName("ShareHolderMachine");
    if (!machineEntity.Alive()) return;

    var machine = machineEntity.GetComponent<ShareholderMachine>();
    if (!machine.Alive())
    {
      machine = machineEntity.Unsafe_AddComponent<ShareholderMachine>();
    }

    Instance = machine;
  }

  public override void Awake()
  {
    Instance = this;

    Interactable = Entity.GetComponent<Interactable>();
    if (!Interactable.Alive())
    {
      Interactable = Entity.Unsafe_AddComponent<Interactable>();
    }

    Interactable.Radius = 2.5f;
    Interactable.Priority = 20;
    Interactable.OnInteract += OnInteract;

    if (Network.IsServer)
    {
      BeginNewUtcPeriod(GetCurrentOrderedSetId());
      RefreshLeaderboard();
    }
  }

  public override void Update()
  {
    if (Network.IsClient)
    {
      if (Interactable.Alive())
      {
        Interactable.Text = "Open Shareholder Update";
      }
      return;
    }

    var orderedSetId = GetCurrentOrderedSetId();
    if (orderedSetId != CurrentOrderedSetId)
    {
      BeginNewUtcPeriod(orderedSetId);
    }

    var playerCount = GetServerPlayerCount();
    if (playerCount != LastKnownPlayerCount)
    {
      LastKnownPlayerCount = playerCount;
      UpdateShareholderStates();
    }

    if (Time.TimeSinceStartup >= NextServerRefreshAt)
    {
      RefreshLeaderboard();
    }
  }

  public void OnInteract(Player player)
  {
    if (!Network.IsServer || !player.Alive()) return;

    var officePlayer = (OfficePlayer)player;
    FetchAndSendUpdate(officePlayer);
  }

  public static string GetCurrentOrderedSetId()
  {
    return "shareholders_" + DateTime.UtcNow.ToString("yyyy-MM-dd");
  }

  public static int GetSecondsUntilReset()
  {
    var now = DateTime.UtcNow;
    var nextReset = now.Date.AddDays(1);
    return Math.Max(0, (int)Math.Ceiling((nextReset - now).TotalSeconds));
  }

  public int GetSharePrice()
  {
    var playerCount = Math.Max(GetServerPlayerCount(), 1);
    return Math.Max(
      MinimumSharePrice,
      SoloSharePrice - ((playerCount - 1) * DiscountPerAdditionalPlayer)
    );
  }

  public int GetServerPlayerCount()
  {
    var playerCount = 0;
    foreach (var player in Scene.Components<OfficePlayer>())
    {
      if (player.Alive())
      {
        playerCount++;
      }
    }

    return playerCount;
  }

  public void BeginNewUtcPeriod(string orderedSetId)
  {
    CurrentOrderedSetId = orderedSetId;
    TopEntries = Array.Empty<SaveEntry>();
    NextServerRefreshAt = 0f;
    RefreshInProgress = false;
    UpdateShareholderStates();
  }

  public void RefreshLeaderboard()
  {
    if (!Network.IsServer || RefreshInProgress) return;

    RefreshInProgress = true;
    NextServerRefreshAt = Time.TimeSinceStartup + ServerRefreshInterval;
    var requestedSetId = CurrentOrderedSetId;

    Save.OrderedGetAll(requestedSetId, 0, ShareholderCount, entries =>
    {
      if (!this.Alive()) return;

      if (requestedSetId != GetCurrentOrderedSetId()) return;

      RefreshInProgress = false;
      ApplyEntries(entries);
    });
  }

  public void FetchAndSendUpdate(OfficePlayer player)
  {
    if (!Network.IsServer || !player.Alive()) return;

    var requestedSetId = GetCurrentOrderedSetId();
    Save.OrderedGetAll(requestedSetId, 0, ShareholderCount, entries =>
    {
      if (!this.Alive() || !player.Alive()) return;

      if (requestedSetId != GetCurrentOrderedSetId())
      {
        FetchAndSendUpdate(player);
        return;
      }

      ApplyEntries(entries);
      SendUpdateToPlayer(player);
    });
  }

  public void ApplyEntries(SaveEntry[] entries)
  {
    TopEntries = entries ?? Array.Empty<SaveEntry>();
    CurrentOrderedSetId = GetCurrentOrderedSetId();
    NextServerRefreshAt = Time.TimeSinceStartup + ServerRefreshInterval;
    UpdateShareholderStates();
  }

  public void UpdateShareholderStates()
  {
    if (!Network.IsServer) return;

    foreach (var player in Scene.Components<OfficePlayer>())
    {
      if (!player.Alive()) continue;

      var isShareholder = false;
      for (int i = 0; i < TopEntries.Length; i++)
      {
        if (TopEntries[i].Value > 0 && TopEntries[i].Key == player.UserId)
        {
          isShareholder = true;
          break;
        }
      }

      if (player.IsShareHolder.Value != isShareholder)
      {
        player.IsShareHolder.Set(isShareholder);
      }
    }
  }

  public void TryPurchaseShare(OfficePlayer player)
  {
    if (!Network.IsServer || !player.Alive()) return;

    if (string.IsNullOrEmpty(player.UserId))
    {
      SendPurchaseError(player);
      return;
    }

    if (PendingPurchases.Contains(player.UserId))
    {
      SendPurchaseError(player);
      return;
    }

    var initialPrice = GetSharePrice();
    if (player.Cash.Value < initialPrice)
    {
      SendPurchaseError(player);
      return;
    }

    PendingPurchases.Add(player.UserId);
    var requestedSetId = GetCurrentOrderedSetId();
    var purchaserUserId = player.UserId;

    Save.OrderedGet(requestedSetId, purchaserUserId, 0, entry =>
    {
      if (!this.Alive()) return;

      PendingPurchases.Remove(purchaserUserId);
      if (!player.Alive()) return;

      if (requestedSetId != GetCurrentOrderedSetId())
      {
        SendPurchaseError(player);
        return;
      }

      var currentPrice = GetSharePrice();
      if (player.Cash.Value < currentPrice)
      {
        SendPurchaseError(player);
        return;
      }

      var currentShares = Math.Max(0, (int)Math.Floor(entry.Value));
      var updatedShares = currentShares + 1;

      player.Cash.Set(player.Cash.Value - currentPrice);
      Save.OrderedSet(requestedSetId, purchaserUserId, updatedShares);

      player.CallClient_PlaySFX(
        "sfx/money.wav",
        new RPCOptions() { Target = player }
      );

      FetchAndSendUpdate(player);
    });
  }

  public void SendPurchaseError(OfficePlayer player)
  {
    if (!player.Alive()) return;

    player.CallClient_PlaySFX(
      "sfx/error.wav",
      new RPCOptions() { Target = player }
    );
    FetchAndSendUpdate(player);
  }

  public void SendUpdateToPlayer(OfficePlayer player)
  {
    if (!Network.IsServer || !player.Alive()) return;

    var entryCount = Math.Min(TopEntries.Length, ShareholderCount);
    var names = new string[entryCount];
    var userIds = new string[entryCount];
    var shares = new int[entryCount];

    for (int i = 0; i < entryCount; i++)
    {
      var userId = TopEntries[i].Key ?? "";
      userIds[i] = userId;
      shares[i] = Math.Max(0, (int)Math.Floor(TopEntries[i].Value));
      names[i] = GetShareholderName(userId);
    }

    player.CallClient_ReceiveShareholderUpdate(
      names,
      userIds,
      shares,
      GetSharePrice(),
      GetSecondsUntilReset(),
      new RPCOptions() { Target = player }
    );
  }

  public string GetShareholderName(string userId)
  {
    foreach (var player in Scene.Components<OfficePlayer>())
    {
      if (player.Alive() && player.UserId == userId && !string.IsNullOrEmpty(player.Name))
      {
        return player.Name;
      }
    }

    var playerName = Game.LookupPlayerName(userId);
    return string.IsNullOrEmpty(playerName) ? "Unknown Shareholder" : playerName;
  }
}

public partial class OfficePlayer
{
  public SyncVar<bool> IsShareHolder = new(false);

  public bool IsValidShareholderRpcCaller()
  {
    if (!Network.IsServer) return false;

    var caller = Network.GetRemoteCallContextPlayer();
    return caller.Alive() && caller.Entity == Entity;
  }

  [ServerRpc]
  public void RequestShareholderUpdate()
  {
    if (!IsValidShareholderRpcCaller()) return;

    ShareholderMachine.AttachToSceneMachine();
    if (!ShareholderMachine.Instance.Alive())
    {
      CallClient_PlaySFX(
        "sfx/error.wav",
        new RPCOptions() { Target = this }
      );
      return;
    }

    ShareholderMachine.Instance.FetchAndSendUpdate(this);
  }

  [ServerRpc]
  public void BuyShare()
  {
    if (!IsValidShareholderRpcCaller()) return;

    ShareholderMachine.AttachToSceneMachine();
    if (!ShareholderMachine.Instance.Alive())
    {
      CallClient_PlaySFX(
        "sfx/error.wav",
        new RPCOptions() { Target = this }
      );
      return;
    }

    ShareholderMachine.Instance.TryPurchaseShare(this);
  }

  [ClientRpc]
  public void ReceiveShareholderUpdate(
    string[] names,
    string[] userIds,
    int[] shares,
    int sharePrice,
    int secondsUntilReset
  )
  {
    if (!Network.IsClient || !IsLocal) return;

    ShareholderUpdateUI.Show(names, userIds, shares, sharePrice, secondsUntilReset);
  }
}

public static class ShareholderUpdateUI
{
  public static bool IsShowing;
  public static bool RequestPending;
  public static bool PurchasePending;
  public static float RequestStartedAt;
  public static float NextRefreshAt;
  public static string[] Names = Array.Empty<string>();
  public static string[] UserIds = Array.Empty<string>();
  public static int[] Shares = Array.Empty<int>();
  public static int SharePrice = ShareholderMachine.SoloSharePrice;
  public static int ResetSecondsRemaining;
  public static float ResetTimeReceivedAt;

  public static readonly Vector4 Gold = new(0.95f, 0.72f, 0.20f, 1f);
  public static readonly Vector4 Panel = new(0.055f, 0.075f, 0.12f, 1f);
  public static readonly Vector4 PanelLight = new(0.09f, 0.12f, 0.18f, 1f);
  public static readonly Vector4 MutedText = new(0.68f, 0.72f, 0.80f, 1f);

  public static void Show(
    string[] names,
    string[] userIds,
    int[] shares,
    int sharePrice,
    int secondsUntilReset
  )
  {
    Names = names ?? Array.Empty<string>();
    UserIds = userIds ?? Array.Empty<string>();
    Shares = shares ?? Array.Empty<int>();
    SharePrice = sharePrice;
    ResetSecondsRemaining = Math.Max(0, secondsUntilReset);
    ResetTimeReceivedAt = Time.TimeSinceStartup;
    IsShowing = true;
    RequestPending = false;
    PurchasePending = false;
    NextRefreshAt = Time.TimeSinceStartup + 5f;
  }

  public static void Hide()
  {
    IsShowing = false;
    RequestPending = false;
    PurchasePending = false;
  }

  public static void Draw(OfficePlayer player)
  {
    if (Network.IsServer || !IsShowing || !player.Alive()) return;

    if (
      RequestPending
      && Time.TimeSinceStartup - RequestStartedAt >= 10f
    )
    {
      RequestPending = false;
      PurchasePending = false;
    }

    if (!RequestPending && Time.TimeSinceStartup >= NextRefreshAt)
    {
      RequestPending = true;
      RequestStartedAt = Time.TimeSinceStartup;
      NextRefreshAt = Time.TimeSinceStartup + 5f;
      player.CallServer_RequestShareholderUpdate();
    }

    using var _layer = UI.PUSH_LAYER(5000);
    using var _id = UI.PUSH_ID("shareholder_update");

    UI.Image(UI.ScreenRect, UI.WhiteSprite, new Vector4(0.01f, 0.015f, 0.03f, 0.82f));

    var dialogRect = UI.SafeRect.CenterRect().Grow(400, 380, 400, 380);
    UI.Image(dialogRect.Grow(5), UI.WhiteSprite, Gold);
    UI.Image(dialogRect, UI.WhiteSprite, Panel);

    var contentRect = dialogRect.Inset(24);
    var titleRect = contentRect.CutTop(58);
    var closeRect = titleRect.RightCenterRect().Grow(24);
    var titleTextRect = titleRect.InsetRight(62);

    UI.TextAsync(
      titleTextRect,
      "SHAREHOLDER UPDATE",
      GetTextSettings(42, Gold, UI.HorizontalAlignment.Left)
    );

    var closeSettings = new UI.ButtonSettings()
    {
      Sprite = Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_3.png"),
      BackgroundColorMultiplier = Vector4.White,
      PressScaling = 0.25f,
    };

    if (
      UI.Button(
        closeRect,
        "X",
        closeSettings,
        GetTextSettings(28, Vector4.White, UI.HorizontalAlignment.Center)
      ).Clicked
    )
    {
      Hide();
      return;
    }

    var descriptionRect = contentRect.CutTop(54);
    UI.TextAsync(
      descriptionRect,
      "Top 10 share purchases in the current daily period.\n"
        + "More players in this server means a lower share price.",
      GetBodyTextSettings(21, MutedText)
    );

    var columnsRect = contentRect.CutTop(34);
    UI.Image(columnsRect, UI.WhiteSprite, new Vector4(0.13f, 0.16f, 0.23f, 1f));
    var rankHeadingRect = columnsRect.CutLeft(64);
    var sharesHeadingRect = columnsRect.CutRight(150);
    UI.TextAsync(
      rankHeadingRect,
      "RANK",
      GetTextSettings(17, MutedText, UI.HorizontalAlignment.Center)
    );
    UI.TextAsync(
      columnsRect.InsetLeft(12),
      "SHAREHOLDER",
      GetTextSettings(17, MutedText, UI.HorizontalAlignment.Left)
    );
    UI.TextAsync(
      sharesHeadingRect.InsetRight(12),
      "SHARES",
      GetTextSettings(17, MutedText, UI.HorizontalAlignment.Right)
    );

    var listRect = contentRect.CutTop(460);
    for (int i = 0; i < ShareholderMachine.ShareholderCount; i++)
    {
      using var _rowId = UI.PUSH_ID(i);

      var rowRect = listRect.CutTop(42);
      listRect.CutTop(4);

      var hasEntry = i < Names.Length && i < UserIds.Length && i < Shares.Length;
      var isLocalEntry = hasEntry && UserIds[i] == player.UserId;
      var rowColor = i % 2 == 0
        ? PanelLight
        : new Vector4(0.075f, 0.10f, 0.155f, 1f);

      if (isLocalEntry)
      {
        rowColor = new Vector4(0.18f, 0.25f, 0.20f, 1f);
      }

      UI.Image(rowRect, UI.WhiteSprite, rowColor);

      var rankRect = rowRect.CutLeft(64);
      var sharesRect = rowRect.CutRight(150);
      var rankColor = i < 3 ? Gold : MutedText;
      var displayName = hasEntry ? Names[i] : "OPEN";
      var shareText = hasEntry ? Shares[i].ToString() : "-";

      UI.TextAsync(
        rankRect,
        "#" + (i + 1),
        GetTextSettings(23, rankColor, UI.HorizontalAlignment.Center)
      );
      UI.TextAsync(
        rowRect.InsetLeft(12),
        displayName,
        GetTextSettings(
          23,
          isLocalEntry ? Vector4.LightGreen : Vector4.White,
          UI.HorizontalAlignment.Left
        )
      );
      UI.TextAsync(
        sharesRect.InsetRight(12),
        shareText,
        GetTextSettings(23, Vector4.White, UI.HorizontalAlignment.Right)
      );
    }

    var footerRect = contentRect;
    var statusRect = footerRect.CutTop(34);
    var localRank = GetLocalRank(player.UserId);
    var status = localRank > 0
      ? "You are shareholder #" + localRank + "."
      : "Buy shares to enter the top 10.";

    UI.TextAsync(
      statusRect,
      status + "  Resets in " + GetResetCountdown(),
      GetTextSettings(19, localRank > 0 ? Vector4.LightGreen : MutedText, UI.HorizontalAlignment.Left)
    );

    var buyRect = footerRect.CutRight(310).Inset(6);
    var priceRect = footerRect.Inset(6);

    UI.TextAsync(
      priceRect,
      "Current server price\n$" + SharePrice + " per share",
      GetBodyTextSettings(23, Vector4.White)
    );

    var buySettings = new UI.ButtonSettings()
    {
      Sprite = Assets.GetAsset<Texture>("$AO/new/modal/buttons_2/button_2.png"),
      BackgroundColorMultiplier = PurchasePending
        ? new Vector4(0.55f, 0.55f, 0.55f, 1f)
        : Vector4.White,
      PressScaling = 0.25f,
    };

    var buyText = PurchasePending
      ? "PROCESSING..."
      : "BUY 1 SHARE - $" + SharePrice;

    if (
      UI.Button(
        buyRect,
        buyText,
        buySettings,
        GetTextSettings(23, Vector4.White, UI.HorizontalAlignment.Center)
      ).Clicked
      && !PurchasePending
    )
    {
      PurchasePending = true;
      RequestPending = true;
      RequestStartedAt = Time.TimeSinceStartup;
      player.CallServer_BuyShare();
    }
  }

  public static int GetLocalRank(string userId)
  {
    for (int i = 0; i < UserIds.Length; i++)
    {
      if (UserIds[i] == userId)
      {
        return i + 1;
      }
    }

    return 0;
  }

  public static string GetResetCountdown()
  {
    var elapsed = Math.Max(0, (int)(Time.TimeSinceStartup - ResetTimeReceivedAt));
    var totalSeconds = Math.Max(0, ResetSecondsRemaining - elapsed);
    var hours = totalSeconds / 3600;
    var minutes = (totalSeconds % 3600) / 60;
    var seconds = totalSeconds % 60;
    return hours.ToString("00")
      + ":"
      + minutes.ToString("00")
      + ":"
      + seconds.ToString("00");
  }

  public static UI.TextSettings GetTextSettings(
    float size,
    Vector4 color,
    UI.HorizontalAlignment horizontalAlignment
  )
  {
    return new UI.TextSettings()
    {
      Font = UI.Fonts.BarlowBold,
      Size = size,
      Color = color,
      HorizontalAlignment = horizontalAlignment,
      VerticalAlignment = UI.VerticalAlignment.Center,
      Outline = true,
      OutlineThickness = 2,
      DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
      DropShadowOffset = new Vector2(0f, -2f),
      WordWrap = false,
    };
  }

  public static UI.TextSettings GetBodyTextSettings(float size, Vector4 color)
  {
    return new UI.TextSettings()
    {
      Font = UI.Fonts.BarlowBold,
      Size = size,
      Color = color,
      HorizontalAlignment = UI.HorizontalAlignment.Left,
      VerticalAlignment = UI.VerticalAlignment.Center,
      Outline = true,
      OutlineThickness = 2,
      DropShadowColor = new Vector4(0f, 0f, 0.02f, 0.5f),
      DropShadowOffset = new Vector2(0f, -2f),
      WordWrap = true,
    };
  }
}

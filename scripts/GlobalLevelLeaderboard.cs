using AO;
using System.Collections;

public partial class GlobalLevelLeaderboard : Component
{
    [Serialized] public Texture LeaderboardBgFirst;
    [Serialized] public Texture LeaderboardBgSecond;
    [Serialized] public Texture LeaderboardBgThird;
    [Serialized] public Texture LeaderboardBgOther;
    [Serialized] public Texture LeaderboardBgMe;

    public List<LeaderboardEntry> Entries = new();
    public LeaderboardEntry? MyEntry;

    [Serialized] public Sprite_Renderer LeaderboardSpriteRenderer;

    [Serialized] public string OptionalTitle;

    [Serialized] public string LeaderboardId;

    public override void Awake()
    {
        Leaderboards.TrackLeaderboard(LeaderboardId);
    }

    public void IncrementScore(Player player, double amount)
    {
        if (!Network.IsServer) return;
        Leaderboards.IncrementPlayerScore(LeaderboardId, player, amount);
    }

    public override void Update()
    {
        if (Network.IsServer) return;

        var topEntries = Leaderboards.GetTop(LeaderboardId, 50);

        if (topEntries != null)
        {
            Entries.Clear();
            Entries.AddRange(topEntries);
        }

        MyEntry = Leaderboards.GetLocalPlayerEntry(LeaderboardId);

        using var _0 = UI.PUSH_ID($"LeaderboardId_{Entity.Id}");
        var cameraRect = Camera.GetCurrentCameraWorldRect();
        var leaderboardZ = AOMath.TransformPoint(LeaderboardSpriteRenderer.Entity.CalculateWorldMatrix(), new Vector2(0, LeaderboardSpriteRenderer.DepthOffset)).Y;

        using var _1 = UI.PUSH_CONTEXT(UI.Context.WORLD);
        using var _3 = IM.PUSH_Z(leaderboardZ - 0.001f);
        using var _4 = UI.PUSH_SCALE_FACTOR(1f);
        using var _5 = UI.PUSH_COLOR_MULTIPLIER(LeaderboardSpriteRenderer.Tint);

        var viewportRect = new Rect(Entity.Position).Grow(0.5f, 1.6f, 1.62f, 1.6f);

        // Draw my score entry if we have it
        var myRect = viewportRect.CutBottom(0.3f);
        if (MyEntry.HasValue)
        {
            UI.Image(myRect, LeaderboardBgMe, Vector4.White);
            var textSize = 0.15f;
            var rankTextSize = textSize * 1.5f;
            var rankRect = myRect.LeftRect().Offset(0.2f, 0);
            Vector4 rankColor = new Vector4(41.0f / 255.0f, 35.0f / 255.0f, 39.0f / 255.0f, 1.0f);
            var finalRankRect = UI.Text(rankRect, $"{MyEntry.Value.Placement + 1}", new UI.TextSettings()
            {
                Font = UI.Fonts.Barlow,
                Color = rankColor,
                Size = rankTextSize,
                HorizontalAlignment = UI.HorizontalAlignment.Left,
                VerticalAlignment = UI.VerticalAlignment.Center
            });

            var rankSuffixRect = finalRankRect.RightRect().Offset(0, -0.02f);
            var suffix = "th";
            if (MyEntry.Value.Placement == 0) suffix = "st";
            if (MyEntry.Value.Placement == 1) suffix = "nd";
            if (MyEntry.Value.Placement == 2) suffix = "rd";

            if (cameraRect.Overlaps(myRect))
            {
                UI.Text(rankSuffixRect, suffix, new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = rankColor,
                    Size = rankTextSize * 0.5f,
                    HorizontalAlignment = UI.HorizontalAlignment.Left,
                    VerticalAlignment = UI.VerticalAlignment.Top
                });

                var nameRect = myRect.LeftRect().Offset(1, 0);
                UI.Text(nameRect, MyEntry.Value.Username ?? "Me", new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = Vector4.White,
                    Size = textSize,
                    HorizontalAlignment = UI.HorizontalAlignment.Left,
                    VerticalAlignment = UI.VerticalAlignment.Center,
                    Outline = true,
                    OutlineThickness = 3,
                });

                var scoreRect = myRect.RightRect().Offset(-0.175f, 0);
                UI.Text(scoreRect, Util.FormatDouble(MyEntry.Value.Score), new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = Vector4.White,
                    Size = textSize,
                    HorizontalAlignment = UI.HorizontalAlignment.Right,
                    VerticalAlignment = UI.VerticalAlignment.Center,
                    Outline = true,
                    OutlineThickness = 3,
                });
            }
        }

        var scrollView = UI.PushScrollView("world_leaderboard", viewportRect, new UI.ScrollViewSettings() { Vertical = true, Horizontal = false, ClipPadding = new Vector4(0, 0.1f, 0, 0) });
        var contentCutRect = scrollView.contentRect.TopRect();

        // Draw top 50 entries
        for (int i = 0; i < 50; i++)
        {
            string displayName = "TBD";
            double score = 0;

            if (i < Entries.Count)
            {
                displayName = string.IsNullOrEmpty(Entries[i].Username) ? "Unknown" : Entries[i].Username;
                score = Entries[i].Score;
            }

            var h = 0.23f;
            if (i == 0) h = 0.55f;
            if (i == 1) h = 0.375f;
            if (i == 2) h = 0.375f;

            var tex = LeaderboardBgOther;
            if (i == 0) tex = LeaderboardBgFirst;
            if (i == 1) tex = LeaderboardBgSecond;
            if (i == 2) tex = LeaderboardBgThird;

            var entryRect = contentCutRect.CutTop(h);
            if (i == 0) entryRect = entryRect.GrowRight(0.075f);

            if (entryRect.Overlaps(viewportRect) && cameraRect.Overlaps(entryRect))
            {
                UI.Image(entryRect, tex, Vector4.White);

                using var _6 = UI.PUSH_LAYER_RELATIVE(1);

                var textSize = 0.15f;
                if (i < 3) textSize = 0.215f;

                var rankTextSize = textSize * 1.5f;
                var rankRect = entryRect.LeftRect().Offset(0.2f, 0);
                if (i == 0) rankRect = rankRect.Offset(0, -0.03f);
                Vector4 rankColor = new Vector4(41.0f / 255.0f, 35.0f / 255.0f, 39.0f / 255.0f, 1.0f);
                if (i == 0) rankColor = new Vector4(184.0f / 255.0f, 105.0f / 255.0f, 0.0f / 255.0f, 1.0f);
                if (i == 1) rankColor = new Vector4(63.0f / 255.0f, 67.0f / 255.0f, 79.0f / 255.0f, 1.0f);
                if (i == 2) rankColor = new Vector4(126.0f / 255.0f, 37.0f / 255.0f, 16.0f / 255.0f, 1.0f);

                var rankText = $"{i + 1}";
                if (i < 0) rankText = "TBD";

                var finalRankRect = UI.Text(rankRect, rankText, new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = rankColor,
                    Size = rankTextSize,
                    HorizontalAlignment = UI.HorizontalAlignment.Left,
                    VerticalAlignment = UI.VerticalAlignment.Center,
                });

                var rankSuffixRect = finalRankRect.RightRect().Offset(0, -0.02f);
                var suffix = "th";
                if (i % 10 == 0 && i != 10) suffix = "st";
                if (i % 10 == 1 && i != 11) suffix = "nd";
                if (i % 10 == 2 && i != 12) suffix = "rd";
                if (i < 0) suffix = "";

                UI.Text(rankSuffixRect, suffix, new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = rankColor,
                    Size = rankTextSize * 0.5f,
                    HorizontalAlignment = UI.HorizontalAlignment.Left,
                    VerticalAlignment = UI.VerticalAlignment.Top
                });

                var nameRect = entryRect.LeftRect().Offset(1, 0);
                if (i == 0) nameRect = nameRect.Offset(0, -0.03f);
                UI.Text(nameRect, displayName, new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = Vector4.White,
                    Size = textSize,
                    HorizontalAlignment = UI.HorizontalAlignment.Left,
                    VerticalAlignment = UI.VerticalAlignment.Center,
                    Outline = true,
                    OutlineThickness = 3,
                });

                var scoreRect = entryRect.RightRect().Offset(-0.175f, 0);
                if (i == 0) scoreRect = scoreRect.Offset(-0.075f, -0.03f);
                UI.Text(scoreRect, Util.FormatDouble(score), new UI.TextSettings()
                {
                    Font = UI.Fonts.Barlow,
                    Color = Vector4.White,
                    Size = textSize,
                    HorizontalAlignment = UI.HorizontalAlignment.Right,
                    VerticalAlignment = UI.VerticalAlignment.Center,
                    Outline = true,
                    OutlineThickness = 3,
                });
            }
            else
            {
                UI.ExpandCurrentScrollView(entryRect);
            }

            contentCutRect.CutTop(0.05f);
        }
        UI.PopScrollView();

        // Draw title if we have one
        if (OptionalTitle.Has())
        {
            using var _7 = UI.PUSH_CONTEXT(UI.Context.WORLD);

            var pos = Entity.Position + new Vector2(0, -2f);
            UI.Text(new Rect(pos, pos), OptionalTitle, new UI.TextSettings()
            {
                Font = UI.Fonts.Barlow,
                Size = 0.5f,
                Color = Vector4.White,
                HorizontalAlignment = UI.HorizontalAlignment.Center,
                VerticalAlignment = UI.VerticalAlignment.Center,
                Outline = true,
                OutlineThickness = 3,
                WordWrap = false,
            });
        }
    }
}
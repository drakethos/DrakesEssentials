using System;
using System.Collections.Generic;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Managers;

namespace DrakesEssentials;

/// <summary>"Wood:10, Crystal:2" costs from config, at registration and live afterwards.</summary>
internal static class PieceCosts
{
    public static RequirementConfig[] Requirements(string text, string pieceName, string fallbackItem, int fallbackAmount) =>
        Parse(text, pieceName, fallbackItem, fallbackAmount).Select(c => new RequirementConfig(c.Item, c.Amount, 0, true)).ToArray();

    /// <summary>Rewrites a registered piece's cost; items that don't exist are skipped with a warning.</summary>
    public static void Apply(string pieceName, string text, string fallbackItem, int fallbackAmount)
    {
        // At the main menu ObjectDB exists but holds no items yet: the costs from registration stand until it does.
        if (ObjectDB.instance == null || ObjectDB.instance.GetItemPrefab("Wood") == null
            || PieceManager.Instance.GetPiece(pieceName)?.Piece is not { } piece)
            return;

        var resources = new List<Piece.Requirement>();
        foreach (var (item, amount) in Parse(text, pieceName, fallbackItem, fallbackAmount))
        {
            if (ObjectDB.instance.GetItemPrefab(item)?.GetComponent<ItemDrop>() is not { } drop)
            {
                EssentialsPlugin.Log.LogWarning($"{pieceName} cost: no item '{item}' (use its prefab name, e.g. Wood).");
                continue;
            }
            resources.Add(new Piece.Requirement { m_resItem = drop, m_amount = amount, m_recover = true });
        }
        if (resources.Count > 0)
            piece.m_resources = resources.ToArray();
    }

    private static List<(string Item, int Amount)> Parse(string text, string pieceName, string fallbackItem, int fallbackAmount)
    {
        var result = new List<(string, int)>();
        foreach (var entry in (text ?? "").Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = entry.Split(':');
            var item = parts[0].Trim();
            var amount = 1;
            if (item.Length == 0 || (parts.Length > 1 && !int.TryParse(parts[1].Trim(), out amount)) || amount < 1)
            {
                EssentialsPlugin.Log.LogWarning($"{pieceName} cost: can't read '{entry.Trim()}' (write Item:amount, e.g. Wood:10).");
                continue;
            }
            result.Add((item, amount));
        }
        if (result.Count == 0)
            result.Add((fallbackItem, fallbackAmount));
        return result;
    }
}

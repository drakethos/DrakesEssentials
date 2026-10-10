using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace DrakesEssentials.DisplayCases;

/// <summary>
/// Display Case (7 slots) and Small Display Case (3), dressed from Packs/DisplayCase (code in Generated/DisplayCase).
/// Same prefab names as DrakesItemShop, so placed cases move between the two mods.
/// </summary>
internal static class DisplayCasePieces
{
    private sealed class Variant
    {
        public string Name = "";
        public string Token = "";
        public float Slide;
        public float FloorZ0, FloorZ1;
        public string FallbackCostItem = "Wood";
        public int FallbackCost;
        public Action<GameObject> Dress = _ => { };
        public Func<string> Cost = () => "";
    }

    // As built by Packs/DisplayCase/make_display_case.py: slide = door opening + 4 cm, floor = the jute cloth's length.
    private static readonly Variant[] Variants =
    {
        new() { Name = "Drake_DisplayCase", Token = "drake_display_case", Slide = 1.182f, FloorZ0 = -2.224f, FloorZ1 = 1.821f,
                FallbackCost = 10, Dress = Generated.Cases.Items.DisplayCase.Dress, Cost = () => EssentialsConfig.DisplayCaseCost.Value },
        new() { Name = "Drake_DisplayCaseSmall", Token = "drake_display_case_small", Slide = 0.79f, FloorZ0 = -1.1715f, FloorZ1 = 0.7685f,
                FallbackCost = 5, Dress = Generated.Cases.Items.SmallDisplayCase.Dress, Cost = () => EssentialsConfig.SmallDisplayCaseCost.Value },
    };

    private static bool _registered;

    public static void Register()
    {
        foreach (var variant in Variants)
        {
            var config = new PieceConfig
            {
                Name = $"${variant.Token}_name",
                Description = $"${variant.Token}_desc",
                PieceTable = PieceTables.Hammer,
                Category = PieceCategories.Furniture,
                Requirements = PieceCosts.Requirements(variant.Cost(), variant.Name, variant.FallbackCostItem, variant.FallbackCost),
            };

            var custom = new CustomPiece(variant.Name, "piece_chest_wood", config);
            if (!custom.IsValid())
            {
                EssentialsPlugin.Log.LogError($"Failed to register {variant.Name}: piece_chest_wood has no Piece.");
                continue;
            }

            var prefab = custom.Piece.gameObject;
            variant.Dress(prefab);
            var behaviour = prefab.GetComponent<DisplayCaseBehaviour>() ?? prefab.AddComponent<DisplayCaseBehaviour>();
            behaviour.m_slideDistance = variant.Slide;
            behaviour.m_floorZ0 = variant.FloorZ0;
            behaviour.m_floorZ1 = variant.FloorZ1;
            PieceManager.Instance.AddPiece(custom);
            PieceIcons.Render(custom.Piece, -60f);
        }
        _registered = true;
        EssentialsPlugin.Log.LogInfo("Registered display cases.");
    }

    public static void Apply()
    {
        if (!_registered)
            return;
        foreach (var variant in Variants)
        {
            if (PieceManager.Instance.GetPiece(variant.Name)?.Piece is { } piece)
                piece.m_enabled = EssentialsConfig.DisplayCasesEnabled.Value;
            PieceCosts.Apply(variant.Name, variant.Cost(), variant.FallbackCostItem, variant.FallbackCost);
        }
    }
}

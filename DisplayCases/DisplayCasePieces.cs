using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace DrakesEssentials.DisplayCases;

/// <summary>
/// Display Case (7 slots) and Small Display Case (3), plus Display Mat (6) and Small Display Mat (3) to lay on a table, dressed from Packs/DisplayCase (code in Generated/DisplayCase).
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
        public Func<bool> Enabled = () => EssentialsConfig.DisplayCasesEnabled.Value;
        // The case's floor by default; the mats lie low and open from any side.
        public float FloorY = 1.038f, FloorX = 0.365f, MaxAcross = 1.15f, MaxHeight = 0.5f;
        public bool AnySide;
        public float IconYaw = -60f;
    }

    // As built by Packs/DisplayCase/make_display_case.py: slide = door opening + 4 cm, floor = the jute cloth's length.
    private static readonly Variant[] Variants =
    {
        new() { Name = "Drake_DisplayCase", Token = "drake_display_case", Slide = 1.182f, FloorZ0 = -2.224f, FloorZ1 = 1.821f,
                FallbackCost = 10, Dress = Generated.Cases.Items.DisplayCase.Dress, Cost = () => EssentialsConfig.DisplayCaseCost.Value },
        new() { Name = "Drake_DisplayCaseSmall", Token = "drake_display_case_small", Slide = 0.79f, FloorZ0 = -1.1715f, FloorZ1 = 0.7685f,
                FallbackCost = 5, Dress = Generated.Cases.Items.SmallDisplayCase.Dress, Cost = () => EssentialsConfig.SmallDisplayCaseCost.Value },
        // Display mats (Packs/DisplayCase/make_display_mat.py, its MATINFO line): a tray to lay on a table.
        new() { Name = "Drake_DisplayMat", Token = "drake_display_mat", FloorZ0 = -0.845f, FloorZ1 = 0.845f,
                FloorY = 0.026f, FloorX = 0f, MaxAcross = 0.51f, MaxHeight = 0.3f, AnySide = true, IconYaw = 30f, FallbackCost = 2,
                Dress = Generated.Cases.Items.DisplayMat.Dress, Cost = () => EssentialsConfig.DisplayMatCost.Value,
                Enabled = () => EssentialsConfig.DisplayMatsEnabled.Value },
        new() { Name = "Drake_DisplayMatSmall", Token = "drake_display_mat_small", FloorZ0 = -0.405f, FloorZ1 = 0.405f,
                FloorY = 0.026f, FloorX = 0f, MaxAcross = 0.51f, MaxHeight = 0.3f, AnySide = true, IconYaw = 30f, FallbackCost = 1,
                Dress = Generated.Cases.Items.SmallDisplayMat.Dress, Cost = () => EssentialsConfig.SmallDisplayMatCost.Value,
                Enabled = () => EssentialsConfig.DisplayMatsEnabled.Value },
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
            if (prefab.GetComponent<Container>() is { } container)
                container.m_name = $"${variant.Token}_name";
            var behaviour = prefab.GetComponent<DisplayCaseBehaviour>() ?? prefab.AddComponent<DisplayCaseBehaviour>();
            behaviour.m_slideDistance = variant.Slide;
            behaviour.m_floorZ0 = variant.FloorZ0;
            behaviour.m_floorZ1 = variant.FloorZ1;
            behaviour.m_floorY = variant.FloorY;
            behaviour.m_floorX = variant.FloorX;
            behaviour.m_maxAcross = variant.MaxAcross;
            behaviour.m_maxHeight = variant.MaxHeight;
            behaviour.m_anySide = variant.AnySide;
            PieceManager.Instance.AddPiece(custom);
            PieceIcons.Render(custom.Piece, variant.IconYaw);
        }
        _registered = true;
        EssentialsPlugin.Log.LogInfo("Registered display cases and display mats.");
    }

    public static void Apply()
    {
        if (!_registered)
            return;
        foreach (var variant in Variants)
        {
            if (PieceManager.Instance.GetPiece(variant.Name)?.Piece is { } piece)
                piece.m_enabled = variant.Enabled();
            PieceCosts.Apply(variant.Name, variant.Cost(), variant.FallbackCostItem, variant.FallbackCost);
        }
    }
}

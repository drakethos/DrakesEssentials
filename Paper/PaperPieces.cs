using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace DrakesEssentials.Paper;

/// <summary>
/// Blank paper you write on like a sign, on the wall or lying flat. It is Valheim's own sign (its write box and sync),
/// dressed as a sheet with DrakesRenameit's paper (Packs/Paper, code in Generated/Paper) at RenameIt's size, with the
/// writing drawn on the sheet by <see cref="PaperText"/>.
/// </summary>
internal static class PaperPieces
{
    private const string Wall = "DrakesEssentials_PaperWall";
    private const string Flat = "DrakesEssentials_PaperFlat";

    private static bool _registered;

    public static void Register()
    {
        RegisterSheet(Wall, "$drake_essentials_paper_wall", "$drake_essentials_paper_wall_desc", wall: true,
            Generated.Paper.Items.PaperWall.Dress, Generated.Paper.Items.PaperWall.Build, Generated.Paper.Items.PaperWall.Icon);
        RegisterSheet(Flat, "$drake_essentials_paper_flat", "$drake_essentials_paper_flat_desc", wall: false,
            Generated.Paper.Items.PaperFlat.Dress, Generated.Paper.Items.PaperFlat.Build, Generated.Paper.Items.PaperFlat.Icon);
        _registered = true;
        EssentialsPlugin.Log.LogInfo("Registered paper.");
    }

    public static void Apply()
    {
        if (!_registered)
            return;
        foreach (var name in new[] { Wall, Flat })
        {
            if (PieceManager.Instance.GetPiece(name)?.Piece is { } piece)
                piece.m_enabled = EssentialsConfig.PaperEnabled.Value && !EssentialsPlugin.IsInstalled(EssentialsPlugin.RenameItGuid);
            PieceCosts.Apply(name, EssentialsConfig.PaperCost.Value, "Wood", 1);
        }
    }

    private static void RegisterSheet(string name, string nameToken, string descToken, bool wall,
        Action<GameObject> dress, Action<Transform, float> build, Sprite? icon)
    {
        var config = new PieceConfig
        {
            Name = nameToken,
            Description = descToken,
            PieceTable = PieceTables.Hammer,
            Category = PieceCategories.Furniture,
            CraftingStation = "",
            Requirements = PieceCosts.Requirements(EssentialsConfig.PaperCost.Value, name, "Wood", 1),
        };
        if (icon != null)
            config.Icon = icon;

        var custom = new CustomPiece(name, "sign", config);
        if (!custom.IsValid())
        {
            EssentialsPlugin.Log.LogError($"Failed to register {name}: sign has no Piece.");
            return;
        }

        var prefab = custom.Piece.gameObject;
        dress(prefab);
        build(prefab.transform, 1f);
        custom.Piece.m_craftingStation = null;
        if (prefab.GetComponent<Sign>() is { } sign)
            sign.m_name = nameToken;
        var text = prefab.GetComponent<PaperText>() ?? prefab.AddComponent<PaperText>();
        text.m_wall = wall;
        PieceManager.Instance.AddPiece(custom);
    }
}

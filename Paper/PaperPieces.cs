using System;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using TMPro;
using UnityEngine;

namespace DrakesEssentials.Paper;

/// <summary>
/// Blank paper you write on like a sign, on the wall or lying flat. It is Valheim's own sign (its write box, sync and
/// text), dressed as a sheet with DrakesRenameit's paper (Packs/Paper, code in Generated/Paper) and the text moved
/// onto the sheet in dark ink.
/// </summary>
internal static class PaperPieces
{
    private const string Wall = "DrakesEssentials_PaperWall";
    private const string Flat = "DrakesEssentials_PaperFlat";

    // Sheet as built by the pack's sprites (US letter, 0.2159 x 0.2794 m).
    private const float SheetWidth = 0.2159f;
    private const float SheetHeight = 0.2794f;
    private const float TextMargin = 0.016f;
    private static readonly Color Ink = new(0.13f, 0.09f, 0.06f, 1f);

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
                piece.m_enabled = EssentialsConfig.PaperEnabled.Value;
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
        {
            sign.m_name = nameToken;
            PutTextOnSheet(sign, wall);
        }
        PieceManager.Instance.AddPiece(custom);
    }

    /// <summary>
    /// The sign's text canvas is a 1 m wide board. Reshape it to the sheet (portrait, with margins), centre it on the
    /// sheet just in front of it, and let the text wrap and shrink to fit.
    /// </summary>
    private static void PutTextOnSheet(Sign sign, bool wall)
    {
        TMP_Text? text = sign.m_textWidget;
        var canvas = text != null ? text.GetComponentInParent<Canvas>(true) : null;
        if (text == null || canvas == null || canvas.transform is not RectTransform board)
        {
            EssentialsPlugin.Log.LogWarning("Paper: the sign's text canvas wasn't found; text keeps the sign's layout.");
            return;
        }

        // Canvas units per metre, from its current width and scale.
        var unitsWide = board.rect.width > 0 ? board.rect.width : 100f;
        var scale = (SheetWidth - TextMargin * 2) / unitsWide;
        board.sizeDelta = new Vector2(unitsWide, unitsWide * (SheetHeight - TextMargin * 2) / (SheetWidth - TextMargin * 2));
        board.localScale = new Vector3(scale, scale, scale);

        if (wall)
        {
            // Keep the sign's facing; sit on the sheet's front face.
            var front = board.localRotation * Vector3.back;
            board.localPosition = new Vector3(0f, -SheetHeight / 2f, Mathf.Sign(front.z == 0 ? -1f : front.z) * 0.004f);
        }
        else
        {
            // Lying flat: readable from above, top of the text at the sheet's top edge (local -Z, see the pack sprite).
            board.localRotation = Quaternion.LookRotation(Vector3.down, Vector3.back);
            board.localPosition = new Vector3(0f, 0.006f, SheetHeight / 2f);
        }

        var rect = text.rectTransform;
        if (rect != board)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            rect.localRotation = Quaternion.identity;
            rect.localScale = Vector3.one;
        }

        text.color = Ink;
        text.enableWordWrapping = true;
        text.enableAutoSizing = true;
        text.fontSizeMin = 4f;
        text.fontSizeMax = Mathf.Max(text.fontSize, 12f);
        text.alignment = TextAlignmentOptions.Top;
    }
}

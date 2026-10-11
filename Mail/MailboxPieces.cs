using System;
using DrakesEssentials.Forge;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace DrakesEssentials.Mail;

/// <summary>
/// Mailbox (a Norse post) and Drop Box (tabletop), dressed from Packs/Mailbox (code in Generated/Mailbox). One chest, two sides:
/// the front (+Z) has a send slot for up to 3 stacks that go in and can't be taken back out from there; the back is an ordinary
/// 10 slot chest door, so wards decide who may open it. Same prefab names as DrakesItemShop, so placed boxes move between the two.
/// </summary>
internal static class MailboxPieces
{
    private sealed class Variant
    {
        public string Name = "";
        public string Token = "";
        public int FallbackCost;
        public Action<GameObject> Dress = _ => { };
        public Func<string> Cost = () => "";
        public Vector3 FlapHinge;
        public float FlapAngle;
        public Vector3 DoorHinge;
        public float DoorAngle;
        public Vector3 FlagHinge;
        public float FlagAngle;
    }

    // Hinge points from DrakesItemShop's Forge/Mailbox/make_mailbox.py (Blender (x, y, z) -> Unity (x, z, y)); keep in step with
    // DrakesItemShop's Mailbox/MailboxPiece.cs.
    private static readonly Variant[] Variants =
    {
        new()
        {
            Name = "Drake_Mailbox", Token = "drake_mailbox", FallbackCost = 10,
            Dress = Generated.Mail.Items.Mailbox.Dress, Cost = () => EssentialsConfig.MailboxCost.Value,
            FlapHinge = new Vector3(0f, 1.375f, 0.265f), FlapAngle = -68f,
            DoorHinge = new Vector3(-0.26f, 1.205f, -0.241f), DoorAngle = 105f,
            FlagHinge = new Vector3(0.40f, 1.41f, 0f), FlagAngle = -90f,
        },
        new()
        {
            Name = "Drake_DropBox", Token = "drake_dropbox", FallbackCost = 5,
            Dress = Generated.Mail.Items.DropBox.Dress, Cost = () => EssentialsConfig.DropBoxCost.Value,
            FlapHinge = new Vector3(0f, 0.329f, -0.034f), FlapAngle = -72f,
            DoorHinge = new Vector3(-0.13f, 0.15f, -0.152f), DoorAngle = 105f,
        },
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
                Requirements = PieceCosts.Requirements(variant.Cost(), variant.Name, "FineWood", variant.FallbackCost),
            };

            var custom = new CustomPiece(variant.Name, "piece_chest_wood", config);
            if (!custom.IsValid())
            {
                EssentialsPlugin.Log.LogError($"Failed to register {variant.Name}: piece_chest_wood has no Piece.");
                continue;
            }

            var prefab = custom.Piece.gameObject;
            variant.Dress(prefab);
            // The inside of the cavities: plain black (no wood grain or grate showing through when a door is open).
            ForgeLook.Material(prefab, "Black", null, "wood_window", "window_wood", null, material =>
            {
                material.color = Color.black;
                foreach (var texture in new[] { "_MainTex", "_BumpMap", "_MetallicGlossMap", "_EmissionMap" })
                    if (material.HasProperty(texture))
                        material.SetTexture(texture, null);
                if (material.HasProperty("_Glossiness"))
                    material.SetFloat("_Glossiness", 0f);
                if (material.HasProperty("_Metallic"))
                    material.SetFloat("_Metallic", 0f);
            });
            if (prefab.GetComponent<Container>() is { } container)
                container.m_name = $"${variant.Token}_name";
            var behaviour = prefab.GetComponent<MailboxBehaviour>() ?? prefab.AddComponent<MailboxBehaviour>();
            behaviour.m_flapHinge = variant.FlapHinge;
            behaviour.m_flapAngle = variant.FlapAngle;
            behaviour.m_doorHinge = variant.DoorHinge;
            behaviour.m_doorAngle = variant.DoorAngle;
            behaviour.m_flagHinge = variant.FlagHinge;
            behaviour.m_flagAngle = variant.FlagAngle;
            PieceManager.Instance.AddPiece(custom);
            PieceIcons.Render(custom.Piece, 25f);
        }
        _registered = true;
        EssentialsPlugin.Log.LogInfo("Registered the mailbox and drop box.");
    }

    public static void Apply()
    {
        if (!_registered)
            return;
        foreach (var variant in Variants)
        {
            if (PieceManager.Instance.GetPiece(variant.Name)?.Piece is { } piece)
                piece.m_enabled = EssentialsConfig.MailboxesEnabled.Value;
            PieceCosts.Apply(variant.Name, variant.Cost(), "FineWood", variant.FallbackCost);
        }
    }
}

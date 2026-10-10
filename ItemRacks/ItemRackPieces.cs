// Rack table copied from DrakesItemShop (Pieces/ItemRackPiece.cs); keep in step with it.
using System;
using System.Linq;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;

namespace DrakesEssentials.ItemRacks;

/// <summary>
/// Finewood racks: armor and weapon racks (free-standing, big/small) and wall racks (5/3 slots) that take both, dressed from Packs/ItemRack (code in
/// Generated/ItemRack). Same prefab names as DrakesItemShop, so placed racks move between the two mods.
/// </summary>
internal static class ItemRackPieces
{
    internal sealed class Variant
    {
        public string Name = "";
        public string Token = "";
        public string ConfigKey = "";
        public string DefaultCost = "";
        public ItemRackBehaviour.RackKind Kind;
        /// <summary>Hang points per container row (row 0 first), column 0 = the stocker's left (+X).</summary>
        public Vector3[][] Rows = Array.Empty<Vector3[]>();
        /// <summary>How far below its hang point an item may reach, per row, before it's shrunk.</summary>
        public float[] RowDrop = Array.Empty<float>();
        /// <summary>Front face of the bar or board the hooks sit on (local Z): items never sink behind it.</summary>
        public float Front;
        public Action<GameObject> Dress = _ => { };
    }

    // Hang points as printed by Packs/ItemRack/make_item_rack.py (RACKINFO, Blender (x, y, z) -> Unity (x, z, y)).
    internal static readonly Variant[] Variants =
    {
        new()
        {
            Name = "Drake_ArmorRack", Token = "drake_armor_rack", ConfigKey = "ArmorRackCost", DefaultCost = "FineWood:12,BronzeNails:6",
            Kind = ItemRackBehaviour.RackKind.Armor, Front = 0.065f, RowDrop = new[] { 0.72f, 0.85f },
            Rows = new[]
            {
                new Vector3[] { new(0.6733f, 1.84f, 0.195f), new(0f, 1.84f, 0.195f), new(-0.6733f, 1.84f, 0.195f) },
                new Vector3[] { new(0.6733f, 1.06f, 0.165f), new(0f, 1.06f, 0.165f), new(-0.6733f, 1.06f, 0.165f) },
            },
            Dress = Generated.Racks.Items.ArmorRack.Dress,
        },
        new()
        {
            Name = "Drake_ArmorRackSmall", Token = "drake_armor_rack_small", ConfigKey = "SmallArmorRackCost", DefaultCost = "FineWood:8,BronzeNails:4",
            Kind = ItemRackBehaviour.RackKind.Armor, Front = 0.065f, RowDrop = new[] { 0.58f, 0.85f },
            Rows = new[]
            {
                new Vector3[] { new(0.305f, 1.69f, 0.195f), new(-0.305f, 1.69f, 0.195f) },
                new Vector3[] { new(0.305f, 1.06f, 0.165f), new(-0.305f, 1.06f, 0.165f) },
            },
            Dress = Generated.Racks.Items.SmallArmorRack.Dress,
        },
        new()
        {
            Name = "Drake_WallRack", Token = "drake_wall_rack", ConfigKey = "WallRackCost", DefaultCost = "FineWood:8,BronzeNails:5",
            Kind = ItemRackBehaviour.RackKind.Any, Front = 0.04f, RowDrop = new[] { 1.1f },
            Rows = new[] { new Vector3[] { new(1.1f, 0f, 0.18f), new(0.55f, 0f, 0.18f), new(0f, 0f, 0.18f), new(-0.55f, 0f, 0.18f), new(-1.1f, 0f, 0.18f) } },
            Dress = Generated.Racks.Items.WallRack.Dress,
        },
        new()
        {
            Name = "Drake_WallRackSmall", Token = "drake_wall_rack_small", ConfigKey = "SmallWallRackCost", DefaultCost = "FineWood:5,BronzeNails:3",
            Kind = ItemRackBehaviour.RackKind.Any, Front = 0.04f, RowDrop = new[] { 1.1f },
            Rows = new[] { new Vector3[] { new(0.55f, 0f, 0.18f), new(0f, 0f, 0.18f), new(-0.55f, 0f, 0.18f) } },
            Dress = Generated.Racks.Items.SmallWallRack.Dress,
        },
        new()
        {
            Name = "Drake_WeaponRack", Token = "drake_weapon_rack", ConfigKey = "WeaponRackCost", DefaultCost = "FineWood:10,BronzeNails:6",
            Kind = ItemRackBehaviour.RackKind.Weapon, Front = 0.065f, RowDrop = new[] { 1.1f },
            Rows = new[]
            {
                new Vector3[] { new(0.8417f, 1.36f, 0.125f), new(0.505f, 1.36f, 0.125f), new(0.1683f, 1.36f, 0.125f),
                                new(-0.1683f, 1.36f, 0.125f), new(-0.505f, 1.36f, 0.125f), new(-0.8417f, 1.36f, 0.125f) },
            },
            Dress = Generated.Racks.Items.WeaponRack.Dress,
        },
        new()
        {
            Name = "Drake_WeaponRackSmall", Token = "drake_weapon_rack_small", ConfigKey = "SmallWeaponRackCost", DefaultCost = "FineWood:7,BronzeNails:4",
            Kind = ItemRackBehaviour.RackKind.Weapon, Front = 0.065f, RowDrop = new[] { 1.1f },
            Rows = new[] { new Vector3[] { new(0.4575f, 1.36f, 0.125f), new(0.1525f, 1.36f, 0.125f), new(-0.1525f, 1.36f, 0.125f), new(-0.4575f, 1.36f, 0.125f) } },
            Dress = Generated.Racks.Items.SmallWeaponRack.Dress,
        },
    };

    private const string FallbackCostItem = "FineWood";
    private const int FallbackCost = 6;
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
                Requirements = PieceCosts.Requirements(EssentialsConfig.RackCost(variant), variant.Name, FallbackCostItem, FallbackCost),
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

            var behaviour = prefab.GetComponent<ItemRackBehaviour>() ?? prefab.AddComponent<ItemRackBehaviour>();
            behaviour.m_kind = variant.Kind;
            behaviour.m_columns = variant.Rows[0].Length;
            behaviour.m_hangPoints = variant.Rows.SelectMany(row => row).ToArray();
            behaviour.m_rowDrop = variant.RowDrop;
            behaviour.m_front = variant.Front;
            behaviour.m_pitch = variant.Rows[0].Length > 1 ? Mathf.Abs(variant.Rows[0][0].x - variant.Rows[0][1].x) : 0.6f;
            PieceManager.Instance.AddPiece(custom);
        }
        _registered = true;
        EssentialsPlugin.Log.LogInfo("Registered armor and weapon racks.");
    }

    public static void Apply()
    {
        if (!_registered)
            return;
        foreach (var variant in Variants)
        {
            if (PieceManager.Instance.GetPiece(variant.Name)?.Piece is { } piece)
                piece.m_enabled = EssentialsConfig.RacksEnabled.Value;
            PieceCosts.Apply(variant.Name, EssentialsConfig.RackCost(variant), FallbackCostItem, FallbackCost);
        }
    }
}

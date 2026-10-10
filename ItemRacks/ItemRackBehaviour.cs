// Copied from DrakesItemShop (Pieces/ItemRackBehaviour.cs); keep in step with it.
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DrakesEssentials.ItemRacks;

/// <summary>
/// Armor, weapon or wall rack: a chest whose contents hang on the rack, one hook/fork per slot (container row 0 = the top
/// hooks, row 1 = the pegs below on the armor stands). Weapons hang the way a vanilla item stand shows them; armor
/// hangs its own model upright. Only the right kind of item goes in (see ItemRackPatches.cs).
/// Contents come from the container's synced ZDO, so every client shows its own copy.
/// </summary>
public class ItemRackBehaviour : MonoBehaviour
{
    public enum RackKind
    {
        Armor,
        Weapon,
        /// <summary>Wall racks: armor and weapons.</summary>
        Any,
    }

    public RackKind m_kind;
    /// <summary>Slots per row (= container width).</summary>
    public int m_columns = 1;
    /// <summary>Hang points, row by row (index = row * columns + column).</summary>
    public Vector3[] m_hangPoints = System.Array.Empty<Vector3>();
    /// <summary>How far below its hang point an item may reach, per row.</summary>
    public float[] m_rowDrop = System.Array.Empty<float>();
    /// <summary>Local Z of the bar/board face the hooks come out of.</summary>
    public float m_front;
    /// <summary>Distance between neighbouring slots.</summary>
    public float m_pitch = 0.6f;

    private const float RefreshSeconds = 0.5f;
    /// <summary>Items may be a bit wider than their slot (shields overlap their neighbours a little).</summary>
    private const float WidthAllowance = 1.25f;
    private const float MaxAbove = 0.4f;

    private static readonly Dictionary<Inventory, ItemRackBehaviour> Racks = new();
    private static Quaternion? _standPose;

    private Container? _container;
    private Inventory? _inventory;
    private Transform? _display;
    private string _shownContents = "";
    private float _nextRefresh;

    /// <summary>The rack an inventory belongs to, if any.</summary>
    public static ItemRackBehaviour? Of(Inventory? inventory) =>
        inventory != null && Racks.TryGetValue(inventory, out var rack) && rack != null ? rack : null;

    /// <summary>Armor racks take armor (helmet, chest, legs, cape), weapon racks weapons and shields (they're held), wall racks both.</summary>
    public bool Accepts(ItemDrop.ItemData? item) => m_kind switch
    {
        RackKind.Armor => IsArmor(item),
        RackKind.Weapon => IsWeapon(item),
        _ => IsArmor(item) || IsWeapon(item),
    };

    private static bool IsArmor(ItemDrop.ItemData? item) =>
        item?.m_shared?.m_itemType is ItemDrop.ItemData.ItemType.Helmet or ItemDrop.ItemData.ItemType.Chest
            or ItemDrop.ItemData.ItemType.Legs or ItemDrop.ItemData.ItemType.Shoulder or ItemDrop.ItemData.ItemType.Hands;

    private static bool IsWeapon(ItemDrop.ItemData? item) =>
        item?.m_shared?.m_itemType is ItemDrop.ItemData.ItemType.OneHandedWeapon or ItemDrop.ItemData.ItemType.TwoHandedWeapon
            or ItemDrop.ItemData.ItemType.TwoHandedWeaponLeft or ItemDrop.ItemData.ItemType.Bow
            or ItemDrop.ItemData.ItemType.Shield or ItemDrop.ItemData.ItemType.Tool;

    public string RefusalMessage => m_kind switch
    {
        RackKind.Armor => "$drake_rack_armor_only",
        RackKind.Weapon => "$drake_rack_weapons_only",
        _ => "$drake_rack_gear_only",
    };

    private void Start()
    {
        _container = GetComponent<Container>();
        _inventory = _container != null ? _container.GetInventory() : null;
        if (_inventory != null)
            Racks[_inventory] = this;
    }

    private void Update()
    {
        if (Time.time < _nextRefresh)
            return;
        _nextRefresh = Time.time + RefreshSeconds;
        Refresh();
    }

    private void Refresh()
    {
        if (_inventory == null || ZNet.instance == null || ZNet.instance.IsDedicated())
            return;

        var contents = Describe(_inventory);
        if (contents == _shownContents)
            return;
        _shownContents = contents;

        if (_display != null)
            Destroy(_display.gameObject);
        _display = new GameObject("drake_rack_items").transform;
        _display.SetParent(transform, false);

        foreach (var item in _inventory.GetAllItems())
        {
            if (item?.m_dropPrefab == null || !Accepts(item) || item.m_gridPos.x >= m_columns)
                continue;
            var index = item.m_gridPos.y * m_columns + item.m_gridPos.x;
            if (index < 0 || index >= m_hangPoints.Length)
                continue;
            var row = Mathf.Clamp(item.m_gridPos.y, 0, Mathf.Max(0, m_rowDrop.Length - 1));
            Hang(item, m_hangPoints[index], m_rowDrop.Length > 0 ? m_rowDrop[row] : 1f);
        }
    }

    private static string Describe(Inventory inventory)
    {
        var text = new StringBuilder();
        foreach (var item in inventory.GetAllItems())
            text.Append(item.m_dropPrefab != null ? item.m_dropPrefab.name : "?").Append('|').Append(item.m_gridPos.x).Append(',')
                .Append(item.m_gridPos.y).Append('|').Append(item.m_quality).Append('|').Append(item.m_variant).Append(';');
        return text.ToString();
    }

    // ---- hanging one item ----

    private void Hang(ItemDrop.ItemData item, Vector3 hangPoint, float maxDrop)
    {
        var holder = new GameObject("slot").transform;
        holder.SetParent(_display, false);
        holder.localPosition = hangPoint;

        var held = IsWeapon(item) && HangLikeItemStand(item, holder);
        if (!held && !HangUpright(item.m_dropPrefab, holder))
        {
            Destroy(holder.gameObject);
            return;
        }

        if (!Bounds(holder, transform, out var bounds))
            return;

        // Shrink around the hang point until it fits its slot: width, how far it may hang down, how far up.
        var scale = Mathf.Min(1f,
            m_pitch * WidthAllowance / Mathf.Max(bounds.size.x, 0.001f),
            maxDrop / Mathf.Max(hangPoint.y - bounds.min.y, 0.001f),
            MaxAbove / Mathf.Max(bounds.max.y - hangPoint.y, 0.001f));
        holder.localScale = Vector3.one * scale;
        var min = hangPoint + (bounds.min - hangPoint) * scale;
        // Never behind the bar/board the hooks come out of.
        if (min.z < m_front + 0.005f)
            holder.localPosition += Vector3.forward * (m_front + 0.005f - min.z);
    }

    /// <summary>
    /// Weapons and shields: the item's "attach" model posed exactly as a vanilla (vertical) item stand poses it,
    /// grip on the fork. False when the item has no attach model.
    /// </summary>
    private static bool HangLikeItemStand(ItemDrop.ItemData item, Transform holder)
    {
        var attach = ItemStand.GetAttachPrefab(item.m_dropPrefab);
        if (attach == null || StandPose() is not { } pose)
            return false;

        holder.localRotation = pose;
        var model = StillCopy(ItemStand.GetAttachGameObject(attach));
        if (model == null)
            return false;
        var t = model.transform;
        t.SetParent(holder, false);
        t.localPosition = attach.transform.localPosition;
        t.localRotation = attach.transform.localRotation;
        t.localScale = attach.transform.localScale;
        // The item's own item-stand offsets (first one for vertical stands), like ItemStand.SetVisualItem.
        if (VerticalStandOffset(item) is { } offset)
        {
            t.localPosition += offset.Position;
            t.localRotation *= Quaternion.Euler(offset.Rotation);
            t.localScale = Vector3.Scale(t.localScale, offset.Scale);
        }
        model.SetActive(true);
        return true;
    }

    /// <summary>
    /// The item's first item-stand offset for vertical stands (SharedData.m_itemStandOffsets). Read by reflection:
    /// the API stubs we build against predate it, and older games simply have none.
    /// </summary>
    private static (Vector3 Position, Vector3 Rotation, Vector3 Scale)? VerticalStandOffset(ItemDrop.ItemData item)
    {
        const int vertical = 1;   // ItemStand.Orientation.Vertical
        if (OffsetsField?.GetValue(item.m_shared) is not System.Collections.IEnumerable offsets)
            return null;
        foreach (var offset in offsets)
        {
            if (offset == null)
                continue;
            var type = offset.GetType();
            var orientations = System.Convert.ToInt32(type.GetField("m_orientations")?.GetValue(offset) ?? vertical);
            if ((orientations & vertical) == 0)
                continue;
            return ((Vector3)(type.GetField("m_position")?.GetValue(offset) ?? Vector3.zero),
                    (Vector3)(type.GetField("m_rotation")?.GetValue(offset) ?? Vector3.zero),
                    (Vector3)(type.GetField("m_scale")?.GetValue(offset) ?? Vector3.one));
        }
        return null;
    }

    private static readonly System.Reflection.FieldInfo? OffsetsField =
        typeof(ItemDrop.ItemData.SharedData).GetField("m_itemStandOffsets");

    /// <summary>Rotation of the vanilla wall item stand's attach point, turned so it faces out of our racks (+Z).</summary>
    private static Quaternion? StandPose()
    {
        if (_standPose != null)
            return _standPose;
        var stand = ZNetScene.instance != null ? ZNetScene.instance.GetPrefab("itemstand")?.GetComponent<ItemStand>() : null;
        if (stand == null || stand.m_attachOther == null)
            return null;
        var root = stand.transform;
        var pose = Quaternion.Inverse(root.rotation) * stand.m_attachOther.rotation;
        if (root.InverseTransformPoint(stand.m_attachOther.position).z < 0f)
            pose = Quaternion.Euler(0f, 180f, 0f) * pose;
        _standPose = pose;
        return pose;
    }

    /// <summary>Armor (and anything without an attach model): its dropped model stood up, flat side to the front, hanging from the hook.</summary>
    private static bool HangUpright(GameObject prefab, Transform holder)
    {
        var model = StillCopy(prefab);
        if (model == null)
            return false;
        if (!Bounds(model.transform, model.transform, out var bounds))
        {
            Destroy(model);
            return false;
        }

        // Thinnest side to the front, longest side up.
        var axes = new List<(Vector3 Dir, float Size)> { (Vector3.right, bounds.size.x), (Vector3.up, bounds.size.y), (Vector3.forward, bounds.size.z) };
        axes.Sort((a, b) => b.Size.CompareTo(a.Size));
        var stand = Quaternion.Inverse(Quaternion.LookRotation(axes[2].Dir, axes[0].Dir));

        var t = model.transform;
        t.SetParent(holder, false);
        t.localRotation = stand;
        // Top of the model just under the hook, centred on it.
        var centre = stand * bounds.center;
        t.localPosition = -centre - Vector3.up * (axes[0].Size / 2f + 0.02f);
        model.SetActive(true);
        return true;
    }

    /// <summary>
    /// A copy of <paramref name="source"/> made while it's inactive (so none of its scripts wake up), stripped of
    /// scripts, physics, sound and particles: only meshes and lights remain. Returned inactive.
    /// </summary>
    private static GameObject? StillCopy(GameObject source)
    {
        var wasActive = source.activeSelf;
        source.SetActive(false);
        GameObject copy;
        try
        {
            copy = Instantiate(source);
        }
        finally
        {
            source.SetActive(wasActive);
        }

        copy.name = source.name + "_rack";
        var behaviours = copy.GetComponentsInChildren<MonoBehaviour>(true);
        for (var i = behaviours.Length - 1; i >= 0; i--)
            if (behaviours[i] is not ZNetView)
                DestroyImmediate(behaviours[i]);
        foreach (var nview in copy.GetComponentsInChildren<ZNetView>(true))
            DestroyImmediate(nview);
        foreach (var joint in copy.GetComponentsInChildren<Joint>(true))
            DestroyImmediate(joint);
        foreach (var body in copy.GetComponentsInChildren<Rigidbody>(true))
            DestroyImmediate(body);
        foreach (var collider in copy.GetComponentsInChildren<Collider>(true))
            DestroyImmediate(collider);
        foreach (var audio in copy.GetComponentsInChildren<AudioSource>(true))
            DestroyImmediate(audio);
        foreach (var particles in copy.GetComponentsInChildren<ParticleSystem>(true))
        {
            if (particles.GetComponent<ParticleSystemRenderer>() is { } renderer)
                DestroyImmediate(renderer);
            DestroyImmediate(particles);
        }
        return copy;
    }

    /// <summary>Bounds of the meshes under <paramref name="root"/> in <paramref name="space"/>'s local space (works while inactive).</summary>
    private static bool Bounds(Transform root, Transform space, out Bounds bounds)
    {
        bounds = default;
        var any = false;
        var toSpace = space.worldToLocalMatrix;
        foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
        {
            Mesh? mesh = renderer switch
            {
                SkinnedMeshRenderer skinned => skinned.sharedMesh,
                MeshRenderer => renderer.GetComponent<MeshFilter>()?.sharedMesh,
                _ => null
            };
            if (mesh == null)
                continue;
            var matrix = toSpace * renderer.transform.localToWorldMatrix;
            var b = mesh.bounds;
            for (var corner = 0; corner < 8; corner++)
            {
                var p = matrix.MultiplyPoint3x4(b.center + Vector3.Scale(b.extents,
                    new Vector3((corner & 1) == 0 ? -1 : 1, (corner & 2) == 0 ? -1 : 1, (corner & 4) == 0 ? -1 : 1)));
                if (!any)
                {
                    bounds = new Bounds(p, Vector3.zero);
                    any = true;
                }
                else
                {
                    bounds.Encapsulate(p);
                }
            }
        }
        return any;
    }

    private void OnDestroy()
    {
        if (_inventory != null && Racks.TryGetValue(_inventory, out var rack) && rack == this)
            Racks.Remove(_inventory);
        if (_display != null)
            Destroy(_display.gameObject);
    }
}

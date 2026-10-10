// Copied from DrakesItemShop (Patches/ItemRackPatches.cs); keep in step with it (only the message differs).
using HarmonyLib;
using UnityEngine;

namespace DrakesEssentials.ItemRacks;

/// <summary>
/// Armor racks only take armor, weapon racks only weapons and shields. Every way an item gets into an inventory
/// (drag and drop, shift-click, take/put all) ends in one of these AddItem overloads; container loading doesn't.
/// </summary>
internal static class ItemRackPatches
{
    private static float _nextMessage;

    /// <summary>True when <paramref name="item"/> may not go into <paramref name="inventory"/> (and tells the player why).</summary>
    internal static bool Refuses(Inventory inventory, ItemDrop.ItemData? item)
    {
        var rack = ItemRackBehaviour.Of(inventory);
        if (rack == null || item == null || rack.Accepts(item))
            return false;

        if (Player.m_localPlayer != null && Time.time >= _nextMessage)
        {
            _nextMessage = Time.time + 1f;
            Tell(Player.m_localPlayer, rack.RefusalMessage);
        }
        return true;
    }

    /// <summary>
    /// Character.Message without binding to one signature: Valheim 1.0 added a parameter, and a direct call compiled
    /// against the older API stubs throws at runtime.
    /// </summary>
    private static void Tell(Character character, string text)
    {
        _message ??= System.Array.Find(typeof(Character).GetMethods(), m => m.Name == "Message"
            && m.GetParameters().Length >= 2 && m.GetParameters()[0].ParameterType == typeof(MessageHud.MessageType));
        if (_message == null)
            return;
        var parameters = _message.GetParameters();
        var args = new object?[parameters.Length];
        args[0] = MessageHud.MessageType.Center;
        args[1] = text;
        for (var i = 2; i < parameters.Length; i++)
            args[i] = parameters[i].HasDefaultValue ? parameters[i].DefaultValue
                : parameters[i].ParameterType.IsValueType ? System.Activator.CreateInstance(parameters[i].ParameterType) : null;
        _message.Invoke(character, args);
    }

    private static System.Reflection.MethodInfo? _message;

    [HarmonyPatch(typeof(Inventory), "AddItem",
        new[] { typeof(ItemDrop.ItemData), typeof(int), typeof(int), typeof(int), typeof(bool) })]
    private static class AddItemAtPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!Refuses(__instance, item))
                return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData) })]
    private static class AddItemPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!Refuses(__instance, item))
                return true;
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(Inventory), nameof(Inventory.AddItem), new[] { typeof(ItemDrop.ItemData), typeof(Vector2i) })]
    private static class AddItemPosPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(Inventory __instance, ItemDrop.ItemData item, ref bool __result)
        {
            if (!Refuses(__instance, item))
                return true;
            __result = false;
            return false;
        }
    }

    /// <summary>
    /// Dropping onto an occupied slot swaps: vanilla removes the dragged item first and then moves both, so a refused
    /// half would lose an item. Refuse the whole swap up front instead.
    /// </summary>
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.DropItem))]
    private static class DropItemPatch
    {
        [HarmonyPrefix]
        private static bool Prefix(InventoryGrid __instance, Inventory fromInventory, ItemDrop.ItemData item, Vector2i pos, ref bool __result)
        {
            var target = __instance.GetInventory();
            var refused = Refuses(target, item);
            if (!refused && fromInventory != null && target != null)
            {
                var swapped = target.GetItemAt(pos.x, pos.y);
                refused = swapped != null && swapped != item && Refuses(fromInventory, swapped);
            }
            if (!refused)
                return true;
            __result = false;
            return false;
        }
    }
}

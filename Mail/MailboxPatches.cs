// Copied from DrakesItemShop (Mailbox/MailboxSendWindow.cs, the patches) without the PO box parts; keep in step with it.
using DrakesEssentials.Ui;
using HarmonyLib;

namespace DrakesEssentials.Mail;

/// <summary>Shift+E on a mailbox opens its Mail settings (sign, who may send, what it takes, recent mail).</summary>
[HarmonyPatch(typeof(Container), nameof(Container.Interact))]
internal static class MailboxSettingsInteractPatch
{
    [HarmonyPrefix]
    [HarmonyPriority(Priority.First)]
    private static bool Prefix(Container __instance, Humanoid character, bool hold, bool alt, ref bool __result)
    {
        var mail = __instance.GetComponent<MailboxBehaviour>();
        if (mail == null || !alt || hold || character is not Player player)
            return true;
        __result = true;
        // The settings and the list of who sent what are for the builder and whoever may open the back.
        if (mail.IsBuilder(player.GetPlayerID()) || mail.CanOpen(player))
            MailboxSettingsWindow.Open(__instance, mail, player);
        else
            HudText.Say("Only the owner can see this box's settings.");
        return false;
    }
}

/// <summary>The public side sends, the back is a plain chest (wards decide who may open that).</summary>
[HarmonyPatch(typeof(Container), nameof(Container.Interact))]
internal static class MailboxInteractPatch
{
    [HarmonyPrefix]
    private static bool Prefix(Container __instance, Humanoid character, bool hold, ref bool __result)
    {
        var mail = __instance.GetComponent<MailboxBehaviour>();
        if (mail == null || character == null || !mail.IsPublicSide(character))
            return true;

        __result = true;
        if (hold || MailboxSendWindow.IsOpen)
            return false;
        if (!mail.AcceptOn)
        {
            HudText.Say("This mailbox is closed.");
            return false;
        }
        if (character is Player visitor && !mail.MaySend(visitor))
        {
            HudText.Say("You're not on this mailbox's list.");
            return false;
        }
        MailboxSendWindow.Open(mail);
        return false;
    }
}

[HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
internal static class MailboxHoverPatch
{
    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    private static void Postfix(Container __instance, ref string __result)
    {
        var mail = __instance.GetComponent<MailboxBehaviour>();
        if (mail == null || Player.m_localPlayer == null)
            return;

        var plain = global::Localization.instance.Localize(__instance.m_name);
        var signed = mail.SignName(plain);
        if (mail.IsPublicSide(Player.m_localPlayer))
        {
            __result = global::Localization.instance.Localize(mail.AcceptOn
                ? $"{signed}\n[<color=yellow><b>$KEY_Use</b></color>] Send items"
                : $"{signed}\n<color=grey>Closed for mail</color>");
            return;
        }

        if (__result.Contains(plain))
            __result = __result.Replace(plain, signed);   // the back keeps the chest's own text
        __result += global::Localization.instance.Localize("\n[<color=yellow><b>$KEY_AltPlace + $KEY_Use</b></color>] Mail settings");
        // Tell the builder when the back door is open to anyone.
        if (!mail.IsProtected && mail.IsBuilder(Player.m_localPlayer.GetPlayerID()))
            __result += "\n<color=orange>Not in a ward: anyone can open this back</color>";
    }
}

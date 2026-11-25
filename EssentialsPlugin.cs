using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using DrakesEssentials.DisplayCases;
using DrakesEssentials.ItemRacks;
using DrakesEssentials.Mail;
using DrakesEssentials.Paper;
using DrakesEssentials.PublicPieces;
using HarmonyLib;
using Jotunn.Managers;

namespace DrakesEssentials;

/// <summary>
/// The best pieces from the DrakeMods suite, stand-alone: display cases, display mats, racks and mailboxes (DrakesItemShop), always-public doors and
/// chests (LockSmith) and writable blank paper (DrakesRenameit). When the full mod is installed it provides its own
/// version of a piece, so Essentials steps aside for that feature (same prefab names: worlds move between them).
/// </summary>
[BepInPlugin(GUID, ModName, Version)]
[BepInDependency(Jotunn.Main.ModGuid)]
// Soft dependencies on every id the full mods have used, so they load first whichever one is installed.
[BepInDependency(ItemShopGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(ItemShopOldGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LockSmithGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LockSmithOldGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RenameItGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RenameItOldGuid, BepInDependency.DependencyFlags.SoftDependency)]
public partial class EssentialsPlugin : BaseUnityPlugin
{
    // The full mods moved from com.drakesworkshop.* to com.drakemods.*; checking only the old id made Essentials
    // register its own copies next to ItemShop's (duplicate prefab names, a wall of Jotunn errors on every start).
    internal const string ItemShopGuid = "com.drakemods.itemshop";
    internal const string ItemShopOldGuid = "com.drakesworkshop.itemshop";
    internal const string LockSmithGuid = "com.drakemods.locksmith";
    internal const string LockSmithOldGuid = "com.drakesworkshop.locksmith";
    internal const string RenameItGuid = "com.drakemods.renameit";        // 1.4 betas and later
    internal const string RenameItOldGuid = "com.drakesworkshop.renameit"; // 1.3.x

    internal static bool ItemShopInstalled => IsInstalled(ItemShopGuid) || IsInstalled(ItemShopOldGuid);
    internal static bool LockSmithInstalled => IsInstalled(LockSmithGuid) || IsInstalled(LockSmithOldGuid);
    internal static bool RenameItInstalled => IsInstalled(RenameItGuid) || IsInstalled(RenameItOldGuid);

    internal static EssentialsPlugin Instance = null!;
    internal static ManualLogSource Log = null!;

    private readonly Harmony _harmony = new(GUID);

    private void Awake()
    {
        Instance = this;
        Log = Logger;
        EssentialsConfig.Bind(Config);
        EssentialsLocalization.Add();
        _harmony.PatchAll();
        PrefabManager.OnVanillaPrefabsAvailable += RegisterPieces;
        EssentialsConfig.Changed += ApplySettings;
        Logger.LogInfo($"{ModName} {Version} loaded.");
    }

    private void RegisterPieces()
    {
        PrefabManager.OnVanillaPrefabsAvailable -= RegisterPieces;

        if (ItemShopInstalled)
            Log.LogInfo("DrakesItemShop is installed: it provides the display cases.");
        else
            DisplayCasePieces.Register();

        if (ItemShopInstalled)
            Log.LogInfo("DrakesItemShop is installed: it provides the armor and weapon racks.");
        else
            ItemRackPieces.Register();

        if (ItemShopInstalled)
            Log.LogInfo("DrakesItemShop is installed: it provides the mailbox and drop box.");
        else
            MailboxPieces.Register();

        // RenameIt's paper has other prefab names, so Essentials' sheets stay registered (placed ones keep working)
        // and are only taken out of the hammer (PaperPieces.Apply).
        PaperPieces.Register();
        if (RenameItInstalled)
            Log.LogInfo("DrakesRenameit is installed: it provides the paper; Essentials' sheets are hidden from the hammer.");

        if (LockSmithInstalled)
            Log.LogInfo("LockSmith is installed: it provides the public doors and chests.");
        else
            PublicPieceRegistration.Start();

        ApplySettings();
    }

    /// <summary>Show/hide each feature's pieces and refresh costs: works live, synced from the server.</summary>
    private static void ApplySettings()
    {
        DisplayCasePieces.Apply();
        ItemRackPieces.Apply();
        MailboxPieces.Apply();
        PaperPieces.Apply();
        PublicPieceRegistration.Apply();
    }

    /// <summary>X on the mailbox send warning sends and stops the warning from showing again.</summary>
    private void Update() => MailboxSendWindow.PollKeys();

    internal static bool IsInstalled(string guid) => Chainloader.PluginInfos.ContainsKey(guid);

    private static readonly System.Collections.Generic.HashSet<string> SkipNoted = new();

    /// <summary>
    /// True when a prefab with this name already exists (another mod, e.g. a full DrakeMods mod under a plugin id
    /// Essentials does not know, registered the same piece first). Essentials then skips its copy instead of letting
    /// Jotunn fail the clone: one Info line per feature, the rest at Debug.
    /// </summary>
    internal static bool SkipIfRegistered(string prefabName, string feature)
    {
        if (PrefabManager.Instance.GetPrefab(prefabName) == null)
            return false;
        if (SkipNoted.Add(feature))
            Log.LogInfo($"{feature}: '{prefabName}' is already provided by another mod; Essentials skips its own copy.");
        else
            Log.LogDebug($"{feature}: '{prefabName}' already provided; skipped.");
        return true;
    }
}

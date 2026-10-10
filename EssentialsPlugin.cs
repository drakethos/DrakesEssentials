using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using DrakesEssentials.DisplayCases;
using DrakesEssentials.ItemRacks;
using DrakesEssentials.Paper;
using DrakesEssentials.PublicPieces;
using HarmonyLib;
using Jotunn.Managers;

namespace DrakesEssentials;

/// <summary>
/// The best pieces from the DrakeMods suite, stand-alone: display cases (DrakesItemShop), always-public doors and
/// chests (LockSmith) and writable blank paper (DrakesRenameit). When the full mod is installed it provides its own
/// version of a piece, so Essentials steps aside for that feature (same prefab names: worlds move between them).
/// </summary>
[BepInPlugin(GUID, ModName, Version)]
[BepInDependency(Jotunn.Main.ModGuid)]
[BepInDependency(ItemShopGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(LockSmithGuid, BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency(RenameItGuid, BepInDependency.DependencyFlags.SoftDependency)]
public partial class EssentialsPlugin : BaseUnityPlugin
{
    internal const string ItemShopGuid = "com.drakesworkshop.itemshop";
    internal const string LockSmithGuid = "com.drakesworkshop.locksmith";
    internal const string RenameItGuid = "com.drakesworkshop.renameit";

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

        if (IsInstalled(ItemShopGuid))
            Log.LogInfo("DrakesItemShop is installed: it provides the display cases.");
        else
            DisplayCasePieces.Register();

        if (IsInstalled(ItemShopGuid))
            Log.LogInfo("DrakesItemShop is installed: it provides the armor and weapon racks.");
        else
            ItemRackPieces.Register();

        // RenameIt's paper has other prefab names, so Essentials' sheets stay registered (placed ones keep working)
        // and are only taken out of the hammer (PaperPieces.Apply).
        PaperPieces.Register();
        if (IsInstalled(RenameItGuid))
            Log.LogInfo("DrakesRenameit is installed: it provides the paper; Essentials' sheets are hidden from the hammer.");

        if (IsInstalled(LockSmithGuid))
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
        PaperPieces.Apply();
        PublicPieceRegistration.Apply();
    }

    internal static bool IsInstalled(string guid) => Chainloader.PluginInfos.ContainsKey(guid);
}

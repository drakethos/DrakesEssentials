using System;
using BepInEx.Configuration;
using ServerSync;

namespace DrakesEssentials;

/// <summary>
/// Server-synced settings (ServerSync is merged into this DLL). Each feature has an on/off switch that only shows or
/// hides its pieces in the hammer, so it can change live without a restart and placed pieces never disappear.
/// </summary>
internal static class EssentialsConfig
{
    private const string General = "1 - General";
    private const string Cases = "2 - Display cases";
    private const string Public = "3 - Public doors and chests";
    private const string Paper = "4 - Paper";

    private static ConfigSync _sync = null!;

    public static ConfigEntry<bool> DisplayCasesEnabled = null!;
    public static ConfigEntry<string> DisplayCaseCost = null!;
    public static ConfigEntry<string> SmallDisplayCaseCost = null!;
    public static ConfigEntry<bool> PublicPiecesEnabled = null!;
    public static ConfigEntry<bool> PaperEnabled = null!;
    public static ConfigEntry<string> PaperCost = null!;

    /// <summary>Any setting changed (locally or synced from the server).</summary>
    public static event Action? Changed;

    public static void Bind(ConfigFile config)
    {
        _sync = new ConfigSync(EssentialsPlugin.GUID)
        {
            DisplayName = EssentialsPlugin.ModName,
            CurrentVersion = SystemVersion(EssentialsPlugin.Version),
            MinimumRequiredVersion = SystemVersion(EssentialsPlugin.Version),
        };

        var locked = config.Bind(General, "LockConfiguration", true,
            "When on, the server's settings apply to everyone and only admins can change them.");
        _sync.AddLockingConfigEntry(locked);

        DisplayCasesEnabled = Synced(config, Cases, "Enabled", true,
            "Display Case and Small Display Case in the hammer (Drake Shop tab). Off hides them; placed cases stay.");
        DisplayCaseCost = Synced(config, Cases, "DisplayCaseCost", "Wood:10",
            "Display Case cost: Item:amount pairs, comma separated (prefab names, e.g. Wood:10, Crystal:2).");
        SmallDisplayCaseCost = Synced(config, Cases, "SmallDisplayCaseCost", "Wood:5",
            "Small Display Case cost: Item:amount pairs, comma separated.");
        PublicPiecesEnabled = Synced(config, Public, "Enabled", true,
            "Always-public copies of every buildable door and chest (hammer tab Public): anyone can open them, wards or not. Off hides them; placed ones stay public.");
        PaperEnabled = Synced(config, Paper, "Enabled", true,
            "Blank paper (wall and flat) you write on like a sign. Off hides them; placed paper stays.");
        PaperCost = Synced(config, Paper, "PaperCost", "Wood:1",
            "Cost of one sheet: Item:amount pairs, comma separated.");
    }

    private static ConfigEntry<T> Synced<T>(ConfigFile config, string section, string key, T value, string description)
    {
        var entry = config.Bind(section, key, value, description);
        _sync.AddConfigEntry(entry).SynchronizedConfig = true;
        entry.SettingChanged += (_, _) => Changed?.Invoke();
        return entry;
    }

    /// <summary>ServerSync parses System.Version, which throws on "-beta.N" suffixes.</summary>
    private static string SystemVersion(string version)
    {
        var core = version.Split('-', '+')[0];
        return System.Version.TryParse(core, out var parsed) ? parsed.ToString() : "0.0.0";
    }
}

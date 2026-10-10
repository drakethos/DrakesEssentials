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
    private const string Racks = "5 - Armor and weapon racks";

    private static ConfigSync _sync = null!;

    public static ConfigEntry<bool> DisplayCasesEnabled = null!;
    public static ConfigEntry<string> DisplayCaseCost = null!;
    public static ConfigEntry<string> SmallDisplayCaseCost = null!;
    public static ConfigEntry<bool> PublicPiecesEnabled = null!;
    public static ConfigEntry<bool> PaperEnabled = null!;
    public static ConfigEntry<string> PaperCost = null!;
    public static ConfigEntry<bool> RacksEnabled = null!;
    private static readonly System.Collections.Generic.Dictionary<string, ConfigEntry<string>> RackCosts = new();

    /// <summary>A rack's build cost (config key per rack, see <see cref="ItemRacks.ItemRackPieces.Variants"/>).</summary>
    public static string RackCost(ItemRacks.ItemRackPieces.Variant rack) =>
        RackCosts.TryGetValue(rack.ConfigKey, out var entry) ? entry.Value : rack.DefaultCost;

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
        DisplayCaseCost = Synced(config, Cases, "DisplayCaseCost", "FineWood:10,JuteRed:2,Crystal:2",
            "Display Case cost: Item:amount pairs, comma separated (prefab names, e.g. Wood:10, Crystal:2).");
        SmallDisplayCaseCost = Synced(config, Cases, "SmallDisplayCaseCost", "FineWood:5,JuteRed:1,Crystal:1",
            "Small Display Case cost: Item:amount pairs, comma separated.");
        // 0.1.0 shipped placeholder costs; move configs still on them to the real defaults.
        if (DisplayCaseCost.Value == "Wood:10")
            DisplayCaseCost.Value = (string)DisplayCaseCost.DefaultValue;
        if (SmallDisplayCaseCost.Value == "Wood:5")
            SmallDisplayCaseCost.Value = (string)SmallDisplayCaseCost.DefaultValue;
        PublicPiecesEnabled = Synced(config, Public, "Enabled", true,
            "Always-public copies of every buildable door and chest (hammer tab Public): anyone can open them, wards or not. Off hides them; placed ones stay public.");
        PaperEnabled = Synced(config, Paper, "Enabled", true,
            "Blank paper (wall and flat) you write on like a sign. Off hides them; placed paper stays.");
        PaperCost = Synced(config, Paper, "PaperCost", "Wood:1",
            "Cost of one sheet: Item:amount pairs, comma separated.");
        RacksEnabled = Synced(config, Racks, "Enabled", true,
            "Armor racks and weapon racks in the hammer (Furniture tab). Off hides them; placed racks stay.");
        foreach (var rack in ItemRacks.ItemRackPieces.Variants)
            RackCosts[rack.ConfigKey] = Synced(config, Racks, rack.ConfigKey, rack.DefaultCost,
                $"{rack.Name.Replace("Drake_", "")} cost: Item:amount pairs, comma separated.");
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

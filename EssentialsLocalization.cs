using Jotunn.Managers;

namespace DrakesEssentials;

internal static class EssentialsLocalization
{
    public static void Add()
    {
        var loc = LocalizationManager.Instance.GetLocalization();
        loc.AddTranslation("English", "drake_display_case_name", "Display Case");
        loc.AddTranslation("English", "drake_display_case_desc", "Glass case to show your things (7 slots). Fill it from the door side.");
        loc.AddTranslation("English", "drake_display_case_small_name", "Small Display Case");
        loc.AddTranslation("English", "drake_display_case_small_desc", "Small glass case to show your things (3 slots). Fill it from the door side.");
        loc.AddTranslation("English", "drake_display_mat_name", "Display Mat");
        loc.AddTranslation("English", "drake_display_mat_desc", "A jute runner in an iron-bound tray (6 slots). Lay it on a table: what you put in it lies on show.");
        loc.AddTranslation("English", "drake_display_mat_small_name", "Small Display Mat");
        loc.AddTranslation("English", "drake_display_mat_small_desc", "A small jute runner in an iron-bound tray (3 slots). Lay it on a table: what you put in it lies on show.");
        loc.AddTranslation("English", "drake_mailbox_name", "Mailbox");
        loc.AddTranslation("English", "drake_mailbox_desc", "A Norse post box. Anyone can send items through the front slot; only those the ward allows can open the back.");
        loc.AddTranslation("English", "drake_dropbox_name", "Drop Box");
        loc.AddTranslation("English", "drake_dropbox_desc", "A slotted drop box for a table or counter. Customers send items through the lid; the owner opens the back.");
        loc.AddTranslation("English", "drake_armor_rack_name", "Armor Rack");
        loc.AddTranslation("English", "drake_armor_rack_desc", "Finewood rack to hang armor on (6 slots: hooks on top, pegs below). Armor only.");
        loc.AddTranslation("English", "drake_armor_rack_small_name", "Small Armor Rack");
        loc.AddTranslation("English", "drake_armor_rack_small_desc", "Small finewood rack to hang armor on (4 slots). Armor only.");
        loc.AddTranslation("English", "drake_wall_rack_name", "Wall Rack");
        loc.AddTranslation("English", "drake_wall_rack_desc", "Row of iron hooks on a finewood board (5 slots) for armor, weapons and shields.");
        loc.AddTranslation("English", "drake_wall_rack_small_name", "Small Wall Rack");
        loc.AddTranslation("English", "drake_wall_rack_small_desc", "Row of iron hooks on a finewood board (3 slots) for armor, weapons and shields.");
        loc.AddTranslation("English", "drake_weapon_rack_name", "Weapon Rack");
        loc.AddTranslation("English", "drake_weapon_rack_desc", "Finewood rack to hang weapons and shields on (6 slots). Weapons only.");
        loc.AddTranslation("English", "drake_weapon_rack_small_name", "Small Weapon Rack");
        loc.AddTranslation("English", "drake_weapon_rack_small_desc", "Small finewood rack to hang weapons and shields on (4 slots). Weapons only.");
        loc.AddTranslation("English", "drake_rack_armor_only", "Only armor goes on an armor rack");
        loc.AddTranslation("English", "drake_rack_weapons_only", "Only weapons and shields go on a weapon rack");
        loc.AddTranslation("English", "drake_rack_gear_only", "Only armor, weapons and shields go on a wall rack");
        loc.AddTranslation("English", "drake_essentials_paper_wall", "Paper (wall)");
        loc.AddTranslation("English", "drake_essentials_paper_wall_desc", "A blank sheet pinned to a wall. Write on it like a sign.");
        loc.AddTranslation("English", "drake_essentials_paper_flat", "Paper (flat)");
        loc.AddTranslation("English", "drake_essentials_paper_flat_desc", "A blank sheet lying flat on a table or floor. Write on it like a sign.");
        loc.AddTranslation("English", "drake_essentials_public_suffix", " (public)");
        loc.AddTranslation("English", "drake_essentials_public_desc", "<color=#FFCC33>Always open to anyone. This cannot be locked.</color>");
    }
}

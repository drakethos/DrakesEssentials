using Jotunn.Managers;

namespace DrakesEssentials;

internal static class EssentialsLocalization
{
    public static void Add()
    {
        var loc = LocalizationManager.Instance.GetLocalization();
        loc.AddTranslation("English", "drake_display_case_name", "Display Case");
        loc.AddTranslation("English", "drake_display_case_desc", "Glass case to show your things (6 slots). Fill it from the door side.");
        loc.AddTranslation("English", "drake_display_case_small_name", "Small Display Case");
        loc.AddTranslation("English", "drake_display_case_small_desc", "Small glass case to show your things (3 slots). Fill it from the door side.");
        loc.AddTranslation("English", "drake_essentials_paper_wall", "Paper (wall)");
        loc.AddTranslation("English", "drake_essentials_paper_wall_desc", "A blank sheet pinned to a wall. Write on it like a sign.");
        loc.AddTranslation("English", "drake_essentials_paper_flat", "Paper (flat)");
        loc.AddTranslation("English", "drake_essentials_paper_flat_desc", "A blank sheet lying flat on a table or floor. Write on it like a sign.");
        loc.AddTranslation("English", "drake_essentials_public_suffix", " (public)");
        loc.AddTranslation("English", "drake_essentials_public_desc", "<color=#FFCC33>Always open to anyone. This cannot be locked.</color>");
    }
}

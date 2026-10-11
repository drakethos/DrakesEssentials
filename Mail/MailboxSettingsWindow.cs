// Copied from DrakesItemShop (Mailbox/MailboxMenuTab.cs, as its own window) without the PO box parts; keep in step with it.
using System;
using DrakesEssentials.Ui;
using UnityEngine;
using UnityEngine.UIElements;

namespace DrakesEssentials.Mail;

/// <summary>Shift+E on a mailbox: its sign, whether the slot is open, who may send, what it takes, and who sent what.</summary>
internal static class MailboxSettingsWindow
{
    private static UkWindow? _window;

    internal static void Open(Container container, MailboxBehaviour mail, Player player)
    {
        _window?.CloseSilent();
        var window = new UkWindow("drakes_essentials_mail_settings", 560);
        _window = window;
        window.SetHeader(mail.SignName(global::Localization.instance.Localize(container.m_name)), "Mail settings");
        Draw(window, mail, player);
        window.Footer.Add(UkControls.Spacer());
        window.Footer.Add(UkControls.MakeButton("Close", () => window.Close(), UkButtonKind.Secondary));
        window.ShowFooter(true);
        window.Open();
    }

    private static void Draw(UkWindow window, MailboxBehaviour mail, Player player)
    {
        window.Body.Clear();
        var body = new VisualElement().Column();
        // The builder changes the settings; whoever may open the back (wards) can read them.
        var canManage = mail.IsBuilder(player.GetPlayerID());
        Build(body, mail, canManage, () => Draw(window, mail, player));
        window.Body.Add(body);
    }

    private static void Build(VisualElement body, MailboxBehaviour mail, bool canManage, Action rebuild)
    {
        // Where the back door stands in the world: the point of a mailbox is that only the owner can open it.
        var protectedBox = mail.IsProtected;
        var banner = UkControls.Card(protectedBox ? UkTheme.PanelDeep : UkTheme.DangerBg, border: true).Column().Pad(12);
        banner.style.marginBottom = 14;
        banner.Add(UkControls.Text(protectedBox ? "Back door protected" : "Back door not protected", 16,
            protectedBox ? UkTheme.Ok : UkTheme.DangerText, heading: true, wrap: false));
        banner.Add(UkControls.Text(protectedBox
            ? "This box is inside a ward, so only people the ward allows can open the back."
            : "This box is not inside a ward. Anyone can walk round and open the back to take everything sent here, which defeats the mailbox. Build it inside a ward.",
            14, UkTheme.TextBody));
        body.Add(banner);

        var sign = new UkField("Sign", multiline: false);
        sign.SetMax(24);
        sign.SetValue(mail.SignRaw);
        sign.Changed = text => mail.SetSign(text);
        sign.Root.style.marginBottom = 4;
        body.Add(sign.Root);
        body.Add(UkControls.Text("Shows on the hover text and the send window. Leave it empty for your name.", 13, UkTheme.TextMuted));

        var allow = new UkField("Who can send", multiline: false);
        allow.SetValue(mail.AllowText);
        allow.Changed = text => mail.AllowText = text;
        allow.Root.style.marginTop = 14;
        body.Add(allow.Root);
        body.Add(UkControls.Text("Player names, separated by commas. Empty means anyone. You can always send.", 13, UkTheme.TextMuted));

        var filter = new UkField("Only takes", multiline: false);
        filter.SetValue(mail.FilterText);
        filter.Changed = text => mail.FilterText = text;
        filter.Root.style.marginTop = 14;
        body.Add(filter.Root);
        body.Add(UkControls.Text("Item names, separated by commas, like Coins, Iron. Empty means anything.", 13, UkTheme.TextMuted));

        var accept = new UkSwitch("Open for mail", "Off closes the send slot: nobody can send until you turn it back on.");
        accept.SetValue(mail.AcceptOn, notify: false);
        accept.Changed = on => mail.AcceptOn = on;
        accept.Root.style.marginTop = 14;
        body.Add(accept.Root);

        var notify = new UkSwitch("Tell me when mail arrives", "A message on your screen when someone sends something and you're nearby.");
        notify.SetValue(mail.NotifyOn, notify: false);
        notify.Changed = on => mail.NotifyOn = on;
        notify.Root.style.marginTop = 8;
        body.Add(notify.Root);

        if (!canManage)
        {
            sign.Input.SetEnabled(false);
            allow.Input.SetEnabled(false);
            filter.Input.SetEnabled(false);
            accept.Root.SetEnabled(false);
            notify.Root.SetEnabled(false);
            var only = UkControls.Text("Only the player who built this mailbox can change these.", 13, UkTheme.TextMuted);
            only.style.marginTop = 8;
            body.Add(only);
        }

        // Recent deliveries, newest first. The items stay behind the door.
        var title = UkControls.Text("RECENT MAIL", 12, UkTheme.TextMuted, wrap: false);
        title.style.marginTop = 16;
        title.style.marginBottom = 6;
        body.Add(title);
        var recent = mail.RecentMail();
        var card = UkControls.Card().Column().Pad(12);
        if (recent.Count == 0)
        {
            card.Add(UkControls.Text("Nothing yet.", 14, UkTheme.TextMuted));
        }
        else
        {
            foreach (var (from, stacks, ago, note) in recent)
            {
                var line = new VisualElement().Row();
                line.Add(UkControls.Text(from, 15, UkTheme.Text, wrap: false));
                line.Add(UkControls.Spacer());
                line.Add(UkControls.Text($"{stacks} stack{(stacks == 1 ? "" : "s")}, {Ago(ago)}", 14, UkTheme.TextMuted, wrap: false));
                card.Add(line);
                if (!string.IsNullOrEmpty(note))
                {
                    var noteLine = UkControls.Text("“" + note + "”", 14, UkTheme.TextBody);
                    noteLine.style.marginBottom = 6;
                    card.Add(noteLine);
                }
            }
        }
        body.Add(card);

        var capacity = mail.Box != null ? mail.Box.GetInventory() : null;
        if (capacity != null)
        {
            var used = capacity.GetAllItems().Count;
            var slots = capacity.GetWidth() * capacity.GetHeight();
            var count = UkControls.Text($"{used} of {slots} slots used behind the door", 14, UkTheme.TextMuted, wrap: false);
            count.style.marginTop = 10;
            body.Add(count);
        }

        if (canManage && recent.Count > 0)
        {
            var clear = UkControls.MakeButton("Clear the list", () =>
            {
                mail.ClearLog();
                rebuild();
            }, UkButtonKind.Ghost);
            clear.style.marginTop = 10;
            clear.style.alignSelf = Align.FlexStart;
            body.Add(clear);
        }
    }

    private static string Ago(TimeSpan span) =>
        span.TotalMinutes < 1 ? "just now"
        : span.TotalHours < 1 ? $"{(int)span.TotalMinutes} min ago"
        : span.TotalDays < 1 ? $"{(int)span.TotalHours} h ago"
        : $"{(int)span.TotalDays} d ago";
}

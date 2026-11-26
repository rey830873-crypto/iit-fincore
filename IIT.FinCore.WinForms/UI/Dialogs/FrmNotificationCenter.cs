using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Dialogs;

/// <summary>
/// Centre de Notifications commercial IIT FINCORE :
/// Filtres (Toutes / Non lues / Critiques), cartes interactives avec barre de gravité,
/// marquage comme lu et navigation directe vers le module concerné.
/// </summary>
public class FrmNotificationCenter : Form
{
    private readonly NotificationService _service = new();
    private readonly Action<string> _onNavigate;
    private readonly FlowLayoutPanel _feed = new();
    private readonly List<Button> _filterBtns = new();
    private List<NotificationItem> _all = new();
    private string _activeFilter = "ALL"; // ALL, UNREAD, CRITIQUE

    public FrmNotificationCenter(Action<string> onNavigate)
    {
        _onNavigate = onNavigate;
        Text = "IIT FINCORE - Centre de Notifications";
        Size = new Size(580, 620);
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        BuildLayout();
        LoadNotifications();
    }

    private void BuildLayout()
    {
        var header = Theme.CreateDialogHeader(
            "Centre de Notifications",
            "Alertes budgétaires, validations, cotisations et suivi temps réel");
        Theme.ApplyModernDialogChrome(this, header);

        // Barre de filtres + Tout marquer comme lu
        var toolBar = new Panel
        {
            Dock = DockStyle.Top,
            Height = 52,
            BackColor = Color.White,
            Padding = new Padding(16, 10, 16, 8)
        };
        toolBar.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, toolBar.Height - 1, toolBar.Width, toolBar.Height - 1);
        };

        var leftFilters = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 310,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        (string Key, string Label)[] filters =
        {
            ("ALL", "Toutes"),
            ("UNREAD", "Non lues"),
            ("CRITIQUE", "Critiques")
        };

        foreach (var f in filters)
        {
            var btn = new Button
            {
                Text = f.Label,
                Tag = f.Key,
                Size = new Size(92, 32),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFont(8.8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 8, 0)
            };
            btn.FlatAppearance.BorderSize = 0;
            Theme.ApplyRoundedRegion(btn, 8);
            btn.Click += (_, _) =>
            {
                _activeFilter = f.Key;
                UpdateFilterStyles();
                RenderFeed();
            };
            _filterBtns.Add(btn);
            leftFilters.Controls.Add(btn);
        }

        var btnMarkAll = Theme.CreateSecondaryButton("Tout marquer lu", 136, 32);
        btnMarkAll.Dock = DockStyle.Right;
        btnMarkAll.Click += (_, _) =>
        {
            _service.MarkAllAsRead();
            LoadNotifications();
        };

        toolBar.Controls.Add(leftFilters);
        toolBar.Controls.Add(btnMarkAll);

        // Liste scrollable des notifications
        _feed.Dock = DockStyle.Fill;
        _feed.FlowDirection = FlowDirection.TopDown;
        _feed.WrapContents = false;
        _feed.AutoScroll = true;
        _feed.BackColor = Theme.PageBg;
        _feed.Padding = new Padding(16, 12, 16, 12);

        // Pied de fenêtre
        var footer = new Panel
        {
            Dock = DockStyle.Bottom,
            Height = 52,
            BackColor = Color.White,
            Padding = new Padding(16, 9, 16, 9)
        };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };

        var lblHint = new Label
        {
            Text = "Cliquez sur une notification pour ouvrir directement le module concerné.",
            Font = Theme.UiFont(8.4F),
            ForeColor = Theme.TextMuted,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleLeft
        };
        var btnClose = Theme.CreatePrimaryButton("Fermer", 110, 34);
        btnClose.Dock = DockStyle.Right;
        btnClose.Click += (_, _) => Close();
        footer.Controls.Add(lblHint);
        footer.Controls.Add(btnClose);

        Controls.Add(_feed);
        Controls.Add(toolBar);
        Controls.Add(header);
        Controls.Add(footer);

        UpdateFilterStyles();
    }

    private void UpdateFilterStyles()
    {
        foreach (var b in _filterBtns)
        {
            bool active = (b.Tag?.ToString() == _activeFilter);
            b.BackColor = active ? Theme.SidebarNavy : Color.FromArgb(242, 245, 250);
            b.ForeColor = active ? Theme.GoldLight : Theme.TextPrimary;
        }
    }

    private void LoadNotifications()
    {
        _service.SyncAutomaticAlerts();
        _all = _service.GetNotificationsForCurrentUser(50);
        RenderFeed();
    }

    private void RenderFeed()
    {
        _feed.SuspendLayout();
        _feed.Controls.Clear();

        IEnumerable<NotificationItem> list = _activeFilter switch
        {
            "UNREAD" => _all.Where(n => !n.EstLue),
            "CRITIQUE" => _all.Where(n => n.Niveau is "CRITIQUE" or "ALERTE"),
            _ => _all
        };

        var items = list.ToList();
        if (items.Count == 0)
        {
            var empty = new Label
            {
                Text = _activeFilter == "UNREAD"
                    ? "Aucune notification non lue. Tout est à jour !"
                    : "Aucune alerte à afficher pour ce filtre.",
                Font = Theme.FontBodyBold,
                ForeColor = Theme.TextMuted,
                Size = new Size(520, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _feed.Controls.Add(empty);
            _feed.ResumeLayout();
            return;
        }

        int cardW = 526;
        foreach (var n in items)
        {
            var card = CreateNotificationCard(n, cardW);
            _feed.Controls.Add(card);
        }
        _feed.ResumeLayout();
    }

    private Control CreateNotificationCard(NotificationItem n, int width)
    {
        Color accent = n.Niveau switch
        {
            "CRITIQUE" => Theme.DangerRed,
            "ALERTE" => Theme.GoldAccent,
            "SUCCES" => Theme.SuccessGreen,
            _ => Theme.SidebarNavyLight
        };
        string badgeLabel = n.Niveau switch
        {
            "CRITIQUE" => "CRITIQUE",
            "ALERTE" => "ALERTE",
            "SUCCES" => "SUCCÈS",
            _ => "INFO"
        };

        bool hovered = false;
        var card = new SmoothPanel
        {
            Size = new Size(width, 94),
            Margin = new Padding(0, 0, 0, 10),
            BackColor = n.EstLue ? Color.FromArgb(248, 250, 253) : Color.White,
            Cursor = Cursors.Hand
        };

        card.MouseEnter += (_, _) => { hovered = true; card.Invalidate(); };
        card.MouseLeave += (_, _) => { hovered = false; card.Invalidate(); };
        card.Click += (_, _) =>
        {
            if (!n.EstLue) _service.MarkAsRead(n.Id);
            var target = n.ModuleCible;
            Close();
            _onNavigate(target);
        };

        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var r = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var path = Theme.CreateRoundedPath(r, 12);
            using var bg = new SolidBrush(hovered ? Theme.GoldSoftBg : card.BackColor);
            g.FillPath(bg, path);

            using var border = new Pen(hovered ? Theme.GoldAccent : (n.EstLue ? Theme.BorderColor : Color.FromArgb(190, accent)), hovered ? 1.5f : 1f);
            g.DrawPath(border, path);

            // Barre verticale de gravité à gauche
            using var barPath = Theme.CreateRoundedPath(new Rectangle(6, 10, 5, card.Height - 20), 3);
            g.FillPath(new SolidBrush(accent), barPath);

            // Pastille niveau
            var badgeRect = new Rectangle(20, 10, 72, 18);
            using var bp = Theme.CreateRoundedPath(badgeRect, 6);
            g.FillPath(new SolidBrush(Color.FromArgb(28, accent)), bp);
            TextRenderer.DrawText(g, badgeLabel, Theme.UiFont(7.2F, FontStyle.Bold),
                badgeRect, accent, TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            // Horodatage à droite
            var timeStr = NotificationService.FormatRelativeTime(n.DateCreation);
            TextRenderer.DrawText(g, timeStr, Theme.UiFont(7.8F, FontStyle.Regular),
                new Rectangle(card.Width - 170, 10, 146, 18), Theme.TextMuted,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            // Point non lu
            if (!n.EstLue)
                g.FillEllipse(new SolidBrush(accent), card.Width - 18, 15, 8, 8);

            // Titre
            TextRenderer.DrawText(g, n.Titre, Theme.UiFont(9.4F, FontStyle.Bold),
                new Rectangle(20, 32, card.Width - 36, 20), Theme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // Message (2 lignes)
            TextRenderer.DrawText(g, n.Message, Theme.UiFont(8.5F, FontStyle.Regular),
                new Rectangle(20, 53, card.Width - 36, 34), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        };

        return card;
    }
}

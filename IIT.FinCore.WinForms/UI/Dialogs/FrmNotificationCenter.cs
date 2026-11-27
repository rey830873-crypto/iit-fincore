using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Dialogs;

/// <summary>
/// Centre de Notifications commercial IIT FINCORE :
/// Filtres (Toutes / Non lues / Critiques), cartes interactives avec barre de gravité,
/// suppression individuelle, nettoyage des lues, partage WhatsApp 1-clic et navigation directe.
/// </summary>
public partial class FrmNotificationCenter : Form
{
    private readonly NotificationService _service = new();
    private readonly Action<string> _onNavigate = _ => { };
    private readonly FlowLayoutPanel _feed = new();
    private readonly List<Button> _filterBtns = new();
    private List<NotificationItem> _all = new();
    private string _activeFilter = "ALL"; // ALL, UNREAD, CRITIQUE

    public FrmNotificationCenter() : this(_ => { }) { }

    public FrmNotificationCenter(Action<string> onNavigate)
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        _onNavigate = onNavigate;
        Text = "IIT FINCORE - Centre de Notifications";
        Size = new Size(630, 640);
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        BuildLayout();
        LoadNotifications();
    }

    private void BuildLayout()
    {
        var header = Theme.CreateDialogHeader(
            "Centre de Notifications",
            "Alertes budgétaires, soldes bas, échéances J-3, validations et suivi temps réel");
        Theme.ApplyModernDialogChrome(this, header);

        // Barre de filtres + actions de nettoyage
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
            Width = 285,
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
                Size = new Size(86, 32),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFont(8.8F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 0, 6, 0)
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

        var rightActions = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        var btnMarkAll = Theme.CreateSecondaryButton("Tout lu", 88, 32);
        btnMarkAll.Margin = new Padding(0, 0, 6, 0);
        btnMarkAll.Click += (_, _) =>
        {
            _service.MarkAllAsRead();
            LoadNotifications();
        };

        var btnClearRead = Theme.CreateSecondaryButton("Effacer lues", 106, 32);
        btnClearRead.Margin = new Padding(0);
        btnClearRead.Click += (_, _) =>
        {
            _service.ClearReadNotifications();
            LoadNotifications();
        };

        rightActions.Controls.Add(btnMarkAll);
        rightActions.Controls.Add(btnClearRead);

        toolBar.Controls.Add(leftFilters);
        toolBar.Controls.Add(rightActions);

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
            Text = "Cliquez sur une alerte pour ouvrir le module concerné • WhatsApp pour partager.",
            Font = Theme.UiFont(8.3F),
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
                Size = new Size(570, 80),
                TextAlign = ContentAlignment.MiddleCenter,
                BackColor = Color.Transparent
            };
            _feed.Controls.Add(empty);
            _feed.ResumeLayout();
            return;
        }

        int cardW = 576;
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

        bool isImportant = n.Niveau is "CRITIQUE" or "ALERTE";
        bool hovered = false;
        var card = new SmoothPanel
        {
            Size = new Size(width, isImportant ? 104 : 94),
            Margin = new Padding(0, 0, 0, 10),
            BackColor = n.EstLue ? Color.FromArgb(248, 250, 253) : Color.White,
            Cursor = Cursors.Hand
        };

        // Bouton discret × pour supprimer la notification
        var btnDel = new Button
        {
            Text = "×",
            Size = new Size(24, 24),
            Location = new Point(width - 32, 8),
            FlatStyle = FlatStyle.Flat,
            Font = Theme.UiFont(10.5F, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnDel.FlatAppearance.BorderSize = 0;
        btnDel.FlatAppearance.MouseOverBackColor = Color.FromArgb(254, 226, 226);
        Theme.ApplyRoundedRegion(btnDel, 6);
        btnDel.Click += (_, _) =>
        {
            _service.DeleteNotification(n.Id);
            LoadNotifications();
        };
        card.Controls.Add(btnDel);

        // Bouton Partager WhatsApp en 1 clic sur les alertes critiques / importantes
        if (isImportant)
        {
            var btnWa = new Button
            {
                Text = "WhatsApp",
                Size = new Size(80, 22),
                Location = new Point(width - 92, card.Height - 28),
                FlatStyle = FlatStyle.Flat,
                Font = Theme.UiFont(7.6F, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Theme.SuccessGreen,
                Cursor = Cursors.Hand
            };
            btnWa.FlatAppearance.BorderSize = 0;
            btnWa.FlatAppearance.MouseOverBackColor = ControlPaint.Dark(Theme.SuccessGreen, 0.1f);
            Theme.ApplyRoundedRegion(btnWa, 6);
            btnWa.Click += (_, _) => ShareOnWhatsApp(n);
            card.Controls.Add(btnWa);
        }

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

            // Point non lu à côté du badge
            if (!n.EstLue)
                g.FillEllipse(new SolidBrush(accent), 100, 15, 8, 8);

            // Horodatage à droite (avant le bouton ×)
            var timeStr = NotificationService.FormatRelativeTime(n.DateCreation);
            TextRenderer.DrawText(g, timeStr, Theme.UiFont(7.8F, FontStyle.Regular),
                new Rectangle(card.Width - 196, 10, 156, 18), Theme.TextMuted,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            // Titre
            TextRenderer.DrawText(g, n.Titre, Theme.UiFont(9.4F, FontStyle.Bold),
                new Rectangle(20, 32, card.Width - 54, 20), Theme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            // Message
            int msgW = isImportant ? card.Width - 120 : card.Width - 36;
            TextRenderer.DrawText(g, n.Message, Theme.UiFont(8.5F, FontStyle.Regular),
                new Rectangle(20, 53, msgW, 42), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        };

        return card;
    }

    private static void ShareOnWhatsApp(NotificationItem n)
    {
        var text = $"[IIT FINCORE - {n.Niveau}]\n{n.Titre}\n{n.Message}\n({n.DateCreation:dd/MM/yyyy HH:mm})";
        var url = "https://wa.me/?text=" + Uri.EscapeDataString(text);
        try
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
        catch (Exception ex)
        {
            MessageBox.Show("Impossible d'ouvrir WhatsApp : " + ex.Message, "Partage WhatsApp", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}

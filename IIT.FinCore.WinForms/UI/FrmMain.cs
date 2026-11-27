using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Controls;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI;

public partial class FrmMain : Form, IMessageFilter
{
    public bool RequestedLogout { get; private set; }

    private readonly SmoothPanel _sidebar = new();
    private readonly SmoothPanel _topbar = new();
    private readonly SmoothPanel _contentHost = new();
    private Panel _titlePanel = null!;
    private FlowLayoutPanel _navPanel = null!;
    private readonly Label _lblPageTitle = new();
    private readonly Label _lblPageSubtitle = new();
    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly Dictionary<string, (string Title, string Subtitle, Form Control)> _viewCache = new();
    private static readonly Dictionary<Type, MethodInfo?> _refreshMethodCache = new();
    private readonly System.Windows.Forms.Timer _idleTimer = new();
    private readonly System.Windows.Forms.Timer _notifTimer = new();
    private readonly System.Windows.Forms.Timer _navAnimTimer = new();
    private readonly NotificationService _notifService = new();
    private NotifyIcon? _trayIcon;
    private SmoothPanel _notifBadgeBtn = null!;
    private int _unreadNotifCount;
    private int _notifSyncRunning;
    private float _navIndicatorY = -1f;
    private float _navStartY;
    private float _navTargetY;
    private float _navAnimProgress = 1f;
    private int _navAnimStep;
    private const int NavAnimTotalSteps = 11; // ~175ms à 60 FPS
    private DateTime _lastActivityUtc = DateTime.UtcNow;
    private static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(15);
    private string _currentKey = "dashboard";

    public FrmMain()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        var roleLabel = UserSession.CurrentUser?.RoleNom ?? "";
        Text = $"IIT FINCORE - Espace {roleLabel}";
        Icon = Theme.LoadAppIcon();
        Size = new Size(1340, 800);
        MinimumSize = new Size(1160, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.PageBg;
        DoubleBuffered = true;
        KeyPreview = true;
        Opacity = 0;

        BuildShell();
        InitNavigationAnimator();
        Theme.ApplyCrispPixelFonts(this);
        Navigate("dashboard");
        StartInactivityGuard();
        StartNotificationGuard();
        KeyDown += HandleGlobalShortcuts;

        var openFade = new System.Windows.Forms.Timer { Interval = 16 };
        openFade.Tick += (_, _) =>
        {
            if (IsDisposed) { openFade.Stop(); openFade.Dispose(); return; }
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.18);
            else { openFade.Stop(); openFade.Dispose(); }
        };
        Shown += (_, _) =>
        {
            TriggerNavigationAnimation(_currentKey, instant: true);
            openFade.Start();
        };
        FormClosed += (_, _) =>
        {
            openFade.Stop();
            openFade.Dispose();
            _navAnimTimer.Stop();
            _navAnimTimer.Dispose();
        };
    }

    private void InitNavigationAnimator()
    {
        _navAnimTimer.Interval = 16;
        _navAnimTimer.Tick += (_, _) =>
        {
            _navAnimStep++;
            float raw = Math.Min(1f, _navAnimStep / (float)NavAnimTotalSteps);
            float eased = Theme.EaseOutCubic(raw);
            _navAnimProgress = eased;

            _navIndicatorY = _navStartY + (_navTargetY - _navStartY) * eased;

            int slideX = (int)MathF.Round(16f * (1f - eased));
            if (_titlePanel != null && _titlePanel.Padding.Left != slideX)
                _titlePanel.Padding = new Padding(slideX, 2, 16, 0);

            _sidebar.Invalidate(new Rectangle(0, 0, 14, _sidebar.Height));
            _topbar.Invalidate(new Rectangle(0, _topbar.Height - 5, _topbar.Width, 5));

            if (_navAnimStep >= NavAnimTotalSteps)
            {
                _navAnimTimer.Stop();
                _navAnimProgress = 1f;
                if (_titlePanel != null && _titlePanel.Padding.Left != 0)
                    _titlePanel.Padding = new Padding(0, 2, 16, 0);
                _topbar.Invalidate(new Rectangle(0, _topbar.Height - 5, _topbar.Width, 5));
            }
        };
    }

    private void TriggerNavigationAnimation(string key, bool instant)
    {
        if (_navButtons.TryGetValue(key, out var activeBtn) && _navPanel != null)
        {
            float targetY = _navPanel.Top + activeBtn.Top + (activeBtn.Height - 22f) / 2f;
            if (instant || _navIndicatorY < 0f)
            {
                _navIndicatorY = _navStartY = _navTargetY = targetY;
                _navAnimProgress = 1f;
                _sidebar.Invalidate(new Rectangle(0, 0, 14, _sidebar.Height));
                return;
            }
            _navStartY = _navIndicatorY;
            _navTargetY = targetY;
        }
        else if (instant)
        {
            return;
        }

        _navAnimStep = 0;
        _navAnimProgress = 0f;
        if (_titlePanel != null)
            _titlePanel.Padding = new Padding(16, 2, 16, 0);
        _navAnimTimer.Start();
    }

    private void StartNotificationGuard()
    {
        // Icône système Windows (zone de notification près de l'horloge)
        var trayMenu = new ContextMenuStrip();
        trayMenu.Items.Add("Ouvrir IIT FINCORE", null, (_, _) => RestoreWindow());
        trayMenu.Items.Add("Centre de Notifications", null, (_, _) => { RestoreWindow(); OpenNotificationCenter(); });
        trayMenu.Items.Add(new ToolStripSeparator());
        trayMenu.Items.Add("Quitter", null, (_, _) => Close());

        _trayIcon = new NotifyIcon
        {
            Icon = Theme.LoadAppIcon() ?? SystemIcons.Application,
            Text = "IIT FINCORE - Gestion de trésorerie",
            ContextMenuStrip = trayMenu,
            Visible = true
        };
        _trayIcon.DoubleClick += (_, _) => RestoreWindow();
        _trayIcon.BalloonTipClicked += (_, _) => { RestoreWindow(); OpenNotificationCenter(); };

        Shown += (_, _) =>
        {
            _ = Task.Run(() =>
            {
                _notifService.SyncAutomaticAlerts();
                int count = _notifService.GetUnreadCount();
                var unread = _notifService.GetNotificationsForCurrentUser(5).Where(n => !n.EstLue).ToList();

                if (IsDisposed || !IsHandleCreated) return;
                BeginInvoke(() =>
                {
                    if (IsDisposed) return;
                    _unreadNotifCount = count;
                    _notifBadgeBtn?.Invalidate();
                    if (unread.Count > 0)
                    {
                        var top = unread[0];
                        var suffix = unread.Count > 1 ? $" (+{unread.Count - 1} autre(s))" : "";
                        NotifyUser($"{top.Titre}{suffix}", top.Message, top.Niveau);
                    }
                });
            });
        };

        _notifTimer.Interval = 45_000; // Vérifie toutes les 45 secondes en arrière-plan (sans bloquer l'UI)
        _notifTimer.Tick += (_, _) =>
        {
            if (Interlocked.CompareExchange(ref _notifSyncRunning, 1, 0) != 0)
                return;

            int previous = _unreadNotifCount;
            _ = Task.Run(() =>
            {
                try
                {
                    _notifService.SyncAutomaticAlerts();
                    int count = _notifService.GetUnreadCount();
                    var latest = count > previous ? _notifService.GetNotificationsForCurrentUser(1).FirstOrDefault() : null;

                    if (IsDisposed || !IsHandleCreated) return;
                    BeginInvoke(() =>
                    {
                        if (IsDisposed) return;
                        _unreadNotifCount = count;
                        _notifBadgeBtn?.Invalidate();
                        if (latest != null)
                            NotifyUser(latest.Titre, latest.Message, latest.Niveau);
                    });
                }
                finally
                {
                    Interlocked.Exchange(ref _notifSyncRunning, 0);
                }
            });
        };
        _notifTimer.Start();
        FormClosed += (_, _) =>
        {
            _notifTimer.Stop();
            foreach (var entry in _viewCache.Values)
                entry.Control.Dispose();
            _viewCache.Clear();

            if (_trayIcon != null)
            {
                _trayIcon.Visible = false;
                _trayIcon.Dispose();
                _trayIcon = null;
            }
        };
    }

    private void RestoreWindow()
    {
        if (WindowState == FormWindowState.Minimized)
            WindowState = FormWindowState.Normal;
        Activate();
    }

    private void NotifyUser(string title, string message, string niveau)
    {
        var prefs = _notifService.GetPreferences();
        if (prefs.SoundEnabled && niveau == "CRITIQUE")
            System.Media.SystemSounds.Exclamation.Play();

        if (WindowState == FormWindowState.Minimized && _trayIcon != null)
        {
            var icon = niveau == "CRITIQUE" ? ToolTipIcon.Error : (niveau == "ALERTE" ? ToolTipIcon.Warning : ToolTipIcon.Info);
            _trayIcon.ShowBalloonTip(4500, title, message, icon);
            return;
        }

        if (prefs.ToastEnabled)
        {
            var toast = new FrmToastNotification(this, title, message, niveau, OpenNotificationCenter);
            toast.Show(this);
        }
    }

    private void RefreshNotificationBadge()
    {
        _ = Task.Run(() =>
        {
            int count = _notifService.GetUnreadCount();
            if (IsDisposed || !IsHandleCreated) return;
            BeginInvoke(() =>
            {
                if (IsDisposed) return;
                _unreadNotifCount = count;
                _notifBadgeBtn?.Invalidate();
            });
        });
    }

    private void OpenNotificationCenter()
    {
        using var dlg = new FrmNotificationCenter(targetModule =>
        {
            if (_navButtons.ContainsKey(targetModule))
                Navigate(targetModule);
        });
        dlg.ShowDialog(this);
        RefreshNotificationBadge();
    }

    private void HandleGlobalShortcuts(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.F5)
        {
            e.Handled = true;
            Navigate(_currentKey);
        }
        else if (e.Control && e.KeyCode == Keys.N && (UserSession.HasPermission("RECETTE_CREATE") || UserSession.HasPermission("DEPENSE_CREATE")))
        {
            e.Handled = true;
            using var dlg = new FrmTransactionDialog("RECETTE");
            if (dlg.ShowDialog(this) == DialogResult.OK) Navigate(_currentKey);
        }
        else if (e.Control && e.KeyCode == Keys.T && (UserSession.HasPermission("RECETTE_CREATE") || UserSession.HasPermission("DEPENSE_CREATE")))
        {
            e.Handled = true;
            using var dlg = new FrmTransfertDialog();
            if (dlg.ShowDialog(this) == DialogResult.OK) Navigate(_currentKey);
        }
        else if (e.Control && e.KeyCode == Keys.D1) { e.Handled = true; Navigate("dashboard"); }
        else if (e.Control && e.KeyCode == Keys.D2) { e.Handled = true; Navigate("comptes"); }
        else if (e.Control && e.KeyCode == Keys.D3) { e.Handled = true; Navigate("transactions"); }
        else if (e.Control && e.KeyCode == Keys.D4) { e.Handled = true; Navigate("rapports"); }
        else if (e.Control && e.KeyCode == Keys.I) { e.Handled = true; OpenNotificationCenter(); }
    }

    private void StartInactivityGuard()
    {
        Application.AddMessageFilter(this);
        _idleTimer.Interval = 15_000; // Vérifie toutes les 15 secondes
        _idleTimer.Tick += (_, _) =>
        {
            if (DateTime.UtcNow - _lastActivityUtc >= InactivityTimeout)
            {
                _idleTimer.Stop();
                Application.RemoveMessageFilter(this);
                new Data.AuthRepository().LogActivity(UserSession.CurrentUser?.Id, "Verrouillage inactivité", "Sécurité", null, null, "Session expirée après 15 min d'inactivité");
                MessageBox.Show("Votre session a expiré après 15 minutes d'inactivité par mesure de sécurité.", "IIT FINCORE - Sécurité", MessageBoxButtons.OK, MessageBoxIcon.Information);
                UserSession.Clear();
                RequestedLogout = true;
                Close();
            }
        };
        _idleTimer.Start();
        FormClosed += (_, _) =>
        {
            _idleTimer.Stop();
            Application.RemoveMessageFilter(this);
        };
    }

    public bool PreFilterMessage(ref Message m)
    {
        // Réinitialise le délai d'inactivité à chaque mouvement/clic souris ou touche clavier
        const int WM_KEYDOWN = 0x0100, WM_LBUTTONDOWN = 0x0201, WM_RBUTTONDOWN = 0x0204, WM_MOUSEWHEEL = 0x020A;
        if (m.Msg is WM_KEYDOWN or WM_LBUTTONDOWN or WM_RBUTTONDOWN or WM_MOUSEWHEEL)
            _lastActivityUtc = DateTime.UtcNow;
        return false;
    }

    private Color GetRoleAccentColor() =>
        UserSession.IsAdmin ? Theme.GoldLight
        : UserSession.IsTresorier ? Theme.SuccessGreen
        : Color.FromArgb(96, 165, 250); // Bleu clair pour Consultant

    private void BuildShell()
    {
        var roleAccent = GetRoleAccentColor();

        // ==================== SIDEBAR ROYAL SAPPHIRE DÉGRADÉ ====================
        _sidebar.Dock = DockStyle.Left;
        _sidebar.Width = 258;
        _sidebar.BackColor = Theme.SidebarNavy;
        _sidebar.Paint += (_, e) =>
        {
            var navyBg = Theme.LoadNavyBackground();
            if (navyBg != null)
            {
                Theme.DrawCachedBackground(e.Graphics, _sidebar, navyBg, Color.FromArgb(110, 5, 20, 46));
            }
            else
            {
                using var bg = new LinearGradientBrush(_sidebar.ClientRectangle, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(bg, _sidebar.ClientRectangle);
            }
            using var rightEdge = new Pen(Color.FromArgb(45, roleAccent), 1);
            e.Graphics.DrawLine(rightEdge, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);

            // Indicateur vertical animé glissant le long du menu actif
            if (_navIndicatorY >= 0f)
            {
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
                var pillRect = new Rectangle(5, (int)MathF.Round(_navIndicatorY), 4, 22);
                using var pillPath = Theme.CreateRoundedPath(pillRect, 2);
                using var pillBrush = new SolidBrush(roleAccent);
                e.Graphics.FillPath(pillBrush, pillPath);
            }
        };

        // En-tête de marque + badge d'espace de rôle bien distinct
        var brandPanel = new Panel { Dock = DockStyle.Top, Height = 132, BackColor = Color.Transparent };
        brandPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            Theme.DrawRoundedLogo(g, new Rectangle(16, 14, 48, 48), 11);

            var fontBrand = Theme.UiFont(13.5F, FontStyle.Bold);
            var typo = StringFormat.GenericTypographic;
            g.DrawString("IIT FIN", fontBrand, Brushes.White, 74, 15, typo);
            var w1 = g.MeasureString("IIT FIN", fontBrand, PointF.Empty, typo).Width;
            g.DrawString("CORE", fontBrand, new SolidBrush(Theme.GoldLight), 74 + w1, 15, typo);

            g.DrawString("GESTION FINANCIÈRE", Theme.UiFont(7.3F, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(180, 200, 225)), 76, 42);

            // Badge d'espace spécifique au rôle connecté
            var badgeRect = new Rectangle(16, 74, brandPanel.Width - 32, 44);
            using var badgePath = Theme.CreateRoundedPath(badgeRect, 9);
            g.FillPath(new SolidBrush(Color.FromArgb(30, roleAccent)), badgePath);
            g.DrawPath(new Pen(Color.FromArgb(150, roleAccent), 1.2f), badgePath);

            string spaceTitle = UserSession.IsAdmin
                ? "ESPACE ADMINISTRATEUR"
                : UserSession.IsTresorier
                    ? "ESPACE TRÉSORIER"
                    : "ESPACE CONSULTANT";

            string spaceSub = UserSession.IsAdmin
                ? "Contrôle global multi-structures"
                : UserSession.IsTresorier
                    ? $"Structure : {UserSession.CurrentUser?.StructureNom ?? "Affectée"}"
                    : "Mode lecture seule & exports";

            g.DrawString(spaceTitle, Theme.UiFont(8.4F, FontStyle.Bold), new SolidBrush(roleAccent), 28, 80);
            g.DrawString(spaceSub, Theme.UiFont(7.8F, FontStyle.Bold), new SolidBrush(Color.FromArgb(228, 238, 250)), 28, 98);

            using var sepPen = new Pen(Color.FromArgb(40, Theme.GoldLight), 1);
            g.DrawLine(sepPen, 16, brandPanel.Height - 1, brandPanel.Width - 16, brandPanel.Height - 1);
        };

        _navPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(14, 10, 14, 10)
        };
        var navPanel = _navPanel;

        // Navigation épurée sans icônes, structurée par sections lisibles
        var sections = new List<(string SectionTitle, List<(string Key, string Label, bool Visible)> Items)>
        {
            ("PILOTAGE", new()
            {
                ("dashboard",  UserSession.IsTresorier ? "Tableau de bord (Structure)" : "Tableau de bord", true),
                ("pilotage",   UserSession.IsAdmin ? "Prévisions & Clôture" : "Prévision Cash-Flow", true),
                ("structures", "Structures", UserSession.IsAdmin)
            }),
            ("TRÉSORERIE", new()
            {
                ("comptes",      UserSession.IsConsultant ? "Comptes (Lecture)" : (UserSession.IsTresorier ? "Mes comptes" : "Comptes"), true),
                ("transactions", UserSession.IsConsultant ? "Transactions (Lecture)" : "Opérations & Transferts", true),
                ("centres",      "Centres analytiques", UserSession.IsAdmin || UserSession.IsTresorier),
                ("budgets",      "Budgets & Alertes",   UserSession.IsAdmin || UserSession.IsTresorier),
                ("validations",  UserSession.IsAdmin ? "Validations" : "Mes demandes", UserSession.IsAdmin || UserSession.IsTresorier),
                ("cotisations",  "Cotisations",         UserSession.IsAdmin || UserSession.IsTresorier)
            }),
            ("ANALYSE & SYSTÈME", new()
            {
                ("rapports",     "Rapports & Export PDF", true),
                ("utilisateurs", "Utilisateurs & Rôles",  UserSession.IsAdmin),
                ("audit",        "Journal d'Audit",       UserSession.IsAdmin),
                ("parametres",   UserSession.IsAdmin ? "Profil & Paramètres" : "Mon Profil", true)
            })
        };

        foreach (var section in sections)
        {
            var visibleItems = section.Items.Where(i => i.Visible).ToList();
            if (visibleItems.Count == 0) continue;

            var lblSec = new Label
            {
                Text = section.SectionTitle,
                UseMnemonic = false,
                Font = Theme.UiFont(7.6F, FontStyle.Bold),
                ForeColor = Color.FromArgb(165, 190, 220),
                BackColor = Color.Transparent,
                AutoSize = true,
                Margin = new Padding(10, 7, 0, 3)
            };
            navPanel.Controls.Add(lblSec);

            foreach (var item in visibleItems)
            {
                var btn = new Button
                {
                    Text = item.Label,
                    UseMnemonic = false,
                    Width = 226,
                    Height = 33,
                    FlatStyle = FlatStyle.Flat,
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding = new Padding(12, 0, 8, 0),
                    Font = Theme.UiFont(9.6F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(228, 237, 250),
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 1, 0, 1)
                };
                btn.FlatAppearance.BorderSize = 0;
                btn.FlatAppearance.MouseOverBackColor = Theme.SidebarHover;
                Theme.ApplyRoundedRegion(btn, 8);
                btn.Click += (_, _) => Navigate(item.Key);

                _navButtons[item.Key] = btn;
                navPanel.Controls.Add(btn);
            }
        }

        // Pied de sidebar : déconnexion épurée
        var footerPanel = new Panel { Dock = DockStyle.Bottom, Height = 62, BackColor = Color.Transparent, Padding = new Padding(14, 10, 14, 14) };
        var btnLogout = new Button
        {
            Text = "Changer de compte",
            Dock = DockStyle.Fill,
            FlatStyle = FlatStyle.Flat,
            BackColor = Color.FromArgb(20, 48, 88),
            ForeColor = Color.FromArgb(230, 238, 248),
            Font = Theme.FontBodyBold,
            Cursor = Cursors.Hand
        };
        btnLogout.FlatAppearance.BorderSize = 0;
        btnLogout.FlatAppearance.MouseOverBackColor = Theme.DangerRed;
        Theme.ApplyRoundedRegion(btnLogout, 9);
        btnLogout.Click += (_, _) =>
        {
            UserSession.Clear();
            RequestedLogout = true;
            Close();
        };
        footerPanel.Controls.Add(btnLogout);

        _sidebar.Controls.Add(navPanel);
        _sidebar.Controls.Add(footerPanel);
        _sidebar.Controls.Add(brandPanel);

        // ==================== TOPBAR BLANCHE ADAPTÉE AU RÔLE ====================
        _topbar.Dock = DockStyle.Top;
        _topbar.Height = 84;
        _topbar.BackColor = Color.White;
        _topbar.Padding = new Padding(26, 12, 26, 12);
        _topbar.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, _topbar.Height - 1, _topbar.Width, _topbar.Height - 1);
            // Filet supérieur discret de la couleur du rôle
            using var topAccent = new SolidBrush(roleAccent);
            e.Graphics.FillRectangle(topAccent, 0, 0, _topbar.Width, 3);

            // Ruban de transition animé lors du changement de module
            if (_navAnimProgress > 0f && _navAnimProgress < 1f)
            {
                int sweepW = Math.Max(24, (int)MathF.Round(_topbar.Width * _navAnimProgress));
                int alpha = Math.Clamp((int)MathF.Round(230f * (1f - _navAnimProgress * 0.65f)), 40, 230);
                using var sweepBrush = new LinearGradientBrush(
                    new Rectangle(0, _topbar.Height - 3, sweepW, 3),
                    Color.FromArgb(alpha / 3, roleAccent),
                    Color.FromArgb(alpha, roleAccent),
                    LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(sweepBrush, 0, _topbar.Height - 3, sweepW, 3);
            }
        };

        _titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 16, 0)
        };
        var titlePanel = _titlePanel;

        _lblPageTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        _lblPageTitle.ForeColor = Theme.TextPrimary;
        _lblPageTitle.UseMnemonic = false;
        _lblPageTitle.AutoSize = false;
        _lblPageTitle.AutoEllipsis = true;
        _lblPageTitle.Dock = DockStyle.Top;
        _lblPageTitle.Height = 34;

        _lblPageSubtitle.Font = Theme.FontSubtitle;
        _lblPageSubtitle.ForeColor = Theme.TextMuted;
        _lblPageSubtitle.UseMnemonic = false;
        _lblPageSubtitle.AutoSize = false;
        _lblPageSubtitle.AutoEllipsis = true;
        _lblPageSubtitle.Dock = DockStyle.Top;
        _lblPageSubtitle.Height = 22;

        titlePanel.Controls.Add(_lblPageSubtitle);
        titlePanel.Controls.Add(_lblPageTitle);

        var rightBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(8, 8, 0, 0)
        };

        if (UserSession.IsConsultant)
        {
            var roBadge = new Label
            {
                Text = "MODE LECTURE SEULE",
                AutoSize = true,
                Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 64, 175),
                BackColor = Color.FromArgb(239, 246, 255),
                Padding = new Padding(12, 10, 12, 10),
                Margin = new Padding(0, 3, 14, 0)
            };
            Theme.ApplyRoundedRegion(roBadge, 8);
            rightBar.Controls.Add(roBadge);
        }
        else if (UserSession.HasPermission("RECETTE_CREATE") || UserSession.HasPermission("DEPENSE_CREATE"))
        {
            var btnQuickOp = Theme.CreatePrimaryButton(
                UserSession.IsTresorier ? "+ Saisir opération" : "+ Nouvelle opération", 172, 38);
            if (UserSession.IsTresorier)
                btnQuickOp.BackColor = Theme.SuccessGreen;
            btnQuickOp.Margin = new Padding(0, 2, 14, 0);
            btnQuickOp.Click += (_, _) =>
            {
                using var dlg = new FrmTransactionDialog("RECETTE");
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    Navigate(_currentKey);
            };
            rightBar.Controls.Add(btnQuickOp);
        }

        var tip = new ToolTip { InitialDelay = 250, AutoPopDelay = 5000 };

        // Bouton Centre de Notifications avec badge compteur temps réel
        bool notifHovered = false;
        _notifBadgeBtn = new SmoothPanel
        {
            Size = new Size(106, 38),
            Margin = new Padding(0, 2, 10, 0),
            BackColor = Color.FromArgb(247, 249, 253),
            Cursor = Cursors.Hand
        };
        _notifBadgeBtn.MouseEnter += (_, _) => { notifHovered = true; _notifBadgeBtn.Invalidate(); };
        _notifBadgeBtn.MouseLeave += (_, _) => { notifHovered = false; _notifBadgeBtn.Invalidate(); };
        _notifBadgeBtn.Click += (_, _) => OpenNotificationCenter();
        _notifBadgeBtn.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            var r = new Rectangle(0, 0, _notifBadgeBtn.Width - 1, _notifBadgeBtn.Height - 1);
            using var path = Theme.CreateRoundedPath(r, 10);
            g.FillPath(new SolidBrush(notifHovered ? Theme.GoldSoftBg : Color.FromArgb(247, 249, 253)), path);
            g.DrawPath(new Pen(notifHovered ? Theme.GoldAccent : (_unreadNotifCount > 0 ? Color.FromArgb(220, Theme.DangerRed) : Theme.BorderColor), 1.2f), path);

            if (_unreadNotifCount > 0)
            {
                TextRenderer.DrawText(g, "Alertes", Theme.UiFont(8.9F, FontStyle.Bold),
                    new Rectangle(10, 0, 56, _notifBadgeBtn.Height), Theme.TextPrimary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

                var badgeText = _unreadNotifCount > 99 ? "99+" : _unreadNotifCount.ToString();
                var badgeRect = new Rectangle(_notifBadgeBtn.Width - 34, 8, 26, 22);
                using var bp = Theme.CreateRoundedPath(badgeRect, 11);
                g.FillPath(new SolidBrush(Theme.DangerRed), bp);
                TextRenderer.DrawText(g, badgeText, Theme.UiFont(8F, FontStyle.Bold),
                    badgeRect, Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
            else
            {
                TextRenderer.DrawText(g, "Alertes", Theme.UiFont(8.9F, FontStyle.Bold),
                    r, Theme.TextMuted,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
            }
        };
        tip.SetToolTip(_notifBadgeBtn, "Ouvrir le Centre de Notifications & Alertes (Ctrl+I)");
        rightBar.Controls.Add(_notifBadgeBtn);

        // Bouton Aide & Raccourcis rapide (?)
        var btnHelp = Theme.CreateSecondaryButton("?", 38, 38);
        btnHelp.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        btnHelp.Margin = new Padding(0, 2, 10, 0);
        tip.SetToolTip(btnHelp, "Aide rapide et raccourcis clavier");
        btnHelp.Click += (_, _) =>
        {
            MessageBox.Show(
                "Prise en main rapide - IIT FINCORE\n\n" +
                "• Cliquez sur n'importe quelle carte KPI ou graphique du tableau de bord pour ouvrir le module correspondant.\n" +
                "• Double-cliquez sur une ligne de tableau pour consulter ou agir directement.\n" +
                "• Faites un clic droit sur un tableau pour copier une cellule ou une ligne.\n\n" +
                "Raccourcis clavier utiles :\n" +
                "  • F5          : Actualiser la page courante\n" +
                "  • Ctrl + I   : Centre de Notifications & Alertes\n" +
                "  • Ctrl + N   : Saisir une nouvelle opération (Recette / Dépense)\n" +
                "  • Ctrl + T   : Effectuer un transfert entre comptes\n" +
                "  • Ctrl + 1   : Tableau de bord\n" +
                "  • Ctrl + 2   : Comptes de trésorerie\n" +
                "  • Ctrl + 3   : Opérations & Transferts\n" +
                "  • Ctrl + 4   : Rapports & Export PDF",
                "IIT FINCORE - Aide & Raccourcis",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        };
        rightBar.Controls.Add(btnHelp);

        var pill = CreateUserProfilePill(roleAccent);
        tip.SetToolTip(pill, "Cliquez pour ouvrir Mon Profil et les paramètres");
        rightBar.Controls.Add(pill);

        // Ordre WinForms Dock : Fill d'abord, puis Right -> titlePanel s'arrête exactement avant rightBar (zéro chevauchement !)
        _topbar.Controls.Add(titlePanel);
        _topbar.Controls.Add(rightBar);

        // ==================== ZONE CENTRALE AVEC FOND D'ARRIÈRE-PLAN HD EN CACHE BITBLT ====================
        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = Theme.PageBg;
        _contentHost.Padding = new Padding(24, 18, 24, 18);
        var lightBg = Theme.LoadLightBackground();
        if (lightBg != null)
        {
            _contentHost.Paint += (_, e) => Theme.DrawCachedBackground(e.Graphics, _contentHost, lightBg);
        }

        var rightContainer = new Panel { Dock = DockStyle.Fill };
        rightContainer.Controls.Add(_contentHost);
        rightContainer.Controls.Add(_topbar);

        Controls.Add(rightContainer);
        Controls.Add(_sidebar);
    }

    private Panel CreateUserProfilePill(Color roleAccent)
    {
        var u = UserSession.CurrentUser;
        var name = u?.NomComplet ?? "Utilisateur";
        var role = UserSession.IsTresorier && !string.IsNullOrEmpty(u?.StructureNom)
            ? $"{u.RoleNom} • {u.StructureNom}"
            : (u?.RoleNom ?? "");
        var initials = string.Concat(name.Split(' ', StringSplitOptions.RemoveEmptyEntries).Take(2).Select(w => char.ToUpperInvariant(w[0])));

        var pill = new Panel
        {
            Size = new Size(248, 42),
            BackColor = Color.FromArgb(247, 249, 253),
            Cursor = Cursors.Hand
        };
        pill.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            using var borderPath = Theme.CreateRoundedPath(new Rectangle(0, 0, pill.Width - 1, pill.Height - 1), 20);
            g.DrawPath(new Pen(Theme.BorderColor, 1), borderPath);

            var avRect = new Rectangle(6, 5, 32, 32);
            g.FillEllipse(new SolidBrush(Theme.SidebarNavy), avRect);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString(initials, Theme.UiFont(9F, FontStyle.Bold), new SolidBrush(roleAccent), avRect, sf);

            var textW = pill.Width - 58;
            TextRenderer.DrawText(g, name, Theme.UiFont(9F, FontStyle.Bold),
                new Rectangle(46, 5, textW, 17), Theme.TextPrimary, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
            TextRenderer.DrawText(g, role, Theme.UiFont(7.6F, FontStyle.Bold),
                new Rectangle(46, 22, textW, 15), Theme.TextMuted, TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
        };
        Theme.ApplyRoundedRegion(pill, 20);
        pill.Click += (_, _) => Navigate("parametres");
        return pill;
    }

    public void Navigate(string key)
    {
        bool isSamePage = key == _currentKey && _viewCache.ContainsKey(key);
        _currentKey = key;
        var roleAccent = GetRoleAccentColor();

        foreach (var (k, btn) in _navButtons)
        {
            bool active = k == key;
            var targetBg = active ? roleAccent : Color.Transparent;
            var targetFg = active ? (UserSession.IsAdmin ? Theme.TextPrimary : Color.White) : Color.FromArgb(220, 232, 248);
            if (btn.BackColor != targetBg) btn.BackColor = targetBg;
            if (btn.ForeColor != targetFg) btn.ForeColor = targetFg;
        }

        _contentHost.SuspendLayout();

        bool created = false;
        if (!_viewCache.TryGetValue(key, out var page))
        {
            string dashTitle = UserSession.IsAdmin
                ? "Tableau de bord - Administration globale"
                : UserSession.IsTresorier
                    ? $"Tableau de bord - {UserSession.CurrentUser?.StructureNom}"
                    : "Tableau de bord - Consultation financière";

            string dashSub = UserSession.IsAdmin
                ? "Vue consolidée multi-structures et pilotage financier IIT FINCORE"
                : UserSession.IsTresorier
                    ? "Suivi opérationnel de votre trésorerie, comptes et imputations analytiques"
                    : "Consultation en lecture seule des indicateurs et synthèses financières";

            page = key switch
            {
                "dashboard"    => (dashTitle, dashSub, new FrmDashboard()),
                "pilotage"     => ("Prévisions Cash-Flow & Clôture", "Projection de trésorerie à 6 mois, charges récurrentes et clôture d'exercice", new FrmPilotageAvance()),
                "structures"   => ("Structures", "Associations, clubs et entités rattachées", new FrmStructures()),
                "comptes"      => ("Comptes de trésorerie", UserSession.IsConsultant ? "Consultation des soldes en lecture seule" : "Banques, caisses et comptes Mobile Money", new FrmComptes()),
                "transactions" => ("Mouvements de trésorerie", UserSession.IsConsultant ? "Journal des opérations en lecture seule" : "Recettes, dépenses multi-ventilées et transferts entre comptes", new FrmTransactions()),
                "centres"      => ("Centres d'imputation analytiques", "Répartition des recettes et dépenses par projet / activité", new FrmCentresAnalytiques()),
                "budgets"      => ("Budgets & Suivi budgétaire", "Enveloppes prévisionnelles et taux d'exécution", new FrmBudgets()),
                "validations"  => (UserSession.IsAdmin ? "Approbation des demandes" : "Mes demandes de validation", UserSession.IsAdmin ? "Validez ou rejetez les dépenses soumises par les trésoriers" : "Suivi de vos dépenses soumises à validation", new FrmValidations()),
                "cotisations"  => ("Cotisations", "Campagnes, règlements des membres et suivi des impayés", new FrmCotisations()),
                "rapports"     => ("Rapports & Exportation", "Synthèse financière, recettes, dépenses et export CSV", new FrmRapports()),
                "utilisateurs" => ("Utilisateurs, Rôles & Permissions", "Gestion des accès Administrateur, Trésorier et Consultant", new FrmUtilisateurs()),
                "audit"        => ("Journal des activités (Audit)", "Traçabilité complète des actions effectuées", new FrmAudit()),
                "parametres"   => (UserSession.IsAdmin ? "Profil & Paramètres généraux" : "Mon Profil", "Informations personnelles et sécurité du compte", new FrmParametres()),
                _              => (dashTitle, dashSub, new FrmDashboard())
            };

            page.Control.TopLevel = false;
            page.Control.FormBorderStyle = FormBorderStyle.None;
            page.Control.Dock = DockStyle.Fill;
            Theme.UpgradeTabControls(page.Control);
            Theme.UpgradeAllTextBoxes(page.Control);
            Theme.MakeLayoutContainersTransparent(page.Control);
            Theme.ApplyCrispPixelFonts(page.Control);
            _contentHost.Controls.Add(page.Control);
            page.Control.Show();
            _viewCache[key] = page;
            created = true;
        }

        _lblPageTitle.Text = page.Title;
        _lblPageSubtitle.Text = page.Subtitle;

        foreach (Control c in _contentHost.Controls)
            c.Visible = c == page.Control;

        page.Control.BringToFront();
        _contentHost.ResumeLayout(true);

        TriggerNavigationAnimation(key, instant: !IsHandleCreated);

        // Si la vue était déjà en cache, rafraîchit uniquement ses données (sans recréer aucun contrôle UI)
        if (!created || isSamePage)
            RefreshCachedViewData(page.Control);

        RefreshNotificationBadge();
    }

    private static void RefreshCachedViewData(Form control)
    {
        var type = control.GetType();
        if (!_refreshMethodCache.TryGetValue(type, out var method))
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            method = type.GetMethod("RefreshData", flags, Type.EmptyTypes)
                  ?? type.GetMethod("LoadData", flags, Type.EmptyTypes)
                  ?? type.GetMethod("LoadAll", flags, Type.EmptyTypes);
            _refreshMethodCache[type] = method;
        }

        try
        {
            method?.Invoke(control, null);
        }
        catch
        {
            // Ignore les erreurs ponctuelles lors du rafraîchissement d'une vue en cache
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Controls;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI;

public class FrmMain : Form, IMessageFilter
{
    public bool RequestedLogout { get; private set; }

    private readonly Panel _sidebar = new();
    private readonly Panel _topbar = new();
    private readonly Panel _contentHost = new();
    private readonly Label _lblPageTitle = new();
    private readonly Label _lblPageSubtitle = new();
    private readonly Dictionary<string, Button> _navButtons = new();
    private readonly System.Windows.Forms.Timer _idleTimer = new();
    private DateTime _lastActivityUtc = DateTime.UtcNow;
    private static readonly TimeSpan InactivityTimeout = TimeSpan.FromMinutes(15);
    private string _currentKey = "dashboard";

    public FrmMain()
    {
        var roleLabel = UserSession.CurrentUser?.RoleNom ?? "";
        Text = $"IIT FINCORE - Espace {roleLabel}";
        Icon = Theme.LoadAppIcon();
        Size = new Size(1340, 800);
        MinimumSize = new Size(1160, 720);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Theme.PageBg;
        DoubleBuffered = true;
        KeyPreview = true;

        BuildShell();
        Theme.ApplyCrispPixelFonts(this);
        Navigate("dashboard");
        StartInactivityGuard();
        KeyDown += HandleGlobalShortcuts;
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
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(navyBg, _sidebar.ClientRectangle);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(110, 5, 20, 46)), _sidebar.ClientRectangle);
            }
            else
            {
                using var bg = new LinearGradientBrush(_sidebar.ClientRectangle, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.Vertical);
                e.Graphics.FillRectangle(bg, _sidebar.ClientRectangle);
            }
            using var rightEdge = new Pen(Color.FromArgb(45, roleAccent), 1);
            e.Graphics.DrawLine(rightEdge, _sidebar.Width - 1, 0, _sidebar.Width - 1, _sidebar.Height);
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

        var navPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoScroll = true,
            BackColor = Color.Transparent,
            Padding = new Padding(14, 10, 14, 10)
        };

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
        };

        var titlePanel = new Panel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 2, 16, 0)
        };

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

        // ==================== ZONE CENTRALE AVEC FOND D'ARRIÈRE-PLAN HD ====================
        _contentHost.Dock = DockStyle.Fill;
        _contentHost.BackColor = Theme.PageBg;
        _contentHost.Padding = new Padding(24, 18, 24, 18);
        var lightBg = Theme.LoadLightBackground();
        if (lightBg != null)
        {
            _contentHost.BackgroundImage = lightBg;
            _contentHost.BackgroundImageLayout = ImageLayout.Stretch;
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
        _currentKey = key;
        var roleAccent = GetRoleAccentColor();

        foreach (var (k, btn) in _navButtons)
        {
            bool active = k == key;
            btn.BackColor = active ? roleAccent : Color.Transparent;
            btn.ForeColor = active ? (UserSession.IsAdmin ? Theme.TextPrimary : Color.White) : Color.FromArgb(220, 232, 248);
            btn.Font = Theme.UiFont(9.6F, FontStyle.Bold);
        }

        _contentHost.SuspendLayout();
        _contentHost.Controls.Clear();

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

        (string Title, string Subtitle, UserControl Control) page = key switch
        {
            "dashboard"    => (dashTitle, dashSub, new UcDashboard()),
            "pilotage"     => ("Prévisions Cash-Flow & Clôture", "Projection de trésorerie à 6 mois, charges récurrentes et clôture d'exercice", new UcPilotageAvance()),
            "structures"   => ("Structures", "Associations, clubs et entités rattachées", new UcStructures()),
            "comptes"      => ("Comptes de trésorerie", UserSession.IsConsultant ? "Consultation des soldes en lecture seule" : "Banques, caisses et comptes Mobile Money", new UcComptes()),
            "transactions" => ("Mouvements de trésorerie", UserSession.IsConsultant ? "Journal des opérations en lecture seule" : "Recettes, dépenses multi-ventilées et transferts entre comptes", new UcTransactions()),
            "centres"      => ("Centres d'imputation analytiques", "Répartition des recettes et dépenses par projet / activité", new UcCentresAnalytiques()),
            "budgets"      => ("Budgets & Suivi budgétaire", "Enveloppes prévisionnelles et taux d'exécution", new UcBudgets()),
            "validations"  => (UserSession.IsAdmin ? "Approbation des demandes" : "Mes demandes de validation", UserSession.IsAdmin ? "Validez ou rejetez les dépenses soumises par les trésoriers" : "Suivi de vos dépenses soumises à validation", new UcValidations()),
            "cotisations"  => ("Cotisations", "Campagnes, règlements des membres et suivi des impayés", new UcCotisations()),
            "rapports"     => ("Rapports & Exportation", "Synthèse financière, recettes, dépenses et export CSV", new UcRapports()),
            "utilisateurs" => ("Utilisateurs, Rôles & Permissions", "Gestion des accès Administrateur, Trésorier et Consultant", new UcUtilisateurs()),
            "audit"        => ("Journal des activités (Audit)", "Traçabilité complète des actions effectuées", new UcAudit()),
            "parametres"   => (UserSession.IsAdmin ? "Profil & Paramètres généraux" : "Mon Profil", "Informations personnelles et sécurité du compte", new UcParametres()),
            _              => (dashTitle, dashSub, new UcDashboard())
        };

        _lblPageTitle.Text = page.Title;
        _lblPageSubtitle.Text = page.Subtitle;
        page.Control.Dock = DockStyle.Fill;
        Theme.UpgradeTabControls(page.Control);
        Theme.UpgradeAllTextBoxes(page.Control);
        Theme.MakeLayoutContainersTransparent(page.Control);
        Theme.ApplyCrispPixelFonts(page.Control);
        _contentHost.Controls.Add(page.Control);
        _contentHost.ResumeLayout();
    }
}

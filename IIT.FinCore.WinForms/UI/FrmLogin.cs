using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI;

/// <summary>
/// Maquette 02 : Écran de connexion commercial Premium IIT FINCORE (Royal Sapphire & Metallic Gold).
/// </summary>
public partial class FrmLogin : Form
{
    private readonly AuthRepository _authRepo = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPassword = new();
    private readonly CheckBox _chkRemember = new();
    private readonly Label _lblError = new();
    private Button _btnLogin = null!;
    private readonly Button _btnTogglePwd = new();
    private readonly ToolTip _toolTip = new();
    private readonly List<Button> _rolePills = new();

    private Panel _emailBox = null!;
    private Panel _pwdBox = null!;
    private bool _emailFocused;
    private bool _pwdFocused;
    private bool _passwordVisible;
    private bool _togglePwdHovered;

    private Point _dragStart;
    private bool _dragging;

    public FrmLogin()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        Text = "IIT FINCORE - Connexion";
        Icon = Theme.LoadAppIcon();
        Size = new Size(960, 600);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Theme.PageBg;
        DoubleBuffered = true;
        Opacity = 0;

        BuildLayout();
        Theme.ApplyCrispPixelFonts(this);
        Theme.ApplyRoundedRegion(this, 18);

        var fadeTimer = new System.Windows.Forms.Timer { Interval = 14 };
        fadeTimer.Tick += (_, _) =>
        {
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.25);
            else fadeTimer.Stop();
        };
        Shown += (_, _) =>
        {
            fadeTimer.Start();
            _txtEmail.Focus();
        };
    }

    private void BuildLayout()
    {
        // ==================== PANNEAU GAUCHE : BRANDING PREMIUM ====================
        var leftPanel = new SmoothPanel
        {
            Dock = DockStyle.Left,
            Width = 370,
            BackColor = Theme.SidebarNavy
        };
        EnableDrag(leftPanel);

        var leftInner = new Panel
        {
            Size = new Size(340, 520),
            BackColor = Color.Transparent
        };
        leftPanel.Resize += (_, _) =>
        {
            leftInner.Location = new Point((leftPanel.Width - leftInner.Width) / 2, Math.Max(20, (leftPanel.Height - leftInner.Height) / 2));
        };

        leftPanel.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var navyBg = Theme.LoadNavyBackground();
            if (navyBg != null)
            {
                Theme.DrawCachedBackground(g, leftPanel, navyBg, Color.FromArgb(85, 5, 20, 46));
            }
            else
            {
                using var bgBrush = new LinearGradientBrush(
                    leftPanel.ClientRectangle,
                    Theme.SidebarNavyLight,
                    Theme.SidebarNavy,
                    LinearGradientMode.ForwardDiagonal);
                g.FillRectangle(bgBrush, leftPanel.ClientRectangle);
            }
        };

        leftInner.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            // Logo officiel IIT FINCORE centré
            Theme.DrawRoundedLogo(g, new Rectangle(100, 28, 140, 140), 28);

            // Titre bi-colore "IIT FINCORE" sans espace parasite
            var fontBrand = Theme.UiFont(21F, FontStyle.Bold);
            var typo = StringFormat.GenericTypographic;
            var s1 = "IIT FIN";
            var s2 = "CORE";
            var w1 = g.MeasureString(s1, fontBrand, PointF.Empty, typo).Width;
            var w2 = g.MeasureString(s2, fontBrand, PointF.Empty, typo).Width;
            float sx = (leftInner.Width - (w1 + w2)) / 2f;
            g.DrawString(s1, fontBrand, Brushes.White, sx, 184, typo);
            g.DrawString(s2, fontBrand, new SolidBrush(Theme.GoldLight), sx + w1, 184, typo);

            using var sf = new StringFormat { Alignment = StringAlignment.Center };
            g.DrawString("EXCELLENCE EN GESTION FINANCIÈRE",
                Theme.UiFont(8F, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(175, 198, 226)),
                new RectangleF(0, 224, leftInner.Width, 20), sf);

            g.FillRectangle(new SolidBrush(Color.FromArgb(110, Theme.GoldLight)), 90, 256, 160, 2);
        };

        var lblQuick = new Label
        {
            Text = "PROFIL DE CONNEXION RAPIDE",
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(165, 190, 218),
            BackColor = Color.Transparent,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(20, 292, 300, 20)
        };
        leftInner.Controls.Add(lblQuick);

        var roles = new (string Label, string Email, string Pwd)[]
        {
            ("Administrateur", "admin@iit.ci", "admin123"),
            ("Trésorier",      "tresorier@iit.ci", "tresorier123"),
            ("Consultant",     "consultant@iit.ci", "consultant123")
        };

        int py = 322;
        foreach (var r in roles)
        {
            var pill = new Button
            {
                Text = r.Label,
                Bounds = new Rectangle(45, py, 250, 38),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(220, 232, 245),
                BackColor = Color.FromArgb(22, 52, 92),
                Cursor = Cursors.Hand,
                Tag = r
            };
            pill.FlatAppearance.BorderSize = 0;
            pill.FlatAppearance.MouseOverBackColor = Theme.GoldAccent;
            Theme.ApplyRoundedRegion(pill, 10);
            pill.Click += (_, _) => SelectRolePill(pill);
            _rolePills.Add(pill);
            leftInner.Controls.Add(pill);
            py += 48;
        }
        leftPanel.Controls.Add(leftInner);

        // ==================== PANNEAU DROIT : CARTE CONNEXION CENTRÉE PLEIN ÉCRAN ====================
        var rightPanel = new SmoothPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Theme.PageBg
        };

        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(38, 38),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(rightPanel.Width - 54, 16),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            BackColor = Color.White,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(225, 232, 242);
        btnClose.Click += (_, _) => Close();
        Theme.ApplyRoundedRegion(btnClose, 10);

        var card = new Panel
        {
            Size = new Size(450, 456),
            BackColor = Color.White
        };

        rightPanel.Resize += (_, _) =>
        {
            card.Location = new Point(Math.Max(20, (rightPanel.Width - card.Width) / 2), Math.Max(40, (rightPanel.Height - card.Height) / 2));
            btnClose.Location = new Point(rightPanel.Width - 54, 16);
            rightPanel.Invalidate();
        };

        // Fond clair HD + ombre douce dynamique autour de la carte centrée
        rightPanel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var lightBg = Theme.LoadLightBackground();
            if (lightBg != null)
            {
                Theme.DrawCachedBackground(e.Graphics, rightPanel, lightBg);
            }
            for (int i = 4; i >= 1; i--)
            {
                var shadowRect = new Rectangle(card.Left - i, card.Top + i * 2, card.Width + i * 2, card.Height + i);
                using var sp = Theme.CreateRoundedPath(shadowRect, 18);
                using var sb = new SolidBrush(Color.FromArgb(7 * (5 - i), 12, 31, 61));
                e.Graphics.FillPath(sb, sp);
            }
        };
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Theme.CreateRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 16);
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawPath(pen, path);

            // Filet or élégant en haut de la carte
            using var topBar = new LinearGradientBrush(new Rectangle(0, 0, card.Width, 4), Theme.GoldLight, Theme.GoldAccent, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(topBar, 0, 0, card.Width, 4);
        };
        Theme.ApplyRoundedRegion(card, 16);

        var lblTitle = new Label
        {
            Text = "Connexion",
            Font = new Font("Segoe UI", 22F, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(20, 32, 410, 40)
        };

        var lblSub = new Label
        {
            Text = "Accédez à votre espace sécurisé IIT FINCORE",
            Font = new Font("Segoe UI", 9.5F),
            ForeColor = Theme.TextMuted,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(20, 74, 410, 22)
        };

        var lblEmail = new Label { Text = "Adresse e-mail", Font = Theme.FontBodyBold, ForeColor = Theme.TextPrimary, Location = new Point(46, 120), AutoSize = true };
        _emailBox = CreateInputBox(46, 144, 358, 44, () => _emailFocused);
        _txtEmail.BorderStyle = BorderStyle.None;
        _txtEmail.Font = new Font("Segoe UI", 10.5F);
        _txtEmail.ForeColor = Theme.TextPrimary;
        _txtEmail.BackColor = Color.White;
        _txtEmail.Bounds = new Rectangle(14, 11, 330, 22);
        _txtEmail.PlaceholderText = "exemple@iit.ci";
        _txtEmail.GotFocus += (_, _) => { _emailFocused = true; _emailBox.Invalidate(); };
        _txtEmail.LostFocus += (_, _) => { _emailFocused = false; _emailBox.Invalidate(); };
        _emailBox.Controls.Add(_txtEmail);

        var lblPwd = new Label { Text = "Mot de passe", Font = Theme.FontBodyBold, ForeColor = Theme.TextPrimary, Location = new Point(46, 202), AutoSize = true };
        _pwdBox = CreateInputBox(46, 226, 358, 44, () => _pwdFocused);
        _txtPassword.BorderStyle = BorderStyle.None;
        _txtPassword.Font = new Font("Segoe UI", 10.5F);
        _txtPassword.ForeColor = Theme.TextPrimary;
        _txtPassword.BackColor = Color.White;
        _txtPassword.UseSystemPasswordChar = true;
        _txtPassword.Bounds = new Rectangle(14, 11, 294, 22);
        _txtPassword.PlaceholderText = "Entrez votre mot de passe";
        _txtPassword.GotFocus += (_, _) => { _pwdFocused = true; _pwdBox.Invalidate(); };
        _txtPassword.LostFocus += (_, _) => { _pwdFocused = false; _pwdBox.Invalidate(); };

        _btnTogglePwd.Text = "";
        _btnTogglePwd.BackColor = Color.White;
        _btnTogglePwd.FlatStyle = FlatStyle.Flat;
        _btnTogglePwd.FlatAppearance.BorderSize = 0;
        _btnTogglePwd.FlatAppearance.MouseOverBackColor = Color.FromArgb(244, 247, 252);
        _btnTogglePwd.FlatAppearance.MouseDownBackColor = Color.FromArgb(234, 240, 249);
        _btnTogglePwd.Cursor = Cursors.Hand;
        _btnTogglePwd.TabStop = false;
        _btnTogglePwd.AccessibleName = "Afficher le mot de passe";
        _btnTogglePwd.Bounds = new Rectangle(314, 6, 36, 32);
        Theme.ApplyRoundedRegion(_btnTogglePwd, 8);
        _toolTip.SetToolTip(_btnTogglePwd, "Afficher le mot de passe");
        Disposed += (_, _) => _toolTip.Dispose();
        _btnTogglePwd.MouseEnter += (_, _) => { _togglePwdHovered = true; _btnTogglePwd.Invalidate(); };
        _btnTogglePwd.MouseLeave += (_, _) => { _togglePwdHovered = false; _btnTogglePwd.Invalidate(); };
        _btnTogglePwd.Paint += (_, e) => DrawEyeIcon(e.Graphics, _btnTogglePwd.ClientRectangle, _passwordVisible, _togglePwdHovered);
        _btnTogglePwd.Click += (_, _) =>
        {
            _passwordVisible = !_passwordVisible;
            _txtPassword.UseSystemPasswordChar = !_passwordVisible;
            var label = _passwordVisible ? "Masquer le mot de passe" : "Afficher le mot de passe";
            _btnTogglePwd.AccessibleName = label;
            _toolTip.SetToolTip(_btnTogglePwd, label);
            _btnTogglePwd.Invalidate();
        };
        _pwdBox.Controls.Add(_txtPassword);
        _pwdBox.Controls.Add(_btnTogglePwd);

        _chkRemember.Text = "Se souvenir de moi";
        _chkRemember.Font = Theme.FontBody;
        _chkRemember.ForeColor = Theme.TextMuted;
        _chkRemember.Checked = true;
        _chkRemember.AutoSize = true;
        _chkRemember.Location = new Point(46, 282);

        _lblError.Bounds = new Rectangle(46, 308, 358, 22);
        _lblError.ForeColor = Theme.DangerRed;
        _lblError.Font = new Font("Segoe UI", 8.5F, FontStyle.Bold);
        _lblError.TextAlign = ContentAlignment.MiddleCenter;

        _btnLogin = Theme.CreatePrimaryButton("Se connecter", 358, 46);
        _btnLogin.Font = new Font("Segoe UI", 10.5F, FontStyle.Bold);
        _btnLogin.Location = new Point(46, 336);
        _btnLogin.Click += (_, _) => DoLogin();

        var lnkForgot = new LinkLabel
        {
            Text = "Mot de passe oublié ?",
            Font = Theme.FontBodyBold,
            LinkColor = Theme.SidebarNavyLight,
            ActiveLinkColor = Theme.GoldAccent,
            LinkBehavior = LinkBehavior.HoverUnderline,
            TextAlign = ContentAlignment.MiddleCenter,
            Bounds = new Rectangle(46, 398, 358, 24)
        };
        lnkForgot.Click += (_, _) =>
        {
            using var dlg = new Dialogs.FrmForgotPassword();
            dlg.ShowDialog(this);
        };

        AcceptButton = _btnLogin;

        card.Controls.AddRange(new Control[]
        {
            lblTitle, lblSub, lblEmail, _emailBox, lblPwd, _pwdBox,
            _chkRemember, _lblError, _btnLogin, lnkForgot
        });

        rightPanel.Controls.Add(btnClose);
        rightPanel.Controls.Add(card);
        Controls.Add(rightPanel);
        Controls.Add(leftPanel);

        if (_rolePills.Count > 0)
            SelectRolePill(_rolePills[0]);
    }

    private static Panel CreateInputBox(int x, int y, int w, int h, Func<bool> isFocused)
    {
        var box = new Panel { Bounds = new Rectangle(x, y, w, h), BackColor = Color.White };
        box.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var borderColor = isFocused() ? Theme.GoldAccent : Theme.BorderColor;
            float width = isFocused() ? 2.0f : 1.0f;
            using var path = Theme.CreateRoundedPath(new Rectangle(1, 1, box.Width - 3, box.Height - 3), 9);
            using var pen = new Pen(borderColor, width);
            e.Graphics.DrawPath(pen, path);
        };
        return box;
    }

    private static void DrawEyeIcon(Graphics g, Rectangle bounds, bool passwordVisible, bool hovered)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.PixelOffsetMode = PixelOffsetMode.HighQuality;

        float cx = bounds.Width / 2f;
        float cy = bounds.Height / 2f;
        var iconColor = hovered ? Theme.SidebarNavyLight : Theme.TextMuted;
        var bgColor = hovered ? Color.FromArgb(244, 247, 252) : Color.White;

        using var pen = new Pen(iconColor, 1.75f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        // Contour de l'œil (courbes de Bézier symétriques)
        using var eyePath = new GraphicsPath();
        eyePath.AddBezier(cx - 9f, cy, cx - 4.5f, cy - 7.2f, cx + 4.5f, cy - 7.2f, cx + 9f, cy);
        eyePath.AddBezier(cx + 9f, cy, cx + 4.5f, cy + 7.2f, cx - 4.5f, cy + 7.2f, cx - 9f, cy);
        g.DrawPath(pen, eyePath);

        // Pupille centrale
        g.DrawEllipse(pen, cx - 2.7f, cy - 2.7f, 5.4f, 5.4f);

        // Barre diagonale lorsque le mot de passe est visible (clic pour masquer)
        if (passwordVisible)
        {
            using var haloPen = new Pen(bgColor, 3.6f)
            {
                StartCap = LineCap.Round,
                EndCap = LineCap.Round
            };
            g.DrawLine(haloPen, cx - 7.2f, cy - 6.8f, cx + 7.2f, cy + 6.8f);
            g.DrawLine(pen, cx - 7.2f, cy - 6.8f, cx + 7.2f, cy + 6.8f);
        }
    }

    private void SelectRolePill(Button selected)
    {
        foreach (var p in _rolePills)
        {
            bool active = p == selected;
            p.BackColor = active ? Theme.GoldLight : Color.FromArgb(20, 50, 92);
            p.ForeColor = active ? Theme.TextPrimary : Color.FromArgb(220, 232, 248);
        }

        if (selected.Tag is ValueTuple<string, string, string> data)
        {
            _txtEmail.Text = data.Item2;
            _txtPassword.Text = data.Item3;
            _lblError.Text = "";
        }
    }

    private void EnableDrag(Control ctrl)
    {
        ctrl.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; _dragStart = e.Location; }
        };
        ctrl.MouseMove += (_, e) =>
        {
            if (_dragging) Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
        };
        ctrl.MouseUp += (_, _) => _dragging = false;
    }

    private async void DoLogin()
    {
        _lblError.Text = "";
        _btnLogin.Enabled = false;
        _btnLogin.Text = "Connexion en cours...";

        var email = _txtEmail.Text;
        var pwd = _txtPassword.Text;

        try
        {
            var (user, perms, errMsg) = await Task.Run(() => _authRepo.Authenticate(email, pwd));
            if (IsDisposed) return;

            if (user == null)
            {
                _lblError.Text = errMsg ?? "Identifiants invalides ou compte désactivé.";
                ShakeForm();
                return;
            }

            UserSession.Start(user, perms);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            if (IsDisposed) return;
            _lblError.Text = "Erreur MySQL : " + ex.Message;
            ShakeForm();
        }
        finally
        {
            if (!IsDisposed)
            {
                _btnLogin.Enabled = true;
                _btnLogin.Text = "Se connecter";
            }
        }
    }

    private void ShakeForm()
    {
        var orig = Location;
        int[] offsets = { -8, 8, -6, 6, -3, 3, 0 };
        int step = 0;
        var t = new System.Windows.Forms.Timer { Interval = 16 };
        t.Tick += (_, _) =>
        {
            if (IsDisposed || step >= offsets.Length)
            {
                t.Stop();
                t.Dispose();
                return;
            }
            Location = new Point(orig.X + offsets[step++], orig.Y);
        };
        t.Start();
    }
}

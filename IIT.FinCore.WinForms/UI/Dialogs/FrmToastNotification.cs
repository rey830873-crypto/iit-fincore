using System.Drawing;
using System.Drawing.Drawing2D;

namespace IIT.FinCore.WinForms.UI.Dialogs;

/// <summary>
/// Notification Toast discrète (coin inférieur droit) avec fondu fluide,
/// sans jamais voler le focus clavier à l'utilisateur.
/// </summary>
public partial class FrmToastNotification : Form
{
    private readonly string _title = "";
    private readonly string _message = "";
    private readonly Color _accent = Theme.GoldLight;
    private readonly Action? _onClick;
    private readonly System.Windows.Forms.Timer _lifeTimer = new();
    private bool _fadingOut;

    protected override bool ShowWithoutActivation => true;

    public FrmToastNotification()
    {
        InitializeComponent();
    }

    public FrmToastNotification(Form owner, string title, string message, string niveau = "ALERTE", Action? onClick = null)
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        _title = title;
        _message = message;
        _onClick = onClick;
        _accent = niveau switch
        {
            "CRITIQUE" => Theme.DangerRed,
            "SUCCES" => Theme.SuccessGreen,
            "ALERTE" => Theme.GoldLight,
            _ => Color.FromArgb(96, 165, 250)
        };

        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        Size = new Size(376, 86);
        StartPosition = FormStartPosition.Manual;
        BackColor = Theme.SidebarNavy;
        DoubleBuffered = true;
        Opacity = 0;
        Cursor = Cursors.Hand;

        var area = Screen.FromControl(owner).WorkingArea;
        int targetX = area.Right - Width - 22;
        int targetY = area.Bottom - Height - 22;
        Location = new Point(targetX, targetY + 18);
        Theme.ApplyRoundedRegion(this, 14);

        Click += (_, _) =>
        {
            _lifeTimer.Stop();
            Close();
            _onClick?.Invoke();
        };

        int ticks = 0;
        const int enterTicks = 10;
        _lifeTimer.Interval = 18;
        _lifeTimer.Tick += (_, _) =>
        {
            ticks++;
            if (!_fadingOut)
            {
                if (ticks <= enterTicks)
                {
                    float eased = Theme.EaseOutCubic(ticks / (float)enterTicks);
                    Opacity = 0.96 * eased;
                    Top = targetY + (int)MathF.Round(18f * (1f - eased));
                }
                if (ticks >= 240) _fadingOut = true; // ~4.3 secondes
            }
            else
            {
                Opacity = Math.Max(0, Opacity - 0.08);
                Top += 1;
                if (Opacity <= 0.02)
                {
                    _lifeTimer.Stop();
                    Close();
                }
            }
        };
        Shown += (_, _) => _lifeTimer.Start();
        FormClosed += (_, _) => { _lifeTimer.Stop(); _lifeTimer.Dispose(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.CreateRoundedPath(rect, 14);
        using var bg = new LinearGradientBrush(rect, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.Horizontal);
        g.FillPath(bg, path);
        g.DrawPath(new Pen(Color.FromArgb(140, _accent), 1.4f), path);

        // Barre verticale d'accentuation
        using var bar = Theme.CreateRoundedPath(new Rectangle(10, 12, 5, Height - 24), 3);
        g.FillPath(new SolidBrush(_accent), bar);

        TextRenderer.DrawText(g, _title, Theme.UiFont(9.4F, FontStyle.Bold),
            new Rectangle(24, 12, Width - 42, 20), _accent,
            TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

        TextRenderer.DrawText(g, _message, Theme.UiFont(8.4F, FontStyle.Regular),
            new Rectangle(24, 34, Width - 40, 38), Color.FromArgb(228, 238, 250),
            TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;

namespace IIT.FinCore.WinForms.UI;

/// <summary>
/// Maquette 01 : Écran d'accueil commercial IIT FINCORE avec logo officiel et barre de progression or.
/// </summary>
public partial class FrmSplash : Form
{
    private readonly System.Windows.Forms.Timer _timer = new();
    private float _progress;
    private string _statusText = "Initialisation du moteur financier...";

    public FrmSplash()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        pnlSplashCanvas.Visible = false;
        FormBorderStyle = FormBorderStyle.None;
        Icon = Theme.LoadAppIcon();
        StartPosition = FormStartPosition.CenterScreen;
        Size = new Size(560, 360);
        DoubleBuffered = true;
        Opacity = 0;

        Theme.ApplyRoundedRegion(this, 18);

        const double durationMs = 7000.0; // Durée exacte : 7 secondes
        var sw = new System.Diagnostics.Stopwatch();

        _timer.Interval = 16;
        _timer.Tick += (_, _) =>
        {
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.08);
            _progress = Math.Min(100f, (float)(sw.ElapsedMilliseconds / durationMs * 100.0));

            if (_progress < 25)
                _statusText = "Initialisation du moteur financier...";
            else if (_progress < 55)
                _statusText = "Connexion sécurisée à la base MySQL...";
            else if (_progress < 85)
                _statusText = "Chargement des tableaux de bord & rôles...";
            else
                _statusText = "Préparation de l'espace IIT FINCORE...";

            Invalidate();

            if (sw.ElapsedMilliseconds >= durationMs)
            {
                _timer.Stop();
                Close();
            }
        };
        Shown += (_, _) => { sw.Start(); _timer.Start(); };
        FormClosed += (_, _) => { _timer.Stop(); _timer.Dispose(); };
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Fond HD Royal Sapphire & Or pré-redimensionné en cache (BitBlt instantané)
        var bgImg = Theme.LoadNavyBackground();
        if (bgImg != null)
        {
            Theme.DrawCachedBackground(g, this, bgImg, Color.FromArgb(70, 4, 18, 42));
        }
        else
        {
            using var bg = new LinearGradientBrush(ClientRectangle, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.ForwardDiagonal);
            g.FillRectangle(bg, ClientRectangle);
        }

        // Bordure extérieure Or fin
        using var borderPath = Theme.CreateRoundedPath(new Rectangle(1, 1, Width - 3, Height - 3), 18);
        using var borderPen = new Pen(Color.FromArgb(140, Theme.GoldLight), 1.5f);
        g.DrawPath(borderPen, borderPath);

        // Anneaux orbitaux 3D (moitié arrière derrière le logo)
        var logoCenter = new PointF(Width / 2f, 112f);
        float orbitAngle = _progress * 0.16f;
        Theme.Draw3DOrbitalRings(g, logoCenter, 86f, orbitAngle, frontHalf: false);

        // Halo doré discret derrière le logo
        using var halo = new SolidBrush(Color.FromArgb(24, Theme.GoldLight));
        g.FillEllipse(halo, (Width - 156) / 2, 34, 156, 156);

        // Logo officiel IIT FINCORE
        Theme.DrawRoundedLogo(g, new Rectangle((Width - 130) / 2, 47, 130, 130), 26);

        // Anneaux orbitaux 3D (moitié avant devant le logo)
        Theme.Draw3DOrbitalRings(g, logoCenter, 86f, orbitAngle, frontHalf: true);

        // Titre bi-colore : "IIT FINCORE" parfaitement collé
        var fontBrand = new Font("Segoe UI", 22F, FontStyle.Bold);
        var typo = StringFormat.GenericTypographic;
        var s1 = "IIT FIN";
        var s2 = "CORE";
        var w1 = g.MeasureString(s1, fontBrand, PointF.Empty, typo).Width;
        var w2 = g.MeasureString(s2, fontBrand, PointF.Empty, typo).Width;
        float startX = (Width - (w1 + w2)) / 2f;
        g.DrawString(s1, fontBrand, Brushes.White, startX, 194, typo);
        g.DrawString(s2, fontBrand, new SolidBrush(Theme.GoldLight), startX + w1, 194, typo);

        using var sf = new StringFormat { Alignment = StringAlignment.Center };
        g.DrawString("GESTION DE TRÉSORERIE & COMPTABILITÉ ANALYTIQUE",
            new Font("Segoe UI", 8.5F, FontStyle.Bold),
            new SolidBrush(Color.FromArgb(190, 210, 235)),
            new RectangleF(0, 234, Width, 20), sf);

        // Statut
        g.DrawString(_statusText, new Font("Segoe UI", 8.5F),
            new SolidBrush(Color.FromArgb(170, 192, 220)),
            new RectangleF(0, 276, Width, 20), sf);

        // Barre de progression arrondie Or Métallique
        var trackRect = new Rectangle(80, 302, Width - 160, 8);
        using var trackPath = Theme.CreateRoundedPath(trackRect, 4);
        g.FillPath(new SolidBrush(Color.FromArgb(45, 255, 255, 255)), trackPath);

        int fillW = Math.Max(8, (int)(trackRect.Width * (_progress / 100f)));
        var fillRect = new Rectangle(trackRect.X, trackRect.Y, fillW, trackRect.Height);
        using var fillPath = Theme.CreateRoundedPath(fillRect, 4);
        using var fillBrush = new LinearGradientBrush(trackRect, Theme.GoldLight, Theme.GoldAccent, LinearGradientMode.Horizontal);
        g.FillPath(fillBrush, fillPath);
    }
}

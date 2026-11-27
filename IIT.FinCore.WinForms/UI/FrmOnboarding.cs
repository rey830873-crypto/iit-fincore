using System.Drawing;
using System.Drawing.Drawing2D;

namespace IIT.FinCore.WinForms.UI;

/// <summary>
/// Onboarding commercial haut de gamme (Design Éditorial Split-Screen) avec visuels HD,
/// badges métriques flottants et 3 points clés par étape - adapté à chacun des 3 rôles.
/// </summary>
public partial class FrmOnboarding : Form
{
    private readonly record struct Slide(
        string ImageFile,
        string StepBadge,
        string StatValue,
        string StatCaption,
        string Title,
        string Subtitle,
        string[] Highlights);

    private readonly Slide[] _slides = Array.Empty<Slide>();
    private int _index;

    private readonly SmoothPanel _leftStage = new();
    private readonly SmoothPanel _rightPanel = new();
    private readonly SmoothPanel _dotsPanel = new();
    private readonly Label _lblStep = new();
    private readonly Label _lblTitle = new();
    private readonly Label _lblSubtitle = new();
    private readonly Panel _highlightsHost = new();
    private Button _btnPrev = null!;
    private Button _btnNext = null!;
    private Image? _currentImage;

    private bool _dragging;
    private Point _dragStart;
    private readonly System.Windows.Forms.Timer _flipTimer = new();
    private float _flipProgress = 1f;

    public FrmOnboarding() : this(null) { }

    public FrmOnboarding(string? roleCode)
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        _slides = GetSlidesForRole(roleCode);
        Text = "IIT FINCORE - Découverte";
        Icon = Theme.LoadAppIcon();
        Size = new Size(980, 585);
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Color.White;
        DoubleBuffered = true;
        Opacity = 0;

        BuildLayout();
        Theme.ApplyCrispPixelFonts(this);
        Theme.ApplyRoundedRegion(this, 20);
        RenderSlide(0);

        var fade = new System.Windows.Forms.Timer { Interval = 14 };
        fade.Tick += (_, _) =>
        {
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.25);
            else fade.Stop();
        };
        Shown += (_, _) => fade.Start();
        _flipTimer.Interval = 16;
        _flipTimer.Tick += (_, _) =>
        {
            _flipProgress = Math.Min(1f, _flipProgress + 0.08f);
            _leftStage.Invalidate(new Rectangle(20, 88, 420, 360));
            if (_flipProgress >= 1f) _flipTimer.Stop();
        };
        FormClosed += (_, _) =>
        {
            _flipTimer.Stop();
            _flipTimer.Dispose();
            _currentImage?.Dispose();
        };
    }

    private void BuildLayout()
    {
        // ==================== COLONNE GAUCHE : SCÈNE VISUELLE ROYAL SAPPHIRE (460px) ====================
        _leftStage.Dock = DockStyle.Left;
        _leftStage.Width = 460;
        _leftStage.BackColor = Theme.SidebarNavy;
        EnableDrag(_leftStage);

        _leftStage.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Fond Royal Sapphire HD en cache BitBlt
            var navyBg = Theme.LoadNavyBackground();
            if (navyBg != null)
            {
                Theme.DrawCachedBackground(g, _leftStage, navyBg, Color.FromArgb(80, 4, 18, 42));
            }
            else
            {
                using var bg = new LinearGradientBrush(_leftStage.ClientRectangle, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.ForwardDiagonal);
                g.FillRectangle(bg, _leftStage.ClientRectangle);
            }

            // En-tête de marque discret en haut à gauche
            Theme.DrawRoundedLogo(g, new Rectangle(28, 26, 44, 44), 10);
            var fBrand = Theme.UiFont(13F, FontStyle.Bold);
            var typo = StringFormat.GenericTypographic;
            g.DrawString("IIT FIN", fBrand, Brushes.White, 82, 28, typo);
            float w1 = g.MeasureString("IIT FIN", fBrand, PointF.Empty, typo).Width;
            g.DrawString("CORE", fBrand, new SolidBrush(Theme.GoldLight), 82 + w1, 28, typo);
            g.DrawString("TRÉSORERIE & ANALYTIQUE", Theme.UiFont(7.2F, FontStyle.Bold),
                new SolidBrush(Color.FromArgb(180, 202, 230)), 83, 53);

            // Cadre arrondi flottant avec transition 3D Perspective (bascule + reflet doré)
            float easeFlip = 1f - MathF.Pow(1f - _flipProgress, 3f);
            float scaleX = 0.25f + 0.75f * easeFlip;
            int artW = Math.Max(40, (int)(404 * scaleX));
            int artH = 340 - (int)((1f - easeFlip) * 18f);
            int artX = 28 + (404 - artW) / 2;
            int artY = 96 + (340 - artH) / 2;
            var artRect = new Rectangle(artX, artY, artW, artH);
            using var artPath = Theme.CreateRoundedPath(artRect, 18);

            // Ombre douce 3D sous l'illustration
            using var shPath = Theme.CreateRoundedPath(new Rectangle(artRect.X + 4, artRect.Y + 8, artRect.Width, artRect.Height), 18);
            g.FillPath(new SolidBrush(Color.FromArgb(95, 0, 8, 22)), shPath);

            if (_currentImage != null)
            {
                var st = g.Save();
                g.SetClip(artPath);
                g.DrawImage(_currentImage, artRect);
                // Reflet lumineux doré balayant la carte pendant la rotation 3D
                if (_flipProgress < 1f)
                {
                    int sweepX = artRect.X + (int)(artRect.Width * easeFlip) - 55;
                    var sweepR = new Rectangle(sweepX, artRect.Y, 110, artRect.Height);
                    using var sheen = new LinearGradientBrush(sweepR, Color.FromArgb(0, Theme.GoldLight), Color.FromArgb(95, Theme.GoldLight), LinearGradientMode.Horizontal);
                    g.FillRectangle(sheen, sweepR);
                }
                g.Restore(st);
            }
            g.DrawPath(new Pen(Color.FromArgb(140, Theme.GoldLight), 1.6f), artPath);

            // Carte métrique glassmorphism flottante en bas du panneau gauche
            var slide = _slides[_index];
            var statRect = new Rectangle(28, 456, 404, 96);
            using var statPath = Theme.CreateRoundedPath(statRect, 14);
            g.FillPath(new SolidBrush(Color.FromArgb(175, 8, 28, 60)), statPath);
            g.DrawPath(new Pen(Color.FromArgb(120, Theme.GoldLight), 1.2f), statPath);

            // Barre verticale or à gauche du badge métrique
            using var accentBar = Theme.CreateRoundedPath(new Rectangle(44, 474, 5, 60), 3);
            g.FillPath(new SolidBrush(Theme.GoldLight), accentBar);

            g.DrawString(slide.StatValue, Theme.UiFont(16F, FontStyle.Bold),
                new SolidBrush(Theme.GoldLight), 62, 470);
            TextRenderer.DrawText(g, slide.StatCaption, Theme.UiFont(8.6F),
                new Rectangle(62, 502, 356, 40), Color.FromArgb(215, 228, 245),
                TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
        };

        // ==================== COLONNE DROITE : CONTENU ÉDITORIAL ÉPURÉ (520px) ====================
        _rightPanel.Dock = DockStyle.Fill;
        _rightPanel.BackColor = Color.White;
        EnableDrag(_rightPanel);

        _rightPanel.Paint += (_, e) =>
        {
            var lightBg = Theme.LoadLightBackground();
            if (lightBg != null)
            {
                Theme.DrawCachedBackground(e.Graphics, _rightPanel, lightBg, Color.FromArgb(215, 255, 255, 255));
            }
        };

        // Barre supérieure droite : Badge étape + Passer
        _lblStep.Font = new Font("Segoe UI", 8.2F, FontStyle.Bold);
        _lblStep.ForeColor = Theme.GoldHover;
        _lblStep.BackColor = Theme.GoldSoftBg;
        _lblStep.TextAlign = ContentAlignment.MiddleCenter;
        _lblStep.Bounds = new Rectangle(40, 28, 255, 28);
        Theme.ApplyRoundedRegion(_lblStep, 14);

        var btnSkip = new Button
        {
            Text = "Passer",
            Bounds = new Rectangle(410, 26, 78, 32),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 9F, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnSkip.FlatAppearance.BorderSize = 0;
        btnSkip.FlatAppearance.MouseOverBackColor = Color.FromArgb(236, 242, 250);
        Theme.ApplyRoundedRegion(btnSkip, 8);
        btnSkip.Click += (_, _) => Close();

        // Titre éditorial fort (hauteur confortable pour 2 lignes à 125% DPI)
        _lblTitle.Font = new Font("Segoe UI", 19.5F, FontStyle.Bold);
        _lblTitle.ForeColor = Theme.TextPrimary;
        _lblTitle.BackColor = Color.Transparent;
        _lblTitle.UseMnemonic = false;
        _lblTitle.Bounds = new Rectangle(40, 72, 444, 88);

        // Sous-titre fluide
        _lblSubtitle.Font = new Font("Segoe UI", 9.6F);
        _lblSubtitle.ForeColor = Theme.TextMuted;
        _lblSubtitle.BackColor = Color.Transparent;
        _lblSubtitle.UseMnemonic = false;
        _lblSubtitle.Bounds = new Rectangle(40, 166, 444, 52);

        // Zone des 3 cartes de points clés
        _highlightsHost.Bounds = new Rectangle(40, 226, 444, 246);
        _highlightsHost.BackColor = Color.Transparent;

        // Barre inférieure : indicateurs de progression + Précédent / Suivant
        _dotsPanel.Bounds = new Rectangle(40, 514, 110, 20);
        _dotsPanel.BackColor = Color.Transparent;
        _dotsPanel.Cursor = Cursors.Hand;
        _dotsPanel.Paint += DrawDots;
        _dotsPanel.MouseClick += (_, e) =>
        {
            int clicked = Math.Clamp(e.X / 36, 0, _slides.Length - 1);
            RenderSlide(clicked);
        };

        _btnPrev = Theme.CreateSecondaryButton("Précédent", 118, 42);
        _btnPrev.Location = new Point(196, 504);
        _btnPrev.Click += (_, _) => { if (_index > 0) RenderSlide(_index - 1); };

        _btnNext = Theme.CreatePrimaryButton("Suivant", 162, 42);
        _btnNext.Location = new Point(324, 504);
        _btnNext.Click += (_, _) =>
        {
            if (_index < _slides.Length - 1) RenderSlide(_index + 1);
            else Close();
        };

        _rightPanel.Controls.AddRange(new Control[]
        {
            _lblStep, btnSkip, _lblTitle, _lblSubtitle, _highlightsHost, _dotsPanel, _btnPrev, _btnNext
        });

        Controls.Add(_rightPanel);
        Controls.Add(_leftStage);
    }

    private void RenderSlide(int idx)
    {
        _index = idx;
        var s = _slides[idx];

        _currentImage?.Dispose();
        _currentImage = null;
        var imgPath = Path.Combine(AppContext.BaseDirectory, "Assets", s.ImageFile);
        if (File.Exists(imgPath))
        {
            using var fs = new FileStream(imgPath, FileMode.Open, FileAccess.Read);
            _currentImage = Image.FromStream(fs);
        }

        _lblStep.Text = s.StepBadge;
        _lblTitle.Text = s.Title;
        _lblSubtitle.Text = s.Subtitle;

        // Construit les 3 cartes de points clés avec barre latérale or
        _highlightsHost.Controls.Clear();
        int y = 4;
        for (int i = 0; i < s.Highlights.Length; i++)
        {
            var text = s.Highlights[i];
            var num = $"0{i + 1}";
            var itemCard = new SmoothPanel
            {
                Bounds = new Rectangle(0, y, 444, 68),
                BackColor = Color.White
            };
            itemCard.Paint += (_, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                var r = new Rectangle(0, 0, itemCard.Width - 1, itemCard.Height - 1);
                using var path = Theme.CreateRoundedPath(r, 11);
                g.FillPath(Brushes.White, path);
                g.DrawPath(new Pen(Theme.BorderColor, 1), path);

                // Pastille numérotée Bleu Saphir + texte Or
                var badgeR = new Rectangle(14, 17, 34, 34);
                using var bp = Theme.CreateRoundedPath(badgeR, 9);
                g.FillPath(new SolidBrush(Theme.SidebarNavy), bp);
                using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.DrawString(num, Theme.UiFont(9F, FontStyle.Bold), new SolidBrush(Theme.GoldLight), badgeR, sf);

                TextRenderer.DrawText(g, text, Theme.UiFont(9.3F, FontStyle.Bold),
                    new Rectangle(62, 12, itemCard.Width - 76, 44), Theme.TextPrimary,
                    TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            };
            _highlightsHost.Controls.Add(itemCard);
            y += 78;
        }

        _btnPrev.Visible = idx > 0;
        _btnNext.Text = idx == _slides.Length - 1 ? "Commencer" : "Suivant";

        _flipProgress = 0.05f;
        _flipTimer.Start();
        _leftStage.Invalidate();
        _dotsPanel.Invalidate();
    }

    private void DrawDots(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        int x = 0;
        for (int i = 0; i < _slides.Length; i++)
        {
            bool active = i == _index;
            int w = active ? 32 : 10;
            using var path = Theme.CreateRoundedPath(new Rectangle(x, 5, w, 9), 4);
            g.FillPath(new SolidBrush(active ? Theme.GoldAccent : Color.FromArgb(205, 216, 230)), path);
            x += w + 10;
        }
    }

    private void EnableDrag(Control ctrl)
    {
        ctrl.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { _dragging = true; _dragStart = e.Location; } };
        ctrl.MouseMove += (_, e) => { if (_dragging) Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y); };
        ctrl.MouseUp += (_, _) => _dragging = false;
    }

    private static Slide[] GetSlidesForRole(string? roleCode) => roleCode?.ToUpperInvariant() switch
    {
        "ADMIN" => new Slide[]
        {
            new("onboarding_1.jpg", "ESPACE ADMINISTRATEUR  •  01 / 03",
                "Vue Consolidée 360°", "Toutes vos structures, banques, caisses et Mobile Money synchronisés",
                "Pilotez la trésorerie\nmulti-structures",
                "Supervisez l'ensemble des entités associatives depuis un tableau de bord exécutif interactif.",
                new[]
                {
                    "Gestion complète des structures, comptes et soldes automatiques",
                    "Rapprochement bancaire & Mobile Money en un clic",
                    "Navigation instantanée depuis chaque carte KPI"
                }),
            new("onboarding_2.jpg", "ESPACE ADMINISTRATEUR  •  02 / 03",
                "Alertes 80 % & 100 %", "Contrôle budgétaire préventif et projection Cash-Flow à 6 mois",
                "Validez, anticipez\net clôturez sereinement",
                "Gardez le contrôle sur chaque dépense soumise et sécurisez vos exercices annuels.",
                new[]
                {
                    "Approbation ou rejet des dépenses avec commentaire tracé",
                    "Prévision de trésorerie à 6 mois et charges récurrentes",
                    "Clôture d'exercice verrouillée par trigger MySQL"
                }),
            new("onboarding_3.jpg", "ESPACE ADMINISTRATEUR  •  03 / 03",
                "Sécurité & Export PDF", "Chiffrement BCrypt, matrice RBAC et sauvegarde MySQL 1-clic",
                "Gouvernance totale\net documents officiels",
                "Administrez les droits de chaque rôle et produisez des rapports prêts pour l'assemblée.",
                new[]
                {
                    "Matrice interactive des permissions (Admin, Trésorier, Consultant)",
                    "Rapports financiers PDF officiels avec logo et signatures",
                    "Sauvegarde MySQL (.sql) et changement de logo client intégrés"
                })
        },
        "TRESORIER" => new Slide[]
        {
            new("onboarding_1.jpg", "ESPACE TRÉSORIER  •  01 / 03",
                "Trésorerie en direct", "Banque, Caisse et Mobile Money de votre structure",
                "Enregistrez vos opérations\nen quelques secondes",
                "Un espace dédié à votre structure pour suivre vos comptes et pointer vos relevés.",
                new[]
                {
                    "Saisie rapide de Recettes, Dépenses avec justificatifs et Transferts",
                    "Rapprochement bancaire : solde comptable vs solde rapproché",
                    "Édition instantanée de Reçus et Bordereaux officiels PDF"
                }),
            new("onboarding_2.jpg", "ESPACE TRÉSORIER  •  02 / 03",
                "Multi-ventilation 100 %", "Répartition précise sur plusieurs centres analytiques",
                "Maîtrisez vos budgets\net vos prévisions",
                "Chaque dépense est contrôlée automatiquement par rapport à vos enveloppes budgétaires.",
                new[]
                {
                    "Ventilation d'une même opération sur plusieurs centres analytiques",
                    "Alerte préventive immédiate en cas de dépassement budgétaire",
                    "Courbe prévisionnelle de trésorerie sur les 6 prochains mois"
                }),
            new("onboarding_3.jpg", "ESPACE TRÉSORIER  •  03 / 03",
                "Cotisations & Relances", "Import CSV massif et avis d'échéance PDF / WhatsApp",
                "Automatisez le suivi\nde vos adhérents",
                "Gagnez des heures sur la collecte des cotisations et le recouvrement des impayés.",
                new[]
                {
                    "Importation en 1 clic de vos listes de membres au format CSV",
                    "Encaissement générant automatiquement la recette sur le compte",
                    "Relance rapide des impayés par Avis PDF ou message WhatsApp"
                })
        },
        "CONSULTANT" => new Slide[]
        {
            new("onboarding_1.jpg", "ESPACE CONSULTANT  •  01 / 03",
                "Mode Lecture Seule", "Consultation sécurisée sans risque de modification",
                "Explorez la santé\nfinancière en toute clarté",
                "Accédez aux indicateurs consolidés, comptes et mouvements en lecture seule.",
                new[]
                {
                    "Tableau de bord analytique avec résultat net global en direct",
                    "Consultation filtrable de tous les comptes et transactions",
                    "Cloisonnement sécurisé garantissant l'intégrité des données"
                }),
            new("onboarding_2.jpg", "ESPACE CONSULTANT  •  02 / 03",
                "Analyse & Prévisions", "Répartition par centre analytique et Cash-Flow 6 mois",
                "Visualisez les tendances\net la rentabilité par projet",
                "Comparez recettes et dépenses par centre d'imputation et anticipez la trésorerie.",
                new[]
                {
                    "Graphique d'évolution mensuelle et répartition analytique",
                    "Synthèse Recettes − Dépenses par centre d'imputation",
                    "Projection de trésorerie à 6 mois basée sur les engagements"
                }),
            new("onboarding_3.jpg", "ESPACE CONSULTANT  •  03 / 03",
                "Rapports Officiels", "Exports PDF certifiés avec logo et fichiers CSV/Excel",
                "Exportez vos états\nfinanciers en un clic",
                "Préparez vos présentations de bureau ou assemblées générales sans ressaisie.",
                new[]
                {
                    "Rapport Financier & Analytique PDF A4 prêt à imprimer",
                    "Rapports détaillés des Recettes et des Dépenses",
                    "Exports CSV complets compatibles Microsoft Excel"
                })
        },
        _ => new Slide[]
        {
            new("onboarding_1.jpg", "BIENVENUE  •  01 / 03",
                "IIT FINCORE v2.0", "Solution professionnelle de gestion de trésorerie associative",
                "Pilotez tous vos comptes\nen temps réel",
                "Centralisez vos banques, caisses espèces et Mobile Money dans une interface élégante.",
                new[]
                {
                    "Gestion multi-structures et soldes actualisés automatiquement",
                    "Rapprochement bancaire & Mobile Money intégré",
                    "Transferts inter-comptes sécurisés et tracés en MySQL"
                }),
            new("onboarding_2.jpg", "BIENVENUE  •  02 / 03",
                "Analytique & Cash-Flow", "Multi-ventilation, alertes budgets et projection à 6 mois",
                "Maîtrisez chaque franc\npar projet et par budget",
                "Transformez vos mouvements de caisse en véritables indicateurs de décision.",
                new[]
                {
                    "Multi-ventilation d'une opération sur plusieurs centres analytiques",
                    "Alertes automatiques de dépassement budgétaire (80 % / 100 %)",
                    "Prévisions de trésorerie à 6 mois et charges récurrentes"
                }),
            new("onboarding_3.jpg", "BIENVENUE  •  03 / 03",
                "Sécurité & Conformité", "3 rôles dédiés, PDF officiels et sauvegarde MySQL 1-clic",
                "Conçu pour collaborer\nen toute sérénité",
                "Une gouvernance rigoureuse adaptée aux associations, écoles et organisations.",
                new[]
                {
                    "3 espaces sur mesure : Administrateur, Trésorier et Consultant",
                    "Rapports PDF officiels, Reçus et Avis de relance avec votre logo",
                    "Authentification BCrypt, Journal d'Audit et Clôture d'exercice"
                })
        }
    };
}

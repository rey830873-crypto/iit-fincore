using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Reflection;

namespace IIT.FinCore.WinForms.UI;

/// <summary>
/// Design System Commercial IIT FINCORE - Inspiré du logo officiel (Royal Sapphire & Metallic Gold).
/// </summary>
public static class Theme
{
    // Palette Royal Sapphire & Metallic Gold (extraite du logo officiel)
    public static readonly Color SidebarNavy = Color.FromArgb(6, 27, 58);          // #061B3A
    public static readonly Color SidebarNavyLight = Color.FromArgb(13, 50, 104);   // #0D3268
    public static readonly Color SidebarHover = Color.FromArgb(18, 62, 122);       // #123E7A

    public static readonly Color GoldLight = Color.FromArgb(246, 200, 76);         // #F6C84C
    public static readonly Color GoldAccent = Color.FromArgb(229, 169, 36);        // #E5A924
    public static readonly Color GoldHover = Color.FromArgb(204, 144, 20);         // #CC9014
    public static readonly Color GoldSoftBg = Color.FromArgb(253, 246, 227);       // #FDF6E3

    public static readonly Color PageBg = Color.FromArgb(242, 245, 250);           // #F2F5FA
    public static readonly Color CardBg = Color.White;
    public static readonly Color BorderColor = Color.FromArgb(224, 231, 240);      // #E0E7F0

    public static readonly Color TextPrimary = Color.FromArgb(12, 31, 61);         // #0C1F3D
    public static readonly Color TextMuted = Color.FromArgb(100, 116, 139);        // #64748B
    public static readonly Color SuccessGreen = Color.FromArgb(16, 152, 104);      // #109868
    public static readonly Color DangerRed = Color.FromArgb(217, 68, 82);          // #D94452

    private static readonly ConcurrentDictionary<(float, FontStyle), Font> _fontCache = new();
    private static readonly PropertyInfo? _dgvDoubleBufferedProp =
        typeof(DataGridView).GetProperty("DoubleBuffered", BindingFlags.Instance | BindingFlags.NonPublic);
    private static readonly Dictionary<(Image, int, int, int), Bitmap> _scaledBgCache = new();
    private static readonly object _scaledBgLock = new();

    public static Font UiFont(float ptSize, FontStyle style = FontStyle.Regular) =>
        _fontCache.GetOrAdd((ptSize, style), static k =>
            new Font("Segoe UI", k.Item1 * 96f / 72f, k.Item2, GraphicsUnit.Pixel));

    /// <summary>
    /// Dessine une image de fond pré-redimensionnée et mise en cache à la taille exacte du contrôle
    /// (avec filtre d'assombrissement optionnel déjà appliqué) pour un rendu BitBlt instantané.
    /// </summary>
    public static void DrawCachedBackground(Graphics g, Control ctrl, Image? source, Color? overlayColor = null)
    {
        int w = ctrl.ClientSize.Width;
        int h = ctrl.ClientSize.Height;
        if (source == null || w <= 0 || h <= 0) return;

        int overlayArgb = overlayColor?.ToArgb() ?? 0;
        var key = (source, w, h, overlayArgb);
        Bitmap? cached;

        lock (_scaledBgLock)
        {
            if (!_scaledBgCache.TryGetValue(key, out cached))
            {
                // Limite la taille du cache mémoire (évite l'accumulation lors des redimensionnements continus)
                if (_scaledBgCache.Count >= 12)
                {
                    foreach (var bmp in _scaledBgCache.Values)
                        bmp.Dispose();
                    _scaledBgCache.Clear();
                }

                cached = new Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                using (var bg = Graphics.FromImage(cached))
                {
                    bg.CompositingQuality = CompositingQuality.HighSpeed;
                    bg.InterpolationMode = InterpolationMode.Bilinear;
                    bg.DrawImage(source, 0, 0, w, h);
                    if (overlayColor.HasValue)
                    {
                        using var ov = new SolidBrush(overlayColor.Value);
                        bg.FillRectangle(ov, 0, 0, w, h);
                    }
                }
                _scaledBgCache[key] = cached;
            }
        }

        g.DrawImageUnscaled(cached, 0, 0);
    }

    public static readonly Font FontTitle = UiFont(17.5F, FontStyle.Bold);
    public static readonly Font FontSubtitle = UiFont(9.5F, FontStyle.Regular);
    public static readonly Font FontSection = UiFont(11.5F, FontStyle.Bold);
    public static readonly Font FontBody = UiFont(9.5F, FontStyle.Regular);
    public static readonly Font FontBodyBold = UiFont(9.5F, FontStyle.Bold);
    public static readonly Font FontKpiValue = UiFont(16.5F, FontStyle.Bold);

    /// <summary>
    /// Convertit les polices Point en Pixel (96 DPI) pour garantir une netteté ClearType HD native (PerMonitorV2)
    /// tout en empêchant tout débordement de texte sur les écrans Windows 125% / 150%.
    /// </summary>
    public static void ApplyCrispPixelFonts(Control root)
    {
        if (root.Font != null && root.Font.Unit == GraphicsUnit.Point)
            root.Font = new Font(root.Font.FontFamily, root.Font.SizeInPoints * 96f / 72f, root.Font.Style, GraphicsUnit.Pixel);

        if (root is Label lbl)
            lbl.UseMnemonic = false;

        if (root is FlowLayoutPanel flp && flp.AutoSize && flp.WrapContents && flp.Parent is Panel parentCard && parentCard.AutoSize)
        {
            parentCard.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flp.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            parentCard.Resize += (_, _) =>
            {
                int avail = Math.Max(120, parentCard.ClientSize.Width - parentCard.Padding.Horizontal);
                if (flp.MaximumSize.Width != avail)
                    flp.MaximumSize = new Size(avail, 0);
            };
        }

        if (root is DataGridView dgv)
        {
            if (dgv.ColumnHeadersDefaultCellStyle.Font is { Unit: GraphicsUnit.Point } hf)
                dgv.ColumnHeadersDefaultCellStyle.Font = new Font(hf.FontFamily, hf.SizeInPoints * 96f / 72f, hf.Style, GraphicsUnit.Pixel);
            if (dgv.DefaultCellStyle.Font is { Unit: GraphicsUnit.Point } cf)
                dgv.DefaultCellStyle.Font = new Font(cf.FontFamily, cf.SizeInPoints * 96f / 72f, cf.Style, GraphicsUnit.Pixel);
        }

        foreach (Control child in root.Controls)
            ApplyCrispPixelFonts(child);
    }

    private static Image? _cachedLogo;
    private static Image? _cachedLightBg;
    private static Image? _cachedNavyBg;

    public static void ResetLogoCache()
    {
        _cachedLogo?.Dispose();
        _cachedLogo = null;
    }

    public static Image? LoadLightBackground()
    {
        if (_cachedLightBg != null) return _cachedLightBg;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "bg_main_light.jpg");
        if (File.Exists(path))
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            _cachedLightBg = Image.FromStream(fs);
        }
        return _cachedLightBg;
    }

    public static Image? LoadNavyBackground()
    {
        if (_cachedNavyBg != null) return _cachedNavyBg;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "bg_navy_sapphire.jpg");
        if (File.Exists(path))
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            _cachedNavyBg = Image.FromStream(fs);
        }
        return _cachedNavyBg;
    }

    /// <summary>
    /// Rend transparents les conteneurs de mise en page (PageBg) d'une vue pour laisser apparaître le fond d'arrière-plan HD.
    /// </summary>
    public static void MakeLayoutContainersTransparent(Control root)
    {
        if (root is Form)
        {
            var lightBg = LoadLightBackground();
            if (lightBg != null)
            {
                root.BackgroundImage = lightBg;
                root.BackgroundImageLayout = ImageLayout.Stretch;
            }
        }
        else if (root.BackColor == PageBg)
        {
            root.BackColor = Color.Transparent;
        }

        foreach (Control child in root.Controls)
        {
            if (child is Panel or TableLayoutPanel or FlowLayoutPanel or SplitContainer)
            {
                if (child.BackColor == PageBg)
                    child.BackColor = Color.Transparent;
                if (child is SplitContainer sc)
                {
                    if (sc.Panel1.BackColor == PageBg) sc.Panel1.BackColor = Color.Transparent;
                    if (sc.Panel2.BackColor == PageBg) sc.Panel2.BackColor = Color.Transparent;
                }
                MakeLayoutContainersTransparent(child);
            }
        }
    }

    private static Icon? _cachedAppIcon;

    public static Icon? LoadAppIcon()
    {
        if (_cachedAppIcon != null) return _cachedAppIcon;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "app_icon.ico");
        if (File.Exists(path))
            _cachedAppIcon = new Icon(path);
        else
            _cachedAppIcon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        return _cachedAppIcon;
    }

    public static Image? LoadLogo()
    {
        if (_cachedLogo != null) return _cachedLogo;
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "logo_fincore.jpg");
        if (File.Exists(path))
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
            _cachedLogo = Image.FromStream(fs);
        }
        return _cachedLogo;
    }

    public static void DrawRoundedLogo(Graphics g, Rectangle rect, int radius = 18)
    {
        var logo = LoadLogo();
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        using var clipPath = CreateRoundedPath(rect, radius);
        if (logo != null)
        {
            var state = g.Save();
            g.SetClip(clipPath);
            g.DrawImage(logo, rect);
            g.Restore(state);
        }
        else
        {
            g.FillPath(new SolidBrush(SidebarNavy), clipPath);
            using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
            g.DrawString("IIT", new Font("Segoe UI", 18F, FontStyle.Bold), Brushes.White, rect, sf);
        }

        using var goldPen = new Pen(GoldLight, 2f);
        g.DrawPath(goldPen, clipPath);
    }

    public static GraphicsPath CreateRoundedPath(Rectangle rect, int radius)
    {
        var path = new GraphicsPath();
        int d = Math.Max(2, radius * 2);
        path.AddArc(rect.X, rect.Y, d, d, 180, 90);
        path.AddArc(rect.Right - d, rect.Y, d, d, 270, 90);
        path.AddArc(rect.Right - d, rect.Bottom - d, d, d, 0, 90);
        path.AddArc(rect.X, rect.Bottom - d, d, d, 90, 90);
        path.CloseFigure();
        return path;
    }

    public static void ApplyRoundedRegion(Control control, int radius)
    {
        void UpdateRegion()
        {
            if (control.Width <= 0 || control.Height <= 0) return;
            using var path = CreateRoundedPath(new Rectangle(0, 0, control.Width, control.Height), radius);
            var oldRegion = control.Region;
            control.Region = new Region(path);
            oldRegion?.Dispose();
        }
        UpdateRegion();
        control.Resize += (_, _) => UpdateRegion();
    }

    /// <summary>
    /// Bouton principal Or Métallique avec dégradé brillant et coins arrondis.
    /// </summary>
    public static Button CreatePrimaryButton(string text, int width = 165, int height = 38)
    {
        var btn = new GradientGoldButton
        {
            Text = text,
            Size = new Size(width, height),
            Font = FontBodyBold,
            ForeColor = TextPrimary,
            Cursor = Cursors.Hand
        };
        return btn;
    }

    public static Button CreateSecondaryButton(string text, int width = 125, int height = 38)
    {
        return new OutlineRoundedButton
        {
            Text = text,
            Size = new Size(width, height),
            Font = FontBodyBold,
            ForeColor = TextPrimary,
            Cursor = Cursors.Hand
        };
    }

    public static DataGridView CreateStyledGrid()
    {
        var grid = new DataGridView
        {
            Dock = DockStyle.Fill,
            BackgroundColor = Color.White,
            BorderStyle = BorderStyle.None,
            CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal,
            GridColor = Color.FromArgb(236, 240, 246),
            RowHeadersVisible = false,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            ReadOnly = true,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
            MultiSelect = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            EnableHeadersVisualStyles = false,
            RowTemplate = { Height = 38 }
        };

        // Active le double-buffering natif pour un défilement 60+ FPS fluide sans scintillement
        _dgvDoubleBufferedProp?.SetValue(grid, true);

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = FontBodyBold;
        grid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.False;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(8, 6, 8, 6);
        grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        grid.ColumnHeadersHeight = 40;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        grid.DefaultCellStyle.Font = FontBody;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = GoldSoftBg;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(8, 0, 8, 0);

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 254);

        // Supprime automatiquement les underscores "_" et optimise les largeurs proportionnelles
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.HeaderText = col.HeaderText.Replace("_", " ");
                var h = col.HeaderText.ToLowerInvariant();
                if (h is "id" or "code") col.FillWeight = 55;
                else if (h is "date" or "heure") col.FillWeight = 88;
                else if (h is "type" or "statut" or "pointée" or "alerte" or "exécution") col.FillWeight = 94;
                else if (h.Contains("référence")) col.FillWeight = 112;
                else if (h.Contains("montant") || h.Contains("solde")) col.FillWeight = 132;
                else if (h.Contains("ventilation")) col.FillWeight = 195;
                else if (h.Contains("libellé") || h.Contains("intitulé") || h.Contains("détails")) col.FillWeight = 180;
                else if (h.Contains("structure") || h.Contains("période") || h.Contains("compte") || h.Contains("centre")) col.FillWeight = 128;
                else col.FillWeight = 100;
            }
        };

        // Coloration intelligente des statuts et montants (+/-)
        grid.CellFormatting += (_, e) =>
        {
            if (e.Value == null) return;
            var text = e.Value.ToString()?.Trim() ?? "";

            if (text is "VALIDEE" or "Validée" or "APPROUVEE" or "Approuvée" or "PAYE" or "Payé" or "Actif" or "Active" or "ACTIVE" or "ACTIF" or "Pointée" or "OUVERT")
            {
                e.CellStyle!.ForeColor = SuccessGreen;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text is "EN_ATTENTE" or "En attente" or "PARTIEL" or "Partiel" or "BROUILLON" or "À pointer")
            {
                e.CellStyle!.ForeColor = GoldHover;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text is "REJETEE" or "Rejetée" or "IMPAYE" or "Impayé" or "ANNULEE" or "Annulée" or "Inactif" or "Inactive" or "Archivé" or "Désactivé" or "CLOTURE")
            {
                e.CellStyle!.ForeColor = DangerRed;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text.StartsWith("+") && text.Contains("FCFA"))
            {
                e.CellStyle!.ForeColor = SuccessGreen;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text.StartsWith("-") && text.Contains("FCFA"))
            {
                e.CellStyle!.ForeColor = DangerRed;
                e.CellStyle.Font = FontBodyBold;
            }
        };

        // Menu contextuel interactif (clic droit) sur tous les tableaux de l'application
        var ctx = new ContextMenuStrip();
        ctx.Items.Add("Copier la cellule", null, (_, _) =>
        {
            if (grid.CurrentCell?.Value != null)
                Clipboard.SetText(grid.CurrentCell.Value.ToString() ?? "");
        });
        ctx.Items.Add("Copier la ligne complète", null, (_, _) =>
        {
            if (grid.CurrentRow != null)
            {
                var line = string.Join(" | ", grid.CurrentRow.Cells.Cast<DataGridViewCell>().Select(c => c.Value?.ToString() ?? ""));
                Clipboard.SetText(line);
            }
        });
        grid.ContextMenuStrip = ctx;

        return grid;
    }

    /// <summary>
    /// Crée un bloc champ (Label au-dessus + contrôle dessous) pour les formulaires responsives en FlowLayoutPanel.
    /// </summary>
    public static Panel CreateFieldGroup(string labelText, Control input, int width = 170)
    {
        var wrap = new Panel
        {
            Size = new Size(width, 60),
            Margin = new Padding(0, 0, 12, 8),
            BackColor = Color.Transparent
        };
        var lbl = new Label
        {
            Text = labelText,
            Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
            ForeColor = TextMuted,
            Location = new Point(0, 0),
            AutoSize = true
        };
        if (input is TextBox tb)
        {
            var box = new ModernInputBox(tb)
            {
                Location = new Point(0, 22),
                Size = new Size(width, 34)
            };
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(box);
        }
        else
        {
            input.Location = new Point(0, 23);
            input.Width = width;
            if (input is ComboBox or NumericUpDown or DateTimePicker)
                input.Font = new Font("Segoe UI", 9.5F);
            wrap.Controls.Add(lbl);
            wrap.Controls.Add(input);
        }
        return wrap;
    }

    /// <summary>
    /// Transforme récursivement tous les TextBox classiques d'un conteneur en champs modernes arrondis avec focus Or.
    /// </summary>
    public static void UpgradeAllTextBoxes(Control root)
    {
        var snapshot = root.Controls.Cast<Control>().ToList();
        foreach (var ctrl in snapshot)
        {
            if (ctrl is ModernInputBox) continue;

            if (ctrl is TextBox tb && !tb.Multiline)
            {
                int idx = root.Controls.GetChildIndex(tb);
                // Sauvegarde position et taille AVANT que ModernInputBox ne repositionne le TextBox interne
                var origLoc = tb.Location;
                var origW = tb.Width;
                var origH = tb.Height;
                var origMargin = tb.Margin;
                var origAnchor = tb.Anchor;
                var origDock = tb.Dock;
                var origTab = tb.TabIndex;

                var box = new ModernInputBox(tb)
                {
                    Location = origLoc,
                    Size = new Size(origW, Math.Max(34, origH + 4)),
                    Margin = origMargin,
                    Anchor = origAnchor,
                    Dock = origDock,
                    TabIndex = origTab
                };
                root.Controls.Add(box);
                root.Controls.SetChildIndex(box, idx);
            }
            else if (ctrl.HasChildren)
            {
                UpgradeAllTextBoxes(ctrl);
            }
        }
    }

    public static float EaseOutCubic(float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        float inv = 1f - t;
        return 1f - inv * inv * inv;
    }

    /// <summary>
    /// Remplace tous les TabControl Windows classiques par une barre d'onglets moderne (pilules arrondies Bleu Saphir & Or)
    /// avec indicateur doré coulissant animé à 60 FPS.
    /// </summary>
    public static void UpgradeTabControls(Control root)
    {
        var snapshot = root.Controls.Cast<Control>().ToList();
        foreach (var ctrl in snapshot)
        {
            if (ctrl is TabControl tc && tc.TabPages.Count > 0)
            {
                int idx = root.Controls.GetChildIndex(tc);
                var host = new Panel { Dock = tc.Dock, Bounds = tc.Bounds, BackColor = PageBg };

                float indicatorX = 0f, indicatorW = 0f;
                float startX = 0f, startW = 0f, targetX = 0f, targetW = 0f;
                int animStep = 0;
                const int animTotalSteps = 10; // ~160ms à 60 FPS
                var animTimer = new System.Windows.Forms.Timer { Interval = 16 };

                var tabBar = new SmoothPanel
                {
                    Dock = DockStyle.Top,
                    Height = 52,
                    BackColor = PageBg,
                    Padding = new Padding(0, 2, 0, 12)
                };
                tabBar.Paint += (_, e) =>
                {
                    var g = e.Graphics;
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using var pen = new Pen(BorderColor, 1);
                    g.DrawLine(pen, 0, tabBar.Height - 1, tabBar.Width, tabBar.Height - 1);

                    if (indicatorW > 2f)
                    {
                        var barRect = new Rectangle((int)MathF.Round(indicatorX), tabBar.Height - 4, Math.Max(8, (int)MathF.Round(indicatorW)), 3);
                        using var barPath = CreateRoundedPath(barRect, 2);
                        using var barBrush = new LinearGradientBrush(barRect, GoldLight, GoldAccent, LinearGradientMode.Horizontal);
                        g.FillPath(barBrush, barPath);
                    }
                };
                tabBar.Disposed += (_, _) => { animTimer.Stop(); animTimer.Dispose(); };

                animTimer.Tick += (_, _) =>
                {
                    animStep++;
                    float p = EaseOutCubic(animStep / (float)animTotalSteps);
                    indicatorX = startX + (targetX - startX) * p;
                    indicatorW = startW + (targetW - startW) * p;
                    tabBar.Invalidate();
                    if (animStep >= animTotalSteps)
                        animTimer.Stop();
                };

                var pillFlow = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false,
                    BackColor = Color.Transparent
                };
                tabBar.Controls.Add(pillFlow);

                var pageHost = new Panel { Dock = DockStyle.Fill, BackColor = PageBg, Padding = new Padding(0, 10, 0, 0) };
                var pages = new List<Panel>();
                var buttons = new List<Button>();

                void AnimateIndicatorTo(Button activeBtn, bool instant)
                {
                    float tx = activeBtn.Left + pillFlow.Left;
                    float tw = activeBtn.Width;
                    if (instant || indicatorW <= 0f)
                    {
                        animTimer.Stop();
                        indicatorX = tx;
                        indicatorW = tw;
                        tabBar.Invalidate();
                        return;
                    }
                    startX = indicatorX;
                    startW = indicatorW;
                    targetX = tx;
                    targetW = tw;
                    animStep = 0;
                    animTimer.Start();
                }

                void SelectTab(int activeIndex, bool instant = false)
                {
                    pageHost.SuspendLayout();
                    for (int i = 0; i < pages.Count; i++)
                    {
                        bool active = i == activeIndex;
                        pages[i].Visible = active;
                        buttons[i].BackColor = active ? SidebarNavy : Color.White;
                        buttons[i].ForeColor = active ? GoldLight : TextMuted;
                        buttons[i].Font = UiFont(9.5F, active ? FontStyle.Bold : FontStyle.Regular);
                    }
                    pageHost.ResumeLayout(true);
                    if (activeIndex >= 0 && activeIndex < buttons.Count)
                        AnimateIndicatorTo(buttons[activeIndex], instant);
                }

                for (int i = 0; i < tc.TabPages.Count; i++)
                {
                    var tp = tc.TabPages[i];
                    var pagePanel = new Panel { Dock = DockStyle.Fill, BackColor = PageBg, Visible = i == 0 };

                    // Déplace tous les contrôles de l'onglet en conservant l'ordre Z
                    var children = tp.Controls.Cast<Control>().ToList();
                    foreach (var ch in children)
                        pagePanel.Controls.Add(ch);

                    pages.Add(pagePanel);
                    pageHost.Controls.Add(pagePanel);

                    int tabIndex = i;
                    var textW = TextRenderer.MeasureText(tp.Text, UiFont(9.5F, FontStyle.Bold)).Width;
                    var btn = new Button
                    {
                        Text = tp.Text,
                        UseMnemonic = false,
                        Size = new Size(Math.Max(140, textW + 34), 36),
                        FlatStyle = FlatStyle.Flat,
                        Cursor = Cursors.Hand,
                        Margin = new Padding(0, 0, 10, 0)
                    };
                    btn.FlatAppearance.BorderSize = 0;
                    btn.FlatAppearance.MouseOverBackColor = SidebarNavyLight;
                    ApplyRoundedRegion(btn, 10);
                    btn.Click += (_, _) => SelectTab(tabIndex);

                    buttons.Add(btn);
                    pillFlow.Controls.Add(btn);
                }

                SelectTab(0, instant: true);
                pillFlow.Layout += (_, _) =>
                {
                    if (indicatorW <= 0f && buttons.Count > 0)
                        AnimateIndicatorTo(buttons[0], instant: true);
                };
                host.Controls.Add(pageHost);
                host.Controls.Add(tabBar);

                root.Controls.Remove(tc);
                root.Controls.Add(host);
                root.Controls.SetChildIndex(host, idx);
            }
            else if (ctrl.HasChildren)
            {
                UpgradeTabControls(ctrl);
            }
        }
    }

    public static Panel CreateDialogHeader(string title, string subtitle)
    {
        var header = new SmoothPanel { Dock = DockStyle.Top, Height = 72, BackColor = SidebarNavy };
        header.Paint += (_, e) =>
        {
            var navyBg = LoadNavyBackground();
            if (navyBg != null)
            {
                DrawCachedBackground(e.Graphics, header, navyBg, Color.FromArgb(95, 5, 20, 46));
            }
            else
            {
                using var bg = new LinearGradientBrush(header.ClientRectangle, SidebarNavyLight, SidebarNavy, LinearGradientMode.Horizontal);
                e.Graphics.FillRectangle(bg, header.ClientRectangle);
            }
            using var goldBar = new LinearGradientBrush(new Rectangle(0, header.Height - 3, header.Width, 3), GoldLight, GoldAccent, LinearGradientMode.Horizontal);
            e.Graphics.FillRectangle(goldBar, 0, header.Height - 3, header.Width, 3);
        };
        header.Controls.Add(new Label { Text = title, Font = new Font("Segoe UI", 13F, FontStyle.Bold), ForeColor = Color.White, BackColor = Color.Transparent, Location = new Point(22, 12), AutoSize = true });
        header.Controls.Add(new Label { Text = subtitle, Font = new Font("Segoe UI", 8.8F), ForeColor = Color.FromArgb(190, 210, 235), BackColor = Color.Transparent, Location = new Point(23, 40), AutoSize = true });
        return header;
    }

    /// <summary>
    /// Transforme n'importe quel dialogue en fenêtre moderne sans bordure Windows (coins arrondis, déplaçable, bouton ✕).
    /// </summary>
    public static void ApplyModernDialogChrome(Form form, Panel header)
    {
        form.FormBorderStyle = FormBorderStyle.None;
        form.BackColor = PageBg;
        ApplyRoundedRegion(form, 16);

        bool dragging = false;
        Point dragStart = Point.Empty;
        header.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
        header.MouseMove += (_, e) => { if (dragging) form.Location = new Point(form.Location.X + e.X - dragStart.X, form.Location.Y + e.Y - dragStart.Y); };
        header.MouseUp += (_, _) => dragging = false;

        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(34, 34),
            Anchor = AnchorStyles.Top | AnchorStyles.Right,
            Location = new Point(form.Width - 48, 18),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 218, 240),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
        btnClose.Click += (_, _) => form.Close();
        ApplyRoundedRegion(btnClose, 8);
        header.Controls.Add(btnClose);

        form.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = CreateRoundedPath(new Rectangle(0, 0, form.Width - 1, form.Height - 1), 16);
            using var pen = new Pen(Color.FromArgb(190, 204, 222), 1.5f);
            e.Graphics.DrawPath(pen, path);
        };

        form.Icon = LoadAppIcon();
        form.Opacity = 0;
        var fadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
        fadeTimer.Tick += (_, _) =>
        {
            if (form.IsDisposed) { fadeTimer.Stop(); fadeTimer.Dispose(); return; }
            if (form.Opacity < 1) form.Opacity = Math.Min(1, form.Opacity + 0.18);
            else { fadeTimer.Stop(); fadeTimer.Dispose(); }
        };
        form.Shown += (_, _) => fadeTimer.Start();
        form.FormClosed += (_, _) => { fadeTimer.Stop(); fadeTimer.Dispose(); };
        form.Load += (_, _) =>
        {
            UpgradeAllTextBoxes(form);
            ApplyCrispPixelFonts(form);
        };
    }

    public static Panel CreateCardPanel(Padding? padding = null)
    {
        var panel = new SmoothPanel
        {
            BackColor = CardBg,
            Padding = padding ?? new Padding(18)
        };
        panel.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
            using var path = CreateRoundedPath(rect, 12);
            using var pen = new Pen(BorderColor, 1);
            e.Graphics.DrawPath(pen, path);
        };
        ApplyRoundedRegion(panel, 12);
        return panel;
    }

    /// <summary>
    /// Projette et dessine des anneaux orbitaux 3D en perspective (X, Y, Z) avec sphères lumineuses or
    /// autour du logo IIT FINCORE. Appeler avec frontHalf=false avant le logo et frontHalf=true après.
    /// </summary>
    public static void Draw3DOrbitalRings(Graphics g, PointF center, float radius, float angleRad, bool frontHalf)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        const float fov = 340f;

        // 2 anneaux 3D inclinés sur des plans orthogonaux pour un vrai volume spatial
        (float tiltX, float tiltZ, float speedMul, Color ringCol)[] rings =
        {
            (0.95f,  0.34f,  1.0f, GoldLight),
            (1.12f, -0.46f, -0.7f, Color.FromArgb(145, 200, 255))
        };

        foreach (var (tiltX, tiltZ, speedMul, ringCol) in rings)
        {
            float cosTx = MathF.Cos(tiltX), sinTx = MathF.Sin(tiltX);
            float cosTz = MathF.Cos(tiltZ), sinTz = MathF.Sin(tiltZ);
            const int segs = 48;

            PointF? prev = null;
            for (int i = 0; i <= segs; i++)
            {
                float t = (i / (float)segs) * MathF.PI * 2f + angleRad * speedMul;
                float x0 = MathF.Cos(t) * radius;
                float y0 = MathF.Sin(t) * radius;

                // Rotation 3D autour de X puis Z
                float y1 = y0 * cosTx;
                float z1 = y0 * sinTx;
                float x2 = x0 * cosTz - y1 * sinTz;
                float y2 = x0 * sinTz + y1 * cosTz;

                bool isFront = z1 >= 0f;
                float persp = fov / (fov - z1);
                var pt = new PointF(center.X + x2 * persp, center.Y + y2 * persp);

                if (prev.HasValue && isFront == frontHalf)
                {
                    int alpha = isFront ? 155 : 55;
                    float width = isFront ? 2.1f : 1.1f;
                    using var pen = new Pen(Color.FromArgb(alpha, ringCol), width);
                    g.DrawLine(pen, prev.Value, pt);
                }
                prev = pt;
            }

            // 2 particules orbitales 3D lumineuses sur chaque anneau
            for (int p = 0; p < 2; p++)
            {
                float t = angleRad * speedMul * 1.6f + p * MathF.PI;
                float x0 = MathF.Cos(t) * radius;
                float y0 = MathF.Sin(t) * radius;
                float y1 = y0 * cosTx;
                float z1 = y0 * sinTx;
                float x2 = x0 * cosTz - y1 * sinTz;
                float y2 = x0 * sinTz + y1 * cosTz;

                if ((z1 >= 0f) != frontHalf) continue;

                float persp = fov / (fov - z1);
                float px = center.X + x2 * persp;
                float py = center.Y + y2 * persp;
                float rNode = (frontHalf ? 4.4f : 2.6f) * persp;

                using var glow = new SolidBrush(Color.FromArgb(frontHalf ? 75 : 30, ringCol));
                g.FillEllipse(glow, px - rNode * 2.1f, py - rNode * 2.1f, rNode * 4.2f, rNode * 4.2f);
                using var core = new SolidBrush(frontHalf ? Color.White : Color.FromArgb(140, ringCol));
                g.FillEllipse(core, px - rNode * 0.65f, py - rNode * 0.65f, rNode * 1.3f, rNode * 1.3f);
            }
        }
    }

    public static Panel CreateKpiCard(string title, string value, Color accentColor, Action? onClick = null)
    {
        var card = CreateCardPanel(new Padding(18, 14, 14, 14));
        card.Height = 108;
        bool hovered = false;
        var hoverPt = new Point(120, 50);

        if (onClick != null)
            card.Cursor = Cursors.Hand;

        // Effet 3D au survol : reflet lumineux doré qui suit la souris + biseau inférieur 3D + soulèvement
        card.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            if (card.Width <= 4 || card.Height <= 4) return;

            var cardRect = new Rectangle(0, 0, card.Width - 1, card.Height - 1);
            using var cardPath = CreateRoundedPath(cardRect, 12);

            if (hovered)
            {
                // Reflet lumineux 3D (specular spotlight) suivant le curseur
                var lightRect = new Rectangle(hoverPt.X - 110, hoverPt.Y - 75, 220, 150);
                using var lp = new GraphicsPath();
                lp.AddEllipse(lightRect);
                using var pgb = new PathGradientBrush(lp)
                {
                    CenterColor = Color.FromArgb(34, GoldLight),
                    SurroundColors = new[] { Color.FromArgb(0, GoldLight) }
                };
                var st = g.Save();
                g.SetClip(cardPath);
                g.FillEllipse(pgb, lightRect);
                // Biseau 3D inférieur (effet carte surélevée)
                using var bevelPen = new Pen(Color.FromArgb(55, SidebarNavy), 3f);
                g.DrawLine(bevelPen, 10, card.Height - 2, card.Width - 10, card.Height - 2);
                g.Restore(st);

                using var hoverPen = new Pen(GoldAccent, 1.8f);
                g.DrawPath(hoverPen, cardPath);
            }

            int lift = hovered ? -2 : 0;
            using var barPath = CreateRoundedPath(new Rectangle(12, 18 + lift, 5, card.Height - 36), 3);
            using var barBrush = new SolidBrush(accentColor);
            g.FillPath(barBrush, barPath);

            using var halo = new SolidBrush(Color.FromArgb(hovered ? 44 : 20, accentColor));
            g.FillEllipse(halo, card.Width - 54, 16 + lift, 36, 36);
            using var dot = new SolidBrush(accentColor);
            g.FillEllipse(dot, card.Width - 41, 29 + lift, 10, 10);
        };

        var lblTitle = new Label
        {
            Text = title.ToUpperInvariant(),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = TextMuted,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(28, 18)
        };

        var lblValue = new Label
        {
            Name = "lblValue",
            Text = value,
            Font = FontKpiValue,
            ForeColor = TextPrimary,
            BackColor = Color.Transparent,
            AutoSize = true,
            Location = new Point(26, 46)
        };

        void AttachEvents(Control c)
        {
            if (onClick != null) c.Cursor = Cursors.Hand;
            c.MouseEnter += (_, _) => { hovered = true; lblTitle.Top = 16; lblValue.Top = 44; card.Invalidate(); };
            c.MouseLeave += (_, _) =>
            {
                if (!card.ClientRectangle.Contains(card.PointToClient(Cursor.Position)))
                {
                    hovered = false;
                    lblTitle.Top = 18;
                    lblValue.Top = 46;
                    card.Invalidate();
                }
            };
            c.MouseMove += (_, me) =>
            {
                hoverPt = c == card ? me.Location : card.PointToClient(c.PointToScreen(me.Location));
                if (hovered) card.Invalidate();
            };
            if (onClick != null)
                c.Click += (_, _) => onClick();
        }
        AttachEvents(card);
        AttachEvents(lblTitle);
        AttachEvents(lblValue);

        card.Controls.Add(lblValue);
        card.Controls.Add(lblTitle);
        return card;
    }
}

/// <summary>
/// Champ de saisie moderne arrondi avec marge interne confortable, survol doux et bordure Or Métallique au focus.
/// </summary>
public class ModernInputBox : SmoothPanel
{
    private readonly TextBox _inner;
    private bool _focused;
    private bool _hovered;

    public ModernInputBox(TextBox inner)
    {
        _inner = inner;
        BackColor = inner.ReadOnly ? Color.FromArgb(248, 250, 253) : Color.White;
        Cursor = Cursors.IBeam;

        inner.BorderStyle = BorderStyle.None;
        inner.Font = new Font("Segoe UI", 9.8F, FontStyle.Regular);
        inner.ForeColor = Theme.TextPrimary;
        inner.BackColor = BackColor;
        inner.Dock = DockStyle.None;
        inner.Anchor = AnchorStyles.Left | AnchorStyles.Right;

        inner.GotFocus += (_, _) => { _focused = true; Invalidate(); };
        inner.LostFocus += (_, _) => { _focused = false; Invalidate(); };
        inner.MouseEnter += (_, _) => { _hovered = true; Invalidate(); };
        inner.MouseLeave += (_, _) => { _hovered = false; Invalidate(); };
        inner.ReadOnlyChanged += (_, _) =>
        {
            BackColor = inner.ReadOnly ? Color.FromArgb(248, 250, 253) : Color.White;
            inner.BackColor = BackColor;
            Invalidate();
        };

        MouseEnter += (_, _) => { _hovered = true; Invalidate(); };
        MouseLeave += (_, _) => { _hovered = false; Invalidate(); };
        Click += (_, _) => inner.Focus();

        Controls.Add(inner);
        Resize += (_, _) => LayoutInner();
        LayoutInner();
    }

    private void LayoutInner()
    {
        int padX = 11;
        int h = _inner.PreferredHeight;
        _inner.SetBounds(padX, Math.Max(4, (Height - h) / 2), Math.Max(20, Width - padX * 2), h);
    }

    private Color GetEffectiveParentBackColor()
    {
        var p = Parent;
        while (p != null)
        {
            if (p.BackColor != Color.Transparent && p.BackColor.A == 255)
                return p.BackColor;
            p = p.Parent;
        }
        return Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 2 || Height <= 2) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var parentBg = new SolidBrush(GetEffectiveParentBackColor());
        g.FillRectangle(parentBg, ClientRectangle);

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = Theme.CreateRoundedPath(rect, 8);
        using var fill = new SolidBrush(BackColor);
        g.FillPath(fill, path);

        var borderColor = _focused
            ? Theme.GoldAccent
            : (_hovered ? Color.FromArgb(165, 184, 208) : Color.FromArgb(210, 220, 232));
        float penWidth = _focused ? 1.8f : 1.1f;

        using var pen = new Pen(borderColor, penWidth);
        g.DrawPath(pen, path);
    }
}

/// <summary>
/// Bouton secondaire blanc avec bordure arrondie anti-aliasée nette sur les 4 côtés.
/// </summary>
public class OutlineRoundedButton : Button
{
    private bool _hovered;

    public OutlineRoundedButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        MouseEnter += (_, _) => { _hovered = true; Invalidate(); };
        MouseLeave += (_, _) => { _hovered = false; Invalidate(); };
    }

    private Color GetEffectiveParentBackColor()
    {
        var p = Parent;
        while (p != null)
        {
            if (p.BackColor != Color.Transparent && p.BackColor.A == 255)
                return p.BackColor;
            p = p.Parent;
        }
        return Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 2 || Height <= 2) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        using var parentBg = new SolidBrush(GetEffectiveParentBackColor());
        g.FillRectangle(parentBg, ClientRectangle);

        var rect = new Rectangle(1, 1, Width - 3, Height - 3);
        using var path = Theme.CreateRoundedPath(rect, 8);
        using var bg = new SolidBrush(_hovered ? Color.FromArgb(242, 246, 252) : Color.White);
        using var pen = new Pen(_hovered ? Theme.SidebarNavyLight : Theme.BorderColor, 1.2f);
        g.FillPath(bg, path);
        g.DrawPath(pen, path);

        TextRenderer.DrawText(g, Text, Font, rect, Theme.TextPrimary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

/// <summary>
/// Panel double-buffered avec ResizeRedraw=true pour éviter tout artefact graphique au redimensionnement.
/// </summary>
public class SmoothPanel : Panel
{
    public SmoothPanel()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
    }
}

/// <summary>
/// Bouton personnalisé avec dégradé Or Métallique (ou couleur solide si BackColor est surchargé en vert/rouge).
/// </summary>
public class GradientGoldButton : Button
{
    private bool _hovered;
    private Color? _customColor;

    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public override Color BackColor
    {
        get => _customColor ?? Theme.GoldAccent;
        set
        {
            if (value != Color.Empty && value != Color.Transparent)
                _customColor = value;
            base.BackColor = value;
            Invalidate();
        }
    }

    public GradientGoldButton()
    {
        FlatStyle = FlatStyle.Flat;
        FlatAppearance.BorderSize = 0;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        MouseEnter += (_, _) => { _hovered = true; Invalidate(); };
        MouseLeave += (_, _) => { _hovered = false; Invalidate(); };
    }

    private Color GetEffectiveParentBackColor()
    {
        var p = Parent;
        while (p != null)
        {
            if (p.BackColor != Color.Transparent && p.BackColor.A == 255)
                return p.BackColor;
            p = p.Parent;
        }
        return Color.White;
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        if (Width <= 2 || Height <= 2) return;
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        // Remplit les 4 coins avec la vraie couleur opaque du conteneur (évite tout pixel noir sur FlowLayoutPanel transparent)
        using var parentBg = new SolidBrush(GetEffectiveParentBackColor());
        g.FillRectangle(parentBg, ClientRectangle);

        var rect = new Rectangle(0, 0, Width - 1, Height - 1);
        using var path = Theme.CreateRoundedPath(rect, 9);

        if (_customColor.HasValue)
        {
            var c1 = _hovered ? ControlPaint.Dark(_customColor.Value, 0.08f) : _customColor.Value;
            using var br = new SolidBrush(c1);
            g.FillPath(br, path);
        }
        else
        {
            var topCol = _hovered ? Theme.GoldAccent : Theme.GoldLight;
            var botCol = _hovered ? Theme.GoldHover : Theme.GoldAccent;
            using var grad = new LinearGradientBrush(rect, topCol, botCol, LinearGradientMode.Vertical);
            g.FillPath(grad, path);
        }

        var textColor = _customColor.HasValue ? Color.White : Theme.TextPrimary;
        TextRenderer.DrawText(g, Text, Font, rect, textColor,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
    }
}

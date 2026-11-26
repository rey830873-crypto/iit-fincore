using System.Drawing;
using System.Drawing.Drawing2D;

namespace IIT.FinCore.WinForms.UI;

/// <summary>
/// Design System Commercial IIT FINCORE — Inspiré du logo officiel (Royal Sapphire & Metallic Gold).
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

    public static Font UiFont(float ptSize, FontStyle style = FontStyle.Regular) =>
        new("Segoe UI", ptSize * 96f / 72f, style, GraphicsUnit.Pixel);

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
        if (root.BackColor == PageBg)
            root.BackColor = Color.Transparent;

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
            control.Region = new Region(path);
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

        grid.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(247, 249, 252);
        grid.ColumnHeadersDefaultCellStyle.ForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(247, 249, 252);
        grid.ColumnHeadersDefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.ColumnHeadersDefaultCellStyle.Font = FontBodyBold;
        grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(10, 6, 10, 6);
        grid.ColumnHeadersHeight = 42;
        grid.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.Single;

        grid.DefaultCellStyle.Font = FontBody;
        grid.DefaultCellStyle.ForeColor = TextPrimary;
        grid.DefaultCellStyle.BackColor = Color.White;
        grid.DefaultCellStyle.SelectionBackColor = GoldSoftBg;
        grid.DefaultCellStyle.SelectionForeColor = TextPrimary;
        grid.DefaultCellStyle.Padding = new Padding(10, 0, 10, 0);

        grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(250, 251, 254);

        // Supprime automatiquement les underscores "_" et optimise les largeurs proportionnelles
        grid.DataBindingComplete += (_, _) =>
        {
            foreach (DataGridViewColumn col in grid.Columns)
            {
                col.HeaderText = col.HeaderText.Replace("_", " ");
                var h = col.HeaderText.ToLowerInvariant();
                if (h is "id" or "code") col.FillWeight = 50;
                else if (h is "date" or "heure" or "type" or "statut" or "alerte" or "exécution") col.FillWeight = 82;
                else if (h.Contains("libellé") || h.Contains("intitulé") || h.Contains("ventilation") || h.Contains("détails")) col.FillWeight = 175;
                else if (h.Contains("structure") || h.Contains("période") || h.Contains("compte") || h.Contains("centre")) col.FillWeight = 135;
                else col.FillWeight = 100;
            }
        };

        // Coloration intelligente des statuts et montants (+/-)
        grid.CellFormatting += (_, e) =>
        {
            if (e.Value == null) return;
            var text = e.Value.ToString()?.Trim() ?? "";

            if (text is "VALIDEE" or "APPROUVEE" or "PAYE" or "Actif" or "Active" or "ACTIVE" or "ACTIF" or "Pointée" or "OUVERT")
            {
                e.CellStyle!.ForeColor = SuccessGreen;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text is "EN_ATTENTE" or "PARTIEL" or "BROUILLON" or "À pointer")
            {
                e.CellStyle!.ForeColor = GoldHover;
                e.CellStyle.Font = FontBodyBold;
            }
            else if (text is "REJETEE" or "IMPAYE" or "ANNULEE" or "Inactif" or "Inactive" or "Archivé" or "Désactivé" or "CLOTURE")
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

    /// <summary>
    /// Remplace tous les TabControl Windows classiques par une barre d'onglets moderne (pilules arrondies Bleu Saphir & Or).
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

                var tabBar = new SmoothPanel
                {
                    Dock = DockStyle.Top,
                    Height = 50,
                    BackColor = PageBg,
                    Padding = new Padding(0, 2, 0, 10)
                };
                tabBar.Paint += (_, e) =>
                {
                    using var pen = new Pen(BorderColor, 1);
                    e.Graphics.DrawLine(pen, 0, tabBar.Height - 1, tabBar.Width, tabBar.Height - 1);
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

                void SelectTab(int activeIndex)
                {
                    for (int i = 0; i < pages.Count; i++)
                    {
                        bool active = i == activeIndex;
                        pages[i].Visible = active;
                        buttons[i].BackColor = active ? SidebarNavy : Color.White;
                        buttons[i].ForeColor = active ? GoldLight : TextMuted;
                        buttons[i].Font = UiFont(9.5F, active ? FontStyle.Bold : FontStyle.Regular);
                    }
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
                    var textW = TextRenderer.MeasureText(tp.Text, new Font("Segoe UI", 9.5F, FontStyle.Bold)).Width;
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

                SelectTab(0);
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
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.DrawImage(navyBg, header.ClientRectangle);
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(95, 5, 20, 46)), header.ClientRectangle);
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

    public static Panel CreateKpiCard(string title, string value, Color accentColor, Action? onClick = null)
    {
        var card = CreateCardPanel(new Padding(18, 14, 14, 14));
        card.Height = 108;
        bool hovered = false;

        if (onClick != null)
            card.Cursor = Cursors.Hand;

        // Pastille latérale arrondie élégante + surbrillance dorée interactive au survol
        card.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (hovered && onClick != null)
            {
                using var hoverPath = CreateRoundedPath(new Rectangle(0, 0, card.Width - 1, card.Height - 1), 12);
                using var hoverPen = new Pen(GoldAccent, 1.8f);
                e.Graphics.DrawPath(hoverPen, hoverPath);
            }

            using var barPath = CreateRoundedPath(new Rectangle(12, 18, 5, card.Height - 36), 3);
            using var barBrush = new SolidBrush(accentColor);
            e.Graphics.FillPath(barBrush, barPath);

            using var halo = new SolidBrush(Color.FromArgb(hovered ? 38 : 20, accentColor));
            e.Graphics.FillEllipse(halo, card.Width - 54, 16, 36, 36);
            using var dot = new SolidBrush(accentColor);
            e.Graphics.FillEllipse(dot, card.Width - 41, 29, 10, 10);
        };

        var lblTitle = new Label
        {
            Text = title.ToUpperInvariant(),
            Font = new Font("Segoe UI", 8F, FontStyle.Bold),
            ForeColor = TextMuted,
            AutoSize = true,
            Location = new Point(28, 18)
        };

        var lblValue = new Label
        {
            Name = "lblValue",
            Text = value,
            Font = FontKpiValue,
            ForeColor = TextPrimary,
            AutoSize = true,
            Location = new Point(26, 46)
        };

        if (onClick != null)
        {
            void AttachEvents(Control c)
            {
                c.Cursor = Cursors.Hand;
                c.MouseEnter += (_, _) => { hovered = true; card.Invalidate(); };
                c.MouseLeave += (_, _) => { hovered = false; card.Invalidate(); };
                c.Click += (_, _) => onClick();
            }
            AttachEvents(card);
            AttachEvents(lblTitle);
            AttachEvents(lblValue);
        }

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

using System.Drawing;
using System.Drawing.Drawing2D;

namespace IIT.FinCore.WinForms.UI.Dialogs;

public enum FinCoreAlertType
{
    Success,
    Info,
    Warning,
    Error,
    Question
}

/// <summary>
/// Boîte de dialogue moderne aux couleurs officielles IIT FINCORE (Royal Sapphire & Metallic Gold),
/// remplaçant le MessageBox Windows natif avec icônes vectorielles anti-aliasées, fondu fluide et hauteur adaptative.
/// </summary>
public sealed class FrmMessageBox : Form
{
    private const int CsDropShadow = 0x00020000;
    private readonly FinCoreAlertType _alertType;
    private readonly Color _accentColor;
    private readonly Color _softBgColor;
    private readonly string _badgeLabel;

    protected override CreateParams CreateParams
    {
        get
        {
            var cp = base.CreateParams;
            cp.ClassStyle |= CsDropShadow;
            return cp;
        }
    }

    private FrmMessageBox(
        string message,
        string caption,
        MessageBoxButtons buttons,
        FinCoreAlertType alertType,
        string? primaryText = null,
        string? secondaryText = null)
    {
        _alertType = alertType;
        (_accentColor, _softBgColor, _badgeLabel) = alertType switch
        {
            FinCoreAlertType.Success => (Theme.SuccessGreen, Color.FromArgb(236, 250, 244), "OPÉRATION RÉUSSIE"),
            FinCoreAlertType.Warning => (Theme.GoldAccent, Theme.GoldSoftBg, "ATTENTION"),
            FinCoreAlertType.Error => (Theme.DangerRed, Color.FromArgb(253, 240, 242), "ERREUR"),
            FinCoreAlertType.Question => (Theme.SidebarNavyLight, Color.FromArgb(238, 244, 252), "CONFIRMATION"),
            _ => (Color.FromArgb(37, 99, 235), Color.FromArgb(239, 246, 255), "INFORMATION")
        };

        FormBorderStyle = FormBorderStyle.None;
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        DoubleBuffered = true;
        KeyPreview = true;
        BackColor = Color.FromArgb(190, 204, 222);
        Padding = new Padding(1);
        Icon = Theme.LoadAppIcon();

        var cleanCaption = string.IsNullOrWhiteSpace(caption) ? "IIT FINCORE" : caption.Trim();
        Text = cleanCaption;

        // Calcul adaptatif des dimensions selon la longueur du message
        const int dialogWidth = 470;
        const int textLeft = 96;
        const int textRightPad = 28;
        int textMaxWidth = dialogWidth - textLeft - textRightPad;

        var msgFont = Theme.UiFont(10.0F, FontStyle.Regular);
        var measured = TextRenderer.MeasureText(
            message,
            msgFont,
            new Size(textMaxWidth, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl | TextFormatFlags.NoPrefix);

        int bodyHeight = Math.Clamp(measured.Height + 48, 108, 320);
        const int headerHeight = 62;
        const int footerHeight = 66;
        int totalHeight = headerHeight + bodyHeight + footerHeight;

        ClientSize = new Size(dialogWidth, totalHeight);
        Theme.ApplyRoundedRegion(this, 16);

        // 1. En-tête Royal Sapphire avec filet Or Métallique
        var header = new SmoothPanel
        {
            Dock = DockStyle.Top,
            Height = headerHeight,
            BackColor = Theme.SidebarNavy
        };
        header.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var navyBg = Theme.LoadNavyBackground();
            if (navyBg != null)
            {
                Theme.DrawCachedBackground(g, header, navyBg, Color.FromArgb(105, 5, 20, 46));
            }
            else
            {
                using var bg = new LinearGradientBrush(header.ClientRectangle, Theme.SidebarNavyLight, Theme.SidebarNavy, LinearGradientMode.Horizontal);
                g.FillRectangle(bg, header.ClientRectangle);
            }

            // Filet Or Métallique en bas de l'en-tête
            using var goldBar = new LinearGradientBrush(new Rectangle(0, header.Height - 3, header.Width, 3), Theme.GoldLight, Theme.GoldAccent, LinearGradientMode.Horizontal);
            g.FillRectangle(goldBar, 0, header.Height - 3, header.Width, 3);
        };

        // Déplacement à la souris par l'en-tête
        bool dragging = false;
        Point dragStart = Point.Empty;
        header.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragStart = e.Location; } };
        header.MouseMove += (_, e) => { if (dragging) Location = new Point(Location.X + e.X - dragStart.X, Location.Y + e.Y - dragStart.Y); };
        header.MouseUp += (_, _) => dragging = false;

        // Mini badge logo / marque dans l'en-tête
        var lblHeaderTitle = new Label
        {
            Text = cleanCaption,
            Font = Theme.UiFont(11.5F, FontStyle.Bold),
            ForeColor = Color.White,
            BackColor = Color.Transparent,
            Location = new Point(22, 11),
            Size = new Size(dialogWidth - 76, 22),
            UseMnemonic = false
        };
        var lblHeaderBadge = new Label
        {
            Text = _badgeLabel,
            Font = Theme.UiFont(7.6F, FontStyle.Bold),
            ForeColor = Theme.GoldLight,
            BackColor = Color.Transparent,
            Location = new Point(23, 35),
            AutoSize = true,
            UseMnemonic = false
        };
        lblHeaderTitle.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { dragging = true; dragStart = new Point(e.X + lblHeaderTitle.Left, e.Y + lblHeaderTitle.Top); } };
        lblHeaderTitle.MouseMove += (_, e) => { if (dragging) Location = new Point(Location.X + e.X + lblHeaderTitle.Left - dragStart.X, Location.Y + e.Y + lblHeaderTitle.Top - dragStart.Y); };
        lblHeaderTitle.MouseUp += (_, _) => dragging = false;

        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(32, 32),
            Location = new Point(dialogWidth - 46, 14),
            FlatStyle = FlatStyle.Flat,
            Font = Theme.UiFont(10.0F, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 218, 240),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand,
            TabStop = false
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(42, 255, 255, 255);
        btnClose.Click += (_, _) =>
        {
            DialogResult = buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.Cancel;
            Close();
        };
        Theme.ApplyRoundedRegion(btnClose, 8);

        header.Controls.Add(btnClose);
        header.Controls.Add(lblHeaderBadge);
        header.Controls.Add(lblHeaderTitle);

        // 2. Corps du message (icône vectorielle + texte clair et aéré)
        var body = new SmoothPanel
        {
            Dock = DockStyle.Fill,
            BackColor = Color.White
        };
        body.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            int iconSize = 52;
            int iconX = 24;
            int iconY = Math.Max(22, (body.Height - iconSize) / 2);
            DrawAlertIcon(g, new Rectangle(iconX, iconY, iconSize, iconSize));
        };

        bool needsScroll = measured.Height + 48 > 320;
        if (needsScroll)
        {
            var txtMsg = new TextBox
            {
                Text = message,
                Font = msgFont,
                ForeColor = Theme.TextPrimary,
                BackColor = Color.White,
                BorderStyle = BorderStyle.None,
                ReadOnly = true,
                Multiline = true,
                ScrollBars = ScrollBars.Vertical,
                Location = new Point(textLeft, 20),
                Size = new Size(textMaxWidth, bodyHeight - 40),
                TabStop = false
            };
            body.Controls.Add(txtMsg);
        }
        else
        {
            var lblMessage = new Label
            {
                Text = message,
                Font = msgFont,
                ForeColor = Theme.TextPrimary,
                BackColor = Color.Transparent,
                Location = new Point(textLeft, Math.Max(20, (bodyHeight - measured.Height) / 2 - 2)),
                Size = new Size(textMaxWidth, measured.Height + 10),
                UseMnemonic = false
            };
            body.Controls.Add(lblMessage);
        }

        // 3. Pied de page avec séparation douce et boutons modernes
        var footer = new SmoothPanel
        {
            Dock = DockStyle.Bottom,
            Height = footerHeight,
            BackColor = Theme.PageBg,
            Padding = new Padding(20, 14, 22, 14)
        };
        footer.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, 0, footer.Width, 0);
        };

        var btnFlow = new FlowLayoutPanel
        {
            Dock = DockStyle.Right,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0)
        };

        if (buttons == MessageBoxButtons.YesNo || buttons == MessageBoxButtons.OKCancel)
        {
            var confirmResult = buttons == MessageBoxButtons.YesNo ? DialogResult.Yes : DialogResult.OK;
            var cancelResult = buttons == MessageBoxButtons.YesNo ? DialogResult.No : DialogResult.Cancel;

            var btnConfirm = Theme.CreatePrimaryButton(
                primaryText ?? (buttons == MessageBoxButtons.YesNo ? "Oui, confirmer" : "Confirmer"),
                148, 38);
            if (_alertType == FinCoreAlertType.Error)
                btnConfirm.BackColor = Theme.DangerRed;
            btnConfirm.Margin = new Padding(10, 0, 0, 0);
            btnConfirm.Click += (_, _) => { DialogResult = confirmResult; Close(); };

            var btnCancel = Theme.CreateSecondaryButton(
                secondaryText ?? (buttons == MessageBoxButtons.YesNo ? "Annuler" : "Annuler"),
                118, 38);
            btnCancel.Margin = new Padding(0);
            btnCancel.Click += (_, _) => { DialogResult = cancelResult; Close(); };

            btnFlow.Controls.Add(btnConfirm);
            btnFlow.Controls.Add(btnCancel);

            AcceptButton = btnConfirm;
            CancelButton = btnCancel;
        }
        else
        {
            var btnOk = Theme.CreatePrimaryButton(primaryText ?? "OK", 124, 38);
            if (_alertType == FinCoreAlertType.Success)
                btnOk.BackColor = Theme.SuccessGreen;
            else if (_alertType == FinCoreAlertType.Error)
                btnOk.BackColor = Theme.DangerRed;
            btnOk.Margin = new Padding(0);
            btnOk.Click += (_, _) => { DialogResult = DialogResult.OK; Close(); };

            btnFlow.Controls.Add(btnOk);
            AcceptButton = btnOk;
            CancelButton = btnOk;
        }

        footer.Controls.Add(btnFlow);

        Controls.Add(body);
        Controls.Add(footer);
        Controls.Add(header);

        // Bordure extérieure nette anti-aliasée
        Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var path = Theme.CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 16);
            using var pen = new Pen(Color.FromArgb(185, 198, 218), 1.4f);
            e.Graphics.DrawPath(pen, path);
        };

        // Fondu d'ouverture fluide (60 FPS)
        Opacity = 0;
        var fadeTimer = new System.Windows.Forms.Timer { Interval = 16 };
        fadeTimer.Tick += (_, _) =>
        {
            if (IsDisposed) { fadeTimer.Stop(); fadeTimer.Dispose(); return; }
            if (Opacity < 1) Opacity = Math.Min(1, Opacity + 0.22);
            else { fadeTimer.Stop(); fadeTimer.Dispose(); }
        };
        Shown += (_, _) => fadeTimer.Start();
        FormClosed += (_, _) => { fadeTimer.Stop(); fadeTimer.Dispose(); };
    }

    private void DrawAlertIcon(Graphics g, Rectangle rect)
    {
        // Halo extérieur doux
        using (var haloBrush = new SolidBrush(_softBgColor))
            g.FillEllipse(haloBrush, rect);

        // Anneau subtil
        using (var ringPen = new Pen(Color.FromArgb(75, _accentColor), 1.6f))
            g.DrawEllipse(ringPen, rect.X + 1, rect.Y + 1, rect.Width - 2, rect.Height - 2);

        // Cercle intérieur plein
        var inner = Rectangle.Inflate(rect, -8, -8);
        using (var coreBrush = new SolidBrush(_accentColor))
            g.FillEllipse(coreBrush, inner);

        // Symbole vectoriel blanc ultra-net
        float cx = inner.X + inner.Width / 2f;
        float cy = inner.Y + inner.Height / 2f;
        using var whitePen = new Pen(Color.White, 2.8f)
        {
            StartCap = LineCap.Round,
            EndCap = LineCap.Round,
            LineJoin = LineJoin.Round
        };

        switch (_alertType)
        {
            case FinCoreAlertType.Success:
                g.DrawLines(whitePen, new[]
                {
                    new PointF(cx - 7.5f, cy + 0.5f),
                    new PointF(cx - 2.0f, cy + 6.0f),
                    new PointF(cx + 8.0f, cy - 5.5f)
                });
                break;

            case FinCoreAlertType.Error:
                g.DrawLine(whitePen, cx - 6f, cy - 6f, cx + 6f, cy + 6f);
                g.DrawLine(whitePen, cx + 6f, cy - 6f, cx - 6f, cy + 6f);
                break;

            case FinCoreAlertType.Warning:
                g.DrawLine(whitePen, cx, cy - 7.5f, cx, cy + 1.5f);
                using (var dotBrush = new SolidBrush(Color.White))
                    g.FillEllipse(dotBrush, cx - 1.8f, cy + 5.2f, 3.6f, 3.6f);
                break;

            case FinCoreAlertType.Question:
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                using (var qFont = Theme.UiFont(15.5F, FontStyle.Bold))
                    g.DrawString("?", qFont, Brushes.White, new RectangleF(inner.X, inner.Y + 1, inner.Width, inner.Height), sf);
                break;

            default: // Info
                using (var dotBrush = new SolidBrush(Color.White))
                    g.FillEllipse(dotBrush, cx - 1.8f, cy - 8.2f, 3.6f, 3.6f);
                g.DrawLine(whitePen, cx, cy - 2.0f, cx, cy + 7.2f);
                break;
        }
    }

    private static FinCoreAlertType ResolveAlertType(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
    {
        if (icon == MessageBoxIcon.Error || icon == MessageBoxIcon.Stop || icon == MessageBoxIcon.Hand)
            return FinCoreAlertType.Error;

        if (icon == MessageBoxIcon.Question)
            return FinCoreAlertType.Question;

        if (icon == MessageBoxIcon.Warning || icon == MessageBoxIcon.Exclamation)
            return buttons == MessageBoxButtons.YesNo ? FinCoreAlertType.Question : FinCoreAlertType.Warning;

        if (icon == MessageBoxIcon.Information || icon == MessageBoxIcon.Asterisk)
        {
            var combined = $"{caption} {message}".ToLowerInvariant();
            if (combined.Contains("succès") || combined.Contains("réussi") || combined.Contains("enregistré")
                || combined.Contains("exporté") || combined.Contains("généré") || combined.Contains("approuv")
                || combined.Contains("modifié") || combined.Contains("mis à jour") || combined.Contains("effectué")
                || combined.Contains("copié") || combined.Contains("importé") || combined.Contains("clôturé"))
            {
                return FinCoreAlertType.Success;
            }
        }

        return FinCoreAlertType.Info;
    }

    private static IWin32Window? GetActiveOwner()
    {
        var active = Form.ActiveForm;
        if (active != null && !active.IsDisposed && active.Visible)
            return active;

        foreach (Form f in Application.OpenForms)
        {
            if (!f.IsDisposed && f.Visible)
                return f;
        }
        return null;
    }

    /// <summary>
    /// Point d'entrée compatible drop-in avec MessageBox.Show(...).
    /// </summary>
    public static DialogResult Show(
        string text,
        string caption = "IIT FINCORE",
        MessageBoxButtons buttons = MessageBoxButtons.OK,
        MessageBoxIcon icon = MessageBoxIcon.Information)
    {
        var alertType = ResolveAlertType(text, caption, buttons, icon);
        using var dlg = new FrmMessageBox(text, caption, buttons, alertType);
        var owner = GetActiveOwner();
        if (owner == null)
        {
            dlg.StartPosition = FormStartPosition.CenterScreen;
            return dlg.ShowDialog();
        }
        return dlg.ShowDialog(owner);
    }

    public static DialogResult ShowSuccess(string text, string caption = "IIT FINCORE", string okText = "Parfait")
    {
        using var dlg = new FrmMessageBox(text, caption, MessageBoxButtons.OK, FinCoreAlertType.Success, primaryText: okText);
        var owner = GetActiveOwner();
        if (owner == null)
        {
            dlg.StartPosition = FormStartPosition.CenterScreen;
            return dlg.ShowDialog();
        }
        return dlg.ShowDialog(owner);
    }

    public static DialogResult ShowError(string text, string caption = "Erreur")
    {
        using var dlg = new FrmMessageBox(text, caption, MessageBoxButtons.OK, FinCoreAlertType.Error, primaryText: "Fermer");
        var owner = GetActiveOwner();
        if (owner == null)
        {
            dlg.StartPosition = FormStartPosition.CenterScreen;
            return dlg.ShowDialog();
        }
        return dlg.ShowDialog(owner);
    }

    public static DialogResult Confirm(string text, string caption = "Confirmation", string confirmText = "Oui, confirmer", string cancelText = "Annuler")
    {
        using var dlg = new FrmMessageBox(text, caption, MessageBoxButtons.YesNo, FinCoreAlertType.Question, confirmText, cancelText);
        var owner = GetActiveOwner();
        if (owner == null)
        {
            dlg.StartPosition = FormStartPosition.CenterScreen;
            return dlg.ShowDialog();
        }
        return dlg.ShowDialog(owner);
    }
}

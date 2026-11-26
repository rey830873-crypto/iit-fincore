using System.Diagnostics;
using System.Drawing;

namespace IIT.FinCore.WinForms.UI.Dialogs;

public class FrmDetailsDialog : Form
{
    public FrmDetailsDialog(string title, Dictionary<string, string> fields, string? attachmentPath = null, List<(string Col1, string Col2, string Col3)>? subTable = null, string? subTableTitle = null)
    {
        Text = $"IIT FINCORE — {title}";
        Size = new Size(660, subTable != null ? 610 : 470);
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        var header = Theme.CreateDialogHeader(title, "Consultation détaillée des informations enregistrées");
        Theme.ApplyModernDialogChrome(this, header);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.White, Padding = new Padding(18, 12, 22, 12) };
        footer.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Theme.BorderColor, 1), 0, 0, footer.Width, 0);

        var btnClose = Theme.CreatePrimaryButton("Fermer", 125, 38);
        btnClose.Dock = DockStyle.Right;
        btnClose.Click += (_, _) => Close();
        footer.Controls.Add(btnClose);

        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 14, 18, 14), BackColor = Theme.PageBg };
        var card = Theme.CreateCardPanel(new Padding(22, 16, 22, 16));
        card.Dock = DockStyle.Fill;
        card.AutoScroll = true;

        int y = 16;
        foreach (var (k, v) in fields)
        {
            card.Controls.Add(new Label { Text = k + " :", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(20, y), Size = new Size(190, 22) });
            card.Controls.Add(new Label { Text = string.IsNullOrWhiteSpace(v) ? "—" : v, Font = Theme.FontBody, ForeColor = Theme.TextPrimary, Location = new Point(216, y), Size = new Size(380, 22) });
            y += 27;
        }

        if (!string.IsNullOrWhiteSpace(attachmentPath))
        {
            y += 8;
            var btnOpen = Theme.CreateSecondaryButton("Ouvrir la pièce justificative", 225, 34);
            btnOpen.Location = new Point(20, y);
            btnOpen.Click += (_, _) =>
            {
                if (File.Exists(attachmentPath))
                    Process.Start(new ProcessStartInfo(attachmentPath) { UseShellExecute = true });
                else
                    MessageBox.Show($"Fichier introuvable :\n{attachmentPath}", "Justificatif", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            };
            card.Controls.Add(btnOpen);
            y += 44;
        }

        if (subTable != null)
        {
            y += 8;
            card.Controls.Add(new Label { Text = subTableTitle ?? "Ventilation analytique", Font = Theme.FontBodyBold, ForeColor = Theme.SidebarNavy, Location = new Point(20, y), AutoSize = true });
            y += 24;

            var gridWrap = new SmoothPanel { Location = new Point(20, y), Size = new Size(575, 126), BackColor = Color.White, Padding = new Padding(1) };
            gridWrap.Paint += (_, e) => e.Graphics.DrawRectangle(new Pen(Theme.BorderColor, 1), 0, 0, gridWrap.Width - 1, gridWrap.Height - 1);

            var grid = Theme.CreateStyledGrid();
            grid.Columns.Add("C1", "Centre analytique");
            grid.Columns.Add("C2", "Montant imputé");
            grid.Columns.Add("C3", "Pourcentage");
            foreach (var row in subTable)
                grid.Rows.Add(row.Col1, row.Col2, row.Col3);
            gridWrap.Controls.Add(grid);
            card.Controls.Add(gridWrap);
        }

        wrap.Controls.Add(card);
        Controls.Add(wrap);
        Controls.Add(footer);
        Controls.Add(header);
    }
}

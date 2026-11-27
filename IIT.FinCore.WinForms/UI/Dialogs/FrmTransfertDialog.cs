using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Dialogs;

public partial class FrmTransfertDialog : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly ComboBox _cboSource = new();
    private readonly ComboBox _cboDest = new();
    private readonly NumericUpDown _numMontant = new();
    private readonly DateTimePicker _dtpDate = new();
    private readonly TextBox _txtMotif = new();

    public FrmTransfertDialog()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        Text = "IIT FINCORE - Transfert entre comptes";
        Size = new Size(560, 470);
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        var header = Theme.CreateDialogHeader("Transfert entre comptes", "Déplacement sécurisé de fonds entre deux comptes de trésorerie");
        Theme.ApplyModernDialogChrome(this, header);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.White, Padding = new Padding(20, 12, 22, 12) };
        footer.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Theme.BorderColor, 1), 0, 0, footer.Width, 0);

        var btnSave = Theme.CreatePrimaryButton("Exécuter le transfert", 190, 38);
        btnSave.Dock = DockStyle.Right;
        btnSave.Click += (_, _) => DoTransfert();

        var spacer = new Panel { Dock = DockStyle.Right, Width = 10, BackColor = Color.White };

        var btnCancel = Theme.CreateSecondaryButton("Annuler", 110, 38);
        btnCancel.Dock = DockStyle.Right;
        btnCancel.Click += (_, _) => Close();

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(spacer);
        footer.Controls.Add(btnSave);

        var bodyWrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 14, 18, 14), BackColor = Theme.PageBg };
        var card = Theme.CreateCardPanel(new Padding(22));
        card.Dock = DockStyle.Fill;

        int y = 18;
        card.Controls.Add(new Label { Text = "Compte source (à débiter)", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _cboSource.SetBounds(22, y, 476, 30);
        _cboSource.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboSource.Font = Theme.FontBody;
        y += 44;

        card.Controls.Add(new Label { Text = "Compte destinataire (à créditer)", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _cboDest.SetBounds(22, y, 476, 30);
        _cboDest.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboDest.Font = Theme.FontBody;
        y += 44;

        card.Controls.Add(new Label { Text = "Montant (FCFA)", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        card.Controls.Add(new Label { Text = "Date du transfert", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(272, y), AutoSize = true });
        y += 22;

        _numMontant.SetBounds(22, y, 228, 28);
        _numMontant.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _numMontant.Maximum = 1_000_000_000;
        _numMontant.ThousandsSeparator = true;
        _numMontant.Value = 100000;

        _dtpDate.SetBounds(272, y, 226, 28);
        _dtpDate.Format = DateTimePickerFormat.Short;
        _dtpDate.Font = Theme.FontBody;
        y += 44;

        card.Controls.Add(new Label { Text = "Motif du transfert", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _txtMotif.SetBounds(22, y, 476, 28);
        _txtMotif.Font = Theme.FontBody;
        _txtMotif.Text = "Réallocation de fonds";

        card.Controls.AddRange(new Control[] { _cboSource, _cboDest, _numMontant, _dtpDate, _txtMotif });
        bodyWrap.Controls.Add(card);
        Controls.Add(bodyWrap);
        Controls.Add(footer);
        Controls.Add(header);

        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        var comptes = _repo.GetComptes(scope);
        _cboSource.DataSource = comptes.ToList();
        _cboDest.DataSource = comptes.ToList();
        if (comptes.Count > 1) _cboDest.SelectedIndex = 1;
    }

    private void DoTransfert()
    {
        if (_cboSource.SelectedItem is not CompteTresorerie src || _cboDest.SelectedItem is not CompteTresorerie dst)
            return;

        try
        {
            _repo.ExecuteTransfert(src.Id, dst.Id, _numMontant.Value, _dtpDate.Value, _txtMotif.Text.Trim(), UserSession.CurrentUser!.Id);
            MessageBox.Show("Transfert effectué avec succès ! Les deux soldes ont été mis à jour.", "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur de transfert", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

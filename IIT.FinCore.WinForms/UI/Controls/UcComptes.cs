using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public class UcComptes : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly TextBox _txtSearch = new();
    private readonly TextBox _txtNom = new();
    private readonly ComboBox _cboStructure = new();
    private readonly ComboBox _cboType = new();
    private readonly TextBox _txtEtab = new();
    private readonly TextBox _txtRef = new();
    private readonly NumericUpDown _numInitial = new();
    private readonly CheckBox _chkActif = new() { Text = "Actif", Checked = true, AutoSize = true, Margin = new Padding(4, 24, 12, 0) };
    private List<CompteTresorerie> _allItems = new();
    private List<CompteTresorerie> _filtered = new();
    private int? _editingId;

    public UcComptes()
    {
        BackColor = Theme.PageBg;
        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        if (UserSession.HasPermission("COMPTE_MANAGE"))
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Créer ou modifier un compte de trésorerie", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };

            _cboStructure.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboType.Items.AddRange(new object[] { "BANQUE", "CAISSE", "MOBILE_MONEY", "AUTRE" });
            _cboType.SelectedIndex = 0;
            _numInitial.Maximum = 1_000_000_000;
            _numInitial.ThousandsSeparator = true;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Nom du compte", _txtNom, 175));
            flow.Controls.Add(Theme.CreateFieldGroup("Structure", _cboStructure, 175));
            flow.Controls.Add(Theme.CreateFieldGroup("Type", _cboType, 125));
            flow.Controls.Add(Theme.CreateFieldGroup("Établissement", _txtEtab, 145));
            flow.Controls.Add(Theme.CreateFieldGroup("Référence / N°", _txtRef, 135));
            flow.Controls.Add(Theme.CreateFieldGroup("Solde initial", _numInitial, 135));
            flow.Controls.Add(_chkActif);

            var btnSave = Theme.CreatePrimaryButton("Enregistrer", 115, 34);
            btnSave.Margin = new Padding(4, 18, 8, 4);
            btnSave.Click += (_, _) => SaveCompte();

            var btnNew = Theme.CreateSecondaryButton("Nouveau", 90, 34);
            btnNew.Margin = new Padding(0, 18, 8, 4);
            btnNew.Click += (_, _) => ResetForm();

            var btnDel = Theme.CreatePrimaryButton("Supprimer", 108, 34);
            btnDel.BackColor = Theme.DangerRed;
            btnDel.Margin = new Padding(0, 18, 0, 4);
            btnDel.Click += (_, _) => DeleteSelectedCompte();

            flow.Controls.Add(btnSave);
            flow.Controls.Add(btnNew);
            flow.Controls.Add(btnDel);

            formCard.Controls.Add(flow);
            formCard.Controls.Add(lbl);

            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            Controls.Add(formCard);

            var structs = _repo.GetStructures();
            if (UserSession.IsTresorier && UserSession.CurrentUser?.StructureId != null)
                structs = structs.Where(s => s.Id == UserSession.CurrentUser.StructureId).ToList();
            _cboStructure.DataSource = structs;
            _cboStructure.DisplayMember = "Nom";
            _cboStructure.ValueMember = "Id";
        }

        var listCard = Theme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
        _txtSearch.SetBounds(0, 4, 280, 28);
        _txtSearch.PlaceholderText = "Rechercher un compte...";
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        var lblHint = new Label { Text = "Clic : éditer  •  Double-clic : fiche détaillée du compte", Font = Theme.FontSubtitle, ForeColor = Theme.TextMuted, AutoSize = true, Location = new Point(296, 8) };
        topBar.Controls.AddRange(new Control[] { _txtSearch, lblHint });

        _grid.CellClick += (_, _) => LoadSelectedToForm();
        _grid.CellDoubleClick += (_, _) => ShowSelectedDetails();
        listCard.Controls.Add(_grid);
        listCard.Controls.Add(topBar);
        Controls.Add(listCard);
        listCard.BringToFront();
    }

    private void LoadData()
    {
        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        _allItems = _repo.GetComptes(scope);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _txtSearch.Text.Trim();
        _filtered = string.IsNullOrEmpty(q)
            ? _allItems
            : _allItems.Where(c => c.NomCompte.Contains(q, StringComparison.OrdinalIgnoreCase) || c.StructureNom.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        _grid.DataSource = _filtered.Select(c => new
        {
            Compte = c.NomCompte,
            Structure = c.StructureNom,
            Type = c.TypeCompte,
            Établissement = c.Etablissement,
            Référence = c.NumeroReference,
            Solde_Initial = $"{c.SoldeInitial:N0} {c.Devise}",
            Solde_Actuel = $"{c.SoldeActuel:N0} {c.Devise}",
            Statut = c.EstActif ? "Actif" : "Archivé"
        }).ToList();
    }

    private void LoadSelectedToForm()
    {
        if (!UserSession.HasPermission("COMPTE_MANAGE") || _grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var c = _filtered[idx];
        _editingId = c.Id;
        _txtNom.Text = c.NomCompte;
        _cboStructure.SelectedValue = c.StructureId; _cboStructure.Enabled = false;
        _cboType.SelectedItem = c.TypeCompte;
        _txtEtab.Text = c.Etablissement ?? "";
        _txtRef.Text = c.NumeroReference ?? "";
        _numInitial.Value = Math.Max(0, c.SoldeInitial); _numInitial.Enabled = false;
        _chkActif.Checked = c.EstActif;
    }

    private void ResetForm()
    {
        _editingId = null;
        _cboStructure.Enabled = true;
        _numInitial.Enabled = true;
        _txtNom.Clear(); _txtEtab.Clear(); _txtRef.Clear(); _numInitial.Value = 0;
        _chkActif.Checked = true;
    }

    private void DeleteSelectedCompte()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var c = _filtered[idx];
        if (MessageBox.Show($"Supprimer ou archiver le compte « {c.NomCompte} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var msg = _repo.DeleteOrArchiveCompte(c.Id, UserSession.CurrentUser!.Id);
        ResetForm();
        LoadData();
        MessageBox.Show(msg, "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSelectedDetails()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var c = _filtered[idx];
        using var dlg = new FrmDetailsDialog($"Compte - {c.NomCompte}", new Dictionary<string, string>
        {
            ["Nom du compte"] = c.NomCompte,
            ["Structure"] = c.StructureNom,
            ["Type de compte"] = c.TypeCompte,
            ["Établissement"] = c.Etablissement ?? "-",
            ["N° de référence"] = c.NumeroReference ?? "-",
            ["Solde initial"] = $"{c.SoldeInitial:N0} {c.Devise}",
            ["Solde actuel"] = $"{c.SoldeActuel:N0} {c.Devise}",
            ["Statut"] = c.EstActif ? "Actif" : "Archivé"
        });
        dlg.ShowDialog(this);
    }

    private void SaveCompte()
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text) || _cboStructure.SelectedValue is not int sid)
        {
            MessageBox.Show("Veuillez renseigner le nom du compte et la structure.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var c = new CompteTresorerie
            {
                Id = _editingId ?? 0,
                NomCompte = _txtNom.Text.Trim(),
                StructureId = sid,
                TypeCompte = _cboType.SelectedItem?.ToString() ?? "CAISSE",
                Etablissement = _txtEtab.Text.Trim(),
                NumeroReference = _txtRef.Text.Trim(),
                SoldeInitial = _numInitial.Value,
                Devise = "FCFA",
                EstActif = _chkActif.Checked
            };

            if (_editingId.HasValue)
                _repo.UpdateCompte(c, UserSession.CurrentUser!.Id);
            else
                _repo.CreateCompte(c, UserSession.CurrentUser!.Id);

            ResetForm();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

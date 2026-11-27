using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class FrmStructures : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly TextBox _txtSearch = new();
    private readonly TextBox _txtCode = new();
    private readonly TextBox _txtNom = new();
    private readonly ComboBox _cboType = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtTel = new();
    private readonly CheckBox _chkActif = new() { Text = "Active", Checked = true, AutoSize = true, Margin = new Padding(4, 24, 12, 0) };
    private List<Structure> _allItems = new();
    private List<Structure> _filtered = new();
    private int? _editingId;

    public FrmStructures()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;
        Controls.Clear();
        BackColor = Theme.PageBg;
        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        if (UserSession.IsAdmin)
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Créer ou modifier une structure", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };

            _cboType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboType.Items.AddRange(new object[] { "ASSOCIATION", "CLUB", "DEPARTEMENT", "INSTITUTION", "AUTRE" });
            _cboType.SelectedIndex = 0;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Code", _txtCode, 105));
            flow.Controls.Add(Theme.CreateFieldGroup("Nom de la structure", _txtNom, 210));
            flow.Controls.Add(Theme.CreateFieldGroup("Type", _cboType, 135));
            flow.Controls.Add(Theme.CreateFieldGroup("E-mail de contact", _txtEmail, 175));
            flow.Controls.Add(Theme.CreateFieldGroup("Téléphone", _txtTel, 135));
            flow.Controls.Add(_chkActif);

            var btnSave = Theme.CreatePrimaryButton("Enregistrer", 118, 34);
            btnSave.Margin = new Padding(4, 18, 8, 4);
            btnSave.Click += (_, _) => SaveStructure();

            var btnNew = Theme.CreateSecondaryButton("Nouveau", 92, 34);
            btnNew.Margin = new Padding(0, 18, 8, 4);
            btnNew.Click += (_, _) => ResetForm();

            var btnDel = Theme.CreatePrimaryButton("Supprimer", 108, 34);
            btnDel.BackColor = Theme.DangerRed;
            btnDel.Margin = new Padding(0, 18, 0, 4);
            btnDel.Click += (_, _) => DeleteSelectedStructure();

            flow.Controls.Add(btnSave);
            flow.Controls.Add(btnNew);
            flow.Controls.Add(btnDel);

            formCard.Controls.Add(flow);
            formCard.Controls.Add(lbl);

            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            Controls.Add(formCard);
        }

        var listCard = Theme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;

        var topBar = new Panel { Dock = DockStyle.Top, Height = 40, BackColor = Color.Transparent };
        _txtSearch.SetBounds(0, 4, 280, 28);
        _txtSearch.PlaceholderText = "Rechercher une structure...";
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        var lblHint = new Label { Text = "Clic : éditer  •  Double-clic : fiche détaillée", Font = Theme.FontSubtitle, ForeColor = Theme.TextMuted, AutoSize = true, Location = new Point(296, 8) };
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
        _allItems = _repo.GetStructures();
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _txtSearch.Text.Trim();
        _filtered = string.IsNullOrEmpty(q)
            ? _allItems
            : _allItems.Where(s => s.Nom.Contains(q, StringComparison.OrdinalIgnoreCase) || s.Code.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        _grid.DataSource = _filtered.Select(s => new
        {
            s.Code,
            Structure = s.Nom,
            Type = s.TypeStructure,
            Parente = s.ParentNom ?? "-",
            Email = s.EmailContact,
            Téléphone = s.Telephone,
            Statut = s.EstActif ? "Active" : "Inactive"
        }).ToList();
    }

    private void LoadSelectedToForm()
    {
        if (!UserSession.IsAdmin || _grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var s = _filtered[idx];
        _editingId = s.Id;
        _txtCode.Text = s.Code; _txtCode.ReadOnly = true;
        _txtNom.Text = s.Nom;
        _cboType.SelectedItem = s.TypeStructure;
        _txtEmail.Text = s.EmailContact ?? "";
        _txtTel.Text = s.Telephone ?? "";
        _chkActif.Checked = s.EstActif;
    }

    private void ResetForm()
    {
        _editingId = null;
        _txtCode.ReadOnly = false;
        _txtCode.Clear(); _txtNom.Clear(); _txtEmail.Clear(); _txtTel.Clear();
        _chkActif.Checked = true;
    }

    private void DeleteSelectedStructure()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var s = _filtered[idx];
        if (MessageBox.Show($"Supprimer ou archiver la structure « {s.Nom} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var msg = _repo.DeleteOrArchiveStructure(s.Id, UserSession.CurrentUser!.Id);
        ResetForm();
        LoadData();
        MessageBox.Show(msg, "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void ShowSelectedDetails()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var s = _filtered[idx];
        var kpi = _repo.GetDashboardKpis(s.Id);
        using var dlg = new FrmDetailsDialog($"Structure - {s.Nom}", new Dictionary<string, string>
        {
            ["Code"] = s.Code,
            ["Nom"] = s.Nom,
            ["Type"] = s.TypeStructure,
            ["Structure parente"] = s.ParentNom ?? "Racine",
            ["E-mail"] = s.EmailContact ?? "-",
            ["Téléphone"] = s.Telephone ?? "-",
            ["Statut"] = s.EstActif ? "Active" : "Inactive",
            ["Solde cumulé"] = $"{kpi.SoldeGlobal:N0} FCFA",
            ["Recettes validées"] = $"{kpi.TotalRecettes:N0} FCFA",
            ["Dépenses validées"] = $"{kpi.TotalDepenses:N0} FCFA"
        });
        dlg.ShowDialog(TopLevelControl as IWin32Window ?? this);
    }

    private void SaveStructure()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtNom.Text))
        {
            MessageBox.Show("Le code et le nom sont obligatoires.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var s = new Structure
            {
                Id = _editingId ?? 0,
                Code = _txtCode.Text.Trim().ToUpperInvariant(),
                Nom = _txtNom.Text.Trim(),
                TypeStructure = _cboType.SelectedItem?.ToString() ?? "ASSOCIATION",
                ParentId = 1,
                EmailContact = _txtEmail.Text.Trim(),
                Telephone = _txtTel.Text.Trim(),
                EstActif = _chkActif.Checked
            };

            if (_editingId.HasValue)
                _repo.UpdateStructure(s, UserSession.CurrentUser!.Id);
            else
                _repo.CreateStructure(s, UserSession.CurrentUser!.Id);

            ResetForm();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

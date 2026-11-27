using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class FrmBudgets : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly TextBox _txtIntitule = new();
    private readonly ComboBox _cboStructure = new();
    private readonly ComboBox _cboCentre = new();
    private readonly NumericUpDown _numPrevu = new();
    private readonly DateTimePicker _dtpDebut = new();
    private readonly DateTimePicker _dtpFin = new();
    private readonly Label _lblAlertBanner = new();
    private List<Models.SuiviBudgetaireRow> _budgetsList = new();
    private int? _editingId;

    public FrmBudgets()
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
        if (UserSession.HasPermission("BUDGET_MANAGE"))
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Créer une enveloppe budgétaire", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };

            _cboStructure.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboCentre.DropDownStyle = ComboBoxStyle.DropDownList;
            _numPrevu.Maximum = 1_000_000_000; _numPrevu.ThousandsSeparator = true; _numPrevu.Value = 1000000;
            _dtpDebut.Format = DateTimePickerFormat.Short; _dtpDebut.Value = new DateTime(DateTime.Now.Year, 1, 1);
            _dtpFin.Format = DateTimePickerFormat.Short; _dtpFin.Value = new DateTime(DateTime.Now.Year, 12, 31);

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Intitulé du budget", _txtIntitule, 185));
            flow.Controls.Add(Theme.CreateFieldGroup("Structure", _cboStructure, 165));
            flow.Controls.Add(Theme.CreateFieldGroup("Centre analytique", _cboCentre, 160));
            flow.Controls.Add(Theme.CreateFieldGroup("Montant prévu", _numPrevu, 140));
            flow.Controls.Add(Theme.CreateFieldGroup("Date de début", _dtpDebut, 115));
            flow.Controls.Add(Theme.CreateFieldGroup("Date de fin", _dtpFin, 115));

            var btnAdd = Theme.CreatePrimaryButton("Enregistrer", 115, 34);
            btnAdd.Margin = new Padding(4, 18, 8, 4);
            btnAdd.Click += (_, _) => SaveBudget();

            var btnNew = Theme.CreateSecondaryButton("Nouveau", 88, 34);
            btnNew.Margin = new Padding(0, 18, 8, 4);
            btnNew.Click += (_, _) => ResetBudgetForm();

            var btnDel = Theme.CreatePrimaryButton("Supprimer", 105, 34);
            btnDel.BackColor = Theme.DangerRed;
            btnDel.Margin = new Padding(0, 18, 0, 4);
            btnDel.Click += (_, _) => DeleteSelectedBudget();

            flow.Controls.Add(btnAdd);
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

            _cboCentre.DataSource = _repo.GetCentresAnalytiques();
            _cboCentre.DisplayMember = "Libelle";
            _cboCentre.ValueMember = "Id";
        }

        var listCard = Theme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;

        _lblAlertBanner.Dock = DockStyle.Top;
        _lblAlertBanner.Height = 32;
        _lblAlertBanner.Font = Theme.FontBodyBold;
        _lblAlertBanner.Visible = false;

        // Attaché une seule fois (évite les doublons lors des rechargements)
        _grid.CellFormatting += (_, e) =>
        {
            if (e.ColumnIndex < 0 || e.ColumnIndex >= _grid.Columns.Count) return;
            if (_grid.Columns[e.ColumnIndex].Name != "Alerte" || e.Value == null) return;
            var raw = e.Value.ToString();
            if (raw == "IMPAYE") { e.Value = "DÉPASSÉ !"; e.CellStyle!.ForeColor = Theme.DangerRed; e.CellStyle.Font = Theme.FontBodyBold; }
            else if (raw == "EN_ATTENTE") { e.Value = "SEUIL ≥ 80%"; e.CellStyle!.ForeColor = Theme.GoldHover; e.CellStyle.Font = Theme.FontBodyBold; }
            else if (raw == "Actif") { e.Value = "Maîtrisé"; e.CellStyle!.ForeColor = Theme.SuccessGreen; e.CellStyle.Font = Theme.FontBodyBold; }
        };

        listCard.Controls.Add(_grid);
        _grid.CellClick += (_, _) => LoadBudgetToForm();
        listCard.Controls.Add(_lblAlertBanner);
        Controls.Add(listCard);
        listCard.BringToFront();
    }

    private void LoadData()
    {
        var data = _repo.GetSuiviBudgetaire();
        if (UserSession.IsTresorier && !string.IsNullOrEmpty(UserSession.CurrentUser?.StructureNom))
            data = data.Where(b => b.StructureNom == UserSession.CurrentUser.StructureNom).ToList();

        int nbDepasses = data.Count(b => b.TauxExecutionPct >= 100m);
        int nbAlertes = data.Count(b => b.TauxExecutionPct >= 80m && b.TauxExecutionPct < 100m);

        if (nbDepasses > 0 || nbAlertes > 0)
        {
            _lblAlertBanner.Visible = true;
            _lblAlertBanner.ForeColor = nbDepasses > 0 ? Theme.DangerRed : Theme.GoldHover;
            _lblAlertBanner.Text = nbDepasses > 0
                ? $"ALERTE BUDGÉTAIRE : {nbDepasses} budget(s) dépassé(s) (≥ 100 %) et {nbAlertes} proche(s) du seuil (≥ 80 %)."
                : $"VIGILANCE BUDGÉTAIRE : {nbAlertes} budget(s) ont atteint ou dépassé 80 % de consommation.";
        }
        else
        {
            _lblAlertBanner.Visible = false;
        }

        _budgetsList = data;
        _grid.DataSource = _budgetsList.Select(b => new
        {
            Intitulé = b.Intitule,
            Structure = b.StructureNom,
            Centre = b.CentreAnalytique,
            Période = $"{b.DateDebut:dd/MM/yyyy} → {b.DateFin:dd/MM/yyyy}",
            Prévu = $"{b.MontantPrevu:N0} FCFA",
            Réalisé = $"{b.MontantRealise:N0} FCFA",
            Reste = $"{b.ResteAConsommer:N0} FCFA",
            Exécution = $"{b.TauxExecutionPct:N1} %",
            Alerte = b.TauxExecutionPct >= 100m ? "IMPAYE" : (b.TauxExecutionPct >= 80m ? "EN_ATTENTE" : "Actif"),
            Statut = b.Statut
        }).ToList();
    }

    private void LoadBudgetToForm()
    {
        if (!UserSession.HasPermission("BUDGET_MANAGE") || _grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _budgetsList.Count) return;

        var b = _budgetsList[idx];
        _editingId = b.BudgetId;
        _txtIntitule.Text = b.Intitule;
        _numPrevu.Value = Math.Max(0, b.MontantPrevu);
        _dtpDebut.Value = b.DateDebut;
        _dtpFin.Value = b.DateFin;
        _cboStructure.Enabled = false;
        _cboCentre.Enabled = false;
    }

    private void ResetBudgetForm()
    {
        _editingId = null;
        _cboStructure.Enabled = true;
        _cboCentre.Enabled = true;
        _txtIntitule.Clear();
        _numPrevu.Value = 1000000;
    }

    private void SaveBudget()
    {
        if (string.IsNullOrWhiteSpace(_txtIntitule.Text)) return;
        try
        {
            if (_editingId.HasValue)
            {
                _repo.UpdateBudget(_editingId.Value, _txtIntitule.Text.Trim(), _numPrevu.Value, _dtpDebut.Value, _dtpFin.Value, UserSession.CurrentUser!.Id);
            }
            else if (_cboStructure.SelectedValue is int sid && _cboCentre.SelectedValue is int cid)
            {
                _repo.CreateBudget(_txtIntitule.Text.Trim(), sid, cid, _numPrevu.Value, _dtpDebut.Value, _dtpFin.Value, UserSession.CurrentUser!.Id);
            }
            ResetBudgetForm();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedBudget()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _budgetsList.Count) return;

        var b = _budgetsList[idx];
        if (MessageBox.Show($"Supprimer le budget « {b.Intitule} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        _repo.DeleteBudget(b.BudgetId, UserSession.CurrentUser!.Id);
        ResetBudgetForm();
        LoadData();
    }
}

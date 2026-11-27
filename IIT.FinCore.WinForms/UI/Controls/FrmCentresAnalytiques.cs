using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class FrmCentresAnalytiques : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _gridCentres = Theme.CreateStyledGrid();
    private readonly DataGridView _gridSynthese = Theme.CreateStyledGrid();
    private readonly TextBox _txtCode = new();
    private readonly TextBox _txtLibelle = new();
    private readonly TextBox _txtDesc = new();
    private List<CentreAnalytique> _centresList = new();
    private int? _editingId;

    public FrmCentresAnalytiques()
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
        if (UserSession.HasPermission("CENTRE_MANAGE"))
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Créer un centre d'imputation analytique", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Code (ex: CA-PROJ)", _txtCode, 145));
            flow.Controls.Add(Theme.CreateFieldGroup("Libellé du centre", _txtLibelle, 230));
            flow.Controls.Add(Theme.CreateFieldGroup("Description", _txtDesc, 300));

            var btnAdd = Theme.CreatePrimaryButton("Enregistrer", 118, 34);
            btnAdd.Margin = new Padding(4, 18, 8, 4);
            btnAdd.Click += (_, _) => SaveCentre();

            var btnNew = Theme.CreateSecondaryButton("Nouveau", 90, 34);
            btnNew.Margin = new Padding(0, 18, 8, 4);
            btnNew.Click += (_, _) => ResetCentreForm();

            var btnDel = Theme.CreatePrimaryButton("Supprimer", 105, 34);
            btnDel.BackColor = Theme.DangerRed;
            btnDel.Margin = new Padding(0, 18, 0, 4);
            btnDel.Click += (_, _) => DeleteSelectedCentre();

            flow.Controls.Add(btnAdd);
            flow.Controls.Add(btnNew);
            flow.Controls.Add(btnDel);

            formCard.Controls.Add(flow);
            formCard.Controls.Add(lbl);

            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            Controls.Add(formCard);
        }

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 240,
            BackColor = Theme.PageBg
        };

        var cardTop = Theme.CreateCardPanel();
        cardTop.Dock = DockStyle.Fill;
        var lblTop = new Label { Text = "Synthèse analytique (Recettes − Dépenses par centre)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
        cardTop.Controls.Add(_gridSynthese);
        cardTop.Controls.Add(lblTop);

        var cardBottom = Theme.CreateCardPanel();
        cardBottom.Dock = DockStyle.Fill;
        var lblBot = new Label { Text = "Référentiel des centres analytiques (clic sur une ligne pour modifier)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
        _gridCentres.CellClick += (_, _) => LoadCentreToForm();
        cardBottom.Controls.Add(_gridCentres);
        cardBottom.Controls.Add(lblBot);

        split.Panel1.Controls.Add(cardTop);
        split.Panel2.Controls.Add(cardBottom);

        Controls.Add(split);
        split.BringToFront();
    }

    private void LoadData()
    {
        var synthese = _repo.GetSyntheseAnalytique();
        if (UserSession.IsTresorier && !string.IsNullOrEmpty(UserSession.CurrentUser?.StructureNom))
            synthese = synthese.Where(s => s.StructureNom == UserSession.CurrentUser.StructureNom).ToList();

        _gridSynthese.DataSource = synthese.Select(s => new
        {
            Code = s.CentreCode,
            Centre = s.CentreLibelle,
            Structure = s.StructureNom,
            Recettes = $"+{s.TotalRecettes:N0} FCFA",
            Dépenses = $"-{s.TotalDepenses:N0} FCFA",
            Résultat_Net = (s.ResultatNet >= 0 ? "+" : "") + $"{s.ResultatNet:N0} FCFA"
        }).ToList();

        _centresList = _repo.GetCentresAnalytiques();
        _gridCentres.DataSource = _centresList.Select(c => new
        {
            c.Code,
            Libellé = c.Libelle,
            Description = c.Description,
            Périmètre = c.StructureNom ?? "Transversal",
            Statut = c.EstActif ? "Actif" : "Inactif"
        }).ToList();
    }

    private void LoadCentreToForm()
    {
        if (_gridCentres.CurrentRow == null) return;
        int idx = _gridCentres.CurrentRow.Index;
        if (idx < 0 || idx >= _centresList.Count) return;

        var c = _centresList[idx];
        _editingId = c.Id;
        _txtCode.Text = c.Code; _txtCode.ReadOnly = true;
        _txtLibelle.Text = c.Libelle;
        _txtDesc.Text = c.Description ?? "";
    }

    private void ResetCentreForm()
    {
        _editingId = null;
        _txtCode.ReadOnly = false;
        _txtCode.Clear(); _txtLibelle.Clear(); _txtDesc.Clear();
    }

    private void SaveCentre()
    {
        if (string.IsNullOrWhiteSpace(_txtCode.Text) || string.IsNullOrWhiteSpace(_txtLibelle.Text))
        {
            MessageBox.Show("Le code et le libellé sont requis.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            var ca = new CentreAnalytique
            {
                Id = _editingId ?? 0,
                Code = _txtCode.Text.Trim().ToUpperInvariant(),
                Libelle = _txtLibelle.Text.Trim(),
                Description = _txtDesc.Text.Trim(),
                EstActif = true
            };

            if (_editingId.HasValue)
                _repo.UpdateCentreAnalytique(ca, UserSession.CurrentUser!.Id);
            else
                _repo.CreateCentreAnalytique(ca, UserSession.CurrentUser!.Id);

            ResetCentreForm();
            LoadData();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedCentre()
    {
        if (_gridCentres.CurrentRow == null) return;
        int idx = _gridCentres.CurrentRow.Index;
        if (idx < 0 || idx >= _centresList.Count) return;

        var c = _centresList[idx];
        if (MessageBox.Show($"Supprimer ou désactiver le centre « {c.Libelle} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var msg = _repo.DeleteOrArchiveCentre(c.Id, UserSession.CurrentUser!.Id);
        ResetCentreForm();
        LoadData();
        MessageBox.Show(msg, "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

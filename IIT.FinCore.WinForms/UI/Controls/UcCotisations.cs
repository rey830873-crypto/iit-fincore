using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class UcCotisations : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _gridCampagnes = Theme.CreateStyledGrid();
    private readonly DataGridView _gridPaiements = Theme.CreateStyledGrid();
    private readonly DataGridView _gridImpayes = Theme.CreateStyledGrid();

    private readonly TextBox _txtNom = new();
    private readonly ComboBox _cboStructure = new();
    private readonly NumericUpDown _numMontant = new();
    private readonly DateTimePicker _dtpLimite = new();
    private readonly TextBox _txtPop = new();

    private readonly TextBox _txtMembreNom = new();
    private readonly TextBox _txtMembreTel = new();
    private readonly ComboBox _cboMembreStruct = new();
    private readonly ComboBox _cboPayCotis = new();
    private readonly ComboBox _cboPayMembre = new();
    private readonly ComboBox _cboPayCompte = new();
    private readonly NumericUpDown _numPayMontant = new();
    private List<PaiementCotisationItem> _impayesList = new();
    private List<CotisationItem> _campagnesList = new();

    public UcCotisations()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;
        Controls.Clear();
        BackColor = Theme.PageBg;
        BuildLayout();
        LoadAll();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBodyBold };

        // ==================== ONGLET 1 : CAMPAGNES ====================
        var tabCamp = new TabPage("Campagnes de cotisations") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        if (UserSession.HasPermission("COTISATION_MANAGE"))
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Créer une campagne de cotisation", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            _cboStructure.DropDownStyle = ComboBoxStyle.DropDownList;
            _numMontant.Maximum = 10_000_000; _numMontant.ThousandsSeparator = true; _numMontant.Value = 10000;
            _dtpLimite.Format = DateTimePickerFormat.Short;
            _txtPop.Text = "Tous les membres";

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Nom de la campagne", _txtNom, 185));
            flow.Controls.Add(Theme.CreateFieldGroup("Structure", _cboStructure, 165));
            flow.Controls.Add(Theme.CreateFieldGroup("Montant / membre", _numMontant, 130));
            flow.Controls.Add(Theme.CreateFieldGroup("Date limite", _dtpLimite, 115));
            flow.Controls.Add(Theme.CreateFieldGroup("Population concernée", _txtPop, 175));

            var btnAdd = Theme.CreatePrimaryButton("+ Créer", 110, 34);
            btnAdd.Margin = new Padding(4, 18, 8, 4);
            btnAdd.Click += (_, _) => AddCotisation();
            flow.Controls.Add(btnAdd);

            var btnDelCamp = Theme.CreatePrimaryButton("Supprimer", 105, 34);
            btnDelCamp.BackColor = Theme.DangerRed;
            btnDelCamp.Margin = new Padding(0, 18, 0, 4);
            btnDelCamp.Click += (_, _) => DeleteSelectedCotisation();
            flow.Controls.Add(btnDelCamp);

            formCard.Controls.Add(flow);
            formCard.Controls.Add(lbl);

            tabCamp.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
            tabCamp.Controls.Add(formCard);
        }

        var listCamp = Theme.CreateCardPanel();
        listCamp.Dock = DockStyle.Fill;
        listCamp.Controls.Add(_gridCampagnes);
        tabCamp.Controls.Add(listCamp);
        listCamp.BringToFront();

        // ==================== ONGLET 2 : PAIEMENTS & MEMBRES ====================
        var tabPay = new TabPage("Membres & Encaissements") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        if (UserSession.HasPermission("COTISATION_MANAGE"))
        {
            var topPanel = Theme.CreateCardPanel(new Padding(16, 10, 16, 10));
            topPanel.Dock = DockStyle.Top;
            topPanel.AutoSize = true;

            _txtMembreNom.PlaceholderText = "Nom complet";
            _txtMembreTel.PlaceholderText = "+225...";
            _cboMembreStruct.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPayCotis.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPayMembre.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboPayCompte.DropDownStyle = ComboBoxStyle.DropDownList;
            _numPayMontant.Maximum = 10_000_000; _numPayMontant.ThousandsSeparator = true; _numPayMontant.Value = 10000;

            var flowMem = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };
            flowMem.Controls.Add(Theme.CreateFieldGroup("Nouveau membre", _txtMembreNom, 190));
            flowMem.Controls.Add(Theme.CreateFieldGroup("Téléphone", _txtMembreTel, 130));
            flowMem.Controls.Add(Theme.CreateFieldGroup("Structure", _cboMembreStruct, 175));
            var btnAddMem = Theme.CreateSecondaryButton("+ Ajouter membre", 145, 34);
            btnAddMem.Margin = new Padding(4, 20, 8, 4);
            btnAddMem.Click += (_, _) => AddMembre();
            flowMem.Controls.Add(btnAddMem);

            var btnImportCsv = Theme.CreatePrimaryButton("Importer CSV Membres", 175, 34);
            btnImportCsv.Margin = new Padding(0, 20, 0, 4);
            btnImportCsv.Click += (_, _) => ImportMembresCsv();
            flowMem.Controls.Add(btnImportCsv);

            var flowPay = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 4, 0, 0) };
            flowPay.Controls.Add(Theme.CreateFieldGroup("Campagne", _cboPayCotis, 175));
            flowPay.Controls.Add(Theme.CreateFieldGroup("Membre", _cboPayMembre, 185));
            flowPay.Controls.Add(Theme.CreateFieldGroup("Compte d'encaissement", _cboPayCompte, 210));
            flowPay.Controls.Add(Theme.CreateFieldGroup("Montant payé (0 = Impayé)", _numPayMontant, 150));
            var btnPay = Theme.CreatePrimaryButton("Encaisser", 120, 34);
            btnPay.BackColor = Theme.SuccessGreen;
            btnPay.Margin = new Padding(4, 18, 0, 4);
            btnPay.Click += (_, _) => SavePaiement();
            flowPay.Controls.Add(btnPay);

            topPanel.Controls.Add(flowPay);
            topPanel.Controls.Add(flowMem);

            tabPay.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
            tabPay.Controls.Add(topPanel);
        }

        var listPay = Theme.CreateCardPanel();
        listPay.Dock = DockStyle.Fill;
        listPay.Controls.Add(_gridPaiements);
        tabPay.Controls.Add(listPay);
        listPay.BringToFront();

        // ==================== ONGLET 3 : SUIVI DES IMPAYÉS & RELANCES ====================
        var tabImp = new TabPage("Suivi des impayés & Relances") { BackColor = Theme.PageBg, Padding = new Padding(8) };

        var relanceBar = Theme.CreateCardPanel(new Padding(14, 10, 14, 10));
        relanceBar.Dock = DockStyle.Top;
        relanceBar.AutoSize = true;

        var flowRel = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };
        flowRel.Controls.Add(new Label { Text = "Relance rapide du membre sélectionné :", Font = Theme.FontBodyBold, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 9, 14, 4) });

        var btnRelPdf = Theme.CreatePrimaryButton("Avis de relance (PDF)", 175, 34);
        btnRelPdf.Margin = new Padding(0, 0, 10, 4);
        btnRelPdf.Click += (_, _) => GenerateReminderPdf();

        var btnRelMsg = Theme.CreateSecondaryButton("Copier message WhatsApp / E-mail", 240, 34);
        btnRelMsg.Margin = new Padding(0, 0, 0, 4);
        btnRelMsg.Click += (_, _) => CopyWhatsAppReminder();

        flowRel.Controls.Add(btnRelPdf);
        flowRel.Controls.Add(btnRelMsg);
        relanceBar.Controls.Add(flowRel);

        var listImp = Theme.CreateCardPanel();
        listImp.Dock = DockStyle.Fill;
        listImp.Controls.Add(_gridImpayes);
        listImp.Controls.Add(new Label { Text = "Membres en attente, partiellement réglés ou impayés", Font = Theme.FontSection, ForeColor = Theme.DangerRed, Dock = DockStyle.Top, Height = 30 });

        tabImp.Controls.Add(listImp);
        tabImp.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
        tabImp.Controls.Add(relanceBar);

        tabs.TabPages.AddRange(new[] { tabCamp, tabPay, tabImp });
        Controls.Add(tabs);
    }

    private void LoadAll()
    {
        var structs = _repo.GetStructures();
        if (UserSession.IsTresorier && UserSession.CurrentUser?.StructureId != null)
            structs = structs.Where(s => s.Id == UserSession.CurrentUser.StructureId).ToList();

        _cboStructure.DataSource = structs.ToList();
        _cboStructure.DisplayMember = "Nom"; _cboStructure.ValueMember = "Id";
        _cboMembreStruct.DataSource = structs.ToList();
        _cboMembreStruct.DisplayMember = "Nom"; _cboMembreStruct.ValueMember = "Id";

        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        var cotisations = _repo.GetCotisations();
        if (scope.HasValue)
            cotisations = cotisations.Where(c => c.StructureId == scope.Value).ToList();

        _campagnesList = cotisations;
        _gridCampagnes.DataSource = _campagnesList.Select(c => new
        {
            Cotisation = c.Nom,
            Structure = c.StructureNom,
            Montant = $"{c.MontantParMembre:N0} FCFA",
            Date_Limite = c.DateLimite.ToString("dd/MM/yyyy"),
            Population = c.PopulationConcernee,
            Statut = c.Statut
        }).ToList();

        _cboPayCotis.DataSource = cotisations;
        _cboPayCotis.DisplayMember = "Nom"; _cboPayCotis.ValueMember = "Id";
        _cboPayMembre.DataSource = _repo.GetMembres(scope);
        _cboPayCompte.DataSource = _repo.GetComptes(scope);

        var paiements = _repo.GetPaiementsCotisations(onlyUnpaid: false);
        var impayes = _repo.GetPaiementsCotisations(onlyUnpaid: true);
        if (UserSession.IsTresorier && !string.IsNullOrEmpty(UserSession.CurrentUser?.StructureNom))
        {
            paiements = paiements.Where(p => p.StructureNom == UserSession.CurrentUser.StructureNom).ToList();
            impayes = impayes.Where(p => p.StructureNom == UserSession.CurrentUser.StructureNom).ToList();
        }

        _gridPaiements.DataSource = paiements.Select(p => new
        {
            Membre = p.MembreNom,
            Structure = p.StructureNom,
            Cotisation = p.CotisationNom,
            Montant_Dû = $"{p.MontantDu:N0} FCFA",
            Payé = $"{p.MontantPaye:N0} FCFA",
            Échéance = p.DateEcheance.ToString("dd/MM/yyyy"),
            Statut = p.Statut
        }).ToList();

        _impayesList = impayes;
        _gridImpayes.DataSource = _impayesList.Select(p => new
        {
            Membre = p.MembreNom,
            Structure = p.StructureNom,
            Cotisation = p.CotisationNom,
            Reste_Dû = $"-{(p.MontantDu - p.MontantPaye):N0} FCFA",
            Échéance = p.DateEcheance.ToString("dd/MM/yyyy"),
            Statut = p.Statut
        }).ToList();
    }

    private void AddCotisation()
    {
        if (string.IsNullOrWhiteSpace(_txtNom.Text) || _cboStructure.SelectedValue is not int sid) return;
        _repo.CreateCotisation(new CotisationItem
        {
            Nom = _txtNom.Text.Trim(),
            StructureId = sid,
            MontantParMembre = _numMontant.Value,
            DateLimite = _dtpLimite.Value.Date,
            PopulationConcernee = _txtPop.Text.Trim(),
            Statut = "ACTIVE"
        }, UserSession.CurrentUser!.Id);
        _txtNom.Clear();
        LoadAll();
    }

    private void DeleteSelectedCotisation()
    {
        if (_gridCampagnes.CurrentRow == null) return;
        int idx = _gridCampagnes.CurrentRow.Index;
        if (idx < 0 || idx >= _campagnesList.Count) return;

        var c = _campagnesList[idx];
        if (MessageBox.Show($"Supprimer la campagne « {c.Nom} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        _repo.DeleteCotisation(c.Id, UserSession.CurrentUser!.Id);
        LoadAll();
    }

    private void AddMembre()
    {
        if (string.IsNullOrWhiteSpace(_txtMembreNom.Text) || _cboMembreStruct.SelectedValue is not int sid) return;
        _repo.CreateMembre(new MembreItem
        {
            Matricule = $"MBR-{Random.Shared.Next(1000, 9999)}",
            NomComplet = _txtMembreNom.Text.Trim(),
            Telephone = _txtMembreTel.Text.Trim(),
            StructureId = sid
        }, UserSession.CurrentUser!.Id);
        _txtMembreNom.Clear(); _txtMembreTel.Clear();
        LoadAll();
    }

    private void SavePaiement()
    {
        if (_cboPayCotis.SelectedValue is not int cotId || _cboPayMembre.SelectedItem is not MembreItem mem || _cboPayCompte.SelectedItem is not CompteTresorerie cpt)
        {
            MessageBox.Show("Veuillez sélectionner une campagne, un membre et un compte.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        try
        {
            _repo.EnregistrerPaiementCotisation(cotId, mem.Id, cpt.Id, _numPayMontant.Value, UserSession.CurrentUser!.Id);
            LoadAll();
            MessageBox.Show("Règlement enregistré avec succès !", "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ImportMembresCsv()
    {
        if (_cboMembreStruct.SelectedValue is not int sid)
        {
            MessageBox.Show("Sélectionnez d'abord la structure cible.", "Import CSV", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        using var ofd = new OpenFileDialog
        {
            Title = "Importer une liste de membres (CSV : Nom;Email;Telephone)",
            Filter = "Fichiers CSV (*.csv;*.txt)|*.csv;*.txt"
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            int count = _repo.ImportMembresFromCsv(ofd.FileName, sid, UserSession.CurrentUser!.Id);
            LoadAll();
            MessageBox.Show($"{count} membre(s) importé(s) avec succès dans la base MySQL !", "Import CSV réussi", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur d'import CSV : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void GenerateReminderPdf()
    {
        if (_gridImpayes.CurrentRow == null) return;
        int idx = _gridImpayes.CurrentRow.Index;
        if (idx < 0 || idx >= _impayesList.Count) return;

        var p = _impayesList[idx];
        using var sfd = new SaveFileDialog
        {
            Filter = "Document PDF (*.pdf)|*.pdf",
            FileName = $"Relance_{p.MembreNom.Replace(" ", "_")}.pdf"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var org = _repo.GetParametres().FirstOrDefault(x => x.CleParam == "NOM_ORGANISATION")?.ValeurParam ?? "IIT FINCORE";
            PdfReportService.GenerateUnpaidReminderPdf(sfd.FileName, org, p);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur PDF : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void CopyWhatsAppReminder()
    {
        if (_gridImpayes.CurrentRow == null) return;
        int idx = _gridImpayes.CurrentRow.Index;
        if (idx < 0 || idx >= _impayesList.Count) return;

        var p = _impayesList[idx];
        var reste = Math.Max(0, p.MontantDu - p.MontantPaye);
        var msg = $"Bonjour {p.MembreNom},\n\nRappel de la trésorerie ({p.StructureNom}) : un solde de {reste:N0} FCFA reste à régler pour la cotisation « {p.CotisationNom} » (échéance : {p.DateEcheance:dd/MM/yyyy}).\n\nMerci pour votre régularisation !\n- IIT FINCORE";
        Clipboard.SetText(msg);
        MessageBox.Show("Message de relance copié dans le presse-papiers ! Collez-le directement dans WhatsApp ou par e-mail.", "Relance prête", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }
}

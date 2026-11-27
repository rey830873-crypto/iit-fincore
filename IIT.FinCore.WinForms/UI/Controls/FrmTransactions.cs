using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class FrmTransactions : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly ComboBox _cboFilterType = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblRapprochement = new();
    private List<TransactionItem> _allItems = new();
    private List<TransactionItem> _filtered = new();

    public FrmTransactions()
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
        var bar = Theme.CreateCardPanel(new Padding(14, 12, 14, 10));
        bar.Dock = DockStyle.Top;
        bar.Height = 104;

        var row1 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 42,
            WrapContents = false,
            BackColor = Color.Transparent
        };

        _cboFilterType.Width = 155;
        _cboFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboFilterType.Items.AddRange(new object[] { "TOUS", "RECETTE", "DEPENSE", "TRANSFERT_SORTANT", "TRANSFERT_ENTRANT" });
        _cboFilterType.SelectedIndex = 0;
        _cboFilterType.Margin = new Padding(0, 4, 10, 0);
        _cboFilterType.SelectedIndexChanged += (_, _) => LoadData();

        _txtSearch.Width = 220;
        _txtSearch.PlaceholderText = "Rechercher libellé / réf...";
        _txtSearch.Margin = new Padding(0, 4, 14, 0);
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        row1.Controls.Add(_cboFilterType);
        row1.Controls.Add(_txtSearch);

        if (UserSession.HasPermission("RECETTE_CREATE"))
        {
            var btnRec = Theme.CreatePrimaryButton("+ Recette", 118, 34);
            btnRec.BackColor = Theme.SuccessGreen;
            btnRec.Margin = new Padding(0, 0, 8, 0);
            btnRec.Click += (_, _) => OpenTrxDialog("RECETTE");
            row1.Controls.Add(btnRec);
        }
        if (UserSession.HasPermission("DEPENSE_CREATE"))
        {
            var btnDep = Theme.CreatePrimaryButton("+ Dépense", 118, 34);
            btnDep.BackColor = Theme.DangerRed;
            btnDep.Margin = new Padding(0, 0, 8, 0);
            btnDep.Click += (_, _) => OpenTrxDialog("DEPENSE");
            row1.Controls.Add(btnDep);
        }
        if (UserSession.HasPermission("TRANSFERT_CREATE"))
        {
            var btnTrf = Theme.CreatePrimaryButton("Transfert", 112, 34);
            btnTrf.Margin = new Padding(0, 0, 0, 0);
            btnTrf.Click += (_, _) =>
            {
                using var dlg = new FrmTransfertDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadData();
            };
            row1.Controls.Add(btnTrf);
        }

        var row2 = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 40,
            WrapContents = false,
            BackColor = Color.Transparent,
            Padding = new Padding(0, 4, 0, 0)
        };

        var lblActions = new Label
        {
            Text = "Opération sélectionnée :",
            Font = Theme.FontBodyBold,
            ForeColor = Theme.TextMuted,
            AutoSize = true,
            Margin = new Padding(0, 8, 10, 0)
        };
        row2.Controls.Add(lblActions);

        var btnDetails = Theme.CreateSecondaryButton("Voir détails", 114, 34);
        btnDetails.Margin = new Padding(0, 0, 8, 0);
        btnDetails.Click += (_, _) => ShowSelectedDetails();
        row2.Controls.Add(btnDetails);

        var btnReceipt = Theme.CreateSecondaryButton("Reçu PDF", 108, 34);
        btnReceipt.Margin = new Padding(0, 0, 8, 0);
        btnReceipt.Click += (_, _) => PrintSelectedReceiptPdf();
        row2.Controls.Add(btnReceipt);

        if (!UserSession.IsConsultant)
        {
            var btnPoint = Theme.CreateSecondaryButton("Pointer relevé", 128, 34);
            btnPoint.Margin = new Padding(0, 0, 8, 0);
            btnPoint.Click += (_, _) => ToggleSelectedRapprochement();
            row2.Controls.Add(btnPoint);

            var btnCancel = Theme.CreatePrimaryButton("Annuler / Suppr.", 138, 34);
            btnCancel.BackColor = Theme.DangerRed;
            btnCancel.Margin = new Padding(0, 0, 0, 0);
            btnCancel.Click += (_, _) => CancelOrDeleteSelected();
            row2.Controls.Add(btnCancel);
        }

        bar.Controls.Add(row2);
        bar.Controls.Add(row1);

        // Bandeau Rapprochement bancaire & Mobile Money
        var rapBar = Theme.CreateCardPanel(new Padding(16, 10, 16, 10));
        rapBar.Dock = DockStyle.Top;
        rapBar.Height = 44;
        _lblRapprochement.Dock = DockStyle.Fill;
        _lblRapprochement.UseMnemonic = false;
        _lblRapprochement.Font = Theme.FontBodyBold;
        _lblRapprochement.ForeColor = Theme.TextPrimary;
        _lblRapprochement.TextAlign = ContentAlignment.MiddleLeft;
        rapBar.Controls.Add(_lblRapprochement);

        var listCard = Theme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;
        _grid.CellDoubleClick += (_, _) => ShowSelectedDetails();
        listCard.Controls.Add(_grid);

        Controls.Add(listCard);
        Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
        Controls.Add(rapBar);
        Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
        Controls.Add(bar);
    }

    private void ShowSelectedDetails()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var t = _filtered[idx];
        var imps = _repo.GetImputationsByTransaction(t.Id);
        var fields = new Dictionary<string, string>
        {
            ["Référence"] = t.Reference,
            ["Date de l'opération"] = t.DateOperation.ToString("dd/MM/yyyy"),
            ["Type"] = t.TypeTransaction,
            ["Libellé"] = t.Libelle,
            ["Montant"] = $"{t.Montant:N0} FCFA",
            ["Mode de paiement"] = t.ModePaiement,
            ["Compte"] = t.CompteNom,
            ["Structure"] = t.StructureNom,
            ["Catégorie"] = t.CategorieLibelle ?? "-",
            ["Statut"] = t.Statut,
            ["Saisi par"] = t.CreeParNom ?? "-",
            ["Observations"] = t.Observations ?? "-"
        };

        var sub = imps.Select(i => (i.CentreLibelle, $"{i.MontantImpute:N0} FCFA", $"{i.Pourcentage:N2} %")).ToList();
        using var dlg = new FrmDetailsDialog("Détails de la transaction", fields, t.JustificatifPath, sub, "Ventilation sur centres analytiques");
        dlg.ShowDialog(this);
    }

    private void PrintSelectedReceiptPdf()
    {
        if (_grid.CurrentRow == null)
        {
            MessageBox.Show("Veuillez sélectionner une transaction dans la liste.", "Reçu PDF", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var t = _filtered[idx];
        using var sfd = new SaveFileDialog
        {
            Filter = "Document PDF (*.pdf)|*.pdf",
            FileName = $"Recu_{t.Reference}.pdf"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var imps = _repo.GetImputationsByTransaction(t.Id);
            var orgName = _repo.GetParametres().FirstOrDefault(p => p.CleParam == "NOM_ORGANISATION")?.ValeurParam ?? "IIT FINCORE";
            PdfReportService.GenerateTransactionReceipt(sfd.FileName, orgName, t, imps);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur lors de la génération du reçu PDF : " + ex.Message, "Erreur PDF", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void OpenTrxDialog(string type)
    {
        using var dlg = new FrmTransactionDialog(type);
        if (dlg.ShowDialog(this) == DialogResult.OK)
            LoadData();
    }

    private void ToggleSelectedRapprochement()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var t = _filtered[idx];
        if (t.Statut != "VALIDEE")
        {
            MessageBox.Show("Seule une transaction VALIDÉE peut être rapprochée sur le relevé bancaire.", "Rapprochement", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        _repo.ToggleRapprochement(t.Id, !t.EstRapprochee, UserSession.CurrentUser!.Id);
        LoadData();
    }

    private void CancelOrDeleteSelected()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _filtered.Count) return;

        var t = _filtered[idx];
        var actionLabel = t.Statut == "VALIDEE" ? "annuler (et rétablir le solde du compte)" : "supprimer définitivement";
        if (MessageBox.Show($"Voulez-vous {actionLabel} l'opération « {t.Libelle} » ({t.Montant:N0} FCFA) ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        var msg = _repo.CancelOrDeleteTransaction(t.Id, UserSession.CurrentUser!.Id);
        LoadData();
        MessageBox.Show(msg, "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void LoadData()
    {
        var filter = _cboFilterType.SelectedItem?.ToString();
        if (filter == "TOUS") filter = null;

        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        _allItems = _repo.GetTransactions(filter, scope);

        var (comptable, rapproche, ecart) = _repo.GetRapprochementSummary(scope);
        _lblRapprochement.Text = $"Rapprochement bancaire & Mobile Money   |   Solde comptable : {comptable:N0} FCFA   •   Solde rapproché : {rapproche:N0} FCFA   •   Écart non pointé : {ecart:N0} FCFA";

        ApplyFilter();
    }

    private void ApplyFilter()
    {
        var q = _txtSearch.Text.Trim();
        _filtered = string.IsNullOrEmpty(q)
            ? _allItems
            : _allItems.Where(t =>
                t.Libelle.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.Reference.Contains(q, StringComparison.OrdinalIgnoreCase) ||
                t.CompteNom.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        _grid.DataSource = _filtered.Select(t => new
        {
            Date = t.DateOperation.ToString("dd/MM/yyyy"),
            Référence = t.Reference,
            Type = t.TypeTransaction switch
            {
                "RECETTE" => "Recette",
                "DEPENSE" => "Dépense",
                "TRANSFERT_ENTRANT" => "Transfert +",
                "TRANSFERT_SORTANT" => "Transfert -",
                _ => t.TypeTransaction
            },
            Libellé = t.Libelle,
            Compte = t.CompteNom,
            Montant = (t.TypeTransaction is "RECETTE" or "TRANSFERT_ENTRANT" ? "+" : "-") + $"{t.Montant:N0} FCFA",
            Ventilation = t.CentresResume ?? "-",
            Pointée = t.EstRapprochee ? "Pointée" : "À pointer",
            Statut = t.Statut switch
            {
                "VALIDEE" => "Validée",
                "EN_ATTENTE" => "En attente",
                "REJETEE" => "Rejetée",
                "ANNULEE" => "Annulée",
                _ => t.Statut
            }
        }).ToList();
    }
}

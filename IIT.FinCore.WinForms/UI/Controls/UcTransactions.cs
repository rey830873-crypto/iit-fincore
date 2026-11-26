using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

public class UcTransactions : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly ComboBox _cboFilterType = new();
    private readonly TextBox _txtSearch = new();
    private readonly Label _lblRapprochement = new();
    private List<TransactionItem> _allItems = new();
    private List<TransactionItem> _filtered = new();

    public UcTransactions()
    {
        BackColor = Theme.PageBg;
        BuildLayout();
        LoadData();
    }

    private void BuildLayout()
    {
        var bar = Theme.CreateCardPanel(new Padding(14, 12, 14, 12));
        bar.Dock = DockStyle.Top;
        bar.AutoSize = true;

        var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };

        _cboFilterType.Width = 155;
        _cboFilterType.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboFilterType.Items.AddRange(new object[] { "TOUS", "RECETTE", "DEPENSE", "TRANSFERT_SORTANT", "TRANSFERT_ENTRANT" });
        _cboFilterType.SelectedIndex = 0;
        _cboFilterType.Margin = new Padding(0, 4, 10, 4);
        _cboFilterType.SelectedIndexChanged += (_, _) => LoadData();

        _txtSearch.Width = 200;
        _txtSearch.PlaceholderText = "Rechercher libellé / réf...";
        _txtSearch.Margin = new Padding(0, 4, 14, 4);
        _txtSearch.TextChanged += (_, _) => ApplyFilter();

        flow.Controls.Add(_cboFilterType);
        flow.Controls.Add(_txtSearch);

        if (UserSession.HasPermission("RECETTE_CREATE"))
        {
            var btnRec = Theme.CreatePrimaryButton("+ Recette", 120, 34);
            btnRec.BackColor = Theme.SuccessGreen;
            btnRec.Margin = new Padding(0, 0, 8, 4);
            btnRec.Click += (_, _) => OpenTrxDialog("RECETTE");
            flow.Controls.Add(btnRec);
        }
        if (UserSession.HasPermission("DEPENSE_CREATE"))
        {
            var btnDep = Theme.CreatePrimaryButton("+ Dépense", 120, 34);
            btnDep.BackColor = Theme.DangerRed;
            btnDep.Margin = new Padding(0, 0, 8, 4);
            btnDep.Click += (_, _) => OpenTrxDialog("DEPENSE");
            flow.Controls.Add(btnDep);
        }
        if (UserSession.HasPermission("TRANSFERT_CREATE"))
        {
            var btnTrf = Theme.CreatePrimaryButton("Transfert", 115, 34);
            btnTrf.Margin = new Padding(0, 0, 8, 4);
            btnTrf.Click += (_, _) =>
            {
                using var dlg = new FrmTransfertDialog();
                if (dlg.ShowDialog(this) == DialogResult.OK)
                    LoadData();
            };
            flow.Controls.Add(btnTrf);
        }

        var btnDetails = Theme.CreateSecondaryButton("Voir détails", 110, 34);
        btnDetails.Margin = new Padding(0, 0, 8, 4);
        btnDetails.Click += (_, _) => ShowSelectedDetails();
        flow.Controls.Add(btnDetails);

        var btnReceipt = Theme.CreateSecondaryButton("Reçu PDF", 105, 34);
        btnReceipt.Margin = new Padding(0, 0, 8, 4);
        btnReceipt.Click += (_, _) => PrintSelectedReceiptPdf();
        flow.Controls.Add(btnReceipt);

        if (!UserSession.IsConsultant)
        {
            var btnPoint = Theme.CreateSecondaryButton("Pointer relevé", 125, 34);
            btnPoint.Margin = new Padding(0, 0, 8, 4);
            btnPoint.Click += (_, _) => ToggleSelectedRapprochement();
            flow.Controls.Add(btnPoint);

            var btnCancel = Theme.CreatePrimaryButton("Annuler / Suppr.", 135, 34);
            btnCancel.BackColor = Theme.DangerRed;
            btnCancel.Margin = new Padding(0, 0, 0, 4);
            btnCancel.Click += (_, _) => CancelOrDeleteSelected();
            flow.Controls.Add(btnCancel);
        }

        bar.Controls.Add(flow);

        // Bandeau Rapprochement bancaire & Mobile Money
        var rapBar = Theme.CreateCardPanel(new Padding(16, 10, 16, 10));
        rapBar.Dock = DockStyle.Top;
        rapBar.Height = 44;
        _lblRapprochement.Dock = DockStyle.Fill;
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
            ["Catégorie"] = t.CategorieLibelle ?? "—",
            ["Statut"] = t.Statut,
            ["Saisi par"] = t.CreeParNom ?? "—",
            ["Observations"] = t.Observations ?? "—"
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
            Type = t.TypeTransaction,
            Libellé = t.Libelle,
            Compte = t.CompteNom,
            Montant = (t.TypeTransaction is "RECETTE" or "TRANSFERT_ENTRANT" ? "+" : "-") + $"{t.Montant:N0} FCFA",
            Ventilation_Analytique = t.CentresResume ?? "—",
            Pointée = t.EstRapprochee ? "Pointée" : "À pointer",
            Statut = t.Statut
        }).ToList();
    }
}

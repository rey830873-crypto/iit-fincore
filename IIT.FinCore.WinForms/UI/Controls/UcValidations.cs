using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class UcValidations : UserControl
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _grid = Theme.CreateStyledGrid();
    private readonly TextBox _txtCommentaire = new();
    private List<DemandeValidationItem> _demandes = new();

    public UcValidations()
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
        if (UserSession.HasPermission("DEPENSE_VALIDATE"))
        {
            var bar = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            bar.Dock = DockStyle.Top;
            bar.AutoSize = true;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };
            flow.Controls.Add(Theme.CreateFieldGroup("Commentaire de décision", _txtCommentaire, 380));

            var btnApprove = Theme.CreatePrimaryButton("Approuver", 135, 34);
            btnApprove.BackColor = Theme.SuccessGreen;
            btnApprove.Margin = new Padding(4, 20, 8, 4);
            btnApprove.Click += (_, _) => DecideSelected(true);

            var btnReject = Theme.CreatePrimaryButton("Rejeter", 115, 34);
            btnReject.BackColor = Theme.DangerRed;
            btnReject.Margin = new Padding(0, 20, 0, 4);
            btnReject.Click += (_, _) => DecideSelected(false);

            flow.Controls.Add(btnApprove);
            flow.Controls.Add(btnReject);
            bar.Controls.Add(flow);

            Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            Controls.Add(bar);
        }

        var listCard = Theme.CreateCardPanel();
        listCard.Dock = DockStyle.Fill;
        _grid.CellDoubleClick += (_, _) => ShowSelectedDetails();
        listCard.Controls.Add(_grid);
        listCard.Controls.Add(new Label { Text = "Double-cliquez sur une demande pour consulter sa fiche complète", Font = Theme.FontSubtitle, ForeColor = Theme.TextMuted, Dock = DockStyle.Top, Height = 26 });
        Controls.Add(listCard);
        listCard.BringToFront();
    }

    private void ShowSelectedDetails()
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _demandes.Count) return;

        var d = _demandes[idx];
        using var dlg = new Dialogs.FrmDetailsDialog($"Demande de validation #{d.Id}", new Dictionary<string, string>
        {
            ["Référence"] = d.Reference,
            ["Libellé"] = d.Libelle,
            ["Structure"] = d.StructureNom,
            ["Compte"] = d.CompteNom,
            ["Montant"] = $"{d.Montant:N0} FCFA",
            ["Demandeur"] = d.DemandeurNom,
            ["Date demande"] = d.DateDemande.ToString("dd/MM/yyyy HH:mm"),
            ["Motif"] = d.MotifDemande,
            ["Statut"] = d.Statut,
            ["Décision"] = d.CommentaireDecision ?? "En attente"
        });
        dlg.ShowDialog(this);
    }

    private void LoadData()
    {
        _demandes = _repo.GetDemandesValidation(onlyPending: false);
        if (UserSession.IsTresorier && !string.IsNullOrEmpty(UserSession.CurrentUser?.StructureNom))
            _demandes = _demandes.Where(d => d.StructureNom == UserSession.CurrentUser.StructureNom).ToList();

        _grid.DataSource = _demandes.Select(d => new
        {
            d.Id,
            Date = d.DateDemande.ToString("dd/MM/yyyy HH:mm"),
            Référence = d.Reference,
            Libellé = d.Libelle,
            Structure = d.StructureNom,
            Compte = d.CompteNom,
            Montant = $"-{d.Montant:N0} FCFA",
            Demandeur = d.DemandeurNom,
            Statut = d.Statut,
            Décision = d.CommentaireDecision ?? "-"
        }).ToList();
    }

    private void DecideSelected(bool approve)
    {
        if (_grid.CurrentRow == null) return;
        int idx = _grid.CurrentRow.Index;
        if (idx < 0 || idx >= _demandes.Count) return;

        var d = _demandes[idx];
        if (d.Statut != "EN_ATTENTE")
        {
            MessageBox.Show("Cette demande a déjà été traitée.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            var comm = string.IsNullOrWhiteSpace(_txtCommentaire.Text)
                ? (approve ? "Approuvée par l'administrateur" : "Rejetée par l'administrateur")
                : _txtCommentaire.Text.Trim();

            _repo.DecideValidation(d.Id, d.TransactionId, approve, UserSession.CurrentUser!.Id, comm);
            _txtCommentaire.Clear();
            LoadData();
            MessageBox.Show(approve ? "Demande approuvée et solde du compte mis à jour." : "Demande rejetée.",
                "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

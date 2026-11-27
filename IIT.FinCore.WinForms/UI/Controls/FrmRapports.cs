using IIT.FinCore.WinForms.UI.Dialogs;
using System.Drawing;
using System.Text;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

public partial class FrmRapports : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly DataGridView _gridSynthese = Theme.CreateStyledGrid();
    private readonly DataGridView _gridRecettes = Theme.CreateStyledGrid();
    private readonly DataGridView _gridDepenses = Theme.CreateStyledGrid();

    public FrmRapports()
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
        var bar = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
        bar.Dock = DockStyle.Top;
        bar.AutoSize = true;

        var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };
        var lbl = new Label { Text = "Exportation des états financiers :", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, AutoSize = true, Margin = new Padding(0, 8, 16, 4) };

        var btnExportPdf = Theme.CreatePrimaryButton("Rapport Officiel (PDF)", 195, 34);
        btnExportPdf.Margin = new Padding(0, 0, 10, 4);
        btnExportPdf.Click += (_, _) => ExportFinancialPdf();

        var btnExportTrx = Theme.CreateSecondaryButton("Transactions (CSV)", 165, 34);
        btnExportTrx.Margin = new Padding(0, 0, 10, 4);
        btnExportTrx.Click += (_, _) => ExportTransactionsCsv();

        var btnExportAna = Theme.CreateSecondaryButton("Analytique (CSV)", 155, 34);
        btnExportAna.Margin = new Padding(0, 0, 0, 4);
        btnExportAna.Click += (_, _) => ExportAnalytiqueCsv();

        flow.Controls.AddRange(new Control[] { lbl, btnExportPdf, btnExportTrx, btnExportAna });
        bar.Controls.Add(flow);

        var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBodyBold };

        var tabSynth = new TabPage("Synthèse financière & analytique") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        var c1 = Theme.CreateCardPanel(); c1.Dock = DockStyle.Fill; c1.Controls.Add(_gridSynthese);
        tabSynth.Controls.Add(c1);

        var tabRec = new TabPage("Rapport des recettes") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        var c2 = Theme.CreateCardPanel(); c2.Dock = DockStyle.Fill; c2.Controls.Add(_gridRecettes);
        tabRec.Controls.Add(c2);

        var tabDep = new TabPage("Rapport des dépenses") { BackColor = Theme.PageBg, Padding = new Padding(8) };
        var c3 = Theme.CreateCardPanel(); c3.Dock = DockStyle.Fill; c3.Controls.Add(_gridDepenses);
        tabDep.Controls.Add(c3);

        tabs.TabPages.AddRange(new[] { tabSynth, tabRec, tabDep });

        Controls.Add(tabs);
        Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
        Controls.Add(bar);
    }

    private List<Models.SyntheseAnalytiqueRow> GetScopedSynthese()
    {
        var list = _repo.GetSyntheseAnalytique();
        if (UserSession.IsTresorier && !string.IsNullOrEmpty(UserSession.CurrentUser?.StructureNom))
            list = list.Where(s => s.StructureNom == UserSession.CurrentUser.StructureNom).ToList();
        return list;
    }

    private void LoadData()
    {
        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;

        _gridSynthese.DataSource = GetScopedSynthese().Select(s => new
        {
            Structure = s.StructureNom,
            Code_Centre = s.CentreCode,
            Centre_Analytique = s.CentreLibelle,
            Recettes = $"+{s.TotalRecettes:N0} FCFA",
            Dépenses = $"-{s.TotalDepenses:N0} FCFA",
            Résultat_Net = (s.ResultatNet >= 0 ? "+" : "") + $"{s.ResultatNet:N0} FCFA"
        }).ToList();

        _gridRecettes.DataSource = _repo.GetTransactions("RECETTE", scope, 1000).Select(t => new
        {
            Date = t.DateOperation.ToString("dd/MM/yyyy"),
            Référence = t.Reference,
            Libellé = t.Libelle,
            Compte = t.CompteNom,
            Structure = t.StructureNom,
            Montant = $"+{t.Montant:N0} FCFA",
            Analytique = t.CentresResume ?? "-",
            Statut = t.Statut
        }).ToList();

        _gridDepenses.DataSource = _repo.GetTransactions("DEPENSE", scope, 1000).Select(t => new
        {
            Date = t.DateOperation.ToString("dd/MM/yyyy"),
            Référence = t.Reference,
            Libellé = t.Libelle,
            Compte = t.CompteNom,
            Structure = t.StructureNom,
            Montant = $"-{t.Montant:N0} FCFA",
            Analytique = t.CentresResume ?? "-",
            Statut = t.Statut
        }).ToList();
    }

    private void ExportFinancialPdf()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Document PDF (*.pdf)|*.pdf",
            FileName = $"IIT_FINCORE_Rapport_Financier_{DateTime.Now:yyyyMMdd}.pdf"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
            var kpi = _repo.GetDashboardKpis(scope);
            var synthese = GetScopedSynthese();
            var trxs = _repo.GetTransactions(null, scope, 200);
            var orgName = _repo.GetParametres().FirstOrDefault(p => p.CleParam == "NOM_ORGANISATION")?.ValeurParam ?? "IIT FINCORE";

            PdfReportService.GenerateFinancialReport(sfd.FileName, orgName, kpi, synthese, trxs, UserSession.CurrentUser?.NomComplet ?? "Utilisateur");
            FrmMessageBox.ShowSuccess(
                $"Rapport financier officiel PDF généré et ouvert avec succès !\n\nFichier : {Path.GetFileName(sfd.FileName)}",
                "IIT FINCORE - Rapport PDF");
        }
        catch (Exception ex)
        {
            FrmMessageBox.ShowError("Erreur lors de la génération PDF : " + ex.Message, "Erreur PDF");
        }
    }

    private void ExportTransactionsCsv()
    {
        using var sfd = new SaveFileDialog { Filter = "Fichier CSV (*.csv)|*.csv", FileName = $"IIT_FINCORE_Transactions_{DateTime.Now:yyyyMMdd}.csv" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        var rows = _repo.GetTransactions(null, scope, 5000);
        var sb = new StringBuilder();
        sb.AppendLine("Date;Reference;Type;Libelle;Compte;Structure;Montant_FCFA;Ventilation_Analytique;Statut");
        foreach (var r in rows)
            sb.AppendLine($"{r.DateOperation:yyyy-MM-dd};{r.Reference};{r.TypeTransaction};\"{r.Libelle}\";\"{r.CompteNom}\";\"{r.StructureNom}\";{r.Montant};\"{r.CentresResume}\";{r.Statut}");

        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        FrmMessageBox.ShowSuccess(
            $"Export CSV des transactions généré avec succès !\n\nFichier : {Path.GetFileName(sfd.FileName)}",
            "IIT FINCORE - Export CSV");
    }

    private void ExportAnalytiqueCsv()
    {
        using var sfd = new SaveFileDialog { Filter = "Fichier CSV (*.csv)|*.csv", FileName = $"IIT_FINCORE_Analytique_{DateTime.Now:yyyyMMdd}.csv" };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        var rows = GetScopedSynthese();
        var sb = new StringBuilder();
        sb.AppendLine("Structure;Code_Centre;Centre_Analytique;Recettes_FCFA;Depenses_FCFA;Resultat_Net_FCFA");
        foreach (var r in rows)
            sb.AppendLine($"\"{r.StructureNom}\";{r.CentreCode};\"{r.CentreLibelle}\";{r.TotalRecettes};{r.TotalDepenses};{r.ResultatNet}");

        File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
        FrmMessageBox.ShowSuccess(
            $"Synthèse analytique exportée avec succès !\n\nFichier : {Path.GetFileName(sfd.FileName)}",
            "IIT FINCORE - Export Analytique");
    }
}

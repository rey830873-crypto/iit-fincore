using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

/// <summary>
/// Module avancé : Prévision Cash-Flow (6 mois), Opérations récurrentes et Clôture d'exercice comptable.
/// </summary>
public partial class FrmPilotageAvance : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly SmoothPanel _forecastChart = new();
    private readonly DataGridView _gridForecast = Theme.CreateStyledGrid();
    private readonly DataGridView _gridRecurrent = Theme.CreateStyledGrid();
    private readonly DataGridView _gridExercices = Theme.CreateStyledGrid();

    private readonly TextBox _txtRecLib = new();
    private readonly ComboBox _cboRecType = new();
    private readonly NumericUpDown _numRecMontant = new();
    private readonly ComboBox _cboRecFreq = new();
    private readonly DateTimePicker _dtpRecEch = new();
    private readonly ComboBox _cboRecCompte = new();
    private readonly ComboBox _cboRecCentre = new();

    private List<CashFlowPoint> _forecast = new();
    private List<ExerciceComptable> _exercices = new();
    private List<OperationRecurrente> _recurrents = new();

    public FrmPilotageAvance()
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

        // ==================== ONGLET 1 : PRÉVISION CASH-FLOW (6 MOIS) ====================
        var tabCash = new TabPage("Prévision Cash-Flow (6 mois)") { BackColor = Theme.PageBg, Padding = new Padding(8) };

        var chartCard = Theme.CreateCardPanel();
        chartCard.Dock = DockStyle.Top;
        chartCard.Height = 235;
        var lblChart = new Label { Text = "Projection du solde de trésorerie sur 6 mois (intègre charges récurrentes & cotisations impayées)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
        _forecastChart.Dock = DockStyle.Fill;
        _forecastChart.BackColor = Color.White;
        _forecastChart.Paint += DrawForecastChart;
        chartCard.Controls.Add(_forecastChart);
        chartCard.Controls.Add(lblChart);

        var gridCard = Theme.CreateCardPanel();
        gridCard.Dock = DockStyle.Fill;
        gridCard.Controls.Add(_gridForecast);

        tabCash.Controls.Add(gridCard);
        tabCash.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
        tabCash.Controls.Add(chartCard);

        // ==================== ONGLET 2 : CHARGES & OPÉRATIONS RÉCURRENTES ====================
        var tabRec = new TabPage("Charges & Opérations récurrentes") { BackColor = Theme.PageBg, Padding = new Padding(8) };

        if (!UserSession.IsConsultant)
        {
            var formCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            formCard.Dock = DockStyle.Top;
            formCard.AutoSize = true;

            var lbl = new Label { Text = "Planifier une opération récurrente (loyer, abonnements, dotations)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 28 };
            _cboRecType.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboRecType.Items.AddRange(new object[] { "DEPENSE", "RECETTE" });
            _cboRecType.SelectedIndex = 0;

            _numRecMontant.Maximum = 1_000_000_000; _numRecMontant.ThousandsSeparator = true; _numRecMontant.Value = 50000;
            _cboRecFreq.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboRecFreq.Items.AddRange(new object[] { "MENSUEL", "TRIMESTRIEL", "ANNUEL" });
            _cboRecFreq.SelectedIndex = 0;

            _dtpRecEch.Format = DateTimePickerFormat.Short;
            _cboRecCompte.DropDownStyle = ComboBoxStyle.DropDownList;
            _cboRecCentre.DropDownStyle = ComboBoxStyle.DropDownList;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent, Padding = new Padding(0, 6, 0, 0) };
            flow.Controls.Add(Theme.CreateFieldGroup("Libellé", _txtRecLib, 195));
            flow.Controls.Add(Theme.CreateFieldGroup("Type", _cboRecType, 105));
            flow.Controls.Add(Theme.CreateFieldGroup("Montant (FCFA)", _numRecMontant, 125));
            flow.Controls.Add(Theme.CreateFieldGroup("Fréquence", _cboRecFreq, 120));
            flow.Controls.Add(Theme.CreateFieldGroup("Prochaine échéance", _dtpRecEch, 125));
            flow.Controls.Add(Theme.CreateFieldGroup("Compte", _cboRecCompte, 185));
            flow.Controls.Add(Theme.CreateFieldGroup("Centre analytique", _cboRecCentre, 155));

            var btnAdd = Theme.CreatePrimaryButton("+ Planifier", 115, 34);
            btnAdd.Margin = new Padding(4, 20, 8, 4);
            btnAdd.Click += (_, _) => AddRecurrent();
            flow.Controls.Add(btnAdd);

            var btnDelRec = Theme.CreatePrimaryButton("Supprimer", 105, 34);
            btnDelRec.BackColor = Theme.DangerRed;
            btnDelRec.Margin = new Padding(0, 20, 0, 4);
            btnDelRec.Click += (_, _) => DeleteSelectedRecurrent();
            flow.Controls.Add(btnDelRec);

            formCard.Controls.Add(flow);
            formCard.Controls.Add(lbl);

            tabRec.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 10, BackColor = Theme.PageBg });
            tabRec.Controls.Add(formCard);
        }

        var listRec = Theme.CreateCardPanel();
        listRec.Dock = DockStyle.Fill;
        listRec.Controls.Add(_gridRecurrent);
        tabRec.Controls.Add(listRec);
        listRec.BringToFront();

        tabs.TabPages.Add(tabCash);
        tabs.TabPages.Add(tabRec);

        // ==================== ONGLET 3 : CLÔTURE D'EXERCICE COMPTABLE (ADMIN) ====================
        if (UserSession.IsAdmin)
        {
            var tabEx = new TabPage("Clôture d'exercice comptable") { BackColor = Theme.PageBg, Padding = new Padding(8) };

            var bar = Theme.CreateCardPanel(new Padding(18, 12, 18, 12));
            bar.Dock = DockStyle.Top;
            bar.Height = 96;

            var lblExInfo = new Label
            {
                Text = "La clôture verrouille définitivement l'exercice sélectionné (trigger MySQL) et ouvre automatiquement l'exercice N+1.",
                Font = Theme.FontBodyBold,
                ForeColor = Theme.TextPrimary,
                Location = new Point(18, 14),
                AutoSize = true
            };

            var btnCloseEx = Theme.CreatePrimaryButton("Clôturer l'exercice sélectionné", 235, 36);
            btnCloseEx.BackColor = Theme.DangerRed;
            btnCloseEx.Location = new Point(18, 44);
            btnCloseEx.Click += (_, _) => CloseSelectedExercice();

            bar.Controls.Add(lblExInfo);
            bar.Controls.Add(btnCloseEx);

            var listEx = Theme.CreateCardPanel();
            listEx.Dock = DockStyle.Fill;
            listEx.Controls.Add(_gridExercices);

            tabEx.Controls.Add(listEx);
            tabEx.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            tabEx.Controls.Add(bar);

            tabs.TabPages.Add(tabEx);
        }

        Controls.Add(tabs);
    }

    private void LoadAll()
    {
        int? scope = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;

        _forecast = _repo.GetCashFlowForecast(6, scope);
        _gridForecast.DataSource = _forecast.Select(f => new
        {
            Mois = f.MoisLabel,
            Entrées_Prévues = $"+{f.EntreesPrevues:N0} FCFA",
            Sorties_Prévues = $"-{f.SortiesPrevues:N0} FCFA",
            Solde_Projeté = $"{f.SoldeProjete:N0} FCFA"
        }).ToList();
        _forecastChart.Invalidate();

        _cboRecCompte.DataSource = _repo.GetComptes(scope);
        _cboRecCentre.DataSource = _repo.GetCentresAnalytiques();

        _recurrents = _repo.GetOperationsRecurrentes(scope);
        _gridRecurrent.DataSource = _recurrents.Select(r => new
        {
            Libellé = r.Libelle,
            Type = r.TypeOperation,
            Montant = (r.TypeOperation == "RECETTE" ? "+" : "-") + $"{r.Montant:N0} FCFA",
            Fréquence = r.Frequence,
            Prochaine_Échéance = r.ProchaineEcheance.ToString("dd/MM/yyyy"),
            Compte = r.CompteNom,
            Centre = r.CentreLibelle,
            Statut = r.EstActif ? "Actif" : "Inactif"
        }).ToList();

        if (UserSession.IsAdmin)
        {
            _exercices = _repo.GetExercices();
            _gridExercices.DataSource = _exercices.Select(e => new
            {
                Année = e.Annee,
                Exercice = e.Libelle,
                Période = $"{e.DateDebut:dd/MM/yyyy} → {e.DateFin:dd/MM/yyyy}",
                Solde_Clôture = e.SoldeCloture.HasValue ? $"{e.SoldeCloture:N0} FCFA" : (e.Statut == "CLOTURE" ? "Clôturé" : "En cours"),
                Date_Clôture = e.DateCloture?.ToString("dd/MM/yyyy HH:mm") ?? "-",
                Statut = e.Statut
            }).ToList();
        }
    }

    private void AddRecurrent()
    {
        if (string.IsNullOrWhiteSpace(_txtRecLib.Text) || _cboRecCompte.SelectedItem is not CompteTresorerie cpt || _cboRecCentre.SelectedItem is not CentreAnalytique ca)
            return;

        try
        {
            _repo.CreateOperationRecurrente(new OperationRecurrente
            {
                Libelle = _txtRecLib.Text.Trim(),
                TypeOperation = _cboRecType.SelectedItem?.ToString() ?? "DEPENSE",
                Montant = _numRecMontant.Value,
                Frequence = _cboRecFreq.SelectedItem?.ToString() ?? "MENSUEL",
                ProchaineEcheance = _dtpRecEch.Value.Date,
                CompteId = cpt.Id,
                CentreAnalytiqueId = ca.Id,
                StructureId = cpt.StructureId
            }, UserSession.CurrentUser!.Id);

            _txtRecLib.Clear();
            LoadAll();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DeleteSelectedRecurrent()
    {
        if (_gridRecurrent.CurrentRow == null) return;
        int idx = _gridRecurrent.CurrentRow.Index;
        if (idx < 0 || idx >= _recurrents.Count) return;

        var r = _recurrents[idx];
        if (MessageBox.Show($"Supprimer l'opération récurrente « {r.Libelle} » ?", "Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes)
            return;

        _repo.DeleteOperationRecurrente(r.Id, UserSession.CurrentUser!.Id);
        LoadAll();
    }

    private void CloseSelectedExercice()
    {
        if (_gridExercices.CurrentRow == null) return;
        int idx = _gridExercices.CurrentRow.Index;
        if (idx < 0 || idx >= _exercices.Count) return;

        var ex = _exercices[idx];
        if (ex.Statut == "CLOTURE")
        {
            MessageBox.Show("Cet exercice est déjà clôturé.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        var confirm = MessageBox.Show(
            $"Confirmez-vous la clôture définitive de « {ex.Libelle} » ?\n\nAucune nouvelle opération ne pourra être saisie sur {ex.Annee}, et l'exercice {ex.Annee + 1} sera automatiquement ouvert.",
            "Clôture d'exercice IIT FINCORE",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning);
        if (confirm != DialogResult.Yes) return;

        try
        {
            _repo.CloturerExercice(ex.Id, UserSession.CurrentUser!.Id);
            LoadAll();
            MessageBox.Show($"Exercice {ex.Annee} clôturé avec succès ! Exercice {ex.Annee + 1} ouvert.", "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception err)
        {
            MessageBox.Show(err.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void DrawForecastChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        if (_forecast.Count == 0) return;

        int padL = 74, padR = 26, padT = 18, padB = 34;
        int plotW = Math.Max(60, _forecastChart.Width - padL - padR);
        int plotH = Math.Max(50, _forecastChart.Height - padT - padB);

        decimal minV = Math.Min(0, _forecast.Min(p => p.SoldeProjete) * 0.9m);
        decimal maxV = Math.Max(1, _forecast.Max(p => p.SoldeProjete) * 1.08m);
        decimal range = Math.Max(1, maxV - minV);

        using var gridPen = new Pen(Color.FromArgb(234, 239, 244), 1);
        for (int i = 0; i <= 3; i++)
        {
            int gy = padT + (plotH * i / 3);
            g.DrawLine(gridPen, padL, gy, padL + plotW, gy);
            decimal val = maxV - (range * i / 3);
            g.DrawString($"{val / 1_000_000m:N1}M", new Font("Segoe UI", 8F), new SolidBrush(Theme.TextMuted), 10, gy - 7);
        }

        var pts = new List<PointF>();
        for (int i = 0; i < _forecast.Count; i++)
        {
            float x = _forecast.Count == 1 ? padL + plotW / 2f : padL + (plotW * i / (float)(_forecast.Count - 1));
            float y = padT + plotH - (float)((_forecast[i].SoldeProjete - minV) / range) * plotH;
            pts.Add(new PointF(x, y));
            g.DrawString(_forecast[i].MoisLabel, new Font("Segoe UI", 8F, FontStyle.Bold), new SolidBrush(Theme.TextMuted), x - 24, padT + plotH + 8);
        }

        if (pts.Count > 1)
        {
            var area = new List<PointF>(pts) { new(pts[^1].X, padT + plotH), new(pts[0].X, padT + plotH) };
            using var areaBrush = new LinearGradientBrush(new Rectangle(padL, padT, plotW, plotH),
                Color.FromArgb(75, Theme.GoldLight), Color.FromArgb(6, Theme.GoldLight), LinearGradientMode.Vertical);
            g.FillPolygon(areaBrush, area.ToArray());

            using var pen = new Pen(Theme.SidebarNavyLight, 3f);
            g.DrawLines(pen, pts.ToArray());
        }

        foreach (var p in pts)
        {
            g.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
            g.DrawEllipse(new Pen(Theme.GoldAccent, 2.5f), p.X - 4, p.Y - 4, 8, 8);
        }
    }
}

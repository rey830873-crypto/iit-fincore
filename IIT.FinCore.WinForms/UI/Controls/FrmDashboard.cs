using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;
using IIT.FinCore.WinForms.UI.Dialogs;

namespace IIT.FinCore.WinForms.UI.Controls;

/// <summary>
/// Tableau de bord structuré spécifiquement pour chacun des 3 rôles :
/// - Administrateur : Gouvernance (KPI + Demandes en attente + Journal d'audit)
/// - Trésorier : Opérations de sa structure (KPI + Mes comptes + Dernières opérations)
/// - Consultant : Analyse & Reporting (KPI Résultat Net + Graphiques + Synthèse analytique)
/// </summary>
public partial class FrmDashboard : Form
{
    private readonly TreasuryRepository _repo = new();
    private readonly ComboBox _cboStructure = new();
    private readonly DataGridView _gridBottomLeft = Theme.CreateStyledGrid();
    private readonly DataGridView _gridBottomRight = Theme.CreateStyledGrid();
    private readonly SmoothPanel _lineChartPanel = new();
    private readonly SmoothPanel _donutChartPanel = new();

    private Label _lblKpi1 = null!;
    private Label _lblKpi2 = null!;
    private Label _lblKpi3 = null!;
    private Label _lblKpi4 = null!;

    private List<SyntheseAnalytiqueRow> _analytique = new();
    private List<MonthlyEvolutionPoint> _evolution = new();

    public FrmDashboard()
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;
        Controls.Clear();
        BackColor = Theme.PageBg;
        BuildLayout();
        InitStructureFilter();
        RefreshData();
    }

    private void BuildLayout()
    {
        // Barre filtre structure + bouton Actualiser (FlowLayoutPanel anti-chevauchement DPI)
        var filterBar = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 46,
            BackColor = Theme.PageBg,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Padding = new Padding(0, 4, 0, 4)
        };
        var lblFilter = new Label
        {
            Text = "Périmètre d'analyse :",
            Font = Theme.FontBodyBold,
            ForeColor = Theme.TextPrimary,
            AutoSize = true,
            Margin = new Padding(2, 7, 10, 0)
        };
        _cboStructure.Width = 300;
        _cboStructure.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboStructure.Margin = new Padding(0, 3, 12, 0);
        _cboStructure.SelectedIndexChanged += (_, _) => RefreshData();

        var btnRefresh = Theme.CreateSecondaryButton("Actualiser", 104, 32);
        btnRefresh.Margin = new Padding(0, 2, 14, 0);
        btnRefresh.Click += (_, _) => RefreshData();

        var tip = new ToolTip { InitialDelay = 250, AutoPopDelay = 5000 };
        tip.SetToolTip(btnRefresh, "Actualiser les indicateurs et graphiques (F5)");

        filterBar.Controls.AddRange(new Control[] { lblFilter, _cboStructure, btnRefresh });

        // Barre d'actions rapides 1-clic pour une prise en main immédiate
        if (UserSession.HasPermission("RECETTE_CREATE") || UserSession.HasPermission("DEPENSE_CREATE"))
        {
            var btnQuickRec = Theme.CreatePrimaryButton("+ Recette", 104, 32);
            btnQuickRec.BackColor = Theme.SuccessGreen;
            btnQuickRec.Margin = new Padding(0, 2, 8, 0);
            btnQuickRec.Click += (_, _) =>
            {
                using var dlg = new FrmTransactionDialog("RECETTE");
                if (dlg.ShowDialog(TopLevelControl as IWin32Window ?? this) == DialogResult.OK) RefreshData();
            };
            tip.SetToolTip(btnQuickRec, "Enregistrer rapidement une recette (Ctrl+N)");

            var btnQuickDep = Theme.CreatePrimaryButton("− Dépense", 108, 32);
            btnQuickDep.BackColor = Theme.DangerRed;
            btnQuickDep.Margin = new Padding(0, 2, 8, 0);
            btnQuickDep.Click += (_, _) =>
            {
                using var dlg = new FrmTransactionDialog("DEPENSE");
                if (dlg.ShowDialog(TopLevelControl as IWin32Window ?? this) == DialogResult.OK) RefreshData();
            };
            tip.SetToolTip(btnQuickDep, "Enregistrer rapidement une dépense");

            var btnQuickTrf = Theme.CreateSecondaryButton("Transfert", 96, 32);
            btnQuickTrf.Margin = new Padding(0, 2, 8, 0);
            btnQuickTrf.Click += (_, _) =>
            {
                using var dlg = new FrmTransfertDialog();
                if (dlg.ShowDialog(TopLevelControl as IWin32Window ?? this) == DialogResult.OK) RefreshData();
            };
            tip.SetToolTip(btnQuickTrf, "Transférer des fonds entre deux comptes (Ctrl+T)");

            var btnQuickPdf = Theme.CreateSecondaryButton("Rapports PDF", 114, 32);
            btnQuickPdf.Margin = new Padding(0, 2, 0, 0);
            btnQuickPdf.Click += (_, _) => (TopLevelControl as FrmMain)?.Navigate("rapports");
            tip.SetToolTip(btnQuickPdf, "Générer et imprimer les rapports officiels PDF (Ctrl+4)");

            filterBar.Controls.AddRange(new Control[] { btnQuickRec, btnQuickDep, btnQuickTrf, btnQuickPdf });
        }
        else
        {
            var btnQuickPdf = Theme.CreatePrimaryButton("Rapports PDF", 122, 32);
            btnQuickPdf.Margin = new Padding(0, 2, 0, 0);
            btnQuickPdf.Click += (_, _) => (TopLevelControl as FrmMain)?.Navigate("rapports");
            tip.SetToolTip(btnQuickPdf, "Consulter et exporter les rapports financiers PDF (Ctrl+4)");
            filterBar.Controls.Add(btnQuickPdf);
        }

        // 1. Ligne des 4 KPI cards adaptées au rôle
        var kpiTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 112,
            ColumnCount = 4,
            RowCount = 1,
            BackColor = Theme.PageBg,
            Padding = new Padding(0, 0, 0, 10)
        };
        for (int i = 0; i < 4; i++)
            kpiTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25F));

        string k1Title = UserSession.IsTresorier ? "Solde de ma structure" : "Solde global consolidé";
        string k4Title = UserSession.IsAdmin
            ? "Demandes à valider"
            : UserSession.IsTresorier
                ? "Comptes actifs"
                : "Résultat net global";
        Color k4Color = UserSession.IsAdmin ? Theme.GoldAccent : (UserSession.IsTresorier ? Theme.SidebarNavyLight : Theme.SuccessGreen);

        void GoTo(string key) => (TopLevelControl as FrmMain)?.Navigate(key);

        string k4Target = UserSession.IsAdmin ? "validations" : (UserSession.IsTresorier ? "comptes" : "rapports");

        var c1 = Theme.CreateKpiCard(k1Title, "0 FCFA", Theme.SidebarNavy, () => GoTo("comptes"));
        var c2 = Theme.CreateKpiCard("Recettes validées", "0 FCFA", Theme.SuccessGreen, () => GoTo("transactions"));
        var c3 = Theme.CreateKpiCard("Dépenses validées", "0 FCFA", Theme.DangerRed, () => GoTo("transactions"));
        var c4 = Theme.CreateKpiCard(k4Title, "0", k4Color, () => GoTo(k4Target));
        _lblKpi1 = (Label)c1.Controls["lblValue"]!;
        _lblKpi2 = (Label)c2.Controls["lblValue"]!;
        _lblKpi3 = (Label)c3.Controls["lblValue"]!;
        _lblKpi4 = (Label)c4.Controls["lblValue"]!;

        c1.Dock = c2.Dock = c3.Dock = c4.Dock = DockStyle.Fill;
        c1.Margin = new Padding(0, 0, 10, 0);
        c2.Margin = new Padding(5, 0, 10, 0);
        c3.Margin = new Padding(5, 0, 10, 0);
        c4.Margin = new Padding(5, 0, 0, 0);

        kpiTable.Controls.Add(c1, 0, 0);
        kpiTable.Controls.Add(c2, 1, 0);
        kpiTable.Controls.Add(c3, 2, 0);
        kpiTable.Controls.Add(c4, 3, 0);

        // 2. Zone milieu : Graphique d'évolution mensuelle + Donut analytique
        var midTable = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 215,
            ColumnCount = 2,
            RowCount = 1,
            BackColor = Theme.PageBg,
            Padding = new Padding(0, 4, 0, 12)
        };
        midTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 55F));
        midTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 45F));

        var cardLine = Theme.CreateCardPanel();
        cardLine.Dock = DockStyle.Fill;
        cardLine.Margin = new Padding(0, 0, 10, 0);
        var lblL = new Label { Text = "Évolution mensuelle (Recettes vs Dépenses)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 26 };
        _lineChartPanel.Dock = DockStyle.Fill;
        _lineChartPanel.BackColor = Color.White;
        _lineChartPanel.Cursor = Cursors.Hand;
        _lineChartPanel.Paint += DrawLineChart;
        _lineChartPanel.Click += (_, _) => GoTo("pilotage");
        tip.SetToolTip(_lineChartPanel, "Cliquez pour ouvrir les prévisions de trésorerie (Cash-Flow 6 mois)");
        cardLine.Controls.Add(_lineChartPanel);
        cardLine.Controls.Add(lblL);

        var cardDonut = Theme.CreateCardPanel();
        cardDonut.Dock = DockStyle.Fill;
        cardDonut.Margin = new Padding(6, 0, 0, 0);
        var lblD = new Label { Text = "Répartition analytique des dépenses", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 26 };
        _donutChartPanel.Dock = DockStyle.Fill;
        _donutChartPanel.BackColor = Color.White;
        _donutChartPanel.Cursor = Cursors.Hand;
        _donutChartPanel.Paint += DrawDonutChart;
        _donutChartPanel.Click += (_, _) => GoTo(UserSession.IsConsultant ? "rapports" : "centres");
        tip.SetToolTip(_donutChartPanel, "Cliquez pour voir le détail par centre analytique");
        cardDonut.Controls.Add(_donutChartPanel);
        cardDonut.Controls.Add(lblD);

        midTable.Controls.Add(cardLine, 0, 0);
        midTable.Controls.Add(cardDonut, 1, 0);

        // 3. Zone inférieure : Architecture dédiée par rôle + navigation interactive au double-clic
        Control bottomSection;
        if (UserSession.IsConsultant)
        {
            var cardAna = Theme.CreateCardPanel();
            cardAna.Dock = DockStyle.Fill;
            var lblAna = new Label { Text = "Synthèse financière par centre analytique (double-clic → Rapports)", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Dock = DockStyle.Top, Height = 30 };
            _gridBottomLeft.CellDoubleClick += (_, _) => GoTo("rapports");
            cardAna.Controls.Add(_gridBottomLeft);
            cardAna.Controls.Add(lblAna);
            bottomSection = cardAna;
        }
        else
        {
            var bottomTable = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 2,
                RowCount = 1,
                BackColor = Theme.PageBg
            };
            bottomTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
            bottomTable.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));

            var leftCard = Theme.CreateCardPanel();
            leftCard.Dock = DockStyle.Fill;
            leftCard.Margin = new Padding(0, 0, 10, 0);
            var lblLeft = new Label
            {
                Text = UserSession.IsAdmin ? "Demandes en attente (double-clic → Validations)" : "Mes comptes (double-clic → Comptes)",
                Font = Theme.FontSection,
                ForeColor = Theme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 30
            };
            _gridBottomLeft.CellDoubleClick += (_, _) => GoTo(UserSession.IsAdmin ? "validations" : "comptes");
            leftCard.Controls.Add(_gridBottomLeft);
            leftCard.Controls.Add(lblLeft);

            var rightCard = Theme.CreateCardPanel();
            rightCard.Dock = DockStyle.Fill;
            rightCard.Margin = new Padding(6, 0, 0, 0);
            var lblRight = new Label
            {
                Text = UserSession.IsAdmin ? "Dernières activités d'audit (double-clic → Audit)" : "Dernières opérations (double-clic → Transactions)",
                Font = Theme.FontSection,
                ForeColor = Theme.TextPrimary,
                Dock = DockStyle.Top,
                Height = 30
            };
            _gridBottomRight.CellDoubleClick += (_, _) => GoTo(UserSession.IsAdmin ? "audit" : "transactions");
            rightCard.Controls.Add(_gridBottomRight);
            rightCard.Controls.Add(lblRight);

            bottomTable.Controls.Add(leftCard, 0, 0);
            bottomTable.Controls.Add(rightCard, 1, 0);
            bottomSection = bottomTable;
        }

        Controls.Add(bottomSection);
        Controls.Add(midTable);
        Controls.Add(kpiTable);
        Controls.Add(filterBar);
    }

    private void InitStructureFilter()
    {
        var list = new List<Structure>();
        if (!UserSession.IsTresorier)
            list.Add(new Structure { Id = 0, Nom = "Toutes les structures (Vue consolidée)" });

        var all = _repo.GetStructures();
        if (UserSession.IsTresorier && UserSession.CurrentUser?.StructureId != null)
            list.AddRange(all.Where(s => s.Id == UserSession.CurrentUser.StructureId));
        else
            list.AddRange(all);

        _cboStructure.DataSource = list;
        _cboStructure.DisplayMember = "Nom";
        _cboStructure.ValueMember = "Id";
    }

    private void RefreshData()
    {
        int? scope = null;
        if (_cboStructure.SelectedItem is Structure st && st.Id > 0)
            scope = st.Id;

        var kpi = _repo.GetDashboardKpis(scope);
        _lblKpi1.Text = $"{kpi.SoldeGlobal:N0} FCFA";
        _lblKpi2.Text = $"{kpi.TotalRecettes:N0} FCFA";
        _lblKpi3.Text = $"{kpi.TotalDepenses:N0} FCFA";

        if (UserSession.IsAdmin)
        {
            var pending = _repo.GetDemandesValidation(onlyPending: true);
            _lblKpi4.Text = $"{pending.Count} en attente";

            _gridBottomLeft.DataSource = pending.Select(d => new
            {
                Date = d.DateDemande.ToString("dd/MM/yyyy"),
                Structure = d.StructureNom,
                Libellé = d.Libelle,
                Montant = $"-{d.Montant:N0} FCFA",
                Statut = d.Statut
            }).ToList();

            _gridBottomRight.DataSource = _repo.GetJournalActivites(12).Select(j => new
            {
                Heure = j.DateAction.ToString("dd/MM HH:mm"),
                Utilisateur = j.UtilisateurNom,
                Action = j.Action,
                Module = j.Module
            }).ToList();
        }
        else if (UserSession.IsTresorier)
        {
            var comptes = _repo.GetComptes(scope);
            _lblKpi4.Text = $"{comptes.Count}";

            _gridBottomLeft.DataSource = comptes.Select(c => new
            {
                Compte = c.NomCompte,
                Type = c.TypeCompte,
                Solde_Actuel = $"{c.SoldeActuel:N0} {c.Devise}",
                Statut = c.EstActif ? "Actif" : "Archivé"
            }).ToList();

            _gridBottomRight.DataSource = _repo.GetTransactions(null, scope, 12).Select(t => new
            {
                Date = t.DateOperation.ToString("dd/MM/yyyy"),
                Libellé = t.Libelle,
                Montant = (t.TypeTransaction is "RECETTE" or "TRANSFERT_ENTRANT" ? "+" : "-") + $"{t.Montant:N0} FCFA",
                Statut = t.Statut
            }).ToList();
        }
        else // Consultant
        {
            var net = kpi.TotalRecettes - kpi.TotalDepenses;
            _lblKpi4.Text = (net >= 0 ? "+" : "") + $"{net:N0} FCFA";
        }

        _analytique = _repo.GetSyntheseAnalytique();
        if (scope.HasValue && _cboStructure.SelectedItem is Structure selected)
            _analytique = _analytique.Where(a => a.StructureNom == selected.Nom).ToList();

        if (UserSession.IsConsultant)
        {
            _gridBottomLeft.DataSource = _analytique.Select(s => new
            {
                Code = s.CentreCode,
                Centre_Analytique = s.CentreLibelle,
                Structure = s.StructureNom,
                Recettes = $"+{s.TotalRecettes:N0} FCFA",
                Dépenses = $"-{s.TotalDepenses:N0} FCFA",
                Résultat_Net = (s.ResultatNet >= 0 ? "+" : "") + $"{s.ResultatNet:N0} FCFA"
            }).ToList();
        }

        _evolution = _repo.GetMonthlyEvolution(scope);
        _lineChartPanel.Invalidate();
        _donutChartPanel.Invalidate();
    }

    private void DrawLineChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var w = _lineChartPanel.Width;
        var h = _lineChartPanel.Height;

        if (_evolution.Count == 0)
        {
            using var br = new SolidBrush(Theme.TextMuted);
            g.DrawString("Aucune donnée mensuelle validée.", Theme.FontBody, br, 16, 60);
            return;
        }

        int padL = 44, padR = 20, padT = 18, padB = 34;
        int plotW = Math.Max(40, w - padL - padR);
        int plotH = Math.Max(40, h - padT - padB);

        decimal maxVal = Math.Max(1m, _evolution.Max(p => Math.Max(p.Recettes, p.Depenses)));

        using var gridPen = new Pen(Color.FromArgb(234, 239, 244), 1);
        for (int i = 0; i <= 3; i++)
        {
            int gy = padT + (plotH * i / 3);
            g.DrawLine(gridPen, padL, gy, padL + plotW, gy);
        }

        var ptsRec = new List<PointF>();
        var ptsDep = new List<PointF>();
        for (int i = 0; i < _evolution.Count; i++)
        {
            float x = _evolution.Count == 1 ? padL + plotW / 2f : padL + (plotW * i / (float)(_evolution.Count - 1));
            float yRec = padT + plotH - (float)(_evolution[i].Recettes / maxVal) * plotH;
            float yDep = padT + plotH - (float)(_evolution[i].Depenses / maxVal) * plotH;
            ptsRec.Add(new PointF(x, yRec));
            ptsDep.Add(new PointF(x, yDep));

            using var lblBr = new SolidBrush(Theme.TextMuted);
            g.DrawString(_evolution[i].Mois, new Font("Segoe UI", 8F), lblBr, x - 20, padT + plotH + 6);
        }

        if (ptsRec.Count > 1)
        {
            var areaRec = new List<PointF>(ptsRec)
            {
                new(ptsRec[^1].X, padT + plotH),
                new(ptsRec[0].X, padT + plotH)
            };
            using var areaRecBrush = new LinearGradientBrush(new Rectangle(padL, padT, plotW, plotH),
                Color.FromArgb(65, Theme.SuccessGreen), Color.FromArgb(5, Theme.SuccessGreen), LinearGradientMode.Vertical);
            g.FillPolygon(areaRecBrush, areaRec.ToArray());

            using var penRec = new Pen(Theme.SuccessGreen, 2.8f);
            using var penDep = new Pen(Theme.DangerRed, 2.8f);
            g.DrawLines(penRec, ptsRec.ToArray());
            g.DrawLines(penDep, ptsDep.ToArray());
        }

        foreach (var p in ptsRec)
        {
            g.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
            g.DrawEllipse(new Pen(Theme.SuccessGreen, 2.5f), p.X - 4, p.Y - 4, 8, 8);
        }
        foreach (var p in ptsDep)
        {
            g.FillEllipse(Brushes.White, p.X - 5, p.Y - 5, 10, 10);
            g.DrawEllipse(new Pen(Theme.DangerRed, 2.5f), p.X - 4, p.Y - 4, 8, 8);
        }

        g.FillRectangle(new SolidBrush(Theme.SuccessGreen), padL, 2, 10, 10);
        g.DrawString("Recettes", new Font("Segoe UI", 8F), new SolidBrush(Theme.TextPrimary), padL + 14, 0);
        g.FillRectangle(new SolidBrush(Theme.DangerRed), padL + 85, 2, 10, 10);
        g.DrawString("Dépenses", new Font("Segoe UI", 8F), new SolidBrush(Theme.TextPrimary), padL + 99, 0);
    }

    private void DrawDonutChart(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;

        var items = _analytique
            .GroupBy(x => x.CentreLibelle)
            .Select(grp => new { Label = grp.Key, Value = grp.Sum(x => x.TotalDepenses) })
            .Where(x => x.Value > 0)
            .OrderByDescending(x => x.Value)
            .ToList();

        if (items.Count == 0)
        {
            TextRenderer.DrawText(g, "Aucune dépense imputée sur ce périmètre.", Theme.FontBody,
                new Rectangle(12, 50, Math.Max(100, _donutChartPanel.Width - 24), 40), Theme.TextMuted,
                TextFormatFlags.Left | TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix);
            return;
        }

        Color[] palette =
        {
            Theme.SidebarNavy, Theme.GoldAccent, Theme.SuccessGreen,
            Color.FromArgb(107, 142, 189), Theme.DangerRed, Color.FromArgb(142, 114, 179)
        };

        // Regroupe au-delà de 5 postes en "Autres centres" pour éviter tout débordement vertical
        if (items.Count > 5)
        {
            var top4 = items.Take(4).ToList();
            top4.Add(new { Label = "Autres centres", Value = items.Skip(4).Sum(x => x.Value) });
            items = top4;
        }

        int panelW = _donutChartPanel.Width;
        int panelH = _donutChartPanel.Height;
        if (panelW < 40 || panelH < 40) return;
        int diameter = Math.Clamp(Math.Min(panelH - 16, panelW / 3), 92, 134);
        int donutX = 12;
        int donutY = Math.Max(6, (panelH - diameter) / 2);

        decimal total = items.Sum(i => i.Value);
        var rect = new Rectangle(donutX, donutY, diameter, diameter);
        float startAngle = -90f;

        for (int i = 0; i < items.Count; i++)
        {
            float sweep = (float)(items[i].Value / total) * 360f;
            using var br = new SolidBrush(palette[i % palette.Length]);
            g.FillPie(br, rect, startAngle, sweep);
            startAngle += sweep;
        }

        // Trou central proportionnel avec total compact centré
        int hole = (int)(diameter * 0.54f);
        int holeX = donutX + (diameter - hole) / 2;
        int holeY = donutY + (diameter - hole) / 2;
        var holeRect = new Rectangle(holeX, holeY, hole, hole);
        g.FillEllipse(Brushes.White, holeRect);

        string compactTotal = total >= 1_000_000m
            ? $"{total / 1_000_000m:0.#} M"
            : total >= 1_000m ? $"{total / 1_000m:0} k" : $"{total:0}";
        TextRenderer.DrawText(g, compactTotal, Theme.UiFont(9.5F, FontStyle.Bold),
            new Rectangle(holeX, holeY + hole / 2 - 16, hole, 18), Theme.TextPrimary,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);
        TextRenderer.DrawText(g, "FCFA", Theme.UiFont(7F, FontStyle.Bold),
            new Rectangle(holeX, holeY + hole / 2 + 1, hole, 14), Theme.TextMuted,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

        // Légende responsive : nom tronqué proprement (EndEllipsis) + pourcentage aligné à droite
        int legX = donutX + diameter + 16;
        int legRight = panelW - 8;
        int pctW = 48;
        int nameW = Math.Max(60, legRight - legX - 16 - pctW);

        int rowH = Math.Min(26, Math.Max(20, (panelH - 12) / items.Count));
        int ly = Math.Max(6, (panelH - items.Count * rowH) / 2);

        var fName = Theme.UiFont(8.8F, FontStyle.Bold);
        var fPct = Theme.UiFont(8.8F, FontStyle.Bold);

        for (int i = 0; i < items.Count; i++)
        {
            using var br = new SolidBrush(palette[i % palette.Length]);
            g.FillEllipse(br, legX, ly + (rowH - 9) / 2, 9, 9);

            var pct = Math.Round(items[i].Value / total * 100m, 1);
            TextRenderer.DrawText(g, items[i].Label, fName,
                new Rectangle(legX + 15, ly, nameW, rowH), Theme.TextPrimary,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix);

            TextRenderer.DrawText(g, $"{pct:0.#}%", fPct,
                new Rectangle(legRight - pctW, ly, pctW, rowH), Theme.SidebarNavy,
                TextFormatFlags.Right | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPrefix);

            ly += rowH;
        }
    }
}

using System.Drawing;
using System.Drawing.Drawing2D;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Models;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Dialogs;

/// <summary>
/// Fenêtre moderne sans bordure pour la saisie Recette / Dépense avec Multi-Ventilation Analytique.
/// </summary>
public partial class FrmTransactionDialog : Form
{
    private readonly TreasuryRepository _repo = new();
    private string _selectedType = "RECETTE";

    private readonly Button _btnTypeRecette = new();
    private readonly Button _btnTypeDepense = new();
    private readonly TextBox _txtLibelle = new();
    private readonly NumericUpDown _numMontant = new();
    private readonly ComboBox _cboCompte = new();
    private readonly ComboBox _cboCategorie = new();
    private readonly ComboBox _cboMode = new();
    private readonly DateTimePicker _dtpDate = new();
    private readonly TextBox _txtJustificatif = new();
    private readonly TextBox _txtObservations = new();

    private readonly ComboBox _cboCentre = new();
    private readonly NumericUpDown _numMontantImpute = new();
    private readonly DataGridView _gridVentilation = Theme.CreateStyledGrid();
    private readonly Label _lblReste = new();
    private readonly SmoothPanel _ventProgressBar = new();
    private readonly List<ImputationAnalytique> _imputations = new();

    private List<CompteTresorerie> _comptes = new();
    private List<CentreAnalytique> _centres = new();
    private float _ventPercent;

    private bool _dragging;
    private Point _dragStart;

    public FrmTransactionDialog() : this("RECETTE") { }

    public FrmTransactionDialog(string initialType)
    {
        InitializeComponent();
        if (System.ComponentModel.LicenseManager.UsageMode == System.ComponentModel.LicenseUsageMode.Designtime)
            return;

        Controls.Clear();
        Text = "IIT FINCORE - Nouvelle opération";
        Size = new Size(768, 760);
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.None;
        BackColor = Theme.PageBg;
        DoubleBuffered = true;

        BuildLayout();
        Theme.ApplyRoundedRegion(this, 16);
        LoadReferenceData(initialType);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
        using var path = Theme.CreateRoundedPath(new Rectangle(0, 0, Width - 1, Height - 1), 16);
        using var pen = new Pen(Color.FromArgb(190, 204, 222), 1.5f);
        e.Graphics.DrawPath(pen, path);
    }

    private void BuildLayout()
    {
        // ==================== EN-TÊTE ROYAL SAPPHIRE DÉPLAÇABLE + BOUTON ✕ ====================
        var header = Theme.CreateDialogHeader("Nouvelle opération de trésorerie", "Enregistrement Recette / Dépense avec ventilation multi-centres");
        header.Height = 72;
        EnableDrag(header);

        var btnClose = new Button
        {
            Text = "✕",
            Size = new Size(34, 34),
            Location = new Point(Width - 48, 18),
            FlatStyle = FlatStyle.Flat,
            Font = new Font("Segoe UI", 10.5F, FontStyle.Bold),
            ForeColor = Color.FromArgb(200, 218, 240),
            BackColor = Color.Transparent,
            Cursor = Cursors.Hand
        };
        btnClose.FlatAppearance.BorderSize = 0;
        btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 255, 255, 255);
        btnClose.Click += (_, _) => Close();
        Theme.ApplyRoundedRegion(btnClose, 8);
        header.Controls.Add(btnClose);

        // ==================== PIED DE PAGE FIXE ====================
        var footer = new Panel { Dock = DockStyle.Bottom, Height = 66, BackColor = Color.White, Padding = new Padding(22, 13, 24, 13) };
        footer.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Theme.BorderColor, 1), 0, 0, footer.Width, 0);

        var btnSave = Theme.CreatePrimaryButton("Enregistrer l'opération", 210, 40);
        btnSave.Dock = DockStyle.Right;
        btnSave.Click += (_, _) => SaveTransaction();

        var footerSpacer = new Panel { Dock = DockStyle.Right, Width = 12, BackColor = Color.White };

        var btnCancel = Theme.CreateSecondaryButton("Annuler", 115, 40);
        btnCancel.Dock = DockStyle.Right;
        btnCancel.Click += (_, _) => Close();

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(footerSpacer);
        footer.Controls.Add(btnSave);

        // ==================== CORPS PRINCIPAL ====================
        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 14, 18, 14), BackColor = Theme.PageBg };
        var card = Theme.CreateCardPanel(new Padding(24, 18, 24, 18));
        card.Dock = DockStyle.Fill;

        int leftX = 24, rightX = 372, colW = 322;
        int y = 16;

        // Ligne 1 : Sélecteur segmenté Recette / Dépense + Date
        AddLabel(card, "Nature de l'opération", leftX, y);
        AddLabel(card, "Date de l'opération", rightX, y);
        y += 22;

        // Sélecteur segmenté moderne (2 boutons pilules)
        var toggleBg = new SmoothPanel
        {
            Bounds = new Rectangle(leftX, y, colW, 34),
            BackColor = Color.FromArgb(238, 243, 249)
        };
        Theme.ApplyRoundedRegion(toggleBg, 9);

        _btnTypeRecette.Text = "+ Recette";
        _btnTypeRecette.Bounds = new Rectangle(3, 3, 156, 28);
        _btnTypeRecette.FlatStyle = FlatStyle.Flat;
        _btnTypeRecette.FlatAppearance.BorderSize = 0;
        _btnTypeRecette.Font = Theme.FontBodyBold;
        _btnTypeRecette.Cursor = Cursors.Hand;
        Theme.ApplyRoundedRegion(_btnTypeRecette, 7);
        _btnTypeRecette.Click += (_, _) => SelectOperationType("RECETTE");

        _btnTypeDepense.Text = "− Dépense";
        _btnTypeDepense.Bounds = new Rectangle(163, 3, 156, 28);
        _btnTypeDepense.FlatStyle = FlatStyle.Flat;
        _btnTypeDepense.FlatAppearance.BorderSize = 0;
        _btnTypeDepense.Font = Theme.FontBodyBold;
        _btnTypeDepense.Cursor = Cursors.Hand;
        Theme.ApplyRoundedRegion(_btnTypeDepense, 7);
        _btnTypeDepense.Click += (_, _) => SelectOperationType("DEPENSE");

        toggleBg.Controls.Add(_btnTypeRecette);
        toggleBg.Controls.Add(_btnTypeDepense);

        _dtpDate.SetBounds(rightX, y + 2, colW, 30);
        _dtpDate.Format = DateTimePickerFormat.Short;
        _dtpDate.Font = Theme.FontBody;
        y += 46;

        // Ligne 2 : Libellé + Montant
        AddLabel(card, "Libellé de l'opération", leftX, y);
        AddLabel(card, "Montant total (FCFA)", rightX, y);
        y += 22;

        _txtLibelle.SetBounds(leftX, y, colW, 28);
        _txtLibelle.Font = Theme.FontBody;
        _txtLibelle.PlaceholderText = "Ex : Achat matériel, Cotisation annuelle...";

        _numMontant.SetBounds(rightX, y, colW, 28);
        _numMontant.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        _numMontant.Maximum = 1_000_000_000;
        _numMontant.ThousandsSeparator = true;
        _numMontant.Value = 50000;
        _numMontant.ValueChanged += (_, _) => UpdateVentilationStatus();
        y += 42;

        // Ligne 3 : Compte + Catégorie
        AddLabel(card, "Compte de trésorerie", leftX, y);
        AddLabel(card, "Catégorie", rightX, y);
        y += 22;

        _cboCompte.SetBounds(leftX, y, colW, 30);
        _cboCompte.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboCompte.Font = Theme.FontBody;

        _cboCategorie.SetBounds(rightX, y, colW, 30);
        _cboCategorie.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboCategorie.Font = Theme.FontBody;
        y += 42;

        // Ligne 4 : Mode paiement + Justificatif
        AddLabel(card, "Mode de paiement", leftX, y);
        AddLabel(card, "Pièce justificative (optionnel)", rightX, y);
        y += 22;

        _cboMode.SetBounds(leftX, y, colW, 30);
        _cboMode.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboMode.Font = Theme.FontBody;
        _cboMode.Items.AddRange(new object[] { "ESPECES", "VIREMENT", "CHEQUE", "MOBILE_MONEY", "CARTE" });
        _cboMode.SelectedIndex = 0;

        _txtJustificatif.SetBounds(rightX, y, 212, 28);
        _txtJustificatif.Font = Theme.FontBody;
        _txtJustificatif.PlaceholderText = "Facture / reçu...";

        var btnBrowse = Theme.CreateSecondaryButton("Parcourir", 102, 30);
        btnBrowse.Location = new Point(rightX + 220, y - 1);
        btnBrowse.Click += (_, _) =>
        {
            using var ofd = new OpenFileDialog { Title = "Choisir une pièce justificative" };
            if (ofd.ShowDialog() == DialogResult.OK)
                _txtJustificatif.Text = ofd.FileName;
        };
        y += 44;

        // ==================== BLOC VENTILATION ANALYTIQUE AVEC BARRE DE PROGRESSION ====================
        var ventBox = new SmoothPanel
        {
            Location = new Point(leftX, y),
            Size = new Size(670, 240),
            BackColor = Color.FromArgb(247, 250, 254)
        };
        ventBox.Paint += (_, e) =>
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using var p = Theme.CreateRoundedPath(new Rectangle(0, 0, ventBox.Width - 1, ventBox.Height - 1), 11);
            e.Graphics.DrawPath(new Pen(Theme.BorderColor, 1), p);
        };

        var lblVent = new Label
        {
            Text = "Ventilation sur centres d'imputation analytiques",
            Font = Theme.FontBodyBold,
            ForeColor = Theme.SidebarNavy,
            Location = new Point(14, 12),
            AutoSize = true
        };

        _cboCentre.SetBounds(14, 38, 240, 28);
        _cboCentre.DropDownStyle = ComboBoxStyle.DropDownList;
        _cboCentre.Font = Theme.FontBody;

        _numMontantImpute.SetBounds(262, 38, 142, 28);
        _numMontantImpute.Font = Theme.FontBody;
        _numMontantImpute.Maximum = 1_000_000_000;
        _numMontantImpute.ThousandsSeparator = true;

        var btnAddImp = Theme.CreatePrimaryButton("+ Ajouter", 118, 30);
        btnAddImp.Location = new Point(414, 36);
        btnAddImp.Click += (_, _) => AddImputationLine();

        var btnClearImp = Theme.CreateSecondaryButton("Effacer tout", 116, 30);
        btnClearImp.Location = new Point(540, 36);
        btnClearImp.Click += (_, _) =>
        {
            _imputations.Clear();
            RefreshVentilationGrid();
        };

        // Cadre de grille doux sans bordure noire dure
        var gridWrap = new SmoothPanel
        {
            Location = new Point(14, 76),
            Size = new Size(642, 114),
            BackColor = Color.White,
            Padding = new Padding(1)
        };
        gridWrap.Paint += (_, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawRectangle(pen, 0, 0, gridWrap.Width - 1, gridWrap.Height - 1);
        };
        _gridVentilation.Columns.Add("Centre", "Centre analytique");
        _gridVentilation.Columns.Add("Montant", "Montant imputé (FCFA)");
        _gridVentilation.Columns.Add("Pct", "Part (%)");
        gridWrap.Controls.Add(_gridVentilation);

        // Barre de progression visuelle de la ventilation (0 % -> 100 %)
        _ventProgressBar.Bounds = new Rectangle(14, 198, 642, 8);
        _ventProgressBar.BackColor = Color.FromArgb(247, 250, 254);
        _ventProgressBar.Paint += (_, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            var track = new Rectangle(0, 0, _ventProgressBar.Width - 1, _ventProgressBar.Height - 1);
            using var tp = Theme.CreateRoundedPath(track, 4);
            g.FillPath(new SolidBrush(Color.FromArgb(224, 232, 242)), tp);

            if (_ventPercent > 0)
            {
                int fw = Math.Max(8, (int)(track.Width * Math.Min(1f, _ventPercent / 100f)));
                using var fp = Theme.CreateRoundedPath(new Rectangle(0, 0, fw, track.Height), 4);
                var col = _ventPercent >= 99.9f ? Theme.SuccessGreen : Theme.GoldAccent;
                g.FillPath(new SolidBrush(col), fp);
            }
        };

        _lblReste.SetBounds(14, 212, 642, 20);
        _lblReste.Font = new Font("Segoe UI", 8.8F, FontStyle.Bold);

        ventBox.Controls.AddRange(new Control[] { lblVent, _cboCentre, _numMontantImpute, btnAddImp, btnClearImp, gridWrap, _ventProgressBar, _lblReste });
        y += 254;

        // Observations
        AddLabel(card, "Observations / Motif (requis si la dépense passe en validation)", leftX, y);
        y += 22;
        _txtObservations.SetBounds(leftX, y, 670, 30);
        _txtObservations.Font = Theme.FontBody;
        _txtObservations.PlaceholderText = "Précisez le motif ou la référence interne...";

        card.Controls.AddRange(new Control[]
        {
            toggleBg, _dtpDate, _txtLibelle, _numMontant,
            _cboCompte, _cboCategorie, _cboMode, _txtJustificatif, btnBrowse,
            ventBox, _txtObservations
        });

        wrap.Controls.Add(card);
        Controls.Add(wrap);
        Controls.Add(footer);
        Controls.Add(header);
        Theme.UpgradeAllTextBoxes(this);
    }

    private void SelectOperationType(string type)
    {
        _selectedType = type == "DEPENSE" ? "DEPENSE" : "RECETTE";

        bool isRec = _selectedType == "RECETTE";
        _btnTypeRecette.BackColor = isRec ? Theme.SuccessGreen : Color.Transparent;
        _btnTypeRecette.ForeColor = isRec ? Color.White : Theme.TextMuted;

        _btnTypeDepense.BackColor = !isRec ? Theme.DangerRed : Color.Transparent;
        _btnTypeDepense.ForeColor = !isRec ? Color.White : Theme.TextMuted;

        LoadCategories();
    }

    private void EnableDrag(Control ctrl)
    {
        ctrl.MouseDown += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) { _dragging = true; _dragStart = e.Location; }
        };
        ctrl.MouseMove += (_, e) =>
        {
            if (_dragging) Location = new Point(Location.X + e.X - _dragStart.X, Location.Y + e.Y - _dragStart.Y);
        };
        ctrl.MouseUp += (_, _) => _dragging = false;
    }

    private static void AddLabel(Control parent, string text, int x, int y)
    {
        parent.Controls.Add(new Label
        {
            Text = text,
            Font = new Font("Segoe UI", 8.8F, FontStyle.Bold),
            ForeColor = Theme.TextMuted,
            Location = new Point(x, y),
            AutoSize = true
        });
    }

    private void LoadReferenceData(string initialType)
    {
        int? scopeStructure = UserSession.IsTresorier ? UserSession.CurrentUser?.StructureId : null;
        _comptes = _repo.GetComptes(scopeStructure);
        _cboCompte.DataSource = _comptes;

        _centres = _repo.GetCentresAnalytiques();
        _cboCentre.DataSource = _centres;

        SelectOperationType(initialType);
        UpdateVentilationStatus();
    }

    private void LoadCategories()
    {
        var cats = _repo.GetCategories(_selectedType);
        _cboCategorie.DataSource = cats;
        _cboCategorie.DisplayMember = "Libelle";
        _cboCategorie.ValueMember = "Id";
    }

    private void AddImputationLine()
    {
        if (_cboCentre.SelectedItem is not CentreAnalytique centre)
            return;

        var montant = _numMontantImpute.Value;
        if (montant <= 0)
        {
            MessageBox.Show("Le montant imputé doit être supérieur à 0.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_imputations.Any(i => i.CentreAnalytiqueId == centre.Id))
        {
            MessageBox.Show("Ce centre analytique est déjà présent dans la ventilation.", "Doublon", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var totalActuel = _imputations.Sum(i => i.MontantImpute);
        if (totalActuel + montant > _numMontant.Value)
        {
            MessageBox.Show("La somme ventilée ne peut pas dépasser le montant total de l'opération.", "Dépassement", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _imputations.Add(new ImputationAnalytique
        {
            CentreAnalytiqueId = centre.Id,
            CentreLibelle = centre.ToString(),
            MontantImpute = montant,
            Pourcentage = _numMontant.Value > 0 ? Math.Round((montant / _numMontant.Value) * 100m, 2) : 0
        });

        RefreshVentilationGrid();
    }

    private void RefreshVentilationGrid()
    {
        _gridVentilation.Rows.Clear();
        foreach (var imp in _imputations)
        {
            imp.Pourcentage = _numMontant.Value > 0 ? Math.Round((imp.MontantImpute / _numMontant.Value) * 100m, 2) : 0;
            _gridVentilation.Rows.Add(imp.CentreLibelle, $"{imp.MontantImpute:N0}", $"{imp.Pourcentage:N2} %");
        }
        UpdateVentilationStatus();
    }

    private void UpdateVentilationStatus()
    {
        var total = _imputations.Sum(i => i.MontantImpute);
        var reste = _numMontant.Value - total;
        _numMontantImpute.Value = Math.Max(0, reste);
        _ventPercent = _numMontant.Value > 0 ? (float)(total / _numMontant.Value * 100m) : 0f;
        _ventProgressBar.Invalidate();

        if (reste == 0 && _numMontant.Value > 0)
        {
            _lblReste.Text = $"Ventilation complète : {total:N0} FCFA (100 %)";
            _lblReste.ForeColor = Theme.SuccessGreen;
        }
        else
        {
            _lblReste.Text = $"Reste à ventiler : {reste:N0} FCFA sur {_numMontant.Value:N0} FCFA ({_ventPercent:N0} % affecté)";
            _lblReste.ForeColor = Theme.DangerRed;
        }
    }

    private void SaveTransaction()
    {
        if (string.IsNullOrWhiteSpace(_txtLibelle.Text))
        {
            MessageBox.Show("Veuillez saisir un libellé.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_cboCompte.SelectedItem is not CompteTresorerie compte)
        {
            MessageBox.Show("Veuillez sélectionner un compte de trésorerie.", "Champ requis", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (_imputations.Count == 0 && _cboCentre.SelectedItem is CentreAnalytique defaultCentre)
        {
            _imputations.Add(new ImputationAnalytique
            {
                CentreAnalytiqueId = defaultCentre.Id,
                CentreLibelle = defaultCentre.ToString(),
                MontantImpute = _numMontant.Value,
                Pourcentage = 100m
            });
        }

        try
        {
            if (_selectedType == "DEPENSE")
            {
                var budgets = _repo.GetSuiviBudgetaire()
                    .Where(b => b.StructureNom == compte.StructureNom && b.Statut == "ACTIF")
                    .ToList();

                var warnings = new List<string>();
                foreach (var imp in _imputations)
                {
                    var matching = budgets.FirstOrDefault(b =>
                        imp.CentreLibelle.Contains(b.CentreAnalytique, StringComparison.OrdinalIgnoreCase));
                    if (matching != null && (matching.MontantRealise + imp.MontantImpute) > matching.MontantPrevu)
                    {
                        var depassement = (matching.MontantRealise + imp.MontantImpute) - matching.MontantPrevu;
                        warnings.Add($"• Budget « {matching.Intitule} » ({matching.CentreAnalytique}) dépassé de {depassement:N0} FCFA");
                    }
                }

                if (warnings.Count > 0)
                {
                    var confirm = MessageBox.Show(
                        "ALERTE DÉPASSEMENT BUDGÉTAIRE :\n\n" + string.Join("\n", warnings) + "\n\nVoulez-vous tout de même enregistrer cette dépense ?",
                        "Contrôle budgétaire IIT FINCORE",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Warning);
                    if (confirm != DialogResult.Yes)
                        return;
                }
            }

            var trx = new TransactionItem
            {
                DateOperation = _dtpDate.Value.Date,
                Libelle = _txtLibelle.Text.Trim(),
                TypeTransaction = _selectedType,
                Montant = _numMontant.Value,
                ModePaiement = _cboMode.SelectedItem?.ToString() ?? "ESPECES",
                JustificatifPath = string.IsNullOrWhiteSpace(_txtJustificatif.Text) ? null : _txtJustificatif.Text.Trim(),
                CompteId = compte.Id,
                StructureId = compte.StructureId,
                CategorieId = _cboCategorie.SelectedValue is int catId ? catId : null,
                CreePar = UserSession.CurrentUser!.Id,
                Observations = _txtObservations.Text.Trim()
            };

            _repo.CreateTransactionWithVentilation(trx, _imputations, UserSession.IsAdmin, _txtObservations.Text.Trim());

            var msg = trx.Statut == "EN_ATTENTE"
                ? "Dépense enregistrée et soumise au circuit de validation (en attente d'approbation par l'Administrateur)."
                : "Opération enregistrée et validée avec succès ! Le solde du compte a été mis à jour.";

            MessageBox.Show(msg, "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur d'enregistrement", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

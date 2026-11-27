namespace IIT.FinCore.WinForms.UI;

partial class FrmMain
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Panel pnlDesignSidebar = null!;
    private System.Windows.Forms.Panel pnlDesignTopbar = null!;
    private System.Windows.Forms.Panel pnlDesignContent = null!;
    private System.Windows.Forms.Label lblDesignBrand = null!;
    private System.Windows.Forms.Label lblDesignRoleBadge = null!;
    private System.Windows.Forms.Label lblDesignNavDash = null!;
    private System.Windows.Forms.Label lblDesignNavComptes = null!;
    private System.Windows.Forms.Label lblDesignNavTrx = null!;
    private System.Windows.Forms.Label lblDesignNavBudgets = null!;
    private System.Windows.Forms.Label lblDesignNavRapports = null!;
    private System.Windows.Forms.Label lblDesignPageTitle = null!;
    private System.Windows.Forms.Label lblDesignPageSub = null!;
    private System.Windows.Forms.Button btnDesignQuickOp = null!;
    private System.Windows.Forms.Button btnDesignAlertes = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.pnlDesignSidebar = new System.Windows.Forms.Panel();
        this.lblDesignNavRapports = new System.Windows.Forms.Label();
        this.lblDesignNavBudgets = new System.Windows.Forms.Label();
        this.lblDesignNavTrx = new System.Windows.Forms.Label();
        this.lblDesignNavComptes = new System.Windows.Forms.Label();
        this.lblDesignNavDash = new System.Windows.Forms.Label();
        this.lblDesignRoleBadge = new System.Windows.Forms.Label();
        this.lblDesignBrand = new System.Windows.Forms.Label();
        this.pnlDesignTopbar = new System.Windows.Forms.Panel();
        this.btnDesignAlertes = new System.Windows.Forms.Button();
        this.btnDesignQuickOp = new System.Windows.Forms.Button();
        this.lblDesignPageSub = new System.Windows.Forms.Label();
        this.lblDesignPageTitle = new System.Windows.Forms.Label();
        this.pnlDesignContent = new System.Windows.Forms.Panel();
        this.pnlDesignSidebar.SuspendLayout();
        this.pnlDesignTopbar.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlDesignSidebar (Royal Sapphire)
        //
        this.pnlDesignSidebar.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignNavRapports);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignNavBudgets);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignNavTrx);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignNavComptes);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignNavDash);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignRoleBadge);
        this.pnlDesignSidebar.Controls.Add(this.lblDesignBrand);
        this.pnlDesignSidebar.Dock = System.Windows.Forms.DockStyle.Left;
        this.pnlDesignSidebar.Location = new System.Drawing.Point(0, 0);
        this.pnlDesignSidebar.Name = "pnlDesignSidebar";
        this.pnlDesignSidebar.Size = new System.Drawing.Size(258, 800);
        this.pnlDesignSidebar.TabIndex = 0;
        //
        // lblDesignBrand
        //
        this.lblDesignBrand.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
        this.lblDesignBrand.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.lblDesignBrand.Location = new System.Drawing.Point(20, 20);
        this.lblDesignBrand.Name = "lblDesignBrand";
        this.lblDesignBrand.Size = new System.Drawing.Size(218, 32);
        this.lblDesignBrand.TabIndex = 0;
        this.lblDesignBrand.Text = "IIT FINCORE";
        //
        // lblDesignRoleBadge
        //
        this.lblDesignRoleBadge.BackColor = System.Drawing.Color.FromArgb(18, 52, 98);
        this.lblDesignRoleBadge.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignRoleBadge.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.lblDesignRoleBadge.Location = new System.Drawing.Point(16, 70);
        this.lblDesignRoleBadge.Name = "lblDesignRoleBadge";
        this.lblDesignRoleBadge.Size = new System.Drawing.Size(226, 42);
        this.lblDesignRoleBadge.TabIndex = 1;
        this.lblDesignRoleBadge.Text = "  ESPACE ADMINISTRATEUR";
        this.lblDesignRoleBadge.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDesignNavDash
        //
        this.lblDesignNavDash.BackColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.lblDesignNavDash.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignNavDash.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.lblDesignNavDash.Location = new System.Drawing.Point(16, 150);
        this.lblDesignNavDash.Name = "lblDesignNavDash";
        this.lblDesignNavDash.Size = new System.Drawing.Size(226, 34);
        this.lblDesignNavDash.TabIndex = 2;
        this.lblDesignNavDash.Text = "   Tableau de bord";
        this.lblDesignNavDash.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDesignNavComptes
        //
        this.lblDesignNavComptes.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignNavComptes.ForeColor = System.Drawing.Color.White;
        this.lblDesignNavComptes.Location = new System.Drawing.Point(16, 194);
        this.lblDesignNavComptes.Name = "lblDesignNavComptes";
        this.lblDesignNavComptes.Size = new System.Drawing.Size(226, 34);
        this.lblDesignNavComptes.TabIndex = 3;
        this.lblDesignNavComptes.Text = "   Comptes de trésorerie";
        this.lblDesignNavComptes.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDesignNavTrx
        //
        this.lblDesignNavTrx.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignNavTrx.ForeColor = System.Drawing.Color.White;
        this.lblDesignNavTrx.Location = new System.Drawing.Point(16, 234);
        this.lblDesignNavTrx.Name = "lblDesignNavTrx";
        this.lblDesignNavTrx.Size = new System.Drawing.Size(226, 34);
        this.lblDesignNavTrx.TabIndex = 4;
        this.lblDesignNavTrx.Text = "   Opérations & Transferts";
        this.lblDesignNavTrx.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDesignNavBudgets
        //
        this.lblDesignNavBudgets.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignNavBudgets.ForeColor = System.Drawing.Color.White;
        this.lblDesignNavBudgets.Location = new System.Drawing.Point(16, 274);
        this.lblDesignNavBudgets.Name = "lblDesignNavBudgets";
        this.lblDesignNavBudgets.Size = new System.Drawing.Size(226, 34);
        this.lblDesignNavBudgets.TabIndex = 5;
        this.lblDesignNavBudgets.Text = "   Budgets & Alertes";
        this.lblDesignNavBudgets.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // lblDesignNavRapports
        //
        this.lblDesignNavRapports.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignNavRapports.ForeColor = System.Drawing.Color.White;
        this.lblDesignNavRapports.Location = new System.Drawing.Point(16, 314);
        this.lblDesignNavRapports.Name = "lblDesignNavRapports";
        this.lblDesignNavRapports.Size = new System.Drawing.Size(226, 34);
        this.lblDesignNavRapports.TabIndex = 6;
        this.lblDesignNavRapports.Text = "   Rapports & Export PDF";
        this.lblDesignNavRapports.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
        //
        // pnlDesignTopbar
        //
        this.pnlDesignTopbar.BackColor = System.Drawing.Color.White;
        this.pnlDesignTopbar.Controls.Add(this.btnDesignAlertes);
        this.pnlDesignTopbar.Controls.Add(this.btnDesignQuickOp);
        this.pnlDesignTopbar.Controls.Add(this.lblDesignPageSub);
        this.pnlDesignTopbar.Controls.Add(this.lblDesignPageTitle);
        this.pnlDesignTopbar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDesignTopbar.Location = new System.Drawing.Point(258, 0);
        this.pnlDesignTopbar.Name = "pnlDesignTopbar";
        this.pnlDesignTopbar.Size = new System.Drawing.Size(1082, 84);
        this.pnlDesignTopbar.TabIndex = 1;
        //
        // lblDesignPageTitle
        //
        this.lblDesignPageTitle.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.lblDesignPageTitle.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.lblDesignPageTitle.Location = new System.Drawing.Point(26, 14);
        this.lblDesignPageTitle.Name = "lblDesignPageTitle";
        this.lblDesignPageTitle.Size = new System.Drawing.Size(560, 32);
        this.lblDesignPageTitle.TabIndex = 0;
        this.lblDesignPageTitle.Text = "Tableau de bord - Administration globale";
        //
        // lblDesignPageSub
        //
        this.lblDesignPageSub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
        this.lblDesignPageSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
        this.lblDesignPageSub.Location = new System.Drawing.Point(28, 48);
        this.lblDesignPageSub.Name = "lblDesignPageSub";
        this.lblDesignPageSub.Size = new System.Drawing.Size(560, 22);
        this.lblDesignPageSub.TabIndex = 1;
        this.lblDesignPageSub.Text = "Vue consolidée multi-structures et pilotage financier IIT FINCORE";
        //
        // btnDesignQuickOp
        //
        this.btnDesignQuickOp.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnDesignQuickOp.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
        this.btnDesignQuickOp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDesignQuickOp.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnDesignQuickOp.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.btnDesignQuickOp.Location = new System.Drawing.Point(740, 22);
        this.btnDesignQuickOp.Name = "btnDesignQuickOp";
        this.btnDesignQuickOp.Size = new System.Drawing.Size(172, 38);
        this.btnDesignQuickOp.TabIndex = 2;
        this.btnDesignQuickOp.Text = "+ Nouvelle opération";
        this.btnDesignQuickOp.UseVisualStyleBackColor = false;
        //
        // btnDesignAlertes
        //
        this.btnDesignAlertes.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
        this.btnDesignAlertes.BackColor = System.Drawing.Color.FromArgb(247, 249, 253);
        this.btnDesignAlertes.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDesignAlertes.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnDesignAlertes.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.btnDesignAlertes.Location = new System.Drawing.Point(926, 22);
        this.btnDesignAlertes.Name = "btnDesignAlertes";
        this.btnDesignAlertes.Size = new System.Drawing.Size(110, 38);
        this.btnDesignAlertes.TabIndex = 3;
        this.btnDesignAlertes.Text = "Alertes";
        this.btnDesignAlertes.UseVisualStyleBackColor = false;
        //
        // pnlDesignContent
        //
        this.pnlDesignContent.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.pnlDesignContent.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlDesignContent.Location = new System.Drawing.Point(258, 84);
        this.pnlDesignContent.Name = "pnlDesignContent";
        this.pnlDesignContent.Size = new System.Drawing.Size(1082, 716);
        this.pnlDesignContent.TabIndex = 2;
        //
        // FrmMain
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.ClientSize = new System.Drawing.Size(1340, 800);
        this.Controls.Add(this.pnlDesignContent);
        this.Controls.Add(this.pnlDesignTopbar);
        this.Controls.Add(this.pnlDesignSidebar);
        this.MinimumSize = new System.Drawing.Size(1160, 720);
        this.Name = "FrmMain";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "IIT FINCORE - Espace Administrateur";
        this.pnlDesignSidebar.ResumeLayout(false);
        this.pnlDesignTopbar.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}

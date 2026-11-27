#nullable disable
namespace IIT.FinCore.WinForms.UI.Controls
{
    partial class UcDashboard
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignToolbar;
        private System.Windows.Forms.Label lblDesignModuleTitle;
        private System.Windows.Forms.Button btnDesignPrimary;
        private System.Windows.Forms.Button btnDesignSecondary;
        private System.Windows.Forms.Panel pnlDesignCard;
        private System.Windows.Forms.DataGridView dgvDesignPreview;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlDesignToolbar = new System.Windows.Forms.Panel();
            this.btnDesignSecondary = new System.Windows.Forms.Button();
            this.btnDesignPrimary = new System.Windows.Forms.Button();
            this.lblDesignModuleTitle = new System.Windows.Forms.Label();
            this.pnlDesignCard = new System.Windows.Forms.Panel();
            this.dgvDesignPreview = new System.Windows.Forms.DataGridView();
            this.pnlDesignToolbar.SuspendLayout();
            this.pnlDesignCard.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignPreview)).BeginInit();
            this.SuspendLayout();
            //
            // pnlDesignToolbar
            //
            this.pnlDesignToolbar.BackColor = System.Drawing.Color.White;
            this.pnlDesignToolbar.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDesignToolbar.Controls.Add(this.btnDesignSecondary);
            this.pnlDesignToolbar.Controls.Add(this.btnDesignPrimary);
            this.pnlDesignToolbar.Controls.Add(this.lblDesignModuleTitle);
            this.pnlDesignToolbar.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDesignToolbar.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignToolbar.Name = "pnlDesignToolbar";
            this.pnlDesignToolbar.Size = new System.Drawing.Size(1020, 64);
            this.pnlDesignToolbar.TabIndex = 0;
            //
            // lblDesignModuleTitle
            //
            this.lblDesignModuleTitle.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblDesignModuleTitle.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.lblDesignModuleTitle.Location = new System.Drawing.Point(16, 16);
            this.lblDesignModuleTitle.Name = "lblDesignModuleTitle";
            this.lblDesignModuleTitle.Size = new System.Drawing.Size(580, 30);
            this.lblDesignModuleTitle.TabIndex = 0;
            this.lblDesignModuleTitle.UseMnemonic = false;
            this.lblDesignModuleTitle.Text = "Tableau de bord - Pilotage financier";
            //
            // btnDesignPrimary
            //
            this.btnDesignPrimary.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnDesignPrimary.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignPrimary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignPrimary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDesignPrimary.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignPrimary.Location = new System.Drawing.Point(690, 14);
            this.btnDesignPrimary.Name = "btnDesignPrimary";
            this.btnDesignPrimary.Size = new System.Drawing.Size(150, 34);
            this.btnDesignPrimary.TabIndex = 1;
            this.btnDesignPrimary.Text = "+ Recette";
            this.btnDesignPrimary.UseVisualStyleBackColor = false;
            //
            // btnDesignSecondary
            //
            this.btnDesignSecondary.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnDesignSecondary.BackColor = System.Drawing.Color.FromArgb(247, 249, 253);
            this.btnDesignSecondary.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignSecondary.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDesignSecondary.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignSecondary.Location = new System.Drawing.Point(852, 14);
            this.btnDesignSecondary.Name = "btnDesignSecondary";
            this.btnDesignSecondary.Size = new System.Drawing.Size(150, 34);
            this.btnDesignSecondary.TabIndex = 2;
            this.btnDesignSecondary.Text = "- Dépense";
            this.btnDesignSecondary.UseVisualStyleBackColor = false;
            //
            // pnlDesignCard
            //
            this.pnlDesignCard.BackColor = System.Drawing.Color.White;
            this.pnlDesignCard.Controls.Add(this.dgvDesignPreview);
            this.pnlDesignCard.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignCard.Location = new System.Drawing.Point(0, 64);
            this.pnlDesignCard.Name = "pnlDesignCard";
            this.pnlDesignCard.Padding = new System.Windows.Forms.Padding(12);
            this.pnlDesignCard.Size = new System.Drawing.Size(1020, 576);
            this.pnlDesignCard.TabIndex = 1;
            //
            // dgvDesignPreview
            //
            this.dgvDesignPreview.BackgroundColor = System.Drawing.Color.White;
            this.dgvDesignPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dgvDesignPreview.Location = new System.Drawing.Point(12, 12);
            this.dgvDesignPreview.Name = "dgvDesignPreview";
            this.dgvDesignPreview.Size = new System.Drawing.Size(996, 552);
            this.dgvDesignPreview.TabIndex = 0;
            //
            // UcDashboard
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.Controls.Add(this.pnlDesignCard);
            this.Controls.Add(this.pnlDesignToolbar);
            this.Name = "UcDashboard";
            this.Size = new System.Drawing.Size(1020, 640);
            this.pnlDesignToolbar.ResumeLayout(false);
            this.pnlDesignCard.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignPreview)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

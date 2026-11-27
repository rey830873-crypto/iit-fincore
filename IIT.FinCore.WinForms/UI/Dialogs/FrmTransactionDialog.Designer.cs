#nullable disable
namespace IIT.FinCore.WinForms.UI.Dialogs
{
    partial class FrmTransactionDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignHeader;
        private System.Windows.Forms.Label lblDesignTitle;
        private System.Windows.Forms.Panel pnlDesignBody;
        private System.Windows.Forms.Button btnDesignRec;
        private System.Windows.Forms.Button btnDesignDep;
        private System.Windows.Forms.TextBox txtDesignLibelle;
        private System.Windows.Forms.NumericUpDown numDesignMontant;
        private System.Windows.Forms.DataGridView dgvDesignVent;
        private System.Windows.Forms.Panel pnlDesignFooter;
        private System.Windows.Forms.Button btnDesignSave;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlDesignHeader = new System.Windows.Forms.Panel();
            this.lblDesignTitle = new System.Windows.Forms.Label();
            this.pnlDesignBody = new System.Windows.Forms.Panel();
            this.dgvDesignVent = new System.Windows.Forms.DataGridView();
            this.numDesignMontant = new System.Windows.Forms.NumericUpDown();
            this.txtDesignLibelle = new System.Windows.Forms.TextBox();
            this.btnDesignDep = new System.Windows.Forms.Button();
            this.btnDesignRec = new System.Windows.Forms.Button();
            this.pnlDesignFooter = new System.Windows.Forms.Panel();
            this.btnDesignSave = new System.Windows.Forms.Button();
            this.pnlDesignHeader.SuspendLayout();
            this.pnlDesignBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignVent)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDesignMontant)).BeginInit();
            this.pnlDesignFooter.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlDesignHeader
            //
            this.pnlDesignHeader.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
            this.pnlDesignHeader.Controls.Add(this.lblDesignTitle);
            this.pnlDesignHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDesignHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignHeader.Name = "pnlDesignHeader";
            this.pnlDesignHeader.Size = new System.Drawing.Size(768, 70);
            this.pnlDesignHeader.TabIndex = 0;
            //
            // lblDesignTitle
            //
            this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblDesignTitle.ForeColor = System.Drawing.Color.White;
            this.lblDesignTitle.Location = new System.Drawing.Point(22, 18);
            this.lblDesignTitle.Name = "lblDesignTitle";
            this.lblDesignTitle.Size = new System.Drawing.Size(600, 32);
            this.lblDesignTitle.TabIndex = 0;
            this.lblDesignTitle.Text = "Nouvelle opération - Recette / Dépense & Ventilation";
            //
            // pnlDesignBody
            //
            this.pnlDesignBody.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.pnlDesignBody.Controls.Add(this.dgvDesignVent);
            this.pnlDesignBody.Controls.Add(this.numDesignMontant);
            this.pnlDesignBody.Controls.Add(this.txtDesignLibelle);
            this.pnlDesignBody.Controls.Add(this.btnDesignDep);
            this.pnlDesignBody.Controls.Add(this.btnDesignRec);
            this.pnlDesignBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignBody.Location = new System.Drawing.Point(0, 70);
            this.pnlDesignBody.Name = "pnlDesignBody";
            this.pnlDesignBody.Size = new System.Drawing.Size(768, 626);
            this.pnlDesignBody.TabIndex = 1;
            //
            // btnDesignRec
            //
            this.btnDesignRec.BackColor = System.Drawing.Color.FromArgb(16, 152, 104);
            this.btnDesignRec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignRec.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignRec.ForeColor = System.Drawing.Color.White;
            this.btnDesignRec.Location = new System.Drawing.Point(24, 18);
            this.btnDesignRec.Name = "btnDesignRec";
            this.btnDesignRec.Size = new System.Drawing.Size(160, 36);
            this.btnDesignRec.TabIndex = 0;
            this.btnDesignRec.Text = "+ Recette";
            this.btnDesignRec.UseVisualStyleBackColor = false;
            //
            // btnDesignDep
            //
            this.btnDesignDep.BackColor = System.Drawing.Color.FromArgb(217, 68, 82);
            this.btnDesignDep.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignDep.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignDep.ForeColor = System.Drawing.Color.White;
            this.btnDesignDep.Location = new System.Drawing.Point(194, 18);
            this.btnDesignDep.Name = "btnDesignDep";
            this.btnDesignDep.Size = new System.Drawing.Size(160, 36);
            this.btnDesignDep.TabIndex = 1;
            this.btnDesignDep.Text = "- Dépense";
            this.btnDesignDep.UseVisualStyleBackColor = false;
            //
            // txtDesignLibelle
            //
            this.txtDesignLibelle.Location = new System.Drawing.Point(24, 72);
            this.txtDesignLibelle.Name = "txtDesignLibelle";
            this.txtDesignLibelle.PlaceholderText = "Libellé de l'opération...";
            this.txtDesignLibelle.Size = new System.Drawing.Size(440, 27);
            this.txtDesignLibelle.TabIndex = 2;
            //
            // numDesignMontant
            //
            this.numDesignMontant.Location = new System.Drawing.Point(480, 72);
            this.numDesignMontant.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            this.numDesignMontant.Name = "numDesignMontant";
            this.numDesignMontant.Size = new System.Drawing.Size(260, 27);
            this.numDesignMontant.TabIndex = 3;
            //
            // dgvDesignVent
            //
            this.dgvDesignVent.BackgroundColor = System.Drawing.Color.White;
            this.dgvDesignVent.Location = new System.Drawing.Point(24, 340);
            this.dgvDesignVent.Name = "dgvDesignVent";
            this.dgvDesignVent.Size = new System.Drawing.Size(720, 250);
            this.dgvDesignVent.TabIndex = 4;
            //
            // pnlDesignFooter
            //
            this.pnlDesignFooter.BackColor = System.Drawing.Color.White;
            this.pnlDesignFooter.Controls.Add(this.btnDesignSave);
            this.pnlDesignFooter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlDesignFooter.Location = new System.Drawing.Point(0, 696);
            this.pnlDesignFooter.Name = "pnlDesignFooter";
            this.pnlDesignFooter.Size = new System.Drawing.Size(768, 64);
            this.pnlDesignFooter.TabIndex = 2;
            //
            // btnDesignSave
            //
            this.btnDesignSave.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignSave.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignSave.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignSave.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignSave.Location = new System.Drawing.Point(550, 13);
            this.btnDesignSave.Name = "btnDesignSave";
            this.btnDesignSave.Size = new System.Drawing.Size(194, 38);
            this.btnDesignSave.TabIndex = 0;
            this.btnDesignSave.Text = "Enregistrer l'opération";
            this.btnDesignSave.UseVisualStyleBackColor = false;
            //
            // FrmTransactionDialog
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.ClientSize = new System.Drawing.Size(768, 760);
            this.Controls.Add(this.pnlDesignBody);
            this.Controls.Add(this.pnlDesignHeader);
            this.Controls.Add(this.pnlDesignFooter);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmTransactionDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IIT FINCORE - Nouvelle opération";
            this.pnlDesignHeader.ResumeLayout(false);
            this.pnlDesignBody.ResumeLayout(false);
            this.pnlDesignBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvDesignVent)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numDesignMontant)).EndInit();
            this.pnlDesignFooter.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

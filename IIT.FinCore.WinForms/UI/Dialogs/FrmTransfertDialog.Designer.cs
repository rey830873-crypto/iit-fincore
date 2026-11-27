#nullable disable
namespace IIT.FinCore.WinForms.UI.Dialogs
{
    partial class FrmTransfertDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignHeader;
        private System.Windows.Forms.Label lblDesignTitle;
        private System.Windows.Forms.Panel pnlDesignBody;
        private System.Windows.Forms.ComboBox cboDesignSrc;
        private System.Windows.Forms.ComboBox cboDesignDst;
        private System.Windows.Forms.NumericUpDown numDesignAmt;
        private System.Windows.Forms.TextBox txtDesignMotif;
        private System.Windows.Forms.Button btnDesignExec;

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
            this.btnDesignExec = new System.Windows.Forms.Button();
            this.txtDesignMotif = new System.Windows.Forms.TextBox();
            this.numDesignAmt = new System.Windows.Forms.NumericUpDown();
            this.cboDesignDst = new System.Windows.Forms.ComboBox();
            this.cboDesignSrc = new System.Windows.Forms.ComboBox();
            this.pnlDesignHeader.SuspendLayout();
            this.pnlDesignBody.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDesignAmt)).BeginInit();
            this.SuspendLayout();
            //
            // pnlDesignHeader
            //
            this.pnlDesignHeader.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
            this.pnlDesignHeader.Controls.Add(this.lblDesignTitle);
            this.pnlDesignHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDesignHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignHeader.Name = "pnlDesignHeader";
            this.pnlDesignHeader.Size = new System.Drawing.Size(560, 70);
            this.pnlDesignHeader.TabIndex = 0;
            //
            // lblDesignTitle
            //
            this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignTitle.ForeColor = System.Drawing.Color.White;
            this.lblDesignTitle.Location = new System.Drawing.Point(22, 20);
            this.lblDesignTitle.Name = "lblDesignTitle";
            this.lblDesignTitle.Size = new System.Drawing.Size(480, 30);
            this.lblDesignTitle.TabIndex = 0;
            this.lblDesignTitle.Text = "Transfert entre comptes";
            //
            // pnlDesignBody
            //
            this.pnlDesignBody.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.pnlDesignBody.Controls.Add(this.btnDesignExec);
            this.pnlDesignBody.Controls.Add(this.txtDesignMotif);
            this.pnlDesignBody.Controls.Add(this.numDesignAmt);
            this.pnlDesignBody.Controls.Add(this.cboDesignDst);
            this.pnlDesignBody.Controls.Add(this.cboDesignSrc);
            this.pnlDesignBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignBody.Location = new System.Drawing.Point(0, 70);
            this.pnlDesignBody.Name = "pnlDesignBody";
            this.pnlDesignBody.Size = new System.Drawing.Size(560, 400);
            this.pnlDesignBody.TabIndex = 1;
            //
            // cboDesignSrc
            //
            this.cboDesignSrc.Location = new System.Drawing.Point(34, 34);
            this.cboDesignSrc.Name = "cboDesignSrc";
            this.cboDesignSrc.Size = new System.Drawing.Size(490, 28);
            this.cboDesignSrc.TabIndex = 0;
            this.cboDesignSrc.Text = "Compte source...";
            //
            // cboDesignDst
            //
            this.cboDesignDst.Location = new System.Drawing.Point(34, 92);
            this.cboDesignDst.Name = "cboDesignDst";
            this.cboDesignDst.Size = new System.Drawing.Size(490, 28);
            this.cboDesignDst.TabIndex = 1;
            this.cboDesignDst.Text = "Compte destinataire...";
            //
            // numDesignAmt
            //
            this.numDesignAmt.Location = new System.Drawing.Point(34, 150);
            this.numDesignAmt.Maximum = new decimal(new int[] { 100000000, 0, 0, 0 });
            this.numDesignAmt.Name = "numDesignAmt";
            this.numDesignAmt.Size = new System.Drawing.Size(240, 27);
            this.numDesignAmt.TabIndex = 2;
            //
            // txtDesignMotif
            //
            this.txtDesignMotif.Location = new System.Drawing.Point(34, 208);
            this.txtDesignMotif.Name = "txtDesignMotif";
            this.txtDesignMotif.PlaceholderText = "Motif du transfert...";
            this.txtDesignMotif.Size = new System.Drawing.Size(490, 27);
            this.txtDesignMotif.TabIndex = 3;
            //
            // btnDesignExec
            //
            this.btnDesignExec.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignExec.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignExec.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignExec.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignExec.Location = new System.Drawing.Point(334, 332);
            this.btnDesignExec.Name = "btnDesignExec";
            this.btnDesignExec.Size = new System.Drawing.Size(190, 38);
            this.btnDesignExec.TabIndex = 4;
            this.btnDesignExec.Text = "Exécuter le transfert";
            this.btnDesignExec.UseVisualStyleBackColor = false;
            //
            // FrmTransfertDialog
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.ClientSize = new System.Drawing.Size(560, 470);
            this.Controls.Add(this.pnlDesignBody);
            this.Controls.Add(this.pnlDesignHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmTransfertDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IIT FINCORE - Transfert entre comptes";
            this.pnlDesignHeader.ResumeLayout(false);
            this.pnlDesignBody.ResumeLayout(false);
            this.pnlDesignBody.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numDesignAmt)).EndInit();
            this.ResumeLayout(false);
        }
    }
}

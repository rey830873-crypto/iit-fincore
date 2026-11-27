#nullable disable
namespace IIT.FinCore.WinForms.UI.Dialogs
{
    partial class FrmDetailsDialog
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignHeader;
        private System.Windows.Forms.Label lblDesignTitle;
        private System.Windows.Forms.Panel pnlDesignBody;
        private System.Windows.Forms.Button btnDesignClose;

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
            this.btnDesignClose = new System.Windows.Forms.Button();
            this.pnlDesignHeader.SuspendLayout();
            this.pnlDesignBody.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlDesignHeader
            //
            this.pnlDesignHeader.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
            this.pnlDesignHeader.Controls.Add(this.lblDesignTitle);
            this.pnlDesignHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlDesignHeader.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignHeader.Name = "pnlDesignHeader";
            this.pnlDesignHeader.Size = new System.Drawing.Size(660, 70);
            this.pnlDesignHeader.TabIndex = 0;
            //
            // lblDesignTitle
            //
            this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignTitle.ForeColor = System.Drawing.Color.White;
            this.lblDesignTitle.Location = new System.Drawing.Point(22, 18);
            this.lblDesignTitle.Name = "lblDesignTitle";
            this.lblDesignTitle.Size = new System.Drawing.Size(560, 30);
            this.lblDesignTitle.TabIndex = 0;
            this.lblDesignTitle.Text = "Détails de l'opération";
            //
            // pnlDesignBody
            //
            this.pnlDesignBody.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.pnlDesignBody.Controls.Add(this.btnDesignClose);
            this.pnlDesignBody.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignBody.Location = new System.Drawing.Point(0, 70);
            this.pnlDesignBody.Name = "pnlDesignBody";
            this.pnlDesignBody.Size = new System.Drawing.Size(660, 400);
            this.pnlDesignBody.TabIndex = 1;
            //
            // btnDesignClose
            //
            this.btnDesignClose.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignClose.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignClose.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignClose.Location = new System.Drawing.Point(510, 340);
            this.btnDesignClose.Name = "btnDesignClose";
            this.btnDesignClose.Size = new System.Drawing.Size(125, 38);
            this.btnDesignClose.TabIndex = 0;
            this.btnDesignClose.Text = "Fermer";
            this.btnDesignClose.UseVisualStyleBackColor = false;
            //
            // FrmDetailsDialog
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.ClientSize = new System.Drawing.Size(660, 470);
            this.Controls.Add(this.pnlDesignBody);
            this.Controls.Add(this.pnlDesignHeader);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmDetailsDialog";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "IIT FINCORE - Détails";
            this.pnlDesignHeader.ResumeLayout(false);
            this.pnlDesignBody.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

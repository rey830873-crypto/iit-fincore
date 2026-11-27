#nullable disable
namespace IIT.FinCore.WinForms.UI
{
    partial class FrmOnboarding
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignerLeft;
        private System.Windows.Forms.Panel pnlDesignerRight;
        private System.Windows.Forms.Panel pnlDesignerArt;
        private System.Windows.Forms.Label lblDesignerBrand;
        private System.Windows.Forms.Label lblDesignerStep;
        private System.Windows.Forms.Label lblDesignerTitle;
        private System.Windows.Forms.Label lblDesignerSub;
        private System.Windows.Forms.Button btnDesignerSkip;
        private System.Windows.Forms.Button btnDesignerNext;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlDesignerLeft = new System.Windows.Forms.Panel();
            this.pnlDesignerArt = new System.Windows.Forms.Panel();
            this.lblDesignerBrand = new System.Windows.Forms.Label();
            this.pnlDesignerRight = new System.Windows.Forms.Panel();
            this.lblDesignerStep = new System.Windows.Forms.Label();
            this.btnDesignerSkip = new System.Windows.Forms.Button();
            this.lblDesignerTitle = new System.Windows.Forms.Label();
            this.lblDesignerSub = new System.Windows.Forms.Label();
            this.btnDesignerNext = new System.Windows.Forms.Button();
            this.pnlDesignerLeft.SuspendLayout();
            this.pnlDesignerRight.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlDesignerLeft (Colonne Royal Sapphire)
            //
            this.pnlDesignerLeft.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
            this.pnlDesignerLeft.Controls.Add(this.pnlDesignerArt);
            this.pnlDesignerLeft.Controls.Add(this.lblDesignerBrand);
            this.pnlDesignerLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlDesignerLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignerLeft.Name = "pnlDesignerLeft";
            this.pnlDesignerLeft.Size = new System.Drawing.Size(460, 585);
            this.pnlDesignerLeft.TabIndex = 0;
            //
            // lblDesignerBrand
            //
            this.lblDesignerBrand.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblDesignerBrand.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
            this.lblDesignerBrand.Location = new System.Drawing.Point(28, 28);
            this.lblDesignerBrand.Name = "lblDesignerBrand";
            this.lblDesignerBrand.Size = new System.Drawing.Size(300, 34);
            this.lblDesignerBrand.TabIndex = 0;
            this.lblDesignerBrand.Text = "IIT FINCORE";
            //
            // pnlDesignerArt
            //
            this.pnlDesignerArt.BackColor = System.Drawing.Color.FromArgb(13, 50, 104);
            this.pnlDesignerArt.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDesignerArt.Location = new System.Drawing.Point(28, 96);
            this.pnlDesignerArt.Name = "pnlDesignerArt";
            this.pnlDesignerArt.Size = new System.Drawing.Size(404, 340);
            this.pnlDesignerArt.TabIndex = 1;
            //
            // pnlDesignerRight (Colonne éditoriale blanche)
            //
            this.pnlDesignerRight.BackColor = System.Drawing.Color.White;
            this.pnlDesignerRight.Controls.Add(this.btnDesignerNext);
            this.pnlDesignerRight.Controls.Add(this.lblDesignerSub);
            this.pnlDesignerRight.Controls.Add(this.lblDesignerTitle);
            this.pnlDesignerRight.Controls.Add(this.btnDesignerSkip);
            this.pnlDesignerRight.Controls.Add(this.lblDesignerStep);
            this.pnlDesignerRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignerRight.Location = new System.Drawing.Point(460, 0);
            this.pnlDesignerRight.Name = "pnlDesignerRight";
            this.pnlDesignerRight.Size = new System.Drawing.Size(520, 585);
            this.pnlDesignerRight.TabIndex = 1;
            //
            // lblDesignerStep
            //
            this.lblDesignerStep.BackColor = System.Drawing.Color.FromArgb(253, 246, 227);
            this.lblDesignerStep.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignerStep.ForeColor = System.Drawing.Color.FromArgb(204, 144, 20);
            this.lblDesignerStep.Location = new System.Drawing.Point(40, 28);
            this.lblDesignerStep.Name = "lblDesignerStep";
            this.lblDesignerStep.Size = new System.Drawing.Size(220, 28);
            this.lblDesignerStep.TabIndex = 0;
            this.lblDesignerStep.Text = "BIENVENUE • 01 / 03";
            this.lblDesignerStep.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnDesignerSkip
            //
            this.btnDesignerSkip.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignerSkip.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDesignerSkip.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.btnDesignerSkip.Location = new System.Drawing.Point(410, 26);
            this.btnDesignerSkip.Name = "btnDesignerSkip";
            this.btnDesignerSkip.Size = new System.Drawing.Size(78, 32);
            this.btnDesignerSkip.TabIndex = 1;
            this.btnDesignerSkip.Text = "Passer";
            //
            // lblDesignerTitle
            //
            this.lblDesignerTitle.Font = new System.Drawing.Font("Segoe UI", 20F, System.Drawing.FontStyle.Bold);
            this.lblDesignerTitle.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.lblDesignerTitle.Location = new System.Drawing.Point(40, 74);
            this.lblDesignerTitle.Name = "lblDesignerTitle";
            this.lblDesignerTitle.Size = new System.Drawing.Size(444, 86);
            this.lblDesignerTitle.TabIndex = 2;
            this.lblDesignerTitle.Text = "Pilotez tous vos comptes en temps réel";
            //
            // lblDesignerSub
            //
            this.lblDesignerSub.Font = new System.Drawing.Font("Segoe UI", 9.8F);
            this.lblDesignerSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblDesignerSub.Location = new System.Drawing.Point(40, 168);
            this.lblDesignerSub.Name = "lblDesignerSub";
            this.lblDesignerSub.Size = new System.Drawing.Size(444, 52);
            this.lblDesignerSub.TabIndex = 3;
            this.lblDesignerSub.Text = "Centralisez vos banques, caisses espèces et Mobile Money dans une interface élégante.";
            //
            // btnDesignerNext
            //
            this.btnDesignerNext.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignerNext.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignerNext.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignerNext.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignerNext.Location = new System.Drawing.Point(324, 504);
            this.btnDesignerNext.Name = "btnDesignerNext";
            this.btnDesignerNext.Size = new System.Drawing.Size(162, 42);
            this.btnDesignerNext.TabIndex = 4;
            this.btnDesignerNext.Text = "Suivant";
            this.btnDesignerNext.UseVisualStyleBackColor = false;
            //
            // FrmOnboarding
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(980, 585);
            this.Controls.Add(this.pnlDesignerRight);
            this.Controls.Add(this.pnlDesignerLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmOnboarding";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IIT FINCORE - Découverte";
            this.pnlDesignerLeft.ResumeLayout(false);
            this.pnlDesignerRight.ResumeLayout(false);
            this.ResumeLayout(false);
        }
    }
}

#nullable disable
namespace IIT.FinCore.WinForms.UI
{
    partial class FrmLogin
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Panel pnlDesignLeft;
        private System.Windows.Forms.Panel pnlDesignRight;
        private System.Windows.Forms.Panel pnlDesignCard;
        private System.Windows.Forms.Label lblDesignLogo;
        private System.Windows.Forms.Label lblDesignTagline;
        private System.Windows.Forms.Button btnDesignAdmin;
        private System.Windows.Forms.Button btnDesignTreso;
        private System.Windows.Forms.Button btnDesignConsult;
        private System.Windows.Forms.Label lblDesignTitle;
        private System.Windows.Forms.Label lblDesignSub;
        private System.Windows.Forms.Label lblDesignEmail;
        private System.Windows.Forms.TextBox txtDesignEmail;
        private System.Windows.Forms.Label lblDesignPwd;
        private System.Windows.Forms.TextBox txtDesignPwd;
        private System.Windows.Forms.CheckBox chkDesignRemember;
        private System.Windows.Forms.Button btnDesignLogin;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
                components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.pnlDesignLeft = new System.Windows.Forms.Panel();
            this.btnDesignConsult = new System.Windows.Forms.Button();
            this.btnDesignTreso = new System.Windows.Forms.Button();
            this.btnDesignAdmin = new System.Windows.Forms.Button();
            this.lblDesignTagline = new System.Windows.Forms.Label();
            this.lblDesignLogo = new System.Windows.Forms.Label();
            this.pnlDesignRight = new System.Windows.Forms.Panel();
            this.pnlDesignCard = new System.Windows.Forms.Panel();
            this.btnDesignLogin = new System.Windows.Forms.Button();
            this.chkDesignRemember = new System.Windows.Forms.CheckBox();
            this.txtDesignPwd = new System.Windows.Forms.TextBox();
            this.lblDesignPwd = new System.Windows.Forms.Label();
            this.txtDesignEmail = new System.Windows.Forms.TextBox();
            this.lblDesignEmail = new System.Windows.Forms.Label();
            this.lblDesignSub = new System.Windows.Forms.Label();
            this.lblDesignTitle = new System.Windows.Forms.Label();
            this.pnlDesignLeft.SuspendLayout();
            this.pnlDesignRight.SuspendLayout();
            this.pnlDesignCard.SuspendLayout();
            this.SuspendLayout();
            //
            // pnlDesignLeft (Panneau Royal Sapphire)
            //
            this.pnlDesignLeft.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
            this.pnlDesignLeft.Controls.Add(this.btnDesignConsult);
            this.pnlDesignLeft.Controls.Add(this.btnDesignTreso);
            this.pnlDesignLeft.Controls.Add(this.btnDesignAdmin);
            this.pnlDesignLeft.Controls.Add(this.lblDesignTagline);
            this.pnlDesignLeft.Controls.Add(this.lblDesignLogo);
            this.pnlDesignLeft.Dock = System.Windows.Forms.DockStyle.Left;
            this.pnlDesignLeft.Location = new System.Drawing.Point(0, 0);
            this.pnlDesignLeft.Name = "pnlDesignLeft";
            this.pnlDesignLeft.Size = new System.Drawing.Size(370, 600);
            this.pnlDesignLeft.TabIndex = 0;
            //
            // lblDesignLogo
            //
            this.lblDesignLogo.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblDesignLogo.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
            this.lblDesignLogo.Location = new System.Drawing.Point(35, 195);
            this.lblDesignLogo.Name = "lblDesignLogo";
            this.lblDesignLogo.Size = new System.Drawing.Size(300, 44);
            this.lblDesignLogo.TabIndex = 0;
            this.lblDesignLogo.Text = "IIT FINCORE";
            this.lblDesignLogo.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblDesignTagline
            //
            this.lblDesignTagline.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignTagline.ForeColor = System.Drawing.Color.FromArgb(175, 198, 226);
            this.lblDesignTagline.Location = new System.Drawing.Point(35, 242);
            this.lblDesignTagline.Name = "lblDesignTagline";
            this.lblDesignTagline.Size = new System.Drawing.Size(300, 22);
            this.lblDesignTagline.TabIndex = 1;
            this.lblDesignTagline.Text = "EXCELLENCE EN GESTION FINANCIÈRE";
            this.lblDesignTagline.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // btnDesignAdmin
            //
            this.btnDesignAdmin.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignAdmin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignAdmin.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignAdmin.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignAdmin.Location = new System.Drawing.Point(60, 326);
            this.btnDesignAdmin.Name = "btnDesignAdmin";
            this.btnDesignAdmin.Size = new System.Drawing.Size(250, 38);
            this.btnDesignAdmin.TabIndex = 2;
            this.btnDesignAdmin.Text = "Administrateur";
            this.btnDesignAdmin.UseVisualStyleBackColor = false;
            //
            // btnDesignTreso
            //
            this.btnDesignTreso.BackColor = System.Drawing.Color.FromArgb(22, 52, 92);
            this.btnDesignTreso.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignTreso.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignTreso.ForeColor = System.Drawing.Color.White;
            this.btnDesignTreso.Location = new System.Drawing.Point(60, 374);
            this.btnDesignTreso.Name = "btnDesignTreso";
            this.btnDesignTreso.Size = new System.Drawing.Size(250, 38);
            this.btnDesignTreso.TabIndex = 3;
            this.btnDesignTreso.Text = "Trésorier";
            this.btnDesignTreso.UseVisualStyleBackColor = false;
            //
            // btnDesignConsult
            //
            this.btnDesignConsult.BackColor = System.Drawing.Color.FromArgb(22, 52, 92);
            this.btnDesignConsult.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignConsult.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.btnDesignConsult.ForeColor = System.Drawing.Color.White;
            this.btnDesignConsult.Location = new System.Drawing.Point(60, 422);
            this.btnDesignConsult.Name = "btnDesignConsult";
            this.btnDesignConsult.Size = new System.Drawing.Size(250, 38);
            this.btnDesignConsult.TabIndex = 4;
            this.btnDesignConsult.Text = "Consultant";
            this.btnDesignConsult.UseVisualStyleBackColor = false;
            //
            // pnlDesignRight
            //
            this.pnlDesignRight.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.pnlDesignRight.Controls.Add(this.pnlDesignCard);
            this.pnlDesignRight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlDesignRight.Location = new System.Drawing.Point(370, 0);
            this.pnlDesignRight.Name = "pnlDesignRight";
            this.pnlDesignRight.Size = new System.Drawing.Size(590, 600);
            this.pnlDesignRight.TabIndex = 1;
            //
            // pnlDesignCard
            //
            this.pnlDesignCard.BackColor = System.Drawing.Color.White;
            this.pnlDesignCard.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlDesignCard.Controls.Add(this.btnDesignLogin);
            this.pnlDesignCard.Controls.Add(this.chkDesignRemember);
            this.pnlDesignCard.Controls.Add(this.txtDesignPwd);
            this.pnlDesignCard.Controls.Add(this.lblDesignPwd);
            this.pnlDesignCard.Controls.Add(this.txtDesignEmail);
            this.pnlDesignCard.Controls.Add(this.lblDesignEmail);
            this.pnlDesignCard.Controls.Add(this.lblDesignSub);
            this.pnlDesignCard.Controls.Add(this.lblDesignTitle);
            this.pnlDesignCard.Location = new System.Drawing.Point(70, 72);
            this.pnlDesignCard.Name = "pnlDesignCard";
            this.pnlDesignCard.Size = new System.Drawing.Size(450, 456);
            this.pnlDesignCard.TabIndex = 0;
            //
            // lblDesignTitle
            //
            this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 22F, System.Drawing.FontStyle.Bold);
            this.lblDesignTitle.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.lblDesignTitle.Location = new System.Drawing.Point(20, 32);
            this.lblDesignTitle.Name = "lblDesignTitle";
            this.lblDesignTitle.Size = new System.Drawing.Size(410, 42);
            this.lblDesignTitle.TabIndex = 0;
            this.lblDesignTitle.Text = "Connexion";
            this.lblDesignTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblDesignSub
            //
            this.lblDesignSub.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.lblDesignSub.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.lblDesignSub.Location = new System.Drawing.Point(20, 76);
            this.lblDesignSub.Name = "lblDesignSub";
            this.lblDesignSub.Size = new System.Drawing.Size(410, 22);
            this.lblDesignSub.TabIndex = 1;
            this.lblDesignSub.Text = "Accédez à votre espace sécurisé IIT FINCORE";
            this.lblDesignSub.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            //
            // lblDesignEmail
            //
            this.lblDesignEmail.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignEmail.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.lblDesignEmail.Location = new System.Drawing.Point(46, 120);
            this.lblDesignEmail.Name = "lblDesignEmail";
            this.lblDesignEmail.Size = new System.Drawing.Size(200, 20);
            this.lblDesignEmail.TabIndex = 2;
            this.lblDesignEmail.Text = "Adresse e-mail";
            //
            // txtDesignEmail
            //
            this.txtDesignEmail.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtDesignEmail.Location = new System.Drawing.Point(46, 146);
            this.txtDesignEmail.Name = "txtDesignEmail";
            this.txtDesignEmail.Size = new System.Drawing.Size(358, 30);
            this.txtDesignEmail.TabIndex = 3;
            this.txtDesignEmail.Text = "admin@iit.ci";
            //
            // lblDesignPwd
            //
            this.lblDesignPwd.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
            this.lblDesignPwd.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.lblDesignPwd.Location = new System.Drawing.Point(46, 198);
            this.lblDesignPwd.Name = "lblDesignPwd";
            this.lblDesignPwd.Size = new System.Drawing.Size(200, 20);
            this.lblDesignPwd.TabIndex = 4;
            this.lblDesignPwd.Text = "Mot de passe";
            //
            // txtDesignPwd
            //
            this.txtDesignPwd.Font = new System.Drawing.Font("Segoe UI", 10.5F);
            this.txtDesignPwd.Location = new System.Drawing.Point(46, 224);
            this.txtDesignPwd.Name = "txtDesignPwd";
            this.txtDesignPwd.Size = new System.Drawing.Size(358, 30);
            this.txtDesignPwd.TabIndex = 5;
            this.txtDesignPwd.UseSystemPasswordChar = true;
            //
            // chkDesignRemember
            //
            this.chkDesignRemember.AutoSize = true;
            this.chkDesignRemember.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.chkDesignRemember.ForeColor = System.Drawing.Color.FromArgb(100, 116, 139);
            this.chkDesignRemember.Location = new System.Drawing.Point(46, 274);
            this.chkDesignRemember.Name = "chkDesignRemember";
            this.chkDesignRemember.Size = new System.Drawing.Size(148, 22);
            this.chkDesignRemember.TabIndex = 6;
            this.chkDesignRemember.Text = "Se souvenir de moi";
            //
            // btnDesignLogin
            //
            this.btnDesignLogin.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
            this.btnDesignLogin.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDesignLogin.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnDesignLogin.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
            this.btnDesignLogin.Location = new System.Drawing.Point(46, 330);
            this.btnDesignLogin.Name = "btnDesignLogin";
            this.btnDesignLogin.Size = new System.Drawing.Size(358, 44);
            this.btnDesignLogin.TabIndex = 7;
            this.btnDesignLogin.Text = "Se connecter";
            this.btnDesignLogin.UseVisualStyleBackColor = false;
            //
            // FrmLogin
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
            this.ClientSize = new System.Drawing.Size(960, 600);
            this.Controls.Add(this.pnlDesignRight);
            this.Controls.Add(this.pnlDesignLeft);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "FrmLogin";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "IIT FINCORE - Connexion";
            this.pnlDesignLeft.ResumeLayout(false);
            this.pnlDesignRight.ResumeLayout(false);
            this.pnlDesignCard.ResumeLayout(false);
            this.pnlDesignCard.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}

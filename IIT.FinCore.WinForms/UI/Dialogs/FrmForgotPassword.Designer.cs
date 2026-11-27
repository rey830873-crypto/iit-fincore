namespace IIT.FinCore.WinForms.UI.Dialogs;

partial class FrmForgotPassword
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Panel pnlDesignHeader = null!;
    private System.Windows.Forms.Label lblDesignTitle = null!;
    private System.Windows.Forms.Panel pnlDesignBody = null!;
    private System.Windows.Forms.TextBox txtDesignEmail = null!;
    private System.Windows.Forms.TextBox txtDesignPhone = null!;
    private System.Windows.Forms.TextBox txtDesignPwd = null!;
    private System.Windows.Forms.Button btnDesignReset = null!;

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
        this.btnDesignReset = new System.Windows.Forms.Button();
        this.txtDesignPwd = new System.Windows.Forms.TextBox();
        this.txtDesignPhone = new System.Windows.Forms.TextBox();
        this.txtDesignEmail = new System.Windows.Forms.TextBox();
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
        this.pnlDesignHeader.Size = new System.Drawing.Size(465, 70);
        this.pnlDesignHeader.TabIndex = 0;
        //
        // lblDesignTitle
        //
        this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignTitle.ForeColor = System.Drawing.Color.White;
        this.lblDesignTitle.Location = new System.Drawing.Point(22, 18);
        this.lblDesignTitle.Name = "lblDesignTitle";
        this.lblDesignTitle.Size = new System.Drawing.Size(400, 30);
        this.lblDesignTitle.TabIndex = 0;
        this.lblDesignTitle.Text = "Mot de passe oublié";
        //
        // pnlDesignBody
        //
        this.pnlDesignBody.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.pnlDesignBody.Controls.Add(this.btnDesignReset);
        this.pnlDesignBody.Controls.Add(this.txtDesignPwd);
        this.pnlDesignBody.Controls.Add(this.txtDesignPhone);
        this.pnlDesignBody.Controls.Add(this.txtDesignEmail);
        this.pnlDesignBody.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlDesignBody.Location = new System.Drawing.Point(0, 70);
        this.pnlDesignBody.Name = "pnlDesignBody";
        this.pnlDesignBody.Size = new System.Drawing.Size(465, 415);
        this.pnlDesignBody.TabIndex = 1;
        //
        // txtDesignEmail
        //
        this.txtDesignEmail.Location = new System.Drawing.Point(34, 42);
        this.txtDesignEmail.Name = "txtDesignEmail";
        this.txtDesignEmail.PlaceholderText = "Adresse e-mail du compte";
        this.txtDesignEmail.Size = new System.Drawing.Size(396, 27);
        this.txtDesignEmail.TabIndex = 0;
        //
        // txtDesignPhone
        //
        this.txtDesignPhone.Location = new System.Drawing.Point(34, 102);
        this.txtDesignPhone.Name = "txtDesignPhone";
        this.txtDesignPhone.PlaceholderText = "Numéro de téléphone de vérification";
        this.txtDesignPhone.Size = new System.Drawing.Size(396, 27);
        this.txtDesignPhone.TabIndex = 1;
        //
        // txtDesignPwd
        //
        this.txtDesignPwd.Location = new System.Drawing.Point(34, 162);
        this.txtDesignPwd.Name = "txtDesignPwd";
        this.txtDesignPwd.PlaceholderText = "Nouveau mot de passe";
        this.txtDesignPwd.Size = new System.Drawing.Size(396, 27);
        this.txtDesignPwd.TabIndex = 2;
        this.txtDesignPwd.UseSystemPasswordChar = true;
        //
        // btnDesignReset
        //
        this.btnDesignReset.BackColor = System.Drawing.Color.FromArgb(229, 169, 36);
        this.btnDesignReset.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDesignReset.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.btnDesignReset.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.btnDesignReset.Location = new System.Drawing.Point(285, 344);
        this.btnDesignReset.Name = "btnDesignReset";
        this.btnDesignReset.Size = new System.Drawing.Size(145, 38);
        this.btnDesignReset.TabIndex = 3;
        this.btnDesignReset.Text = "Réinitialiser";
        this.btnDesignReset.UseVisualStyleBackColor = false;
        //
        // FrmForgotPassword
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.ClientSize = new System.Drawing.Size(465, 485);
        this.Controls.Add(this.pnlDesignBody);
        this.Controls.Add(this.pnlDesignHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "FrmForgotPassword";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "IIT FINCORE - Réinitialisation sécurisée";
        this.pnlDesignHeader.ResumeLayout(false);
        this.pnlDesignBody.ResumeLayout(false);
        this.pnlDesignBody.PerformLayout();
        this.ResumeLayout(false);
    }
}

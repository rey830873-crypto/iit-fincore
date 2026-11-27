namespace IIT.FinCore.WinForms.UI.Dialogs;

partial class FrmToastNotification
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Label lblDesignToastTitle = null!;
    private System.Windows.Forms.Label lblDesignToastMsg = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.lblDesignToastTitle = new System.Windows.Forms.Label();
        this.lblDesignToastMsg = new System.Windows.Forms.Label();
        this.SuspendLayout();
        //
        // lblDesignToastTitle
        //
        this.lblDesignToastTitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignToastTitle.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.lblDesignToastTitle.Location = new System.Drawing.Point(22, 12);
        this.lblDesignToastTitle.Name = "lblDesignToastTitle";
        this.lblDesignToastTitle.Size = new System.Drawing.Size(336, 20);
        this.lblDesignToastTitle.TabIndex = 0;
        this.lblDesignToastTitle.Text = "Notification IIT FINCORE";
        //
        // lblDesignToastMsg
        //
        this.lblDesignToastMsg.Font = new System.Drawing.Font("Segoe UI", 8.5F);
        this.lblDesignToastMsg.ForeColor = System.Drawing.Color.FromArgb(228, 238, 250);
        this.lblDesignToastMsg.Location = new System.Drawing.Point(22, 36);
        this.lblDesignToastMsg.Name = "lblDesignToastMsg";
        this.lblDesignToastMsg.Size = new System.Drawing.Size(336, 38);
        this.lblDesignToastMsg.TabIndex = 1;
        this.lblDesignToastMsg.Text = "Alerte temps réel affichée en bas à droite.";
        //
        // FrmToastNotification
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.ClientSize = new System.Drawing.Size(376, 86);
        this.Controls.Add(this.lblDesignToastMsg);
        this.Controls.Add(this.lblDesignToastTitle);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "FrmToastNotification";
        this.ShowInTaskbar = false;
        this.Text = "Toast Notification";
        this.ResumeLayout(false);
    }
}

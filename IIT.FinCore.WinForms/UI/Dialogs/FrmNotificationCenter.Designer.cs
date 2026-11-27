namespace IIT.FinCore.WinForms.UI.Dialogs;

partial class FrmNotificationCenter
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Panel pnlDesignHeader = null!;
    private System.Windows.Forms.Label lblDesignTitle = null!;
    private System.Windows.Forms.Panel pnlDesignBar = null!;
    private System.Windows.Forms.Button btnDesignAll = null!;
    private System.Windows.Forms.Button btnDesignUnread = null!;
    private System.Windows.Forms.Panel pnlDesignList = null!;

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
        this.pnlDesignBar = new System.Windows.Forms.Panel();
        this.btnDesignUnread = new System.Windows.Forms.Button();
        this.btnDesignAll = new System.Windows.Forms.Button();
        this.pnlDesignList = new System.Windows.Forms.Panel();
        this.pnlDesignHeader.SuspendLayout();
        this.pnlDesignBar.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlDesignHeader
        //
        this.pnlDesignHeader.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.pnlDesignHeader.Controls.Add(this.lblDesignTitle);
        this.pnlDesignHeader.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDesignHeader.Location = new System.Drawing.Point(0, 0);
        this.pnlDesignHeader.Name = "pnlDesignHeader";
        this.pnlDesignHeader.Size = new System.Drawing.Size(630, 70);
        this.pnlDesignHeader.TabIndex = 0;
        //
        // lblDesignTitle
        //
        this.lblDesignTitle.Font = new System.Drawing.Font("Segoe UI", 13.5F, System.Drawing.FontStyle.Bold);
        this.lblDesignTitle.ForeColor = System.Drawing.Color.White;
        this.lblDesignTitle.Location = new System.Drawing.Point(22, 18);
        this.lblDesignTitle.Name = "lblDesignTitle";
        this.lblDesignTitle.Size = new System.Drawing.Size(500, 30);
        this.lblDesignTitle.TabIndex = 0;
        this.lblDesignTitle.Text = "Centre de Notifications & Alertes";
        //
        // pnlDesignBar
        //
        this.pnlDesignBar.BackColor = System.Drawing.Color.White;
        this.pnlDesignBar.Controls.Add(this.btnDesignUnread);
        this.pnlDesignBar.Controls.Add(this.btnDesignAll);
        this.pnlDesignBar.Dock = System.Windows.Forms.DockStyle.Top;
        this.pnlDesignBar.Location = new System.Drawing.Point(0, 70);
        this.pnlDesignBar.Name = "pnlDesignBar";
        this.pnlDesignBar.Size = new System.Drawing.Size(630, 52);
        this.pnlDesignBar.TabIndex = 1;
        //
        // btnDesignAll
        //
        this.btnDesignAll.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.btnDesignAll.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDesignAll.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnDesignAll.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.btnDesignAll.Location = new System.Drawing.Point(16, 10);
        this.btnDesignAll.Name = "btnDesignAll";
        this.btnDesignAll.Size = new System.Drawing.Size(86, 32);
        this.btnDesignAll.TabIndex = 0;
        this.btnDesignAll.Text = "Toutes";
        this.btnDesignAll.UseVisualStyleBackColor = false;
        //
        // btnDesignUnread
        //
        this.btnDesignUnread.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.btnDesignUnread.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
        this.btnDesignUnread.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
        this.btnDesignUnread.ForeColor = System.Drawing.Color.FromArgb(12, 31, 61);
        this.btnDesignUnread.Location = new System.Drawing.Point(110, 10);
        this.btnDesignUnread.Name = "btnDesignUnread";
        this.btnDesignUnread.Size = new System.Drawing.Size(92, 32);
        this.btnDesignUnread.TabIndex = 1;
        this.btnDesignUnread.Text = "Non lues";
        this.btnDesignUnread.UseVisualStyleBackColor = false;
        //
        // pnlDesignList
        //
        this.pnlDesignList.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.pnlDesignList.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlDesignList.Location = new System.Drawing.Point(0, 122);
        this.pnlDesignList.Name = "pnlDesignList";
        this.pnlDesignList.Size = new System.Drawing.Size(630, 518);
        this.pnlDesignList.TabIndex = 2;
        //
        // FrmNotificationCenter
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(242, 245, 250);
        this.ClientSize = new System.Drawing.Size(630, 640);
        this.Controls.Add(this.pnlDesignList);
        this.Controls.Add(this.pnlDesignBar);
        this.Controls.Add(this.pnlDesignHeader);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "FrmNotificationCenter";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "IIT FINCORE - Centre de Notifications";
        this.pnlDesignHeader.ResumeLayout(false);
        this.pnlDesignBar.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}

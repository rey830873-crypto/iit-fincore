namespace IIT.FinCore.WinForms.UI;

partial class FrmSplash
{
    private System.ComponentModel.IContainer components = null!;
    private System.Windows.Forms.Panel pnlSplashCanvas = null!;
    private System.Windows.Forms.Label lblBrandTitle = null!;
    private System.Windows.Forms.Label lblBrandSubtitle = null!;
    private System.Windows.Forms.Label lblSplashStatus = null!;
    private System.Windows.Forms.ProgressBar prgLoading = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        this.pnlSplashCanvas = new System.Windows.Forms.Panel();
        this.lblBrandTitle = new System.Windows.Forms.Label();
        this.lblBrandSubtitle = new System.Windows.Forms.Label();
        this.lblSplashStatus = new System.Windows.Forms.Label();
        this.prgLoading = new System.Windows.Forms.ProgressBar();
        this.pnlSplashCanvas.SuspendLayout();
        this.SuspendLayout();
        //
        // pnlSplashCanvas (aperçu fidèle visible dans le Concepteur Visual Studio)
        //
        this.pnlSplashCanvas.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.pnlSplashCanvas.Controls.Add(this.prgLoading);
        this.pnlSplashCanvas.Controls.Add(this.lblSplashStatus);
        this.pnlSplashCanvas.Controls.Add(this.lblBrandSubtitle);
        this.pnlSplashCanvas.Controls.Add(this.lblBrandTitle);
        this.pnlSplashCanvas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.pnlSplashCanvas.Location = new System.Drawing.Point(0, 0);
        this.pnlSplashCanvas.Name = "pnlSplashCanvas";
        this.pnlSplashCanvas.Size = new System.Drawing.Size(560, 360);
        this.pnlSplashCanvas.TabIndex = 0;
        //
        // lblBrandTitle
        //
        this.lblBrandTitle.Font = new System.Drawing.Font("Segoe UI", 24F, System.Drawing.FontStyle.Bold);
        this.lblBrandTitle.ForeColor = System.Drawing.Color.FromArgb(246, 200, 76);
        this.lblBrandTitle.Location = new System.Drawing.Point(40, 150);
        this.lblBrandTitle.Name = "lblBrandTitle";
        this.lblBrandTitle.Size = new System.Drawing.Size(480, 48);
        this.lblBrandTitle.TabIndex = 0;
        this.lblBrandTitle.Text = "IIT FINCORE";
        this.lblBrandTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblBrandSubtitle
        //
        this.lblBrandSubtitle.Font = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);
        this.lblBrandSubtitle.ForeColor = System.Drawing.Color.FromArgb(180, 202, 230);
        this.lblBrandSubtitle.Location = new System.Drawing.Point(40, 200);
        this.lblBrandSubtitle.Name = "lblBrandSubtitle";
        this.lblBrandSubtitle.Size = new System.Drawing.Size(480, 24);
        this.lblBrandSubtitle.TabIndex = 1;
        this.lblBrandSubtitle.Text = "GESTION FINANCIÈRE & TRÉSORERIE ASSOCIATIVE";
        this.lblBrandSubtitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // lblSplashStatus
        //
        this.lblSplashStatus.Font = new System.Drawing.Font("Segoe UI", 9F);
        this.lblSplashStatus.ForeColor = System.Drawing.Color.FromArgb(200, 216, 238);
        this.lblSplashStatus.Location = new System.Drawing.Point(60, 270);
        this.lblSplashStatus.Name = "lblSplashStatus";
        this.lblSplashStatus.Size = new System.Drawing.Size(440, 22);
        this.lblSplashStatus.TabIndex = 2;
        this.lblSplashStatus.Text = "Initialisation du moteur financier...";
        this.lblSplashStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
        //
        // prgLoading
        //
        this.prgLoading.Location = new System.Drawing.Point(90, 302);
        this.prgLoading.Name = "prgLoading";
        this.prgLoading.Size = new System.Drawing.Size(380, 10);
        this.prgLoading.TabIndex = 3;
        this.prgLoading.Value = 45;
        //
        // FrmSplash
        //
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
        this.BackColor = System.Drawing.Color.FromArgb(6, 27, 58);
        this.ClientSize = new System.Drawing.Size(560, 360);
        this.Controls.Add(this.pnlSplashCanvas);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
        this.Name = "FrmSplash";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "IIT FINCORE - Démarrage";
        this.pnlSplashCanvas.ResumeLayout(false);
        this.ResumeLayout(false);
    }
}

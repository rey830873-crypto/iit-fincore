using System.Drawing;
using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.Services;

namespace IIT.FinCore.WinForms.UI.Controls;

/// <summary>
/// Maquettes 44 (Profil utilisateur) & 45 (Paramètres généraux).
/// </summary>
public class UcParametres : UserControl
{
    private readonly AuthRepository _auth = new();
    private readonly TreasuryRepository _repo = new();
    private readonly TextBox _txtOldPwd = new();
    private readonly TextBox _txtNewPwd = new();
    private readonly TextBox _txtConfirmPwd = new();
    private readonly DataGridView _gridParams = Theme.CreateStyledGrid();
    private readonly TextBox _txtCle = new();
    private readonly TextBox _txtValeur = new();

    public UcParametres()
    {
        BackColor = Theme.PageBg;
        BuildLayout();
    }

    private void BuildLayout()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Font = Theme.FontBodyBold };

        var tabProfil = new TabPage("Mon Profil") { BackColor = Theme.PageBg, Padding = new Padding(12) };
        var cardProfil = Theme.CreateCardPanel();
        cardProfil.Dock = DockStyle.Fill;

        var u = UserSession.CurrentUser!;
        int y = 18;
        cardProfil.Controls.Add(new Label { Text = "Informations du compte connecté", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Location = new Point(22, y), AutoSize = true });
        y += 38;

        cardProfil.Controls.Add(new Label { Text = $"Nom complet : {u.NomComplet}", Font = Theme.FontBodyBold, Location = new Point(22, y), AutoSize = true }); y += 26;
        cardProfil.Controls.Add(new Label { Text = $"E-mail : {u.Email}", Font = Theme.FontBody, Location = new Point(22, y), AutoSize = true }); y += 26;
        cardProfil.Controls.Add(new Label { Text = $"Rôle : {u.RoleNom} ({u.RoleCode})", Font = Theme.FontBody, Location = new Point(22, y), AutoSize = true }); y += 26;
        cardProfil.Controls.Add(new Label { Text = $"Structure : {u.StructureNom ?? "Accès global multi-structures"}", Font = Theme.FontBody, Location = new Point(22, y), AutoSize = true }); y += 40;

        cardProfil.Controls.Add(new Label { Text = "Changer mon mot de passe", Font = Theme.FontSection, ForeColor = Theme.TextPrimary, Location = new Point(22, y), AutoSize = true });
        y += 34;

        cardProfil.Controls.Add(new Label { Text = "Mot de passe actuel", Font = Theme.FontBodyBold, ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true }); y += 22;
        _txtOldPwd.SetBounds(22, y, 340, 28); _txtOldPwd.UseSystemPasswordChar = true; y += 46;

        cardProfil.Controls.Add(new Label { Text = "Nouveau mot de passe", Font = Theme.FontBodyBold, ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true }); y += 22;
        _txtNewPwd.SetBounds(22, y, 340, 28); _txtNewPwd.UseSystemPasswordChar = true; y += 46;

        cardProfil.Controls.Add(new Label { Text = "Confirmer le nouveau mot de passe", Font = Theme.FontBodyBold, ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true }); y += 22;
        _txtConfirmPwd.SetBounds(22, y, 340, 28); _txtConfirmPwd.UseSystemPasswordChar = true; y += 50;

        var btnPwd = Theme.CreatePrimaryButton("Mettre à jour le mot de passe", 230, 38);
        btnPwd.Location = new Point(22, y);
        btnPwd.Click += (_, _) => ChangePassword();

        var btnReplayGuide = Theme.CreateSecondaryButton("Revoir le guide de mon espace", 225, 38);
        btnReplayGuide.Location = new Point(266, y);
        btnReplayGuide.Click += (_, _) =>
        {
            using var guide = new FrmOnboarding(UserSession.CurrentUser?.RoleCode);
            guide.ShowDialog(FindForm());
        };

        cardProfil.Controls.AddRange(new Control[] { _txtOldPwd, _txtNewPwd, _txtConfirmPwd, btnPwd, btnReplayGuide });
        tabProfil.Controls.Add(cardProfil);
        tabs.TabPages.Add(tabProfil);

        // Onglet 2 : Paramètres généraux (Admin) - 100% responsive FlowLayoutPanel
        if (UserSession.IsAdmin)
        {
            var tabParams = new TabPage("Paramètres généraux") { BackColor = Theme.PageBg, Padding = new Padding(12) };
            var topCard = Theme.CreateCardPanel(new Padding(16, 12, 16, 12));
            topCard.Dock = DockStyle.Top;
            topCard.AutoSize = true;

            _txtCle.ReadOnly = true;

            var flow = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true, WrapContents = true, BackColor = Color.Transparent };
            flow.Controls.Add(Theme.CreateFieldGroup("Clé du paramètre", _txtCle, 210));
            flow.Controls.Add(Theme.CreateFieldGroup("Valeur", _txtValeur, 220));

            var btnSave = Theme.CreatePrimaryButton("Enregistrer", 120, 34);
            btnSave.Margin = new Padding(4, 20, 10, 4);
            btnSave.Click += (_, _) => SaveParam();

            var btnBackup = Theme.CreateSecondaryButton("Sauvegarder MySQL (.sql)", 190, 34);
            btnBackup.Margin = new Padding(0, 20, 10, 4);
            btnBackup.Click += (_, _) => BackupMySqlDatabase();

            var btnLogo = Theme.CreateSecondaryButton("Changer Logo Client", 165, 34);
            btnLogo.Margin = new Padding(0, 20, 0, 4);
            btnLogo.Click += (_, _) => ChangeClientLogo();

            flow.Controls.AddRange(new Control[] { btnSave, btnBackup, btnLogo });
            topCard.Controls.Add(flow);

            var listCard = Theme.CreateCardPanel();
            listCard.Dock = DockStyle.Fill;
            _gridParams.SelectionChanged += (_, _) =>
            {
                if (_gridParams.CurrentRow?.Cells["Clé"].Value is string cle)
                {
                    _txtCle.Text = cle;
                    _txtValeur.Text = _gridParams.CurrentRow.Cells["Valeur"].Value?.ToString() ?? "";
                }
            };
            listCard.Controls.Add(_gridParams);

            tabParams.Controls.Add(listCard);
            tabParams.Controls.Add(new Panel { Dock = DockStyle.Top, Height = 12, BackColor = Theme.PageBg });
            tabParams.Controls.Add(topCard);
            tabs.TabPages.Add(tabParams);
            LoadParams();
        }

        Controls.Add(tabs);
    }

    private void ChangePassword()
    {
        var (okPwd, pwdMsg) = SecurityService.ValidatePasswordStrength(_txtNewPwd.Text);
        if (!okPwd)
        {
            MessageBox.Show(pwdMsg, "Politique de mot de passe", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_txtNewPwd.Text != _txtConfirmPwd.Text)
        {
            MessageBox.Show("La confirmation ne correspond pas au nouveau mot de passe.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_auth.ChangePassword(UserSession.CurrentUser!.Id, _txtOldPwd.Text, _txtNewPwd.Text))
        {
            _txtOldPwd.Clear(); _txtNewPwd.Clear(); _txtConfirmPwd.Clear();
            MessageBox.Show("Mot de passe modifié avec succès !", "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        else
        {
            MessageBox.Show("Le mot de passe actuel est incorrect.", "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void LoadParams()
    {
        _gridParams.DataSource = _repo.GetParametres().Select(p => new
        {
            Clé = p.CleParam,
            Valeur = p.ValeurParam,
            Description = p.Description
        }).ToList();
    }

    private void SaveParam()
    {
        if (string.IsNullOrWhiteSpace(_txtCle.Text)) return;
        _repo.SaveParametre(_txtCle.Text.Trim(), _txtValeur.Text.Trim(), UserSession.CurrentUser!.Id);
        LoadParams();
        MessageBox.Show("Paramètre mis à jour.", "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BackupMySqlDatabase()
    {
        using var sfd = new SaveFileDialog
        {
            Filter = "Sauvegarde SQL (*.sql)|*.sql",
            FileName = $"Backup_iit_fincore_{DateTime.Now:yyyyMMdd_HHmm}.sql"
        };
        if (sfd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var dumpExe = FindMySqlDump();
            if (dumpExe == null)
            {
                MessageBox.Show("mysqldump.exe introuvable (vérifiez WampServer/MySQL).", "Sauvegarde MySQL", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = dumpExe,
                Arguments = "--default-character-set=utf8mb4 -h 127.0.0.1 -P 3306 -u root --routines --triggers iit_fincore",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = System.Text.Encoding.UTF8
            };

            using var proc = System.Diagnostics.Process.Start(psi)!;
            var sqlDump = proc.StandardOutput.ReadToEnd();
            var err = proc.StandardError.ReadToEnd();
            proc.WaitForExit();

            if (proc.ExitCode != 0)
                throw new InvalidOperationException(string.IsNullOrWhiteSpace(err) ? "Échec de mysqldump." : err);

            File.WriteAllText(sfd.FileName, sqlDump, System.Text.Encoding.UTF8);
            _auth.LogActivity(UserSession.CurrentUser!.Id, "Sauvegarde MySQL", "Paramètres", "iit_fincore", null, Path.GetFileName(sfd.FileName));
            MessageBox.Show($"Sauvegarde MySQL complète enregistrée :\n{sfd.FileName}", "IIT FINCORE - Sauvegarde réussie", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Erreur lors de la sauvegarde : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private static string? FindMySqlDump()
    {
        string[] roots = { @"C:\wamp64\bin\mysql", @"C:\wamp\bin\mysql", @"C:\xampp\mysql\bin", @"C:\Program Files\MySQL" };
        foreach (var root in roots)
        {
            if (!Directory.Exists(root)) continue;
            var found = Directory.GetFiles(root, "mysqldump.exe", SearchOption.AllDirectories).FirstOrDefault();
            if (found != null) return found;
        }
        return null;
    }

    private void ChangeClientLogo()
    {
        using var ofd = new OpenFileDialog
        {
            Title = "Choisir le logo de l'organisation",
            Filter = "Images (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png"
        };
        if (ofd.ShowDialog() != DialogResult.OK) return;

        try
        {
            var destDir = Path.Combine(AppContext.BaseDirectory, "Assets");
            Directory.CreateDirectory(destDir);
            var destPath = Path.Combine(destDir, "logo_fincore.jpg");
            Theme.ResetLogoCache();
            File.Copy(ofd.FileName, destPath, overwrite: true);
            _auth.LogActivity(UserSession.CurrentUser!.Id, "Changement de logo client", "Paramètres", null, null, Path.GetFileName(ofd.FileName));
            MessageBox.Show("Logo client mis à jour ! Il apparaîtra sur tous les écrans et les rapports PDF (redémarrez ou changez de page pour actualiser).",
                "IIT FINCORE", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show("Impossible de changer le logo : " + ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

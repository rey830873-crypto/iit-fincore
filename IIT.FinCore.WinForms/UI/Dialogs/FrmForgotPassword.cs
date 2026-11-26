using System.Drawing;
using IIT.FinCore.WinForms.Data;

namespace IIT.FinCore.WinForms.UI.Dialogs;

public class FrmForgotPassword : Form
{
    private readonly AuthRepository _auth = new();
    private readonly TextBox _txtEmail = new();
    private readonly TextBox _txtPhone = new();
    private readonly TextBox _txtNewPwd = new();
    private readonly TextBox _txtConfirm = new();

    public FrmForgotPassword()
    {
        Text = "IIT FINCORE - Réinitialisation sécurisée";
        Size = new Size(465, 485);
        StartPosition = FormStartPosition.CenterParent;
        DoubleBuffered = true;

        var header = Theme.CreateDialogHeader("Mot de passe oublié", "Vérification d'identité requise avant réinitialisation");
        Theme.ApplyModernDialogChrome(this, header);

        var footer = new Panel { Dock = DockStyle.Bottom, Height = 64, BackColor = Color.White, Padding = new Padding(18, 12, 22, 12) };
        footer.Paint += (_, e) => e.Graphics.DrawLine(new Pen(Theme.BorderColor, 1), 0, 0, footer.Width, 0);

        var btnReset = Theme.CreatePrimaryButton("Réinitialiser", 145, 38);
        btnReset.Dock = DockStyle.Right;
        btnReset.Click += (_, _) => DoReset();

        var spacer = new Panel { Dock = DockStyle.Right, Width = 10, BackColor = Color.White };

        var btnCancel = Theme.CreateSecondaryButton("Annuler", 105, 38);
        btnCancel.Dock = DockStyle.Right;
        btnCancel.Click += (_, _) => Close();

        footer.Controls.Add(btnCancel);
        footer.Controls.Add(spacer);
        footer.Controls.Add(btnReset);

        var wrap = new Panel { Dock = DockStyle.Fill, Padding = new Padding(18, 14, 18, 14), BackColor = Theme.PageBg };
        var card = Theme.CreateCardPanel();
        card.Dock = DockStyle.Fill;

        int y = 18;
        card.Controls.Add(new Label { Text = "Adresse e-mail du compte", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _txtEmail.SetBounds(22, y, 380, 28); _txtEmail.Font = Theme.FontBody; _txtEmail.PlaceholderText = "exemple@iit.ci";
        y += 46;

        card.Controls.Add(new Label { Text = "Téléphone enregistré (vérification d'identité)", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _txtPhone.SetBounds(22, y, 380, 28); _txtPhone.Font = Theme.FontBody; _txtPhone.PlaceholderText = "Ex : 07 00 00 00 01";
        y += 46;

        card.Controls.Add(new Label { Text = "Nouveau mot de passe (min. 8 car., Maj, min, chiffre)", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _txtNewPwd.SetBounds(22, y, 380, 28); _txtNewPwd.Font = Theme.FontBody; _txtNewPwd.UseSystemPasswordChar = true;
        y += 46;

        card.Controls.Add(new Label { Text = "Confirmer le nouveau mot de passe", Font = new Font("Segoe UI", 8.8F, FontStyle.Bold), ForeColor = Theme.TextMuted, Location = new Point(22, y), AutoSize = true });
        y += 22;
        _txtConfirm.SetBounds(22, y, 380, 28); _txtConfirm.Font = Theme.FontBody; _txtConfirm.UseSystemPasswordChar = true;

        card.Controls.AddRange(new Control[] { _txtEmail, _txtPhone, _txtNewPwd, _txtConfirm });
        wrap.Controls.Add(card);
        Controls.Add(wrap);
        Controls.Add(footer);
        Controls.Add(header);
    }

    private void DoReset()
    {
        if (string.IsNullOrWhiteSpace(_txtEmail.Text) || string.IsNullOrWhiteSpace(_txtPhone.Text))
        {
            MessageBox.Show("Veuillez saisir votre e-mail et votre numéro de téléphone enregistré.", "Vérification requise", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        if (_txtNewPwd.Text != _txtConfirm.Text)
        {
            MessageBox.Show("Les mots de passe ne correspondent pas.", "Validation", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            var (ok, msg) = _auth.ResetPasswordVerified(_txtEmail.Text, _txtPhone.Text, _txtNewPwd.Text);
            MessageBox.Show(msg, ok ? "IIT FINCORE" : "Sécurité", MessageBoxButtons.OK, ok ? MessageBoxIcon.Information : MessageBoxIcon.Warning);
            if (ok) Close();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Erreur", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }
}

using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.UI;

namespace IIT.FinCore.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        using (var splash = new FrmSplash())
        {
            splash.ShowDialog();
        }

        using (var onboarding = new FrmOnboarding())
        {
            onboarding.ShowDialog();
        }

        try
        {
            var auth = new AuthRepository();
            auth.EnsureDemoPasswordsHashed();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                "Impossible de se connecter à la base MySQL 'iit_fincore'.\n" +
                "Vérifiez que WampServer (MySQL 3306) est bien démarré.\n\nDétail : " + ex.Message,
                "IIT FINCORE - Erreur Base de Données",
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            return;
        }

        while (true)
        {
            using var login = new FrmLogin();
            if (login.ShowDialog() != DialogResult.OK)
                break;

            using var main = new FrmMain();
            Application.Run(main);

            if (!main.RequestedLogout)
                break;
        }
    }
}

using IIT.FinCore.WinForms.Data;
using IIT.FinCore.WinForms.UI;

namespace IIT.FinCore.WinForms;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        // Préchauffe la connexion MySQL et les mots de passe démo en parallèle pendant le splash screen
        Exception? dbStartupError = null;
        var dbInitTask = Task.Run(() =>
        {
            try
            {
                var auth = new AuthRepository();
                auth.EnsureDemoPasswordsHashed();
            }
            catch (Exception ex)
            {
                dbStartupError = ex;
            }
        });

        using (var splash = new FrmSplash())
        {
            splash.ShowDialog();
        }

        using (var onboarding = new FrmOnboarding())
        {
            onboarding.ShowDialog();
        }

        dbInitTask.GetAwaiter().GetResult();
        if (dbStartupError != null)
        {
            MessageBox.Show(
                "Impossible de se connecter à la base MySQL 'iit_fincore'.\n" +
                "Vérifiez que WampServer (MySQL 3306) est bien démarré.\n\nDétail : " + dbStartupError.Message,
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

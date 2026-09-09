using KMC.OrganizerDesktop.Forms;

namespace KMC.OrganizerDesktop
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Application.Run(new LoginForm());
        }
    }
}
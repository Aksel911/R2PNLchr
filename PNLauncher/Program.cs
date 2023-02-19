namespace PNLauncher
{
    using System;
    using System.Security.Principal;
    using System.Windows.Forms;

    internal static class Program
    {
        [STAThread]
        private static void Main(string[] args)
        {
            bool flag = false;
            bool flag2 = false;
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-admin")
                {
                    flag = true;
                }
                else if (args[i] == "-ignore_launcher")
                {
                    flag2 = true;
                }
            }
            if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                MessageBox.Show("Запустите программу от имени администратора!", "P&N", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            }
            else
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new MainForm(flag, flag2));
            }
        }
    }
}


using System;
using System.Windows.Forms;
using AplikasiPerpustakaanSiJess.UI.Forms;

namespace AplikasiPerpustakaanSiJess
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new LoginForm());
        }
    }
}

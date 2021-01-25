using System;
using System.Windows.Forms;

namespace ChuckHill2.LoggerEditor
{
    static class Program
    {
        [STAThread] static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new FormMain());
            Environment.Exit(0);
        }
    }
}


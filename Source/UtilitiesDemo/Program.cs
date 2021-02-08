using System;
using System.Windows.Forms;

namespace UtilitiesDemo
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            ChuckHill2.Forms.MiniMessageBox.ShowDialog(null, "This shows MiniMessageBox working without any other forms existing. Press OK to continue.", "MiniMessageBox Test", MessageBoxButtons.OK,MessageBoxIcon.Information);
            Application.Run(new FormMain());
        }
    }
}

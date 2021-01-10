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
            //ChuckHill2.MiniMessageBox.ShowDialog(null, "This is the message body.", "This is the caption",MessageBoxButtons.OKCancel,MessageBoxIcon.Question);
            Application.Run(new FormMain());
        }
    }
}

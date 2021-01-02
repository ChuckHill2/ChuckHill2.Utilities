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
            //ChuckHill2.Utilities.MiniMessageBox.ShowDialog(null, "This is the message body.", "This is the caption");
            Application.Run(new FormMain());
        }
    }
}

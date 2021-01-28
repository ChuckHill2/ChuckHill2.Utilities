using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChuckHill2.Extensions;

namespace ChuckHill2.LoggerEditor
{
    public partial class HelpPopup : Form
    {
        public enum HelpItem
        {
            Main,
            Trace,
            Switches,
            Sources,
            SharedListeners
        }

        public static void Show(IWin32Window owner, HelpItem helpItem)
        {
            var stream = typeof(HelpPopup).GetManifestResourceStream(helpItem.ToString() + ".rtf");
            if (stream == null) return; //should never happen
            using(var dlg = new HelpPopup())
            {
                using (var sr = new StreamReader(stream))
                    dlg.m_rtfHelp.Rtf = sr.ReadToEnd();

                dlg.ShowDialog(owner);
            }
        }

        private HelpPopup()
        {
            InitializeComponent();
        }
    }
}

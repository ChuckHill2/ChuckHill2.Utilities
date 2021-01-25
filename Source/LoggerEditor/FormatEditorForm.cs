using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuckHill2.LoggerEditor
{
    public partial class FormatEditorForm : Form
    {
        public static string Show(IWin32Window owner, string formatString)
        {
            using (var dlg = new FormatEditorForm())
            {
                dlg.m_ctlBuilderEditor.Text = formatString;
                return dlg.ShowDialog(owner) == DialogResult.OK ? dlg.m_ctlBuilderEditor.Text.Trim() : null;
            }
        }

        private FormatEditorForm()
        {
            InitializeComponent();
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}

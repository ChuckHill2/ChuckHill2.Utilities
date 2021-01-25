using System;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Resources;
using System.Windows.Forms;
using ChuckHill2.Forms;

namespace ChuckHill2.LoggerEditor
{
    public partial class ListenerChooser : Form
    {
        public static Type Show(IWin32Window owner, Type defalt)
        {
            using (var dlg = new ListenerChooser())
            {
                dlg.Listener = defalt;
                return dlg.ShowDialog(owner) == DialogResult.OK ? dlg.Listener : null;
            }
        }

        private Type Listener
        {
            get
            {
                return (Type)(m_lvListeners.SelectedItems.Count > 0 ? m_lvListeners.SelectedItems[0].Tag : null);
            }
            set
            {
                if (value == null) return;
                var item = m_lvListeners.Items.Cast<ListViewItem>().FirstOrDefault(m => m.Tag.Equals(value));
                if (item == null) return;
                item.Selected = true;
                item.Focused = true;
            }
        }

        private ListenerChooser()
        {
            InitializeComponent();

            m_lvListeners.Groups.Add(new ListViewGroup("Custom") { Name = "Custom" });
            m_lvListeners.Groups.Add(new ListViewGroup("System") { Name = "System" });

            m_lvListeners.SmallImageList = new ImageList();

            foreach (var t in ListenersCtrl.ListenerTypes)
            {
                AddListViewItem(t.Key, t.Value.Description);
            }
        }

        private static readonly ResourceManager ResMan = new ResourceManager(typeof(ChuckHill2.LoggerEditor.Properties.Resources));
        private void AddListViewItem(Type listenerType, string tooltip)
        {
            m_lvListeners.SmallImageList.Images.Add(listenerType.Name, (Image)ResMan.GetObject(listenerType.Name));

            var item = new ListViewItem(listenerType.Name);
            item.Name = listenerType.Name;
            item.ToolTipText = tooltip;
            item.Group = m_lvListeners.Groups[listenerType.Namespace=="System.Diagnostics"?1:0];
            item.Tag = listenerType;
            item.ImageKey = listenerType.Name;

            m_lvListeners.Items.Add(item);
        }

        private void m_btnOK_Click(object sender, EventArgs e)
        {
            if (m_lvListeners.SelectedItems.Count==0)
            {
                MiniMessageBox.ShowDialog(this, "No listener selected.", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            this.DialogResult = DialogResult.OK;
        }

        private void m_btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
        }
    }
}

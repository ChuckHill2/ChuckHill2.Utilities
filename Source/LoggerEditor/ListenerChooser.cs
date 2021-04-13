//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ListenerChooser.cs" company="Chuck Hill">
// Copyright (c) 2020 Chuck Hill.
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public License
// as published by the Free Software Foundation; either version 2.1
// of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
//
// The GNU Lesser General Public License can be viewed at
// http://www.opensource.org/licenses/lgpl-license.php. If
// you unfamiliar with this license or have questions about
// it, here is an http://www.gnu.org/licenses/gpl-faq.html.
//
// All code and executables are provided "as is" with no warranty
// either express or implied. The author accepts no liability for
// any damage or loss of business that this product may cause.
// </copyright>
// <repository>https://github.com/ChuckHill2/ChuckHill2.Utilities</repository>
// <author>Chuck Hill</author>
//--------------------------------------------------------------------------
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

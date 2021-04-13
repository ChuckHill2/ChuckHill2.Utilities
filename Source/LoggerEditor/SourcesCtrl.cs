//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="SourcesCtrl.cs" company="Chuck Hill">
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
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;
using ChuckHill2;
using ChuckHill2.Forms;
using ChuckHill2.Extensions;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel.DataAnnotations.Schema;
using System.Windows.Forms.Design;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Collections;

namespace ChuckHill2.LoggerEditor
{
    public partial class SourcesCtrl : UserControl
    {
        private XmlElement PrevNode;

        private static readonly List<string> SpecialSourceLevels = new List<string>() { SourceLevels.Off.ToString(), SourceLevels.All.ToString() };
        private static readonly string[] SpecialSources = new string[] { "TRACE", "CONSOLE", "FIRSTCHANCE" };
        private readonly List<string> KnownSourceLevels = Enum.GetNames(typeof(SourceLevels)).ToList();

        public SourcesCtrl()
        {
            InitializeComponent();
            m_cmbName.DataSource = SpecialSources;
            m_cmbName.TextUpdate += (s, e) =>
            {
                var name = m_cmbName.Text;
                if (m_cmbName.Tag != null) ((SourceItem)m_cmbName.Tag).Name = name;
                SetSourceLevelDataSource(name);
            };
            m_cmbName.SelectedValueChanged += (s, e) =>
            {
                var name = (string)m_cmbName.SelectedItem;
                if (m_cmbName.Tag != null)
                {
                    ((SourceItem)m_cmbName.Tag).Name = name;
                    SetSourceLevelDataSource(((SourceItem)m_cmbName.Tag).Name);
                }
            };
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlElement Node
        {
            get
            {
                var xdoc = PrevNode?.OwnerDocument ?? new XmlDocument();

                //Retrieve the changes from the current item.
                if (m_lvSources.SelectedItems.Count>0)
                    m_lvSources_ItemSelectionChanged(m_lvSources,
                        new ListViewItemSelectionChangedEventArgs(m_lvSources.SelectedItems[0], m_lvSources.SelectedIndices[0], false));

                var sb = new StringBuilder();
                foreach (var prop in m_lvSources.Items.Cast<ListViewItem>().Select(m=>(SourceItem)m.Tag))
                {
                    prop.XmlComments.ForEach((c) => sb.Append(c.OuterXml));

                    var attrName = Enum.TryParse<SourceLevels>(prop.SourceLevel, out var dummy) ? "switchValue" : "switchName";
                    sb.Append($"<source name=\"{prop.Name}\" {attrName}=\"{prop.SourceLevel}\"><listeners>");
                    if (prop.Listeners.Count > 0) sb.Append("<clear/>");
                    foreach(string listener in prop.Listeners)
                    {
                        sb.Append($"<add name=\"{listener}\"/>");
                    }
                    sb.Append("</listeners></source>");
                }
                var node = (XmlElement)xdoc.CreateElement("sources");
                node.InnerXml = sb.ToString();
                return node;
            }
            set
            {
                if (value == null) { Clear(); return; }
                if (value.Name != "sources") throw new ArgumentException("Node is not a <sources> node.");
                PrevNode = value;

                foreach (XmlElement source in value.SelectNodes("source"))
                {
                    var item = new SourceItem();

                    //Save xml comments in order to restore them
                    XmlNode n = source.PreviousSibling;
                    while (n is XmlComment)
                    {
                        item.XmlComments.Insert(0, (XmlComment)n);
                        n = n.PreviousSibling;
                    }

                    item.Name = source.Attributes["name"]?.Value ?? "UNKNOWN";
                    item.SourceLevel = source.Attributes["switchValue"]?.Value ?? source.Attributes["switchName"]?.Value ?? SourceLevels.Off.ToString();

                    if (SpecialSources.Any(m=>m.EqualsI(item.Name)) && !SpecialSourceLevels.Any(m => m.EqualsI(item.SourceLevel))) item.SourceLevel = SourceLevels.Off.ToString();

                    foreach (XmlElement listener in source.SelectNodes("listeners/add"))
                    {
                        var listenerName = listener.Attributes["name"]?.Value;
                        if (listenerName == null) continue;
                        int index = m_clbListeners.Items.IndexOf(listenerName);
                        if (index == -1) continue;
                        item.Listeners.Add(listenerName);
                    }

                    AddListViewItem(item);
                }

                if (m_lvSources.Items.Count > 0) //set to the first item
                {
                    var item = m_lvSources.Items[0];
                    item.Selected = true;
                    item.Focused = true;
                }
            }
        }

        public void Clear()
        {
            m_cmbName.Tag = null;
            m_cmbName.Text = null;
            m_clbListeners.Items.Clear();
            m_lvSources.Items.Clear();
            KnownSourceLevels.Clear();
            KnownSourceLevels.AddRange(Enum.GetNames(typeof(SourceLevels)));
            m_cmbSourceLevel.SelectedIndex = -1;
            m_cmbSourceLevel.DataSource = KnownSourceLevels;
            PrevNode = null;
        }

        public void ReplaceListeners(string[] listeners)
        {
            var checkedItems = m_clbListeners.CheckedItems.Cast<string>().ToList();
            m_clbListeners.SuspendLayout();
            m_clbListeners.Items.Clear();
            m_clbListeners.Items.AddRange(listeners);
            foreach (string name in checkedItems)
            {
                int index = m_clbListeners.Items.IndexOf(name);
                if (index == -1) continue;
                m_clbListeners.SetItemChecked(index, true);
            }
            m_clbListeners.ResumeLayout();
        }

        public void ReplaceSwitches(string[] switches)
        {
            KnownSourceLevels.Clear();
            KnownSourceLevels.AddRange(Enum.GetNames(typeof(SourceLevels)));
            KnownSourceLevels.AddRange(switches);
            SetSourceLevelDataSource(m_cmbName.Text);
        }

        private void SetSourceLevelDataSource(string sourceName)
        {
            //Update the 'Source levels' combobox available choices based upon the 'Source Name'

            if (sourceName == null || sourceName.Length == 0) //dispose
            {
                m_cmbSourceLevel.DataSource = null;
                return;
            }

            var isSpecial = SpecialSources.Any(m => m.EqualsI(sourceName));
            if (m_cmbSourceLevel.DataSource == null) m_cmbSourceLevel.DataSource = isSpecial ? SpecialSourceLevels : KnownSourceLevels;
            else if (isSpecial && ((IList)m_cmbSourceLevel.DataSource).Count > 2)
            {
                m_cmbSourceLevel.DataSource = SpecialSourceLevels;
            }
            else if (!isSpecial && ((IList)m_cmbSourceLevel.DataSource).Count != KnownSourceLevels.Count)
            {
                m_cmbSourceLevel.DataSource = KnownSourceLevels;
            }
            //else no change
        }

        private ListViewItem AddListViewItem(SourceItem props)
        {
            const string toolTipText = "Source {0} is a special built-in source that copies these messages\r\nto a log. As there is no mechanism to to set severity for these sources,\r\nthere are no severity levels, logging is either On (All) or Off.";
            var item = new ListViewItem(props.Name);
            item.Name = props.Name;
            item.Tag = props;
            if (SpecialSources.Any(m => m.EqualsI(props.Name))) item.ToolTipText = string.Format(toolTipText, props.Name);

            props.NamePropertyChanging += (s, e) =>
            {
                if (m_lvSources.Items.Cast<ListViewItem>().Any(m => m.Text.EqualsI(e.NewValue)))
                {
                    MiniMessageBox.ShowDialog(m_cmbName, "Duplicate Source Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;

                    //Callstack to this point:
                    // m_cmbName.TextUpdate => props.Name => props.NamePropertyChanging
                    // m_cmbName.SelectedValueChanged => props.Name => props.NamePropertyChanging

                    m_cmbName.Text = e.CurrentValue; //This works when the source is from TextUpdate, but not from SelectedValueChanged.
                    //To revert from SelectedValueChanged we need to update AFTER the event. Not within it. For TextUpdate this is merely redundent.
                    System.Threading.ThreadPool.QueueUserWorkItem((arg) =>
                    {
                        System.Threading.Thread.Sleep(5);
                        m_cmbName.BeginInvoke(new Action(() => { m_cmbName.Text = e.CurrentValue; }));
                    });
                    return;
                }

                if (SpecialSources.Any(m => m.EqualsI(e.NewValue)))
                    item.ToolTipText = string.Format(toolTipText, e.NewValue);
                else item.ToolTipText = "";

                item.Text = e.NewValue;
            };

            m_lvSources.Items.Add(item);
            return item;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_btnAddSource_Click(sender, e);
        }
        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_lvSources.SelectedItems.Count > 0)
            {
                var item = m_lvSources.SelectedItems[0];
                if (MiniMessageBox.ShowDialog(m_lvSources, $"Are you sure you want to remove {item.Name}?", "Remove Source", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    m_cmbName.Tag = null;
                    m_cmbName.Text = "";
                    m_cmbSourceLevel.Text = "";

                    m_lvSources.Items.Remove(item);
                    if (m_lvSources.Items.Count>0)
                    {
                        var firstItem = m_lvSources.Items[0];
                        firstItem.Selected = true;
                        firstItem.Focused = true;
                    }
                }
            }
        }

        private void m_lvSources_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            var props = (SourceItem)e.Item.Tag;
            if (e.IsSelected)
            {
                if (props == null)
                {
                    m_cmbName.Text = "";

                    m_cmbSourceLevel.Text = SourceLevels.Off.ToString();
                    SetSourceLevelDataSource(null);

                    for (int i = 0; i < m_clbListeners.Items.Count; i++) m_clbListeners.SetItemChecked(i, false);
                    return;
                }

                m_cmbName.Tag = props;
                m_cmbName.Text = props.Name;
                SetSourceLevelDataSource(props.Name);

                m_cmbSourceLevel.SelectedItem = props.SourceLevel;
               // m_cmbSourceLevel.Text = props.SourceLevel;
                for (int i = 0; i < m_clbListeners.Items.Count; i++)
                {
                    var found = props.Listeners.Any(m => m.EqualsI((string)m_clbListeners.Items[i]));
                    m_clbListeners.SetItemChecked(i, found);
                }
            }
            else
            {
                props.Name = m_cmbName.Text;
                props.SourceLevel = m_cmbSourceLevel.Text;
                props.Listeners.Clear();
                props.Listeners.AddRange(m_clbListeners.CheckedItems.Cast<string>());
            }
        }

        private void m_btnAddSource_Click(object sender, EventArgs e)
        {
            //find a unique default name
            var namePrefix = "Source_";
            int maxInt = 0;
            if (m_lvSources.Items.Count > 0)
                maxInt = m_lvSources.Items.OfType<ListViewItem>().Max(d => d.Text.Length > namePrefix.Length && int.TryParse(d.Text.Substring(namePrefix.Length), out int index) ? index : 0);
            var props = new SourceItem();

            props.Name = $"{namePrefix}{++maxInt}"; //There are no properties, so we have to assign a bogus unique name now. The user can change it in the UI.
            var item = AddListViewItem(props);
            item.Selected = true;
            item.Focused = true;
        }

        public class SourceItem
        {
            public event PropertyChangingEventHandler<string> NamePropertyChanging;

            public List<XmlComment> XmlComments { get; } = new List<XmlComment>(); //preserve XML comments

            private string __name = "";
            public string Name
            {
                get => __name;
                set
                {
                    value = value.ToIdentifier();
                    if (SpecialSources.Any(m => m.EqualsI(value))) value = value.ToUpper();
                    if (__name == value) return; //don't want to trigger NamePropertyChanged event unnecessarily.
                    if (NamePropertyChanging == null) { __name = value; return; }
                    var args = new PropertyChangingEventArgs<string>(nameof(Name), __name, value);
                    NamePropertyChanging(this, args);
                    if (args.Cancel) return;
                    __name = value;
                }
            }

            //includes enum SourceLevel values plus custom SwitchGroup values.
            public string SourceLevel { get; set; } = SourceLevels.Off.ToString();

            private List<string> __listeners = new List<string>();
            public List<string> Listeners => __listeners;
        }
    }

    /// <summary>
    /// Generic property changing event handler.
    /// </summary>
    /// <typeparam name="T">Type of value being changed.</typeparam>
    /// <param name="sender">The caller containing this property</param>
    /// <param name="e">Event args</param>
    public delegate void PropertyChangingEventHandler<T>(object sender, PropertyChangingEventArgs<T> e);
    /// <summary>
    /// Event args for PropertyChanging<T> event.
    /// </summary>
    /// <typeparam name="T">Type of value being changed.</typeparam>
    public class PropertyChangingEventArgs<T> : CancelEventArgs
    {
        /// <summary>
        /// Name of property changing.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Current value of property.
        /// </summary>
        public T CurrentValue { get; private set; }
        /// <summary>
        /// Candidate value of property. Value not committed if this.Cancel is set to true.
        /// </summary>
        public T NewValue { get; private set; }

        public PropertyChangingEventArgs(string propertyName, T currentPropertyValue, T newPropertyValue)
        {
            Name = propertyName;
            CurrentValue = currentPropertyValue;
            NewValue = newPropertyValue;
        }
    }
}

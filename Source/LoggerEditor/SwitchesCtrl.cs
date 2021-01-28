using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using ChuckHill2.Extensions;
using ChuckHill2.Forms;

namespace ChuckHill2.LoggerEditor
{
    public partial class SwitchesCtrl : UserControl
    {
        private XmlElement PrevNode;

        public SwitchesCtrl()
        {
            InitializeComponent();

            //Associate Enum column datasource with array of enum names.
            m_gridcolSourceLevel.DataSource = Enum.GetNames(typeof(SourceLevels));

            //replace fake newlines for real ones in column tooltips
            foreach (DataGridViewColumn c in m_grid.Columns)
            {
                c.ToolTipText = c.ToolTipText.Replace("\\n", "\r\n");
            }
        }

        private List<Data> SwitchList = new List<Data>(); //our core array of switch groups

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] KnownSwitches => m_gridBindingSource.List.Cast<Data>().Select(m => m.Name).ToArray();
        public event Action<string[]> SwitchesListChanged;
        private void OnSwitchesListChanged() { if (SwitchesListChanged != null) SwitchesListChanged(KnownSwitches); }

        //[Browsable(false)] //Must be visible to designer
        public List<Data> Groups => SwitchList; //exclusively used by  m_gridBindingSource.DataMember
        //public SortableBindingList<Data> Groups => new SortableBindingList<Data>(SwitchList); //Don't need as our list is not sortable..

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlElement Node
        {
            get
            {
                var xdoc = PrevNode?.OwnerDocument ?? new XmlDocument();
                if (PrevNode == null) return null;

                var sb = new StringBuilder();
                foreach (Data kv in m_gridBindingSource.List)
                {
                    sb.AppendFormat($"<add name=\"{kv.Name}\" value=\"{kv.SourceLevel}\"/>");
                }

                var node = xdoc.CreateElement("switches");
                // Can't use node.AppendChild(n); because it scrambles the order of the attributes. We always want 'name' and 'type' first for human readability.
                node.InnerXml = sb.ToString();
                return node;
            }
            set
            {
                if (value == null) { Clear(); return; }
                if (value.Name != "switches") throw new ArgumentException("Node is not a <switches> node.");
                PrevNode = value;

                foreach (var node in PrevNode.SelectNodes("add").OfType<XmlElement>())
                {
                    var name = node.Attributes["name"]?.Value;
                    var val = node.Attributes["value"]?.Value;
                    if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(val)) continue;
                    if (!Enum.TryParse<SourceLevels>(val, true, out var e)) continue;
                    SwitchList.Add(new Data(name, e));
                }

                //re-initialize grid with data.

                // DataSource=ChuckHill2.LoggerEditor.SwitchesCtrl  //Data class must be public
                // Member="Groups"
                // This works because 'this' has the public property 'Groups'. Property cannot have [Browsable(false)] attribute
                m_gridBindingSource.DataSource = this;

                // DataSource=ChuckHill2.LoggerEditor.SwitchesCtrl+Data //Data class must be public
                // Member=""
                // This would also work fine. Groups may be private.
                // m_gridBindingSource.DataSource = Groups;

                // m_gridBindingSource deleted.
                // This also works but rows cannot be added or removed.
                // Also cannot use Designer to configure columns.
                // m_grid.DataSource = SwitchList;
            }
        }

        public void Clear()
        {
            SwitchList.Clear();
            m_gridBindingSource.ResetBindings(false);
            PrevNode = null;
        }

        //Handle grid errors/exceptions in a safe and sane fashion
        private void m_grid_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            MessageBoxEx.Show(m_grid, e.Exception.Message, $"{e.Context} Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            e.ThrowException = false;
        }

        private void m_grid_UserDeletingRow(object sender, DataGridViewRowCancelEventArgs e)
        {
            var msg = $"Are you sure you want to delete\nswitch group \"{e.Row.Cells[nameof(m_gridcolName)].Value.ToString()}\"?";

            e.Cancel = MessageBoxEx.Show(m_grid, msg, "Delete Row", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No;
        }

        private void m_grid_DefaultValuesNeeded(object sender, DataGridViewRowEventArgs e)
        {
            //find a unique default name
            int index = 0;
            var namePrefix = "Group_";
            foreach (DataGridViewRow row in m_grid.Rows)
            {
                var name = row.Cells[nameof(m_gridcolName)].Value?.ToString();
                if (name == null) continue;
                if (name.Length < namePrefix.Length+1) continue;
                if (!name.StartsWith(namePrefix)) continue;
                if (!int.TryParse(name.Substring(namePrefix.Length), out int iindex)) continue;
                if (iindex > index) index = iindex;
            }

            e.Row.Cells[nameof(m_gridcolName)].Value = $"{namePrefix}{++index}";
        }

        private static readonly string[] KnownSeverities = Enum.GetNames(typeof(SourceLevels));
        private void m_grid_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {
            if (m_grid.Columns[nameof(m_gridcolName)].Index != e.ColumnIndex) return;
            var newName = e.FormattedValue.ToString();

            if (KnownSeverities.Any(m=>m.EqualsI(newName)))
            {
                MiniMessageBox.ShowDialog(m_grid, "Illegal Switch Name. Name cannot be the same as a trace level.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                e.Cancel = true;
                return;
            }

            foreach (DataGridViewRow row in m_grid.Rows)
            {
                if (row.Index == e.RowIndex) continue;
                var name = row.Cells[nameof(m_gridcolName)].Value?.ToString();
                if (name.EqualsI(newName))
                {
                    MiniMessageBox.ShowDialog(m_grid, "Duplicate Switch Name", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Cancel = true;
                    return;
                }
            }
        }

        private void m_grid_UserAddedRow(object sender, DataGridViewRowEventArgs e) => OnSwitchesListChanged();
        private void m_grid_UserDeletedRow(object sender, DataGridViewRowEventArgs e) => OnSwitchesListChanged();
        private void m_grid_CurrentCellChanged(object sender, EventArgs e) { if (m_grid.CurrentCell != null && m_grid.CurrentCell.ColumnIndex == 0) OnSwitchesListChanged(); }

        public class Data //m_grid Item-Row. Each property represents Columns
        {
            private string __name = ""; //see m_grid_DefaultValuesNeeded() for defult value.
            public string Name
            {
                get => __name;
                set => __name = value.ToIdentifier(); //Ensure name is a valid name.
            }

            //This is not used in the grid column as the grid does not understand enums. Tnis is not used/hidden in the griid
            public SourceLevels SourceLevel { get; set; } = SourceLevels.Off;
            //Grid does not understand Enums, so we have to convert to a string.
            public string SourceLevelString
            {
                get => SourceLevel.ToString();
                set => SourceLevel = Enum.TryParse<SourceLevels>(value, true, out var sl) ? sl : SourceLevels.Off;
            }

            public Data() { } //Used by grid 'Add'
            public Data(string n, string sl) { Name = n; SourceLevelString = sl; } //used by grid update
            public Data(string n, SourceLevels sl) { Name = n; SourceLevel = sl; } //used by us

            public Data(Data d) : this(d.Name, d.SourceLevel) { } //Clone. Dont know where this is used.
            public override string ToString() => $"\"{Name}\" = {SourceLevel}"; //for debugging.
        }
    }
}

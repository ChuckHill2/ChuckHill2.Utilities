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
using ChuckHill2.Extensions;
using ChuckHill2.Forms;
using ChuckHill2;

namespace ChuckHill2.LoggerEditor
{
    public partial class TraceCtrl : UserControl
    {
        private XmlElement PrevNode;

        public TraceCtrl()
        {
            InitializeComponent();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlElement Node
        {
            get
            {
                var xdoc = PrevNode?.OwnerDocument ?? new XmlDocument();
                if (PrevNode == null) return null;
                var sb = new StringBuilder($"<trace autoflush=\"{m_radAutoFlushYes.Checked.ToString().ToLower()}\" indentsize=\"{Decimal.ToInt32(m_numIndentSize.Value)}\"><listeners>");
                if (m_clbListeners.CheckedItems.Count > 0) sb.Append("<clear/>");

                foreach (string listener in m_clbListeners.CheckedItems)
                {
                    sb.AppendFormat($"<add name=\"{listener}\"/>");
                }
                sb.Append("</listeners></trace>");

                var node = (XmlElement)xdoc.CreateElement("trace");
                node.InnerXml = sb.ToString();
                return (XmlElement)node.FirstChild;
            }
            set
            {
                if (value == null)
                {
                    PrevNode = null;
                    return;
                }

                if (value.Name != "trace") throw new ArgumentException("Node is not a <trace> node.");
                PrevNode = value;

                bool autoflush = (value.Attributes["autoflush"]?.Value).CastTo<bool>();
                (autoflush ? m_radAutoFlushYes : m_radAutoFlushNo).Checked = true;

                m_numIndentSize.Value = (value.Attributes["indentsize"]?.Value).CastTo<int>(4);

                foreach (var node in PrevNode.SelectNodes("listeners/add").OfType<XmlElement>())
                {
                    var name = node.Attributes["name"]?.Value;
                    int index = m_clbListeners.Items.IndexOf(name);
                    if (index == -1) continue;
                    m_clbListeners.SetItemChecked(index, true);
                }
            }
        }

        public void ReplaceListeners(string[] listeners)
        {
            var checkedItems = m_clbListeners.CheckedItems.Cast<string>().ToArray();

            m_clbListeners.SuspendLayout();
            m_clbListeners.Items.Clear();
            m_clbListeners.Items.AddRange(listeners);
            foreach(string name in checkedItems)
            {
                int index = m_clbListeners.Items.IndexOf(name);
                if (index == -1) continue;
                m_clbListeners.SetItemChecked(index, true);
            }
            m_clbListeners.ResumeLayout();
        }
    }
}

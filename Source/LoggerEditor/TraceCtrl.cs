//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="TraceCtrl.cs" company="Chuck Hill">
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
                if (value == null) { Clear(); return; }
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

        public void Clear()
        {
            m_clbListeners.Items.Clear();
            m_numIndentSize.Value = 4;
            m_radAutoFlushNo.Checked = false;
            PrevNode = null;
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

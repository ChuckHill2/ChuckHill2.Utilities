using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using ChuckHill2.Extensions;
using ChuckHill2.Forms;

/// <summary>
/// This standalone tool edits the System.Diagnostics block of any app.config.
/// This does not use any 3rd-party assemblies.
/// </summary>
/// <remarks>
/// System.Diagnostics no longer exists in .NET Core app.config. However one may be able to bring it
/// back by adding the following to the top of app.config.
/// <configSections>
///    < section name = "system.diagnostics" type = "System.Diagnostics.DiagnosticsConfigurationHandler" />
/// </configSections>
/// However, DiagnosticsConfigurationHandler is also deprecated. I don't know how long this logging
/// mechanism will last as the lemmings are migrating to .Net Core before it is a complete solution.
/// <see cref="https://stackoverflow.com/questions/57078166/net-core-using-system-diagnostics-in-app-config"/>
/// </remarks>
namespace ChuckHill2.LoggerEditor
{
    public partial class FormMain : Form
    {
        private XmlDocument XDoc;
        private XmlElement SystemDiagnosticsWorkNode;

        public FormMain()
        {
            InitializeComponent();
            HelpInit();
            m_btnCommit.Enabled = false;

            try
            {
                string[] args = Environment.GetCommandLineArgs();
                if (args.Length > 1 && File.Exists(args[1]))
                {
                    m_appConfigFile.Text = Path.GetFullPath(args[1]);
                    LoadXDoc();
                }
                else LoadXDoc();
            }
            catch { }
        }

        //Only attempt to save if modified.
        private bool __isDirty;
        private bool IsDirty
        {
            get
            {
                if (!__isDirty && SystemDiagnosticsWorkNode != null)
                    __isDirty = !XmlEquals(SystemDiagnosticsWorkNode, (XmlElement)XDoc.SelectSingleNode("/configuration/system.diagnostics"));

                return __isDirty;
            }
            set => __isDirty = value;
        }

        private void LoadXDoc()
        {
            XmlDocument xdoc = new XmlDocument();
            SystemDiagnosticsWorkNode = null;
            IsDirty = false;

            m_ListenersControl.Clear();
            m_SwitchesControl.Clear();
            m_SourcesControl.Clear();
            m_TraceControl.Clear();

            if (string.IsNullOrWhiteSpace(m_appConfigFile.Text))
            {
                //Start with a new app.config with new system.diagnostics node 
                m_appConfigFile.Text = string.Empty;
                xdoc.LoadXml(@"<?xml version=""1.0""?><configuration><system.diagnostics><trace autoflush=""false"" indentsize=""4""><listeners></listeners></trace><switches /><sources /><sharedListeners /></system.diagnostics></configuration>");
                IsDirty = true;
            }
            else
            {
                //Attempt to load Xml application config file. Popup messagebox upon error.
                try
                {
                    XmlReaderSettings settings = new XmlReaderSettings();
                    settings.CloseInput = true;
                    //settings.IgnoreComments = true;  //XmlComment
                    settings.IgnoreWhitespace = true; //XmlWhitespace
                    settings.IgnoreProcessingInstructions = true;

                    using (XmlReader reader = XmlReader.Create(m_appConfigFile.Text, settings))
                        xdoc.Load(reader);

                    if (xdoc.SelectSingleNode("/configuration") == null)
                        throw new NotSupportedException("File is not an application configuration file");
                }
                catch (Exception ex)
                {
                    MessageBoxEx.Show(this, ex.FullMessage("\r\n    "), this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //If this application config does not have a system.diagnostics node, add it.
                var sysDiag = xdoc.SelectSingleNode("/configuration/system.diagnostics");
                if (sysDiag == null)
                {
                    var conf = xdoc.SelectSingleNode("/configuration");
                    conf.AppendChild(xdoc.CreateElement("system.diagnostics")).InnerXml = @"<trace autoflush=""false"" indentsize=""4""><listeners></listeners></trace><switches /><sources /><sharedListeners />";
                    IsDirty = true;
                }
            }

            XDoc = xdoc;
            SystemDiagnosticsWorkNode = (XmlElement)xdoc.SelectSingleNode("/configuration/system.diagnostics").CloneNode(true);

            m_btnCommit.Enabled = true;

            m_ListenersControl.Node = (XmlElement)xdoc.SelectSingleNode("/configuration/system.diagnostics/sharedListeners");

            m_SwitchesControl.Node = (XmlElement)xdoc.SelectSingleNode("/configuration/system.diagnostics/switches");

            m_SourcesControl.ReplaceListeners(m_ListenersControl.KnownListeners);  //update listeners list in SourceControl before populating.
            m_SourcesControl.ReplaceSwitches(m_SwitchesControl.KnownSwitches);     //update switches list in SourceControl before populating.
            m_SourcesControl.Node = (XmlElement)xdoc.SelectSingleNode("/configuration/system.diagnostics/sources");

            m_TraceControl.ReplaceListeners(m_ListenersControl.KnownListeners);    //update listeners list in traceControl before populating.
            m_TraceControl.Node = (XmlElement)xdoc.SelectSingleNode("/configuration/system.diagnostics/trace");

            //Dynamically update dependent trace and sources control
            m_ListenersControl.ListenerListChanged += m_ListenersControl_ListenerListChanged;
            m_SwitchesControl.SwitchesListChanged += m_SwitchesControl_SwitchesListChanged;
        }

        private void m_ListenersControl_ListenerListChanged(string[] list)
        {
            m_TraceControl.ReplaceListeners(list);
            m_SourcesControl.ReplaceListeners(list);
        }

        private void m_SwitchesControl_SwitchesListChanged(string[] list)
        {
            m_SourcesControl.ReplaceSwitches(list);
        }

        private void m_btnCommit_Click(object sender, EventArgs e)
        {
            UpdateWorkNode();

            if (IsDirty)
            {
                var filename = GetSaveFilename();
                if (filename == null) return; //user cancelled save dialog

                var oldNode = XDoc.SelectSingleNode("/configuration/system.diagnostics");
                oldNode.ParentNode.ReplaceChild(SystemDiagnosticsWorkNode, oldNode);

                XmlWriterSettings ws = new XmlWriterSettings();
                ws.CloseOutput = true;
                ws.Indent = true;
                ws.IndentChars = "  ";
                ws.NewLineChars = Environment.NewLine;
                ws.NewLineHandling = NewLineHandling.Replace;
                ws.NewLineOnAttributes = false;
                using (XmlWriter writer = XmlWriter.Create(filename, ws))
                    XDoc.Save(writer);

                IsDirty = false;
            }

            //this.Close();  //User must click exit
        }

        private void UpdateWorkNode()
        {
            XmlNode node;

            node = SystemDiagnosticsWorkNode.SelectSingleNode("switches");
            node.InnerXml = m_SwitchesControl.Node.InnerXml;

            node = SystemDiagnosticsWorkNode.SelectSingleNode("trace");
            node.ParentNode.ReplaceChild(m_TraceControl.Node, node); //have to do it a little differently as the trace node itself has attributes that need to be updated.

            node = SystemDiagnosticsWorkNode.SelectSingleNode("sources");
            node.InnerXml = m_SourcesControl.Node.InnerXml;

            node = SystemDiagnosticsWorkNode.SelectSingleNode("sharedListeners");
            node.InnerXml = m_ListenersControl.Node.InnerXml;
        }

        private string GetSaveFilename()
        {
            var filename = m_appConfigFile.Text;
            if (!filename.IsNullOrEmpty()) return filename;

            using(var dlg = new SaveFileDialog())
            {
                dlg.ValidateNames = true;
                dlg.Title = "Select a new filename";
                dlg.Filter = "Application Configuration (*.config)|*.config|All Files (*.*)|*.*";
                dlg.DefaultExt = "*.config";
                dlg.Title = "Select New Application Configuration File to Save";

                if (dlg.ShowDialog(this) == DialogResult.OK) return dlg.FileName;
                return null;
            }
        }

        private void m_btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void m_btnOpen_Click(object sender, EventArgs e)
        {
            OpenFileDialog fileDlg = new OpenFileDialog();
            fileDlg.Filter = "Application Configuration (*.config)|*.config|All Files (*.*)|*.*";
            fileDlg.DefaultExt = "*.config";
            fileDlg.Title = "Select New Application Configuration File to Open";
            //fileDlg.AutoUpgradeEnabled = false;
            fileDlg.CheckFileExists = true;
            if (fileDlg.ShowDialog(this) != DialogResult.OK) return;
            m_appConfigFile.Text = fileDlg.FileName;
            fileDlg.Dispose();

            if (IsDirty)
            {
                switch (MessageBox.Show(this, "The current logging configuration has been\nchanged. Do you want to save it first?", this.Text, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes: m_btnCommit_Click(null, EventArgs.Empty); break;
                    case DialogResult.No: /*Throw it away*/   break;
                    case DialogResult.Cancel: /*Do not open a new config after all*/ return;
                }
            }

            LoadXDoc();
        }

        private void m_appConfigFile_DragDrop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop)) return;
            var file = ((string[])e.Data.GetData(DataFormats.FileDrop))[0];
            if (Path.GetExtension(file).EqualsI(".config"))
            {
                m_appConfigFile.Text = file;
                LoadXDoc();
            }
        }

        private void m_appConfigFile_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop) && Path.GetExtension((((string[])e.Data.GetData(DataFormats.FileDrop))[0])).EqualsI(".config"))
            {
                e.Effect = DragDropEffects.Copy;
            }
            else e.Effect = DragDropEffects.None;
        }

        private void m_appConfigFile_KeyPress(object sender, KeyPressEventArgs e)
        {
            var c = (TextBox)sender;
            if (e.KeyChar != '\r' && e.KeyChar != '\n') return;
            e.Handled = true;
            var file = c.Text;

            if (string.IsNullOrWhiteSpace(file))
            {
                c.Text = string.Empty;
                if (MessageBoxEx.Show(this, "Continue with a new application config file?", "New File", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.No)
                {
                    return;
                }
            }

            LoadXDoc();
        }

        private bool InTabControl = false;
        private void HelpInit()
        {
            //Determine if the focus is in the Tab control vs anywhere else so we know which help file to pop up.
            m_tcMain.Enter += (s, e) => InTabControl = true;
            m_btnHelp.Leave += (s, e) => InTabControl = false;
            this.Click += (s, e) => InTabControl = false;
            foreach (Control c in this.Controls)
            {
                if (c == m_btnHelp) continue;
                if (c == m_tcMain) continue;
                c.Click += (s,e) => InTabControl = false;
            }
        }

        private void m_btnHelp_Click(object sender, EventArgs e)
        {
            HelpPopup.HelpItem tab = HelpPopup.HelpItem.Main;

            if (InTabControl)
            {
                if (m_tcMain.SelectedTab == m_tabTrace) tab = HelpPopup.HelpItem.Trace;
                else if (m_tcMain.SelectedTab == m_tabSwitches) tab = HelpPopup.HelpItem.Switches;
                else if (m_tcMain.SelectedTab == m_tabSources) tab = HelpPopup.HelpItem.Sources;
                else if (m_tcMain.SelectedTab == m_tabListeners) tab = HelpPopup.HelpItem.SharedListeners;
                else if (m_tcMain.SelectedTab == m_tabSwitches) tab = HelpPopup.HelpItem.Switches;
            }

            HelpPopup.Show(this, tab);
        }

        /// <summary>
        /// Recursivly compare 2 XML elements for equality.
        /// </summary>
        /// <param name="primary">First XmlElement to compare.</param>
        /// <param name="secondary">Second XmlElement to compare.</param>
        /// <remarks>
        ///   • Attributes are not order dependent, however child XmlElement nodes are.
        ///   • Only XmlElement, XmlText, and XmlAttribute nodes are compared. Other XmlNode types are ignored.
        ///   • Comparison of XmlAttribute and XmlText content is case-insensitive.
        /// </remarks>
        /// <returns>True if equal</returns>
        public static bool XmlEquals(XmlElement primary, XmlElement secondary)
        {
            if (primary.HasAttributes)
            {
                if (primary.Attributes.Count != secondary.Attributes.Count) return false;
                foreach (XmlAttribute attr in primary.Attributes)
                {
                    if (secondary.Attributes[attr.Name] == null) return false;
                    if (!attr.Value.EqualsI(secondary.Attributes[attr.Name].Value)) return false;
                }
            }

            if (primary.HasChildNodes)
            {
                var e1 = primary.ChildNodes.OfType<XmlNode>().GetEnumerator();
                var e2 = secondary.ChildNodes.OfType<XmlNode>().GetEnumerator();

                while(e1.MoveNext())
                {
                    if (e1.Current.NodeType != XmlNodeType.Text && e1.Current.NodeType != XmlNodeType.Element) continue;
                    if (e1.Current.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(e1.Current.Value)) continue; //ignore empty whitespace text elements.
                    while (e2.MoveNext())
                    {
                        if (e2.Current.NodeType != XmlNodeType.Text && e2.Current.NodeType != XmlNodeType.Element) continue;
                        if (e2.Current.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(e2.Current.Value)) continue; //ignore empty whitespace text elements.
                        break;
                    }
                    if (e2.Current == null) return false; //secondary node tree too short.

                    if (e1.Current.NodeType == XmlNodeType.Text && e2.Current.NodeType != XmlNodeType.Text) return false;
                    if (e1.Current.NodeType != XmlNodeType.Text && e2.Current.NodeType == XmlNodeType.Text) return false;
                    if (e1.Current.NodeType == XmlNodeType.Text && e2.Current.NodeType == XmlNodeType.Text)
                    {
                         if (!e1.Current.Value.Squeeze().EqualsI(e2.Current.Value.Squeeze())) return false;
                    }

                    if (e1.Current.Name != e2.Current.Name) return false;

                    if (!XmlEquals((XmlElement)e1.Current, (XmlElement)e2.Current)) return false;
                }

                while (e2.MoveNext())
                {
                    if (e2.Current.NodeType != XmlNodeType.Text && e2.Current.NodeType != XmlNodeType.Element) continue;
                    if (e2.Current.NodeType == XmlNodeType.Text && string.IsNullOrWhiteSpace(e2.Current.Value)) continue;
                    break;
                }
                if (e2.Current != null) return false; //still more elements in secondary node tree.
            }

            return true;
        }
    }
}

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

namespace ChuckHill2.LoggerEditor
{
    public partial class ListenersCtrl : UserControl
    {
        public class vx //exclusively used by ListenerTypes dictionary to essentially support 3 items
        {

            public readonly Type Type;
            public readonly string Description;
            public vx(Type t, string d) { Type = t; Description = d; }
        }
        public static readonly Dictionary<Type, vx> ListenerTypes = new Dictionary<Type, vx>
        {
            { typeof(DatabaseTraceListener), new vx(typeof(DatabaseTraceListenerProps), "Asynchronously write formatted log\r\nmessages to a relational database table.") },
            { typeof(DebugTraceListener), new vx(typeof(DebugTraceListenerProps), "Asynchronously write formatted log messages to the\r\ndebugger output. Output is viewable via an external\r\ndebug viewer such as Microsoft's Dbgview.exe or the\r\nVisualStudio debugger output window, but not both.") },
            { typeof(EmailTraceListener), new vx(typeof(EmailTraceListenerProps), "Asynchronously write formatted log messages\r\nas email messages to through a mail server.") },
            { typeof(EventLogTraceListener), new vx(typeof(EventLogTraceListenerProps), "Asynchronously write formatted log messages\r\nto the Windows Event Log. Creates Event log\r\nand/or source if it does not already exist.") },
            { typeof(FileTraceListener), new vx(typeof(FileTraceListenerProps), "Asynchronously write formatted log\r\nmessages to a rolling plain text file.") },
            { typeof(ConsoleTraceListener), new vx(typeof(SysConsoleTraceListenerProps), "Built-in .Net write tracing or debugging\r\noutput to the current console window. Does\r\nnot create/allocate a new console window.") },
            { typeof(DefaultTraceListener), new vx(typeof(SysDefaultTraceListenerProps), "Built-in .Net write tracing or debugging\r\noutput to the Visual Studio Output window\r\nsimilar to DebugTraceListener.") },
            { typeof(DelimitedListTraceListener), new vx(typeof(SysDelimitedListTraceListenerProps), "Built-in .Net write tracing or debugging output\r\nto a plain text file where fields are delimited.") },
            { typeof(System.Diagnostics.EventLogTraceListener), new vx(typeof(SysEventLogTraceListenerProps), "Built-in .Net write tracing or\r\ndebugging output to an EventLog.") },
            { typeof(EventSchemaTraceListener), new vx(typeof(SysEventSchemaTraceListenerProps), "Built-in .Net write tracing or debugging\r\noutput of end-to-end events to an XML-\r\nencoded, schema-compliant log file.\r\nThis dumps a lot of properties so this\r\nshould be used sparingly.") },
            { typeof(TextWriterTraceListener), new vx(typeof(SysTextWriterTraceListenerProps), "Built-in .Net write tracing or debugging\r\noutput to a free-form plain text file.") },
            { typeof(XmlWriterTraceListener), new vx(typeof(SysXmlWriterTraceListenerProps), "Built-in .Net write as tracing or\r\ndebugging XML-encoded data\r\nto the TextWriterTraceListener.") },
        };
        private XmlElement PrevNode;

        public ListenersCtrl()
        {
            InitializeComponent();
            m_lvListeners.SmallImageList = new ImageList();
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string[] KnownListeners => m_lvListeners.Items.Cast<ListViewItem>().Select(m => m.Text).ToArray();
        public event Action<string[]> ListenerListChanged;
        private void OnListenerListChanged()
        {
            if (ListenerListChanged != null) ListenerListChanged(KnownListeners);
        }

        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public XmlElement Node
        {
            get
            {
                if (PrevNode == null) return null;

                var sb = new StringBuilder();
                foreach (var item in m_lvListeners.Items.OfType<ListViewItem>())
                {
                    var n = ((IListener)item.Tag).Node.OuterXml;
                    sb.Append(n);
                }

                var e = PrevNode.OwnerDocument.CreateElement("sharedListeners");
                // Can't use e.AppendChild(n); because it scrambles the order of the attributes. We always want 'name' and 'type' first for human readability.
                e.InnerXml = sb.ToString();
 
                return e;
            }
            set
            {
                if (value==null) { Clear(); return; }
                if (value.Name != "sharedListeners") throw new ArgumentException("Node is not a <sharedListeners> node.");
                PrevNode = value;

                foreach (var node in PrevNode.SelectNodes("add").OfType<XmlElement>())
                {
                    var t = Type.GetType(node.Attributes["type"].Value, false);
                    if (t == null) continue;
                    if (!ListenerTypes.TryGetValue(t, out var propsType)) continue;
                    var props = (IListener)Activator.CreateInstance(propsType.Type);

                    props.Node = node; //Initializes the property class immediately., including props.Name.
                    var item = AddListViewItem(props);
                }

                if (m_lvListeners.Items.Count > 0) //set to the first item
                {
                    var item = m_lvListeners.Items[0];
                    item.Selected = true;
                    item.Focused = true;
                }
            }
        }

        public void Clear()
        {
            m_pgListenerProps.SelectedObject = null;
            m_lvListeners.Clear();
            m_lblListenerProps.Text = "";
            PrevNode = null;
        }

        private ListViewItem AddListViewItem(IListener props)
        {
            if (!m_lvListeners.SmallImageList.Images.ContainsKey(props.Type.Name))
                m_lvListeners.SmallImageList.Images.Add(props.Type.Name, (Image)Properties.Resources.ResourceManager.GetObject(props.Type.Name));

            var item = new ListViewItem(props.Name);
            item.Name = props.Name;
            item.Tag = props;
            item.ImageKey = props.Type.Name;
            m_lvListeners.Items.Add(item);
            props.NamePropertyChanged += (s, ev) => { item.Text = props.Name; OnListenerListChanged(); };
            OnListenerListChanged();
            return item;
        }

        private void addToolStripMenuItem_Click(object sender, EventArgs e)
        {
            m_btnAddListener_Click(sender, e);
        }

        private void removeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (m_lvListeners.SelectedItems.Count > 0)
            {
                var item = m_lvListeners.SelectedItems[0];
                if (MiniMessageBox.ShowDialog(m_lvListeners, $"Are you sure you want to remove {item.Name} ({((IListener)item.Tag).Type.Name})?", "Remove Listener", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    m_lvListeners.Items.Remove(item);
                    OnListenerListChanged();
                }
            }
        }

        private void m_lvListeners_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (m_lvListeners.SelectedItems.Count == 0) return;
            var item = m_lvListeners.SelectedItems[0];
            var listener = (IListener)item.Tag;
            m_lblListenerProps.Text = $"{listener.Name}\x00A0({listener.Type.FullName})";
            m_pgListenerProps.SelectedObject = listener;
        }

        private void m_btnAddListener_Click(object sender, EventArgs e)
        {
            var t = ListenerChooser.Show(this, null);
            if (t == null) return;
            t = ListenerTypes.GetValue(t)?.Type;

            int maxInt = 0;
            if (m_lvListeners.Items.Count > 0)
                maxInt = m_lvListeners.Items.OfType<ListViewItem>().Max(d => d.Text.Length > 9 && int.TryParse(d.Text.Substring(9), out int index) ? index : 0);
            var props = (IListener)Activator.CreateInstance(t);

            props.Name = "Listener_" + (maxInt+1).ToString(); //There are no properties, so we have to assign a bogus unique name now. The user can change it in the UI.
            var item = AddListViewItem(props);
            item.Selected = true;
            item.Focused = true;
        }
    }

    //Common interface for all the following listener property classes
    public interface IListener
    {
        event PropertyChangedEventHandler NamePropertyChanged;

        Type Type { get; }
        string Name { get; set; }
        XmlElement Node { get; set; }
    }

    #region Custom Listener property classes
    public abstract class ListenerPropBase : IListener
    {
        protected XmlElement PrevNode;
        protected Dictionary<string, string> InitializeData;

        public event PropertyChangedEventHandler NamePropertyChanged;

        [Browsable(false), XmlIgnore]
        public virtual XmlElement Node
        {
            get
            {
                var xdoc = PrevNode?.OwnerDocument ?? new XmlDocument(); //PrevNode==null for a new listener
                var add = xdoc.CreateElement("add");
                add.Attributes.Append(xdoc.CreateAttribute("name")).Value = Name;
                add.Attributes.Append(xdoc.CreateAttribute("type")).Value = AssemblyQualifiedName(Type);
                string propDict = GetListenerProperties(this);
                if (!propDict.IsNullOrEmpty())
                    add.Attributes.Append(xdoc.CreateAttribute("initializeData")).Value = propDict;

                if (PrevNode != null)
                {
                    foreach (var node in PrevNode.ChildNodes.OfType<XmlNode>())
                    {
                        add.AppendChild(node.Clone()); //for stuff we don't support
                    }
                }

                return add;
            }
            set
            {
                if (value.Name != "add") throw new ArgumentException("Node is not an <add> node.");
                PrevNode = value;

                Name = value.Attributes["name"]?.Value ?? "UNKNOWN";

                //Our listeners only use the official initializeData attribute to hold all it's properties as a list of key/value pairs.
                InitializeData = value.Attributes["initializeData"]?.Value.ToDictionary(';', '=');

                SqueezeMsg = InitializeData.GetValue("SqueezeMsg").CastTo(false);
                Async = InitializeData.GetValue("Async").CastTo(true);
                Format = InitializeData.GetValue("Format") ?? "";
                IndentSize = InitializeData.GetValue("IndentSize").CastTo(4);
            }
        }

        #region Common Properties
        private string __name = "";
        [Category("Design"), Description("The unique name of this listener object used by the other logging components. ")]
        [DefaultValue("")]
        [XmlIgnore]
        public string Name
        {
            get => __name;
            set
            {
                __name = value.ToIdentifier();
                if (NamePropertyChanged == null) return;
                NamePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [Browsable(false), XmlIgnore]
        public abstract Type Type { get; }

        [Category("Layout"), Description("Squeeze multi-line FullMessage and duplicate whitespace into a single line.")]
        [DefaultValue(false)]
        public bool SqueezeMsg { get; set; } = false;

        [Category("Layout"), Description("How many spaces to indent succeeding lines in a multi-line FullMessage")]
        [DefaultValue(4)]
        public int IndentSize { get; set; } = 4;

        [Category("Layout"), Description("Same as string.Format format specifier with arguments. See string.Format() (default determined by listener type)")]
        [DefaultValue("")]
        [Editor(typeof(FormatEditor), typeof(UITypeEditor))]
        public string Format { get; set; } = "";

        [Category("Performance"), Description("Lazily write messages to output destination. False may incur performance penalties as messages are written immediately.")]
        [DefaultValue(true)]
        public bool Async { get; set; } = true;
        #endregion

        //Get properties from my listeners and package them up as a dictionary string for 'initializeData' XmlAttribute
        protected static string GetListenerProperties(IListener obj)
        {
            var t = obj.GetType();
            var sb = new StringBuilder();

            foreach (var pi in t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.CustomAttributes.Any(m => m.AttributeType.Name.Contains("Ignore")))
                .OrderBy(x => x.MetadataToken)) //properties ordered by declaration in class.
            {
                var defalt = pi.GetCustomAttribute<DefaultValueAttribute>()?.Value?.ToString() ?? "";
                string name = pi.GetCustomAttribute<NameAttribute>()?.Name ?? pi.Name;
                //Type type = pi.GetCustomAttribute<NameAttribute>()?.Type ?? typeof(string);

                object value = pi.GetValue(obj);
                if (value == null) continue;
                string val;
                if (value is Type) val = ((Type)value).AssemblyQualifiedName;
                else val = value.ToString();
                if (val == defalt) continue;

                sb.Append(pi.Name);
                sb.Append('=');
                sb.Append(val);
                sb.Append(';');
            }

            if (sb.Length > 0) sb.Length -= 1;
            return sb.ToString();
        }

        //Get full assembly qualified type name WITHOUT trailing Version, Culture, and PublicKeyToken fields so we don't run into upgrade versionitis.
        protected static string AssemblyQualifiedName(Type t)
        {
            var name = t.AssemblyQualifiedName;
            int i = name.IndexOf(',');
            i = name.IndexOf(',', i + 1);
            return name.Substring(0, i);
        }
    }

    public class DatabaseTraceListenerProps : ListenerPropBase
    {
        [Browsable(false), XmlIgnore]
        public override Type Type => typeof(DatabaseTraceListener);

        [Browsable(false), XmlIgnore]
        public override XmlElement Node
        {
            get => base.Node;
            set
            {
                base.Node = value;
                ConnectionString = InitializeData.GetValue("ConnectionString") ?? "";
                SqlStatement = InitializeData.GetValue("SqlStatement") ?? "";
            }
        }

        //Note: Properties 'Indent' and 'Format' are not used

        [Category("Output"), Description("A string key representing AppConfig ConfigurationManager.ConnectionStrings[] dictionary entry OR literal full SQL connection string.")]
        [DefaultValue("")]
        [Editor(typeof(ConnectionStringEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string ConnectionString { get; set; } = "";

        [Category("Output"), Description("SQL statement to insert logging values into the database table.")]
        [DefaultValue("")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string SqlStatement { get; set; } = "";
    }

    public class DebugTraceListenerProps : ListenerPropBase, IListener
    {
        [Browsable(false), XmlIgnore]
        public override Type Type => typeof(DebugTraceListener);
    }

    public class EmailTraceListenerProps : ListenerPropBase
    {
        [Browsable(false), XmlIgnore]
        public override Type Type => typeof(EmailTraceListener);

        [Browsable(false), XmlIgnore]
        public override XmlElement Node
        {
            get => base.Node;
            set
            {
                base.Node = value;

                string from = "";
                string clientDomain = "";
                bool defaultCredentials = true;
                bool enableSsl = false;
                string host = "";
                int port = 25;
                string userName = "";
                string password = "";

                var smtp = value.OwnerDocument.SelectSingleNode("/configuration/system.net/mailSettings/smtp");
                if (smtp != null && (smtp.SelectSingleNode("@deliveryMethod")?.Value ?? "Network")=="Network")
                {
                    from = smtp.SelectSingleNode("@from")?.Value ?? "";
                    clientDomain = smtp.SelectSingleNode("network/@clientDomain")?.Value ?? "";
                    defaultCredentials = (smtp.SelectSingleNode("network/@defaultCredentials")?.Value).CastTo(true);
                    enableSsl = (smtp.SelectSingleNode("network/@clientDomain")?.Value).CastTo(false);
                    host = smtp.SelectSingleNode("network/@host")?.Value ?? "";
                    port = (smtp.SelectSingleNode("network/@clientDomain")?.Value).CastTo(25);
                    userName = smtp.SelectSingleNode("network/@userName")?.Value ?? "";
                    password = smtp.SelectSingleNode("network/@password")?.Value ?? "";
                }

                Subject = InitializeData.GetValue("Subject") ?? "";
                SentFrom = InitializeData.GetValue("SentFrom") ?? from;
                SendTo = InitializeData.GetValue("SendTo") ?? "";
                ClientDomain = InitializeData.GetValue("ClientDomain") ?? clientDomain;
                MailServer = InitializeData.GetValue("MailServer") ?? host;
                Port = InitializeData.GetValue("Port").CastTo(port);
                EnableSsl = InitializeData.GetValue("EnableSsl").CastTo(enableSsl);
                DefaultCredentials = InitializeData.GetValue("DefaultCredentials").CastTo(defaultCredentials);
                UserName = InitializeData.GetValue("UserName") ?? userName;
                Password = InitializeData.GetValue("Password") ?? password;
            }
        }

        [Category("Layout"), Description("Email subject line. Default='Log: '+SourceName")]
        [DefaultValue("")]
        public string Subject { get; set; } = "";

        [Category("Output"), Description("Comma-delimited list of email addresses to send to. Whitespace is ignored. Addresses may be in the form of 'username@domain.com' or 'UserName <username@domain.com>'. If undefined, email logging is disabled.")]
        [DefaultValue("")]
        public string SendTo { get; set; } = "";

        ///   The following are explicitly defined here or defaulted from app.config configuration/system.net/mailSettings/smtp;

        [Category("Output"), Description("The 'from' email address. Whitespace is ignored. Addresses may be in the form of 'username@domain.com' or 'UserName <username@domain.com>'. If undefined, uses the value from 'system.net/mailSettings/smtp/@from'")]
        [DefaultValue("")]
        public string SentFrom { get; set; } = "";

        [Category("Output"), Description("LocalHost - aka 'www.gmail.com'")]
        [DefaultValue("")]
        public string ClientDomain { get; set; } = "";

        [Category("Output"), Description("True to use windows authentication, false to use UserName and Password.")]
        [DefaultValue(true)]
        public bool DefaultCredentials { get; set; } = true;

        [Category("Output"), Description("")]
        [DefaultValue("")]
        public string UserName { get; set; } = "";

        [Category("Output"), Description("")]
        [DefaultValue("")]
        public string Password { get; set; } = "";

        [Category("Output"), Description("")]
        [DefaultValue(false)]
        public bool EnableSsl { get; set; } = false;

        [Category("Output"), Description(" aka 'smtp.gmail.com'")]
        [DefaultValue("")]
        public string MailServer { get; set; } = "";

        [Category("Output"), Description("The mail server listener port to send messages to.")]
        [DefaultValue(25)]
        public int Port { get; set; } = 25;
    }

    public class EventLogTraceListenerProps : ListenerPropBase
    {
        [Browsable(false), XmlIgnore]
        public override Type Type => typeof(ChuckHill2.EventLogTraceListener);

        [Browsable(false), XmlIgnore]
        public override XmlElement Node
        {
            get => base.Node;
            set
            {
                base.Node = value;
                Machine = InitializeData.GetValue("Machine") ?? ".";
                Log = InitializeData.GetValue("Log") ?? "";
                Source = InitializeData.GetValue("Source") ?? "";
            }
        }

        [Category("Output"), Description("Computer whos event log to write to. Requires write access.")]
        [DefaultValue(".")]
        public string Machine { get; set; } = ".";

        [Category("Output"), Description("EventLog log to write to. If undefined EventLog logging disabled.")]
        [DefaultValue("")]
        public string Log { get; set; } = "";

        [Category("Output"), Description("EventLog source to write to. If undefined EventLog logging disabled.")]
        [DefaultValue("")]
        public string Source { get; set; } = "";
    }

    public class FileTraceListenerProps : ListenerPropBase
    {
        [Browsable(false), XmlIgnore]
        public override Type Type => typeof(ChuckHill2.FileTraceListener);

        [Browsable(false), XmlIgnore]
        public override XmlElement Node
        {
            get => base.Node;
            set
            {
                base.Node = value;
                Filename = InitializeData.GetValue("Filename") ?? "";
                MaxSize = InitializeData.GetValue("MaxSize").CastTo(100);
                MaxFiles = InitializeData.GetValue("MaxFiles").CastTo(0);
                FileHeader = InitializeData.GetValue("FileHeader") ?? "";
                FileFooter = InitializeData.GetValue("FileFooter") ?? "";
            }
        }

        [Category("Output"), Description("By default, logfile name is the same as appname with a '.log' extension appended - " +
            "The filename is a relative or full filepath which may contain environment variables enclosed in '%' chars, including pseudo-environment " +
            "variables: %ProcessName%, %ProcessId% (as 4 hex digits), %AppDomainName%, and %BaseDir%. BaseDir is the current AppDomain " +
            "startup folder which may be different than the application startup folder as in ASP.NET. " +
            "The filename or folder does not need to exist.")]
        [DefaultValue("")]
        [Editor(typeof(SaveLogNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Filename { get; set; } = "";

        [Category("Output"), Description("Maximum file size (MB) before starting over with a new file.")]
        [DefaultValue(100)]
        public int MaxSize { get; set; } = 100;

        [Category("Output"), Description("Maximum number of log files before deleting the oldest. 0 = never delete.")]
        [DefaultValue(0)]
        public int MaxFiles { get; set; } = 0;

        [Category("Layout"), Description("String literal to insert as the first line(s) of a new log file. Good for CSV-style formatting.")]
        [DefaultValue("")]
        public string FileHeader { get; set; } = "";

        [Category("Layout"), Description("String literal to append as the last line(s) in a file being closed.")]
        [DefaultValue("")]
        public string FileFooter { get; set; } = "";
    }
    #endregion //Custom Listener property classes

    #region System.Diagnostics Listener property classes
    /// <summary>
    /// Private copy of the System.Diagnostics trace options with tooltips
    /// </summary>
    [Flags] public enum TraceOptions
    {
        [Description("Do not write any elements.")]
        None = 0,

        [Description("Write the logical operation stack, which is represented by the return value\r\nof the System.Diagnostics.CorrelationManager.LogicalOperationStack property.")]
        LogicalOperationStack = 1,

        [Description("Write the date and time.")]
        DateTime = 2,

        [Description("Write the timestamp, which is represented by the return value\r\nof the System.Diagnostics.Stopwatch.GetTimestamp method.")]
        Timestamp = 4,

        [Description("Write the process identity, which is represented by the\r\nreturn value of the System.Diagnostics.Process.Id property.")]
        ProcessId = 8,

        [Description("Write the thread identity, which is represented by the\r\nreturn value of the System.Threading.Thread.ManagedThreadId\r\nproperty for the current thread.")]
        ThreadId = 16,

        [Description("Write the call stack, which is represented by the return\r\nvalue of the System.Environment.StackTrace property.")]
        Callstack = 32
    }

    public abstract class SysListenerPropBase : IListener
    {
        protected XmlElement PrevNode;

        public event PropertyChangedEventHandler NamePropertyChanged;

        [Browsable(false), XmlIgnore]
        public virtual XmlElement Node
        {
            get
            {
                var xdoc = PrevNode?.OwnerDocument ?? new XmlDocument(); //PrevNode==null for a new listener
                var add = xdoc.CreateElement("add");

                foreach (var kv in GetSysListenerProperties(this))
                    add.Attributes.Append(xdoc.CreateAttribute(kv.Key)).Value = kv.Value;

                if (PrevNode != null)
                {
                    foreach (var node in PrevNode.ChildNodes.OfType<XmlNode>())
                    {
                        add.AppendChild(node.Clone()); //for stuff we don't support
                    }
                }

                return add;
            }
            set
            {
                if (value.Name != "add") throw new ArgumentException("Node is not an <add> node.");
                PrevNode = value;

                SetSysListenerProperties(value.Attributes, this);

                Name = value.Attributes["name"]?.Value ?? "UNKNOWN";
            }
        }

        #region Common Properties
        private string __name = "";
        [Name("name", 0)]
        [Category("Design"), Description("The unique name of this listener object used by the other logging components. ")]
        [DefaultValue("")]
        public string Name
        {
            get => __name;
            set
            {
                __name = value.ToIdentifier();
                if (NamePropertyChanged == null) return;
                NamePropertyChanged(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        [Browsable(false)]
        [Name("type", 1)]
        public abstract Type Type { get; }
        #endregion

        //Get ordered list of System listener properties
        protected static List<KeyValuePair<string, string>> GetSysListenerProperties(IListener obj)
        {
            var t = obj.GetType();
            var list = new List<KeyValuePair<string, string>>();

            foreach (var pi in t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance)
                .Where(p => p.CanRead && !p.CustomAttributes.Any(m => m.AttributeType.Name.Contains("Ignore")))
                .OrderBy(x => x.GetCustomAttribute<NameAttribute>()?.Order ?? int.MaxValue))
            {
                var defalt = pi.GetCustomAttribute<DefaultValueAttribute>()?.Value?.ToString() ?? "";
                string name = pi.GetCustomAttribute<NameAttribute>()?.Name ?? pi.Name;
                //Type type = pi.GetCustomAttribute<NameAttribute>()?.Type ?? typeof(string);

                object value = pi.GetValue(obj);
                if (value == null) continue;
                string val;
                if (value is Type) val = ((Type)value).AssemblyQualifiedName;
                else val = value.ToString();

                if (val == defalt) continue;

                list.Add(new KeyValuePair<string, string>(name, val));
            }

            return list;
        }

        //Take System listener xml attributes and write them to the listener object
        protected static void SetSysListenerProperties(XmlAttributeCollection xattributes, IListener obj)
        {
            var t = obj.GetType();
            foreach (var pi in t.GetProperties(BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance))
            {
                if (!pi.CanRead || !pi.CanWrite) continue;
                if (pi.CustomAttributes.Any(a => a.AttributeType.Name.ContainsI("Ignore"))) continue;

                object defalt = pi.GetCustomAttribute<DefaultValueAttribute>()?.Value;
                string name = pi.GetCustomAttribute<NameAttribute>()?.Name ?? pi.Name;
                Type type = pi.GetCustomAttribute<NameAttribute>()?.Type ?? typeof(string);

                object val = xattributes[name]?.Value;
                if (val == null) continue;

                //Cast.To() trims strings, so we can't use it for string types. Leading and trailing whitespace is significant.
                if (type != typeof(string)) val = Cast.To(type, val, defalt);
                else if (((string)val).Length == 0 && defalt is string) val = defalt;

                pi.SetValue(obj, val);
            }
        }
    }

    public class SysConsoleTraceListenerProps : SysListenerPropBase
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.consoletracelistener?view=net-5.0
        // <add name="myconsoleListener"
        //   type="System.Diagnostics.ConsoleTraceListener"
        //   traceOutputOptions="ProcessId, DateTime" />

        public override Type Type => typeof(System.Diagnostics.ConsoleTraceListener);

        [Name("traceOutputOptions", 2, typeof(TraceOptions))]
        [Category("Layout"), Description("Additional properties to write in the logging event.")]
        [DefaultValue(TraceOptions.None)]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TraceOptions TraceOutputOptions { get; set; } = TraceOptions.None;
    }

    public class SysDefaultTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.defaulttracelistener?view=net-5.0
        // <add name="delimitedListener"   
        //   type="System.Diagnostics.DefaultTraceListener"   

        public override Type Type => typeof(System.Diagnostics.DefaultTraceListener);
    }

    public class SysDelimitedListTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.delimitedlisttracelistener?view=net-5.0
        // <add name="delimitedListener"   
        //   type="System.Diagnostics.DelimitedListTraceListener"   
        //   delimiter=","   
        //   initializeData="delimitedOutput.csv"   
        //   traceOutputOptions="ProcessId, DateTime" />

        public override Type Type => typeof(System.Diagnostics.DelimitedListTraceListener);

        [Name("traceOutputOptions", 2, typeof(TraceOptions))]
        [Category("Layout"), Description("Additional properties to write in the logging event.")]
        [DefaultValue(TraceOptions.None)]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TraceOptions TraceOutputOptions { get; set; } = TraceOptions.None;

        [Name("initializeData", 3)]
        [Category("Output"), Description("The file name to write output to. It is relative to the executable.")]
        //[DefaultValue("DelimitedOutput.csv")]
        [Editor(typeof(SaveLogNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Filename { get; set; } = "DelimitedOutput.csv";

        [Name("delimiter", 4)]
        [Category("Layout"), Description("The field separator string in the resulting CSV-style output.")]
        //[DefaultValue(",")]
        public string Delimiter { get; set; } = ",";
    }

    public class SysEventLogTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.eventlogtracelistener?view=dotnet-plat-ext-5.0
        // <add name="myListener"  
        //   type="System.Diagnostics.EventLogTraceListener"  
        //   initializeData="TraceListenerLog" />  

        public override Type Type => typeof(System.Diagnostics.EventLogTraceListener);

        [Name("initializeData", 2)]
        [Category("Output"), Description("The source to associate with the log event in the local application event log. If it doesn't exist, it will be created.")]
        [DefaultValue("")]
        public string Source { get; set; } = "";
    }

    public class SysEventSchemaTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.eventschematracelistener?view=netframework-4.8
        // <add name="eventListener"   
        //   type="System.Diagnostics.EventSchemaTraceListener,  system.core"  
        //   initializeData="TraceOutput.xml"   
        //   traceOutputOptions="ProcessId, DateTime, Timestamp"   
        //   bufferSize="65536"   //note hardcode
        //   maximumFileSize="20480000"  //note==20MB
        //   logRetentionOption="LimitedCircularFiles"  
        //   maximumNumberOfFiles="2"/>  

        public override Type Type => typeof(System.Diagnostics.EventSchemaTraceListener);

        [Name("initializeData", 2)]
        [Category("Output"), Description("The file name to write output to. It is relative to the executable.")]
        //[DefaultValue("TraceOutput.xml")]
        [Editor(typeof(SaveLogNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Filename { get; set; } = "TraceOutput.xml";

        [Name("traceOutputOptions", 3, typeof(TraceOptions))]
        [Category("Layout"), Description("Additional properties to write in the logging event.")]
        [DefaultValue(TraceOptions.None)]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TraceOptions TraceOutputOptions { get; set; } = TraceOptions.None; //!xml attribute==traceOutputOptions = TraceOutputOptions.ToString()

        //Not modified but needed in XML
        [Browsable(false)]
        [Name("bufferSize", 4, typeof(int))]
        public int BufferSize { get; set; } = 65536;

        [Name("maximumFileSize", 5, typeof(int))]
        [Category("Output"), Description("Maximum file size in bytes (default=20MB) before starting over with a new file.")]
        //[DefaultValue(20971520)]
        public int MaxSize { get; set; } = 20971520;

        //Not modified but needed in XML
        [Browsable(false)]
        [Name("logRetentionOption", 6)]
        public string LogRetentionOption { get; set; } = "LimitedCircularFiles";

        [Name("maximumNumberOfFiles", 7, typeof(int))]
        [Category("Output"), Description("Maximum number of log files before deleting the oldest.")]
        //[DefaultValue(2)]
        public int MaxFiles { get; set; } = 2;
    }

    public class SysTextWriterTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.textwritertracelistener?view=net-5.0
        // <add name="myListener"   
        //   type="System.Diagnostics.TextWriterTraceListener"   
        //   initializeData="TextWriterOutput.log" />  

        public override Type Type => typeof(System.Diagnostics.TextWriterTraceListener);

        [Name("initializeData", 2)]
        [Category("Output"), Description("The file name to write output to. It is relative to the executable.")]
        //[DefaultValue("TraceOutput.xml")]
        [Editor(typeof(SaveLogNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Filename { get; set; } = "TraceOutput.xml";

        [Name("traceOutputOptions", 3, typeof(TraceOptions))]
        [Category("Layout"), Description("Additional properties to write in the logging event.")]
        [DefaultValue(TraceOptions.None)]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TraceOptions TraceOutputOptions { get; set; } = TraceOptions.None;
    }

    public class SysXmlWriterTraceListenerProps : SysListenerPropBase, IListener
    {
        // https://docs.microsoft.com/en-us/dotnet/api/system.diagnostics.xmlwritertracelistener?view=net-5.0
        // <add name="xmlListener"   
        //   type="System.Diagnostics.XmlWriterTraceListener"   
        //   initializeData="xmlOutput.xml"   
        //   traceOutputOptions="ProcessId, DateTime" />  

        public override Type Type => typeof(System.Diagnostics.XmlWriterTraceListener);

        [Name("initializeData", 2)]
        [Category("Output"), Description("The file name to write output to. It is relative to the executable.")]
        //[DefaultValue("XmlOutput.xml")]
        [Editor(typeof(SaveLogNameEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public string Filename { get; set; } = "XmlOutput.xml";

        [Name("traceOutputOptions", 3, typeof(TraceOptions))]
        [Category("Layout"), Description("Additional properties to write in the logging event.")]
        [DefaultValue(TraceOptions.None)]
        [Editor(typeof(EnumUIEditor), typeof(System.Drawing.Design.UITypeEditor))]
        public TraceOptions TraceOutputOptions { get; set; } = TraceOptions.None;
    }

    //Exclusively used for the System.Diagnostics listener properties, not our custom listeners.
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
    internal class NameAttribute : Attribute
    {
        private readonly string _name;
        private Type _type;
        private int _order;

        public NameAttribute() : this(null, int.MaxValue, null) { }
        public NameAttribute(string name) : this(name, int.MaxValue, null) { }
        public NameAttribute(string name, Type type) : this(name, int.MaxValue, type) { }
        public NameAttribute(string name, int order) : this(name, order, null) { }
        public NameAttribute(string name, int order, Type type) { _name = name; _order = order; _type = type; }

        public string Name => this._name ?? "UNKNOWN";
        public Type Type => this._type == null ? typeof(string) : _type;
        public int Order => _order;
    }
    #endregion //System.Diagnostics Listeners
}

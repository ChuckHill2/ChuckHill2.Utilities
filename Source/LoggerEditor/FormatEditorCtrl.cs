//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="FormatEditorCtrl.cs" company="Chuck Hill">
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

namespace ChuckHill2.LoggerEditor
{
    public partial class FormatEditorCtrl : UserControl
    {
        /// <summary>
        /// Get or set format string.
        /// </summary>
        public override string Text
        {
            get => m_txtFormat.Text;
            set
            {
                m_txtFormat.Text = value;
                m_txtFormat.Select(9999, 0);
            }
        }

        public FormatEditorCtrl()
        {
            InitializeComponent();

            this.SuspendLayout();

            m_lvChoices.Columns.Add(new System.Windows.Forms.ColumnHeader() { Text = "Variable", Name = "Variable", Width = -2 });
            m_lvChoices.Columns.Add(new System.Windows.Forms.ColumnHeader() { Text = "Type", Name = "Text", Width = -2 });
            m_lvChoices.Columns.Add(new System.Windows.Forms.ColumnHeader() { Text = "Example", Name = "Example", Width = -2 });

            m_lvChoices.Groups.Add(new ListViewGroup("Local Properties") { Name = "Local" });
            m_lvChoices.Groups.Add(new ListViewGroup("HTTP Request Properties") { Name = "HTTPRequest" });

            AddListViewItem(0, "ActivityId", "Guid", "18BBBDCD-A732-496C-9F87-45E46B9DE5E5", "Gets the correlation activity id. ");
            AddListViewItem(0, "CallStack", "String", "(multi-line callstack)", "Gets the call stack at the point of this event.");
            AddListViewItem(0, "DateTime", "DateTime", "1/15/2021 6:45:06 PM", "Gets the UTC datetime this event was posted. Use the datetime string formatting specifiers to get the format exactly as you want it.");
            AddListViewItem(0, "DomainName", "String", "My Worker Service", "Gets the AppDomain friendly name for this event.");
            AddListViewItem(0, "EntryAssemblyName", "String", "System.Windows.Forms", "Gets the assembly name for this event.");
            AddListViewItem(0, "Exception", "String", "'Exception:\\r\\n' + exception.ToString()", "Gets the current exception or empty if there is no exception.");
            AddListViewItem(0, "ExceptionMessage", "String", "exception.Message", "Gets the message part of exception or empty if there is no exception.");
            AddListViewItem(0, "ExceptionOrCallStack", "String", "Example", " Get the call stack or exception (if it exists) for Verbose log messages only. Returns empty if not verbose.");
            AddListViewItem(0, "LocalDateTime", "DateTime", "1/15/2021 6:45:06 PM", "Gets the local datetime this event was posted. Use the datetime string formatting specifiers to get the format exactly as you want it.");
            AddListViewItem(0, "LogicalOperationStack", "String", "(multi-line callstack)", "Gets the entire correlated logical call stack form the call context.");
            AddListViewItem(0, "ProcessId", "Int32", "12345", "Gets the unique identiier of the current process (PID)");
            AddListViewItem(0, "ProcessName", "String", "devenv.exe", "Gets the name of this process.");
            AddListViewItem(0, "Severity", "TraceEventType", "Error", "Gets the severity level for this log event.");
            AddListViewItem(0, "SeverityString", "String", "Error", "Gets the severity level for this log event.");
            AddListViewItem(0, "SourceId", "UInt16", "3", "Gets the integer source ID");
            AddListViewItem(0, "SourceName", "String", "General", "Gets the source name.");
            AddListViewItem(0, "ThreadId", "Int32", "1", "Gets the current managed thread ID.");
            AddListViewItem(0, "ThreadName", "String", "My Worker Thread", " Gets the current thread name or 'Thread ' + ThreadId if no thread name has been assigned.");
            AddListViewItem(0, "Timestamp", "Int64", "12345678", "Gets the current number of ticks in the timer mechanism.");
            AddListViewItem(0, "UserData", "String", "Width=112, Height=53", "Custom data provided by the user. Object must have overridden ToString() else the string output will be just the class name.");
            AddListViewItem(0, "UserMessage", "String", "Volume E: (\\Device\\HarddiskVolume2) is healthy.  No action is needed.", "The formatted user log message for this event.");
            AddListViewItem(0, "Version", "String", "4.0.0.0", "Gets the version of the assembly that called this event.");

            //The System.Web.HttpContext instance for the current HTTP request.
            AddListViewItem(1, "RequestBrowserType", "String", "Chrome 87 on Windows 10", "Gets the name and major (integer) version number of the browser.");
            AddListViewItem(1, "RequestData", "String", "(data)", "Data associated with the Get or Post request.");
            AddListViewItem(1, "RequestHttpMethod", "String", "POST", "Gets the HTTP data transfer method (such as GET, POST, or HEAD) used by the client.");
            AddListViewItem(1, "RequestUrl", "String", "https://developer.mozilla.org/en-US/docs/Web/API/Request/url", "Gets the request Url.");
            AddListViewItem(1, "RequestUrlLocalPath", "String", @"C:\Temp\image.jpg", "Gets the request url local file name.");
            AddListViewItem(1, "RequestUserAgent", "String", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)....", "Gets the raw user agent string of the client browser.");
            AddListViewItem(1, "UserHostAddress", "String", "104.123.74.28", "Gets IP address of remote client.");
            AddListViewItem(1, "UserName", "String", "jdoe", "Gets the current user name associated with this HttpContext.");
            this.ResumeLayout();
        }

        private void AddListViewItem(int group, params string[] args)
        {
            var item = new ListViewItem(new string[] { args[0], args[1], args[2] }, -1);
            if (group >= 0) item.Group = m_lvChoices.Groups[group];
            item.Name = args[0];
            item.ToolTipText = args[3];
            m_lvChoices.Items.Add(item);
        }

        private void m_lvChoices_DoubleClick(object sender, EventArgs e)
        {
            m_txtFormat.Paste(string.Concat("{", m_lvChoices.SelectedItems[0].Text, "}"));
        }
    }
}

using System;
using System.Threading;
using System.Windows.Forms;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Display a simple message dialog while executing a time intensive operation.
    /// </summary>
    /// <remarks>
    /// The dialog is blocking (e.g. user cannot perform any actions on the parent form),
    /// however the parent form itself is not blocked from any programatic actions. E.g. the message pump is still running.
    /// It is possible to dynamically update the parent form while running, but care must be taken to handle cross-thread UI actions.
    /// </remarks>
    public class PleaseWait : Form
    {
        /// <summary>
        /// Display a simple message dialog while executing a time intensive operation.
        /// </summary>
        /// <param name="owner">Parent/owner of this messagebox</param>
        /// <param name="message">text message to display while waiting for the operation to complete</param>
        /// <param name="command">time intensive delegate/method to execute. Must not contain any UI operations</param>
        /// <param name="value">user data to pass to method to execute or null if nothing is required by method</param>
        /// <param name="timeout">Maximum allowed time in seconds time-intensive method is allowed to execute or -1 to wait forever.</param>
        public static void Show(IWin32Window owner, string message, Action<object> command, object value = null, int timeout=-1)
        {
            PleaseWait dlg = new PleaseWait(message, command, value, timeout);
            dlg.ShowDialog(owner);
        }

        private PleaseWait(string message, Action<object> command, object value, int timeout)
        {
            var m_lblMessage = new Label();
            this.SuspendLayout();
            m_lblMessage.Dock = System.Windows.Forms.DockStyle.Fill;
            m_lblMessage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            m_lblMessage.Text = message;

            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(204, 64);
            this.Controls.Add(m_lblMessage);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Name = "PleaseWait";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Please Wait...";
            this.UseWaitCursor = true;
            this.ResumeLayout(false);

            this.Shown += (sender, e) =>
            {
                if (command == null) { this.Close(); return; }
                bool done = false;
                var th = new Thread(delegate()
                {
                    command(value);
                    Volatile.Write(ref done, true);
                });
                th.IsBackground = true;
                th.Name = "PleaseWait Worker";
                th.Start();
                int endTicks = 0;
                if (timeout>0) endTicks = Environment.TickCount + timeout * 1000;
                while (!Volatile.Read(ref done))
                {
                    if (endTicks>0 && Environment.TickCount>endTicks)
                    {
                        th.Abort();
                        break;
                    }
                    Application.DoEvents();
                    Thread.Sleep(100);
                }
                this.Close();
            };
        }
    }
}

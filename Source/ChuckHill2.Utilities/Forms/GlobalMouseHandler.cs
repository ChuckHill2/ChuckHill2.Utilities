using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Mouse handler for the entire form. Captures all mouse and click
    /// events for entire form, including those within child controls.
    /// * Mouse detection only occurs when the form is active.
    /// * Mouse detection may be explictly disabled or enabled.
    /// * Use Dispose() upon closure to release shared resources.
    /// </summary>
    public class GlobalMouseHandler : IMessageFilter, IDisposable
    {
        private const int WM_MOUSEMOVE = 0x0200;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_XBUTTONDOWN = 0x020B;
        private const int WM_MOUSELEAVE = 0x02A3;

        private Point previousMousePosition = Point.Empty;
        private Form Host;

        /// <summary>
        ///  Detect mouse movement anywhare in the client area of the form. Including over controls.
        ///  'sender' refers to the host form and the mouse position is in screen coordinates.
        /// </summary>
        public event EventHandler<MouseEventArgs> MouseMoved; // = delegate { };

        /// <summary>
        ///  This event is triggered when a mouse click occured somewhere on the form.
        ///  Use Control.MousePosition to get the location of the click.
        /// </summary>
        public event EventHandler<EventArgs> Click; // = delegate { };

        /// <summary>
        /// This event is triggered when the mouse is moved off the host client area..
        /// </summary>
        /// <remarks>
        /// Warning: This event may be missed if the mouse is moved too fast, because we we don't get any messages once we leave the confines of the form.
        /// </remarks>
        public event EventHandler<EventArgs> MouseLeave; // = delegate { };

        /// <summary>
        /// Construct this mouse handler.
        /// </summary>
        /// <param name="host">The form that is hosting this mouse handler.</param>
        public GlobalMouseHandler(Form host)
        {
            Host = host;
            Host.Activated += Host_Activated;
            Host.Deactivate += Host_Deactivate;
        }

        private bool __enabled = false;
        /// <summary>
        /// Explicitly Enable/Disable mouse detection on form.
        /// </summary>
        public bool Enabled
        {
            get => __enabled;
            set
            {
                if (value == __enabled) return;
                __enabled = value;
                if (__enabled) EnableMsgFilter(true);
                else EnableMsgFilter(false);
            }
        }

        // Disable mouse detection if host form is not active.
        private void Host_Activated(object s, EventArgs e)
        {
            if (Enabled) EnableMsgFilter(true);
        }
        private void Host_Deactivate(object s, EventArgs e)
        {
            if (Enabled) EnableMsgFilter(false);
        }

        #region IDisposable Members
        /// <summary>
        /// Disposes this instance. This class uses shared application resources.
        /// </summary>
        public void Dispose()
        {
            if (Host == null) return;
            Host.Activated -= Host_Activated;
            Host.Deactivate -= Host_Deactivate;
            Enabled = false;
            Host = null;
        }
        #endregion

        //Avoid adding the message filter multiple times between Enable/Disable and Activate/Deactivate
        private bool __shown = false;
        private void EnableMsgFilter(bool show)
        {
            if (show && !__shown)
            {
                __shown = true;
                Application.AddMessageFilter(this);
                return;
            }
            if (!show && __shown)
            {
                __shown = false;
                Application.RemoveMessageFilter(this);
                return;
            }

        }

        #region IMessageFilter Members
        /// <summary>
        /// For internal use only.
        /// </summary>
        public bool PreFilterMessage(ref Message m)
        {
            //Exploratory Testing...
            //if (m.Msg != WM_MOUSEMOVE && m.Msg != 0x0113 && m.Msg != 0x0118 && m.Msg != 0x00A0) //WM_TIMER=0x0113, WM_SYSTIMER=0x0118, WM_NCMOUSEMOVE=0x00A0
            //    Diagnostics.WriteLine($"PreFilterMessage: {(m.HWnd == IntPtr.Zero ? "(null)" : Control.FromChildHandle(m.HWnd)?.Name ?? m.HWnd.ToString())} {Win32.TranslateWMMessage(m.HWnd, m.Msg)}");

            if (m.Msg == WM_MOUSEMOVE)
            {
                if (MouseMoved == null) return false; //This event has never been subscribed to.
                var mousePosition = Control.MousePosition;
                if (previousMousePosition == mousePosition) return false; //ignore redundant events for the same mouse position
                previousMousePosition = mousePosition;
                //To be accurate we should probably use Control.FromChildHandle(m.HWnd) instead of 'Host', but we want this filter to be as efficient as possible.
                MouseMoved(Host, new MouseEventArgs(0, 0, mousePosition.X, mousePosition.Y, 0));
                return false;
            }

            if (m.Msg == WM_LBUTTONDOWN ||
                m.Msg == WM_RBUTTONDOWN ||
                m.Msg == WM_MBUTTONDOWN ||
                m.Msg == WM_XBUTTONDOWN)
            {
                Click?.Invoke(Host, EventArgs.Empty);
                return false;
            }

            //We also get WM_MOUSELEAVE events when we move from the form into a child control, so we have to check if we are still within the host client area.
            //Warning: This event may be missed if the mouse is moved too fast, because we we don't get any messages once we leave the confines of the form.
            // To be reliable we must use a GlobalMouseHook(). But that has its own problems.
            if (m.Msg == WM_MOUSELEAVE && MouseLeave != null && m.HWnd == Host.Handle &&
                !Host.ClientRectangle.Contains(Host.PointToClient(Control.MousePosition)))
            {
                MouseLeave.Invoke(Host, EventArgs.Empty);
                return false;
            }

            return false;  //Always allow message to continue to the next filter control
        }
        #endregion
    }
}

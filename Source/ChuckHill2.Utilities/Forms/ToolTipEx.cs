//--------------------------------------------------------------------------
// <summary>
//   A custom tooltip component that extends System.Windows.Forms.ToolTip().
// </summary>
// <copyright file="ToolTipEx.cs" company="Chuck Hill">
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
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Extends System.Windows.Forms.Tooltip.
    /// Includes automatic tooltip initialization for all the controls on a form.
    /// Foreach Control.AccessableDescription that is not empty, a tooltip is set for the control.
    /// In addition, a special one-off tooltip popup may be shown on a control. Typically due to the result of some action.
    /// Upside:
    ///    Leverages pre-existing System.Windows.Forms.Tooltip.
    ///    Includes the ability to have post-action status popup.
    ///    Includes the ability to automatically retrieve popup messges from other sources.
    /// Downside:
    ///    TextBox popup works but is intermittent.
    ///    Popup text does not autowrap. Newlines must be used.
    ///    Popup title and icon properties are global to the entire ToolTip object not individual popups.
    ///    The one-off popup balloon cannot be positioned properly over the control. The position is from the top-left corner, Not the arrow origin as expected.
    /// </summary>
    public class ToolTipEx : ToolTip
    {
        private static readonly HashSet<Form> AllHosts = new HashSet<Form>();  //Do not allow multiple tooltip objects for a given Form.

        private CustomToolTip __showToolTip = null;

        private CustomToolTip ShowToolTip
        {
            get
            {
                if (__showToolTip == null) __showToolTip = new CustomToolTip(this); //Load-on-demand
                return __showToolTip;
            }
            set
            {
                if (__showToolTip == value) return;
                if (__showToolTip != null) __showToolTip.Dispose(); //Dispose upon null
                __showToolTip = value;
            }
        }

        private Form __host;

        /// <summary>
        /// The Form that is using this instance of the ToolTipEx.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Browsable(false)]
        public Form Host
        {
            get => __host;
            set
            {
                if (value == null)
                {
                    if (__host != null)
                    {
                        Destroy();
                        AllHosts.Remove(__host);
                        __host = null;
                    }
                    return;
                }

                if (value.Equals(__host)) return;
                if (AllHosts.Contains(__host)) throw new ArgumentException($"{nameof(ToolTipEx)} already exists for {__host.Name}.", nameof(Host));
                __host = value;
                AllHosts.Add(__host);

                //Event Sequence: Control.HandleCreated. Control.BindingContextChanged. Form.Load. Control.VisibleChanged. Form.Activated. Form.Shown
                __host.Load += (s, e) => Initialize(); //never called in designer
            }
        }

        /// <summary>
        /// Function that retrieves the custom tooltip popup message for the specified control. If undefined, uses Control.AccessibleDescription.
        /// This is used exclusively by AddToolTips() which is never called from within designed code.
        /// The Designer has its own mechanism for adding tooltips.
        /// </summary>
        [Category("Data"), Description("The method to retrieve runtime tooltip message string for a given control. A tooltip will not be set if the return string is null or empty. " + nameof(CreateRuntimeToolTips) + "() recursively calls this method for all controls when " + nameof(AddRuntimeToolTips) + "==true.")]
        public event TipMessageReaderDelegate TipMessageReader
        {
            add
            {
                //Try to add null and it reverts back to the default.
                if (value == null) _tipMessageReader = DefaultGetAccessibleDescription; //cannot be null!
                else _tipMessageReader = value;
            }
            remove
            {
                //Remove anything and it reverts back to the default.
                _tipMessageReader = DefaultGetAccessibleDescription; //cannot be null!
            }
        }

        private TipMessageReaderDelegate _tipMessageReader = DefaultGetAccessibleDescription;

        public delegate string TipMessageReaderDelegate(Control control);

        private static string DefaultGetAccessibleDescription(Control c) => c.AccessibleDescription;

        private void ResetTipMessageReader() => _tipMessageReader = DefaultGetAccessibleDescription;

        private bool ShouldSerializeMyFont() => _tipMessageReader != null && _tipMessageReader != DefaultGetAccessibleDescription;  // Returns true if the font has changed; otherwise, returns false. The designer writes code to the form only if true is returned.

        /// <summary>
        /// Add custom tooltips to all controls upon form load.
        /// </summary>
        [Category("Behavior"), Description("Enables runtime tooltips for all controls upon form shown. See " + nameof(CreateRuntimeToolTips) + "() and event " + nameof(TipMessageReader))]
        [DefaultValue(false)]
        public bool AddRuntimeToolTips { get; set; } = false;

        /// <summary>
        /// Flag to show if this instance is disposed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed { get; private set; } = true;

        /// <summary>
        /// Create tooltip-style help for all the controls in the specified form.
        /// Tooltips must be manually added via SetToolTip() or AddToolTips().
        /// ToolTipEx.Host must be set before this object can be used.
        /// There can be only ONE instance per owner form.
        /// </summary>
        public ToolTipEx()
        {
        }

        /// <summary>
        /// Create tooltip-style help for all the controls in the specified form with a specified container.
        /// There can be only ONE instance per owner form.
        /// It is assumed that this constructor is only called from within the designer.
        /// </summary>
        /// <param name="cont">An <see cref="T:System.ComponentModel.IContainer" /> that represents the container of the ToolTipEx class./>. </param>
        public ToolTipEx(IContainer cont) : base(cont)
        {
        }

        /// <summary>
        /// Initializes a new instance of the ToolTipEx class with automatic tooltip initialization.
        /// There can be only ONE instance per owner form.
        /// </summary>
        /// <param name="host">Form owner of these tooltips. Null throws an exception.</param>
        /// <param name="tipMessageReader">
        ///   Delegate that retrieves the tooltip popup message for the specified control.
        ///   If undefined, uses Control.AccessibleDescription.
        ///   This is used exclusively by AddToolTips().
        /// </param>
        public ToolTipEx(Form host, TipMessageReaderDelegate tipMessageReader = null)
        {
            if (host == null) throw new ArgumentNullException(nameof(host), "Form owner cannot be null.");
            AddRuntimeToolTips = true;
            Host = host;
            if (tipMessageReader != null) TipMessageReader += tipMessageReader;
        }

        private void Initialize()
        {
            //Called by Host property setter as this is only valid at that point.
            if (Host == null) return;
            IsDisposed = false;
            if (AddRuntimeToolTips) Host.Shown += (s, e) => CreateRuntimeToolTips();
        }

        private void Destroy()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Host.FormClosing -= Host_FormClosing;

            ShowToolTip = null;
        }

        protected override void Dispose(bool disposing)
        {
            Host = null;
            base.Dispose(disposing);
        }

        private void Host_FormClosing(object sender, FormClosingEventArgs e) => Dispose();

        #region Example OwnerDrawn ToolTip

        //void m_ToolTips_Popup(object sender, PopupEventArgs e)
        //{
        //    string tip = e.AssociatedControl.AccessibleDescription.Replace("\\n", Environment.NewLine);

        //    Form AssociatedForm = null;
        //    for(var c = e.AssociatedControl; c != null; c = c.Parent) AssociatedForm = c as Form;

        //    Graphics g = Graphics.FromHwnd(AssociatedForm.Handle);
        //    var font = new Font("Microsoft Sans Serif", 11.0f, FontStyle.Regular, GraphicsUnit.World);
        //    var sizeF = g.MeasureString(tip, font, AssociatedForm.ClientSize.Width - 20);
        //    e.ToolTipSize = Size.Add(sizeF.ToSize(),new Size(10,10));
        //    font.Dispose();
        //    g.Dispose();
        //}

        //void m_ToolTips_Draw(object sender, DrawToolTipEventArgs e)
        //{
        //    Graphics g = e.Graphics;
        //    g.SmoothingMode = SmoothingMode.AntiAlias;
        //    g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        //    var b = new LinearGradientBrush(e.Bounds, Color.Lavender, Color.SkyBlue, 45f);
        //    g.FillRectangle(b, e.Bounds);
        //    //FillRoundedRectangle(g, b, e.Bounds, 20);
        //    b.Dispose();

        //    g.DrawRectangle(Pens.LightSlateGray, Rectangle.Inflate(e.Bounds, -2, -2));
        //    //DrawRoundedRectangle(g, Pens.LightSlateGray, Rectangle.Inflate(e.Bounds, 0, 0), 20);

        //    var font = new Font("Microsoft Sans Serif", 11.0f, FontStyle.Regular, GraphicsUnit.World);
        //    g.DrawString(e.ToolTipText, font, Brushes.Black, e.Bounds.X + 5, e.Bounds.Y + 5);
        //    font.Dispose();
        //}

        #endregion Example OwnerDrawn ToolTip

        /// <summary>
        /// Recursivly assign non-empty/non-null tooltip messages to all controls on the host form.
        /// May be called more than once to update all tooltip messages.<br />
        /// Note: Popup messages DO NOT autowrap. The message strings must contain newline characters
        /// or literal @"\n" strings. These are automatically converted to newline characters.
        /// </summary>
        /// <param name="msgReader">Override TipMessageReader delegate that retrieves a control's tooltip message.</param>
        public void CreateRuntimeToolTips(TipMessageReaderDelegate msgReader = null)
        {
            if (Host == null) return;
            RecurseCreateRuntimeToolTips(Host, msgReader ?? _tipMessageReader);
        }

        private void RecurseCreateRuntimeToolTips(Control control, TipMessageReaderDelegate msgReader)
        {
            foreach (Control c in control.Controls)
            {
                if (c.HasChildren && c.Controls != null) RecurseCreateRuntimeToolTips(c, msgReader);  //recurse
                var message = msgReader(c);
                if (string.IsNullOrEmpty(message)) continue;  //no help tip
                base.SetToolTip(c, message.Replace(@"\n", Environment.NewLine));
            }
        }

        /// <summary>
        /// Show a custom one-shot popup tooltip. The lifetime is only once and cannot be reused. Useful for a post-action status.
        /// </summary>
        /// <param name="control">Control to assign the tooltip to.</param>
        /// <param name="msg">Tooltip message</param>
        /// <param name="title">Optional title of message. May be null or empty.</param>
        /// <param name="icon">Optional severity status icon</param>
        public void Show(Control control, string msg, string title = null, ToolTipIcon icon = ToolTipIcon.None) => ShowToolTip.Show(control, msg, title, icon);

        /// <summary>
        ///  When in the Forms designer, this sets ToolTipEx.Host as soon as the parent form becomes avilable.
        ///  This ensures ToolTipEx.Host property is serialized by the Designer.
        ///  This has no effect during runtime.
        /// </summary>
        public override ISite Site
        {
            // See: https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel.design.icomponentchangeservice?view=netcore-3.1
            get => base.Site;
            set
            {
                base.Site = value;
                var changeService = (IComponentChangeService)GetService(typeof(IComponentChangeService));
                if (changeService != null)  //==null if not in design mode
                    changeService.ComponentAdded += ChangeService_ComponentAdded;
            }
        }

        private void ChangeService_ComponentAdded(object sender, ComponentEventArgs e)
        {
            var changeService = (IComponentChangeService)sender;
            changeService.ComponentAdded -= ChangeService_ComponentAdded;
            if (!DesignMode) return;
            IDesignerHost designerHost = (IDesignerHost)GetService(typeof(IDesignerHost));
            if (designerHost == null || !(designerHost.RootComponent is Form)) return;
            Host = (Form)designerHost.RootComponent;
        }

        /// <summary>
        /// Mouse handler for the entire form. Captures all mouse and click
        /// events for entire form. Including those within child controls.
        /// Multiple instances may be used on the same form (I don't know why...).
        /// Just Dispose() upon completion to release shared resources..
        /// </summary>
        private class GlobalMouseHandler : IMessageFilter, IDisposable
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
                Debug.WriteLine($"Host Activated: {Host.Name}");
                if (Enabled) EnableMsgFilter(true);
            }

            private void Host_Deactivate(object s, EventArgs e)
            {
                Debug.WriteLine($"Host Deactivated: {Host.Name}");
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

            #endregion IDisposable Members

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
                //         To be reliable we must use a GlobalMouseHook(). But that has its own problems.
                if (m.Msg == WM_MOUSELEAVE && MouseLeave != null && m.HWnd == Host.Handle &&
                    !Host.ClientRectangle.Contains(Host.PointToClient(Control.MousePosition)))
                {
                    MouseLeave.Invoke(Host, EventArgs.Empty);
                    return false;
                }

                return false;  //Always allow message to continue to the next filter control
            }

            #endregion IMessageFilter Members
        }

        //Handle custom one-shot Tooltip. Typically for some action status.
        private class CustomToolTip : Timer
        {
            private const int FadeDelay = 400; //Regisry: HKEY_CURRENT_USER\Control Panel\Desktop\MenuShowDelay [REG_SZ] 400
            private readonly ToolTipEx TT;  //We need the Form host and parent ToolTip object so we can copy its properties.
            private bool Fading;
            private GlobalMouseHandler MouseHandler;

            private ToolTip __privateToolTip;

            private ToolTip PrivateToolTip
            {
                get => __privateToolTip;
                set
                {
                    if (__privateToolTip != null) __privateToolTip.Dispose();
                    __privateToolTip = value;
                }
            }

            public CustomToolTip(ToolTipEx tt) : base()
            {
                TT = tt;
                if (TT.Host == null) throw new ArgumentNullException("Host", "Cannot use Custom tooltips because the Form owner has not been set.");
                MouseHandler = new GlobalMouseHandler(TT.Host);
                MouseHandler.MouseMoved += MouseHandler_MouseMovedEvent;
                MouseHandler.Click += MouseHandler_ClickEvent;
            }

            protected override void Dispose(bool disposing)
            {
                base.Stop();
                MouseHandler.Dispose();
                PrivateToolTip = null;
                base.Dispose(disposing);
            }

            public void Show(Control control, string msg, string title = null, ToolTipIcon icon = ToolTipIcon.None)
            {
                if (string.IsNullOrWhiteSpace(msg)) return;

                // HACK: Icon and Title are associated with the entire ToolTip object not just the individual tooltip on the control.
                // In addition, the control may already have a tooltip associated with it. We don't want to overwrite it.
                // SO we need to clone an empty copy of this underlying ToolTip object and use the clone with a modified Icon and Title.
                // In the end, the clone will be disposed either when this method is called again or when Dispose() is called.

                // Compute duration the tooltip will be displayed.
                // It equals at least base.AutoPopDelay or at most 20 secs, whichever is larger (== 120 words. more words is still 20 sec).
                // Mouse movement or clicks will hide the tooltip immediately, if they're not interested. We want to give them time to read...
                int duration = Math.Max(Math.Min(msg.Split(new[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries).Length * 167, 20000), TT.AutoPopDelay);

                OnTick(EventArgs.Empty); //Stop prevous custom ToolTip
                PrivateToolTip = new ToolTip();
                PrivateToolTip.BackColor = TT.BackColor;
                PrivateToolTip.ForeColor = TT.BackColor;
                PrivateToolTip.AutoPopDelay = duration;
                PrivateToolTip.InitialDelay = 0;
                PrivateToolTip.ReshowDelay = Int32.MaxValue;
                PrivateToolTip.IsBalloon = false; // TT.IsBalloon; //Balloon gets positioned strangely relative to control, so it is disabled.
                PrivateToolTip.UseAnimation = TT.UseAnimation;
                PrivateToolTip.UseFading = TT.UseFading;
                PrivateToolTip.ShowAlways = TT.ShowAlways;
                PrivateToolTip.StripAmpersands = TT.StripAmpersands;
                PrivateToolTip.ToolTipIcon = icon;
                PrivateToolTip.ToolTipTitle = title == string.Empty ? (icon == ToolTipIcon.None ? null : icon.ToString()) : title;
                PrivateToolTip.Tag = control; //needed if the tooltip needs to be forceably closed due to mousemovements or clicks.
                if (TT.OwnerDraw)
                {
                    PrivateToolTip.OwnerDraw = TT.OwnerDraw;
                    //Hack: Copy these ToolTip events en masse
                    _onDrawField.SetValue(PrivateToolTip, _onDrawField.GetValue(TT));  // TempTip.Draw = base.Draw;
                    _onPopupField.SetValue(PrivateToolTip, _onPopupField.GetValue(TT)); //TempTip.Popup = base.Popup;
                }

                PrivateToolTip.Show(msg, control, 10, control.Height / 2, PrivateToolTip.AutoPopDelay); //Show the tool tip
                MouseHandler.Enabled = true; //Enable mouse detection.
                Hide(false); //Set the close trigger.
            }

            private static readonly FieldInfo _onDrawField = typeof(ToolTip).GetField("onDraw", BindingFlags.NonPublic | BindingFlags.Instance);
            private static readonly FieldInfo _onPopupField = typeof(ToolTip).GetField("onPopup", BindingFlags.NonPublic | BindingFlags.Instance);

            protected override void OnTick(EventArgs e)
            {
                Fading = false;
                base.Stop();
                PrivateToolTip = null; //Also disposes the Tooltip.
            }

            private void MouseHandler_ClickEvent(object sender, EventArgs e)
            {
                //If the Mouse is clicked anywhere, the CustomToolTip is hidden.
                if (PrivateToolTip == null) return;
                MouseHandler.Enabled = false; //We're done. we don't need mouse clicks any more.
                Hide(true); //True to hide immediately, accomadating for fade
            }

            private Point StartMousePos = Point.Empty; //set in Hide()

            private void MouseHandler_MouseMovedEvent(object sender, MouseEventArgs e)
            {
                //If the Mouse mouse moves too far from the cursor position as defined in the Hide() trigger, the CustomToolTip is hidden.
                if (PrivateToolTip == null) return;
                if (e.X > StartMousePos.X + 20 ||
                    e.X < StartMousePos.X - 20 ||
                    e.Y > StartMousePos.Y + 20 ||
                    e.Y < StartMousePos.Y - 20)
                {
                    MouseHandler.Enabled = false; //We're done. we don't need mouse movement any more.
                    Hide(true); //True to hide immediately, accommodating for fade
                }
            }

            private void Hide(bool now)
            {
                if (Fading) return;  //don't re-trigger if we are in the middle of a closing fade
                base.Stop();

                StartMousePos = Control.MousePosition; //Used by MouseHandler_MouseMovedEvent().

                int delay = PrivateToolTip.UseFading ? FadeDelay : 1;

                if (now) //Even though AutoPopDelay has not completed, Hide the tooltip immediately, accommodating for fade.
                {
                    Fading = true; //don't re-trigger if we are in the middle of a fade, Fading is turned off in OnTick()
                    base.Interval = delay;
                    PrivateToolTip.Hide((Control)PrivateToolTip.Tag);
                    base.Start();
                    return;
                }

                base.Interval = PrivateToolTip.AutoPopDelay + delay;
                base.Start();
            }

            //Get List of controls in a ToolTip object
            //private static readonly FieldInfo _tools = typeof(ToolTip).GetField("tools", BindingFlags.NonPublic | BindingFlags.Instance);
            //private static Control[] GetToolTipControls(ToolTip tt)
            //{
            //    if (tt == null) return new Control[0];
            //    var controls = ((Hashtable)_tools.GetValue(tt)).Keys.Cast<Control>().ToArray();
            //    return controls;
            //}
        }
    }
}

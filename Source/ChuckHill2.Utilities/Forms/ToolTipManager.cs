//--------------------------------------------------------------------------
// <summary>
//   A custom tooltip component that replaces System.Windows.Forms.ToolTip().
// </summary>
// <copyright file="ToolTipManager.cs" company="Chuck Hill">
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
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

//References:
//  https://cboard.cprogramming.com/csharp-programming/119414-custom-tooltip.html
//  http://newapputil.blogspot.com/2015/08/create-custom-tooltip-dialog-from-form.html
//  https://stackoverflow.com/questions/2461448/how-to-make-a-floating-tooltip-control-in-windows-forms
//  https://stackoverflow.com/questions/14695357/show-tooltip-on-textbox-entry
// https://flylib.com/books/en/2.742.1/design_time_support_for_custom_controls.html
// https://www.codeproject.com/Articles/98967/A-ToolTip-with-Title-Multiline-Contents-and-Image
//Nomenclature Rules:
//  • All variable and function names are CamelCased. With the exception of the following, use of underscores is limited to Visual Studio event method auto creation.
//  • Const variables are uppercase (e.g. THISISMYCONST).
//  • Class-level variables are Capitalized (e.g. ThisIsMyVariable).
//  • Function-level variables begin with a lowercase letter  (e.g. thisIsMyVarable).
//  • Class-level variables whose scope is limited to a single function have a leading underscore followed by a lowercase letter (e.g. _thisIsMyVarable).
//  • Class-level variables whose scope is limited to a single property has the same name as the property followed by 2 underscores and the fist character is lowercase (e.g. MyProperty uses __myProperty).
//  • Controls within a form start with "m_' followed by a 2-3 letter mnemonic describing the type of control, followed by the CamelCase'd name (e.g. m_txtMyTextbox). This makes them easy to find with intellisense.

namespace ChuckHill2.Forms
{
    /// <summary>
    /// ToolTips for all the form controls. Does not support non-Control-based objects.
    /// </summary>
    [ProvideProperty("Message", typeof(Control))]
    [ProvideProperty("Title", typeof(Control))]
    [ProvideProperty("Icon", typeof(Control))]
    [ToolboxItemFilter("System.Windows.Forms")]
    public class ToolTipManager : Component, IExtenderProvider, IDisposable
    {
        private const int FadeDelay = 400; //Regisry: HKEY_CURRENT_USER\Control Panel\Desktop\MenuShowDelay [REG_SZ] 400
        private const int defaultAutoPopDelay = 5000; //These are the System.Windows.Forms.ToolTip defaults.
        private const int defaultInitialDelay = 1000;
        private const int defaultReshowDelay  = 1500;
        private const int defaultFadeInterval = 15;
        private static readonly HashSet<Form> AllHosts = new HashSet<Form>();  //Do not allow multiple tooltip objects for a given Form.

        private GlobalMouseHandler MouseHandler;

        private readonly Dictionary<Control, TipProp> TipProps = new Dictionary<Control, TipProp>();

        TipProp __customToolTip = null;
        private TipProp CustomToolTip //keep last used for disposing only
        {
            get => __customToolTip;
            set
            {
                if (__customToolTip != null) __customToolTip.Dispose();
                __customToolTip = value;
            }
        }

        //Required 'global' objects used by ALL instances of the TipProp class
        private PopupPanel TTPanel; //Only ONE tooltip may be shown at a time.
        private TTResources Resources;
        private MyTimer IdleTimer;
        private MyTimer AutoPopTimer;
        private MyTimer ShowTimer; //fade-in
        private MyTimer HideTimer; //fade-out

        #region All Properties

        private Form __host;
        /// <summary>
        /// The Form that is using this instance of the ToolTipManager.
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
                if (AllHosts.Contains(__host)) throw new ArgumentException($"{nameof(ToolTipManager)} already exists for {__host.Name}.", nameof(Host));
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
        [Category("Behavior"), Description("Enables runtime tooltips for all controls upon form first shown. See " + nameof(CreateRuntimeToolTips) + "() and event " + nameof(TipMessageReader))]
        [DefaultValue(false)]
        public bool AddRuntimeToolTips { get; set; } = false;

        /// <summary>
        /// Flag to show if this instance is disposed.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsDisposed { get; private set; } = true;

        #region Graphics Properties
        //https://stackoverflow.com/questions/16428389/creating-an-ambient-property-that-inherits-from-a-parents-property

        private Font __messageFont;
        /// <summary>
        /// The tooltip popup consists of 3 parts, the main message, an optional title/caption, and an optional severity icon.
        /// This is the font used to draw the message string.
        /// </summary>
        [Category("Appearance"), Description("The font used to draw the message string.")]
        public Font MessageFont
        {
            get
            {
                if (__messageFont == null)
                {
                    if (Host != null) return new Font(Host.Font.FontFamily, (int)(Host.Font.Size + 0.5f - 1), Host.Font.Style, Host.Font.Unit, Host.Font.GdiCharSet, Host.Font.GdiVerticalFont);
                    return SystemFonts.MessageBoxFont;
                }
                return __messageFont;
            }
            set
            {
                __messageFont = value;
                if (Resources == null) return;
                Resources.MessageFont = value; //Use Binding? https://www.codeproject.com/Articles/15822/Bind-Better-with-INotifyPropertyChanged
            }
        }
        private bool ShouldSerializeMessageFont()
        {
            if (Host == null || MessageFont == SystemFonts.MessageBoxFont) return false;

            return Host.Font.FontFamily.Name != MessageFont.FontFamily.Name ||
                   (float)(int)(Host.Font.Size + 0.5f - 1f) != MessageFont.Size ||
                   Host.Font.Style != MessageFont.Style;
        }
        private void ResetMessageFont()
        {
            if (!MessageFont.IsSystemFont) MessageFont.Dispose();
            MessageFont = null;
        }

        private Font __titleFont;
        /// <summary>
        /// The tooltip popup consists of 3 parts, the main message, an optional title/caption, and an optional severity icon.
        /// This is the font used to draw the title string.
        /// </summary>
        [Category("Appearance"), Description("The font used to draw the title string.")]
        public Font TitleFont
        {
            get
            {
                if (__titleFont == null)
                {
                    if (Host != null) return new Font(Host.Font.FontFamily, (int)(Host.Font.Size + 0.5f - 1), FontStyle.Bold, Host.Font.Unit, Host.Font.GdiCharSet, Host.Font.GdiVerticalFont);
                    return SystemFonts.MessageBoxFont;
                }
                return __titleFont;
            }
            set
            {
                __titleFont = value;
                if (Resources == null) return;
                Resources.TitleFont = value;
            }
        }
        private bool ShouldSerializeTitleFont()
        {
            if (Host == null || TitleFont == SystemFonts.MessageBoxFont) return false;

            return Host.Font.FontFamily.Name != TitleFont.FontFamily.Name ||
                   (float)(int)(Host.Font.Size + 0.5f - 1f) != TitleFont.Size ||
                   FontStyle.Bold != TitleFont.Style;
        }
        private void ResetTitleFont()
        {
            if (!TitleFont.IsSystemFont) TitleFont.Dispose();
            TitleFont = null;
        }

        private Color __messageTextColor;
        /// <summary>
        /// The tooltip popup consists of 3 parts, the main message, an optional title/caption, and an optional severity icon.
        /// This is the color of the message text.
        /// </summary>
        [Category("Appearance"), Description("The color of the message text.")]
        public Color MessageTextColor
        {
            get
            {
                if (__messageTextColor == Color.Empty) return SystemColors.InfoText;
                return __messageTextColor;
            }
            set
            {
                __messageTextColor = value;
                if (Resources == null) return;
                Resources.MessageTextColor = value;
            }
        }
        private bool ShouldSerializeMessageTextColor() => MessageTextColor != SystemColors.InfoText;
        private void ResetMessageTextColor() { MessageTextColor = SystemColors.InfoText; }

        private Color __titleTextColor;
        /// <summary>
        /// The tooltip popup consists of 3 parts, the main message, an optional title/caption, and an optional severity icon.
        /// This is the color of the title text.
        /// </summary>
        [Category("Appearance"), Description("The color of the title text.")]
        public Color TitleTextColor
        {
            get
            {
                if (__titleTextColor == Color.Empty) return SystemColors.InfoText;
                return __titleTextColor;
            }
            set
            {
                __titleTextColor = value;
                if (Resources == null) return;
                Resources.TitleTextColor = value;
            }
        }
        private bool ShouldSerializeTitleTextColor() => TitleTextColor != SystemColors.InfoText;
        private void ResetTitleTextColor() { TitleTextColor = SystemColors.InfoText; }

        private Color __fillColor;
        /// <summary>
        /// The tooltip popup consists of 3 parts, the main message, an optional title/caption, and an optional severity icon.
        /// This is the color of the tooltip background.
        /// </summary>
        [Category("Appearance"), Description("The color of the tooltip background.")]
        public Color FillColor
        {
            get
            {
                if (__fillColor == Color.Empty) return SystemColors.Info;
                return __fillColor;
            }
            set
            {
                __fillColor = value;
                if (Resources == null) return;
                Resources.FillColor = value;
            }
        }
        private bool ShouldSerializeFillColor() => FillColor != SystemColors.Info;
        private void ResetFillColor() { FillColor = SystemColors.Info; }

        #endregion [Graphics Properties]

        #region Timer Properties
        private int __autoPopDelay = defaultAutoPopDelay;
        /// <summary>
        /// Controls the period of time the tooltips remain visible if the
        /// mouse pointer is stationary within the control.
        /// </summary>
        /// <remarks>
        /// This property enables you to shorten or lengthen the time that the
        /// tooltip window is displayed when the mouse pointer is over a control.
        /// For example, if you display extensive help in a tooltip window, you
        /// can increase the value of this property to ensure that the user has
        /// sufficient time to read the text.
        /// </remarks>
        [Category("Behavior"), Description("Controls the period of time the tooltips remain visible if the mouse pointer is stationary within the control.")]
        [DefaultValue(defaultAutoPopDelay)]
        public int AutoPopDelay
        {
            get => __autoPopDelay;
            set { __autoPopDelay = value; AutoPopTimer.Interval = __autoPopDelay; }
        }

        private int __initialDelay = defaultInitialDelay;
        /// <summary>
        /// Gets or sets the time that passes before the tooltip appears.
        /// </summary>
        /// <remarks>
        /// This property enables you to shorten or lengthen the time that the
        /// control waits before displaying a tooltip window. If the value
        /// of the InitialDelay property is set to a value that is too long
        /// in duration, the user of your application may not know that your
        /// application provides tooltip help. You can use this property to
        /// ensure that the user has tooltips displayed quickly by shortening
        /// the time specified.
        /// </remarks>
        [Category("Behavior"), Description("Gets or sets the time that passes before the tooltip appears.")]
        [DefaultValue(defaultInitialDelay)]
        public int InitialDelay
        {
            get => __initialDelay;
            set { __initialDelay = value; IdleTimer.Interval = __initialDelay; }
        }

        /// <summary>
        /// Gets or sets the length of time that must transpire before subsequent
        /// tooltip windows appear as the mouse pointer moves from one control
        /// to another.
        /// </summary>
        /// <remarks>
        /// This property enables you to shorten or lengthen the time that the
        /// tooltip waits before displaying a tooltip window after a previous
        /// tooltip window is displayed. The first time a tooltip window is
        /// displayed the value of the InitialDelay property is used to determine
        /// the delay to apply before initially showing the tooltip window. When
        /// a tooltip window is currently being displayed and the user moves the
        /// cursor to another controlt or control that displays a tooltip
        /// window, the value of the ReshowDelay property is used before showing
        /// the tooltip for the new control. The tooltip window from the previous
        /// control must still be displayed in order for the delay specified in
        /// the ReshowDelay property to be used; otherwise the InitialDelay
        /// property value is used.
        /// </remarks>
        [Category("Behavior"), Description("Gets or sets the time that must transpire before subsequent tooltip windows appear.")]
        [DefaultValue(defaultReshowDelay)]
        public int ReshowDelay { get; set; } = defaultReshowDelay;

        /// <summary>
        /// Gets or sets a value determining whether a fade effect should be used when displaying and hiding the ToolTip.
        /// </summary>
        [Category("Behavior"), Description("When set to true, a fade effect is used when ToolTips are shown and hidden.")]
        [DefaultValue(true)]
        public bool UseFading { get; set; } = true;

#endregion //[Timer Properties]
        #endregion //[All Properties]

        #region Constructors/Destructors
        /// <summary>
        /// Create tooltip-style help for all the controls in the specified form.
        /// ToolTipManager.Host must be set before this object can be used.
        /// There can be only ONE instance per owner form.
        /// </summary>
        public ToolTipManager()
        {
        }

        /// <summary>
        /// Create tooltip-style help for all the controls in the specified form with a specified container.
        /// There can be only ONE instance per owner form.
        /// It is assumed that this constructor is only called from within the designer.
        /// </summary>
        /// <param name="cont">An <see cref="T:System.ComponentModel.IContainer" /> that represents the container of the ToolTipManager class./>. </param>
        public ToolTipManager(IContainer cont) : this()
        {
            if (cont == null) throw new ArgumentNullException(nameof(cont));
            cont.Add((IComponent)this);
        }

        /// <summary>
        /// Initializes a new instance of the ToolTipManager class with automatic tooltip initialization.
        /// There can be only ONE instance per owner form.
        /// </summary>
        /// <param name="host">Form owner of these tooltips. Null throws an exception.</param>
        /// <param name="tipMessageReader">
        ///   Delegate that retrieves the tooltip popup message for the specified control.
        ///   If undefined, uses Control.AccessibleDescription.
        ///   This is used exclusively by AddToolTips().
        /// </param>
        public ToolTipManager(Form host, TipMessageReaderDelegate tipMessageReader = null)
        {
            if (host == null) throw new ArgumentNullException(nameof(host), "Form owner cannot be null.");
            AddRuntimeToolTips = true;
            Host = host;
            if (tipMessageReader != null) TipMessageReader += tipMessageReader;
        }

        private void Initialize()
        {
            if (Host == null || !IsDisposed || DesignMode) return;
            IsDisposed = false;

            Resources = new TTResources();
            Resources.MessageFont = MessageFont;
            Resources.TitleFont = TitleFont;
            Resources.MessageTextColor = MessageTextColor;
            Resources.TitleTextColor = TitleTextColor;
            Resources.FillColor = FillColor;

            IdleTimer = new MyTimer() { Name = "IdleTimer", Interval = InitialDelay };
            AutoPopTimer = new MyTimer() { Name = "AutoPopTimer", Interval = AutoPopDelay };
            ShowTimer = new MyTimer() { Name = "ShowTimer", Interval = defaultFadeInterval };  //fade-in
            HideTimer = new MyTimer() { Name = "HideTimer", Interval = defaultFadeInterval };  //fade-out
            MouseHandler = new GlobalMouseHandler(Host);
            MouseHandler.MouseMoved += MouseHandler_MouseMovedEvent;
            MouseHandler.MouseLeave += MouseHandler_MouseLeave;
            MouseHandler.Click += (s, e) => Hide(UseFading);
            MouseHandler.Enabled = true;
            TTPanel = new PopupPanel(Host, true);

            IdleTimer.Tick += (s, e) =>
            {
                IdleTimer.Stop();
                this.Hide(UseFading);
                Debug.WriteLine($"IdleTimer.Tick: Showing {(IdleTimer.TipProp?.Control.Name ?? "(null)")}");
                IdleTimer.TipProp.Show(UseFading);
            };
            ShowTimer.Tick += (s, e) =>
            {
                TTPanel.Opacity += 0.04;
                if (TTPanel.Opacity >= 0.99)
                {
                    ShowTimer.Stop();
                    ShowTimer.Interval = defaultFadeInterval;
                    Debug.WriteLine($"ShowTimer.Tick: Shown {(ShowTimer.TipProp?.Control.Name ?? "(null)")}");
                    AutoPopTimer.TipProp = ShowTimer.TipProp;
                    HideTimer.TipProp = AutoPopTimer.TipProp;
                    AutoPopTimer.Start();
                }
            };
            AutoPopTimer.Tick += (s, e) =>
            {
                AutoPopTimer.Stop();
                Debug.WriteLine($"AutoPopTimer.Tick: Hiding {(AutoPopTimer.TipProp?.Control.Name ?? "(null)")}");
                HideTimer.Start();
            };
            HideTimer.Tick += (s, e) =>
            {
                TTPanel.Opacity -= 0.04;
                if (TTPanel.Opacity <= 0.04)
                {
                    HideTimer.Stop();
                    HideTimer.Interval = defaultFadeInterval;
                    Debug.WriteLine($"HideTimer.Tick: Hidden {(HideTimer.TipProp?.Control.Name ?? "(null)")}");
                    TTPanel.Opacity = 0;
                    TTPanel.Hide();
                    AddReshowDelay(HideTimer.TipProp);
                    HideTimer.TipProp = null;
                }
            };

            Host.FormClosing += Owner_FormClosing;
            if (AddRuntimeToolTips) Host.Shown += (s, e) => CreateRuntimeToolTips();
        }

        private void Destroy()
        {
            if (IsDisposed) return;
            IsDisposed = true;
            Host.FormClosing -= Owner_FormClosing;

            this.Clear();
            DestroyReshowDelayPool();
            if (Resources != null) { Resources.Dispose(); Resources = null; }
            if (MouseHandler != null) { MouseHandler.Dispose(); MouseHandler = null; }
            if (IdleTimer != null) { IdleTimer.Stop(); IdleTimer.Dispose(); IdleTimer = null; }
            if (AutoPopTimer != null) { AutoPopTimer.Stop(); AutoPopTimer.Dispose(); AutoPopTimer = null; }
            if (ShowTimer != null) { ShowTimer.Stop(); ShowTimer.Dispose(); ShowTimer = null; }
            if (HideTimer != null) { HideTimer.Stop(); HideTimer.Dispose(); HideTimer = null; }
            if (TTPanel != null) { TTPanel.Close(); TTPanel.Dispose(); TTPanel = null; }
            CustomToolTip = null;
        }

        /// <summary>
        /// Close/Dispose/Disassemble everything, so the GC can easily clean up the fragments.
        /// Calling this more than once is harmless.
        /// </summary>
        /// <param name="disposing">True to release both managed and unmanaged resources; False to release only unmanaged</param>
        protected override void Dispose(bool disposing)
        {
            Host = null;
            base.Dispose(disposing);
        }

        private void Owner_FormClosing(object sender, FormClosingEventArgs e) => Dispose();
#endregion

        /// <summary>
        /// Recursivly assign non-empty/non-null tooltip messages to all controls on the host form.
        /// May be called more than once to update all tooltip messages.<br />
        /// Note: Messages autowrap to a 3x2 box if message does not contain newlines.<br />
        /// Note: This cannot overwrite any tooltips created by RegisterTooltip() or the Designer. It must be unregistered first.
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
                var tp = GetTipProp(c);
                if (tp != null)
                {
                    if (tp.CreatedBy != TipProp.CreationSource.Auto) continue; //Registered via RegisterTooltip() do not overwrite.
                    tp.Dispose();
                }

                TipProps[c] = new TipProp(this, c, message);
            }
        }

        /// <summary>
        /// Clear/Remove/Purge all tooltips
        /// </summary>
        public void Clear()
        {
            foreach (var kv in TipProps) { kv.Value.Dispose(); }
            TipProps.Clear();
        }

        /// <summary>
        /// Show a custom one-shot popup tooltip. The lifetime is only once and cannot be reused. Useful for a post-action status.
        /// </summary>
        /// <param name="control">Control to associate tooltip with. Null will throw an exception.</param>
        /// <param name="message">Message to display.</param>
        /// <param name="title">Optional title of message. May be null or empty.</param>
        /// <param name="icon">Optional severity icon to show.</param>
        public void Show(Control control, string message, string title = null, ToolTipIcon icon = ToolTipIcon.None)
        {
            if (control == null) return;
            if (icon == ToolTipIcon.None && string.IsNullOrEmpty(message) && string.IsNullOrEmpty(title)) return;
            if (Host==null) Host = control.FindForm();

            if (title == null) title = icon == ToolTipIcon.None ? string.Empty : icon.ToString();
            else title = Squeeze(title);
            CustomToolTip = new TipProp(this, control, message, title, icon, TipProp.CreationSource.Custom);
            Hide(UseFading);
            CustomToolTip.Show(UseFading);
        }

        /// <summary>
        /// Associate or update a tooltip of a control. If all 3 properties are empty, the tooltip is removed.
        /// </summary>
        /// <param name="control">Control to associate tooltip with. If null, this method does nothing.</param>
        /// <param name="message">Message to display.  If empty, no message will be shown.</param>
        /// <param name="title">Title of message. If empty, no no title will be shown. If null, title will default to the severity name</param>
        /// <param name="icon">Severity icon to show or None</param>
        public void RegisterTooltip(Control control, string message, string title, ToolTipIcon icon)
        {
            if (control == null) return;
            message = message?.Trim() ?? string.Empty;

            if (icon == ToolTipIcon.None && message.Length == 0 && string.IsNullOrEmpty(title))  //Remove tooltip
            {
                TipProps.Remove(control);
                return;
            }

            if (title == null) title = icon == ToolTipIcon.None ? string.Empty : icon.ToString();
            else title = Squeeze(title);

            var tp = GetTipProp(control);
            TipProps[control] = new TipProp(this, control, message, title, icon, TipProp.CreationSource.Registered);
            tp?.Dispose();
        }
        public void RegisterTooltip(Control control, string message) => RegisterTooltip(control, message, null, 0);
        public void RegisterTooltip(Control control, string message, string title) => RegisterTooltip(control, message, title, 0);
        public void RegisterTooltip(Control control, string message, ToolTipIcon icon) => RegisterTooltip(control, message, null, icon);
        public void RegisterTooltip(Control control, ToolTipIcon icon) => RegisterTooltip(control, null, null, icon);

        /// <summary>
        /// Remove/unassociate a tooltip from a control.
        /// </summary>
        /// <param name="control">Control to associate tooltip with. Null will throw an exception.</param>
        public void UnregisterTooltip(Control control) => RegisterTooltip(control, null, null, 0);

        private void MouseHandler_MouseLeave(object sender, EventArgs e)
        {
            Debug.WriteLine($"Form MouseLeave");
            IdleTimer.Stop(); //Mouse has left the form. Hide all the tooltips.
            Hide(UseFading);
        }

        Control _currentControl = null;
        private void MouseHandler_MouseMovedEvent(object sender, MouseEventArgs ev)
        {
            var screenPos = ev.Location;

            Control c = GetVisibleNestedChildAtPoint(Host, Host.PointToClient(screenPos));

            if (ReshowDelayed(c)) return;

            if (c == _currentControl)
            {
                Debug.WriteLine($"MouseMovedEvent: {c?.Name ?? "(null)"}: Still in current control.");
                return;
            }

            if (Host.Controls.Count < 2 || c != null) _currentControl = c;

            var tp = GetTipProp(c);
            if (tp == null)  //ignore controls without a tooltip
            {
                Debug.WriteLine($"MouseMovedEvent: {c?.Name ?? "(null)"}: HasTipProp=false. Stopping IdleTimer and Hiding.");
                //_currentControl = null; //uncomment if user should  be allowed to re-enter the control from form space to see the tooltip again/
                IdleTimer.Stop();
                Hide(UseFading);
                return;
            }

            Debug.WriteLine($"MouseMovedEvent: {c?.Name ?? "(null)"}: Starting new tooltip.");
            //ReshowDelayTimer.Stop();
            //ReshowDelayTimer.TipProp = null;
            IdleTimer.Stop();
            Hide(UseFading);
            IdleTimer.TipProp = tp;
            IdleTimer.Start();
        }

        private void Hide(bool useFading = true)
        {
            if (TTPanel.Visible)
            {
                AutoPopTimer.Stop();
                ShowTimer.Stop();
                ShowTimer.TipProp.Hide(useFading);
            }
        }
        private TipProp GetTipProp(Control c)
        {
            if (c == null) return null;
            return TipProps.TryGetValue(c, out var v) ? v : null;
        }
        private Control GetVisibleNestedChildAtPoint(Control owner, Point pos)
        {
            Control c = owner.GetChildAtPoint(pos, GetChildAtPointSkip.Invisible); //doesn't search grandchildren.
            if (c == null) return null;

            //NumericUpDown consists of 2 internal controls that are not accessible, so we ignore them. Are there others?
            if (c is System.Windows.Forms.NumericUpDown) return c;

            if (c.Controls.Count == 0) return c;

            pos.X -= c.Bounds.X;
            pos.Y -= c.Bounds.Y;
            var c2 = GetVisibleNestedChildAtPoint(c, pos);
            return c2 == null ? c : c2;
        }

        private readonly List<MyTimer> ReshowDelayTimerPool = new List<MyTimer>();
        private void AddReshowDelay(TipProp tp)
        {
            if (tp == null) return;
            if (tp.CreatedBy == TipProp.CreationSource.Custom) return;
            var tmr = ReshowDelayTimerPool.FirstOrDefault(t => t.TipProp == null);
            if (tmr==null)
            {
                tmr = new MyTimer() { Name = "ReshowDelayTimer" };
                tmr.Tick += ReshowDelay_Tick;
                ReshowDelayTimerPool.Add(tmr);
            }

            tmr.Interval = ReshowDelay; //just in case caller dynamically changes timeout after first use.
            tmr.TipProp = tp;
            Debug.WriteLine($"AddReshowDelay: {tmr.TipProp?.Control.Name ?? "(null)"} Unblocked");
            tmr.Start();
        }
        private void ReshowDelay_Tick(object sender, EventArgs e)
        {
            var tmr = (MyTimer)sender;
            tmr.Stop();
            Debug.WriteLine($"ReshowDelay_Tick: {tmr.TipProp?.Control.Name??"(null)"} Unblocked");
            tmr.TipProp = null;
        }
        private bool ReshowDelayed(Control c)
        {
            var blocked = ReshowDelayTimerPool.Any(t => t.TipProp == null ? false : t.TipProp.Control == c);
            //if (blocked) Diagnostics.WriteLine($"ReshowDelayed: {c?.Name ?? "(null)"} Blocked");
            return blocked;
        }
        private void DestroyReshowDelayPool() { ReshowDelayTimerPool.ForEach(tmr => { tmr.Stop(); tmr.Dispose(); }); ReshowDelayTimerPool.Clear(); }

        /// <summary>
        /// Strip one or more whitspace chars (including newlines) and replace with a single space char.
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <returns>fixed up single-line string</returns>
        public static string Squeeze(string s)
        {
            if (string.IsNullOrEmpty(s)) return string.Empty;
            //This is 2.6x faster than 'return Regex.Replace(s.Trim(), "[\r\n \t]+", " ");'
            StringBuilder sb = new StringBuilder(s.Length);
            char prev = ' ';
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if (c > 0 && c < 32) c = ' ';
                if (prev == ' ' && prev == c) continue;
                prev = c;
                sb.Append(c);
            }
            if (prev == ' ') sb.Length = sb.Length - 1;
            return sb.ToString();
        }

        /// <summary>
        ///  When in the Forms designer, this sets ToolTipManager.Host as soon as the parent form becomes avilable.
        ///  This ensures ToolTipManager.Host is serialized by the Designer.
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
                if (changeService !=null)  //==null if not in design mode
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

        #region IExtenderProvider
        //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel?view=net-5.0
        //https://www.codeproject.com/Articles/4683/Getting-to-know-IExtenderProvider

        /// <summary>Returns True if the ToolTip can offer an extender property to the specified target component. Exclusively used by Forms Designer.</summary>
        /// <param name="target">The target object to add an extender property to. </param>
        /// <returns>True if the <see cref="T:System.Windows.Forms.ToolTip" /> class can offer one or more extender properties; otherwise, False.</returns>
        /// <remarks>Used by the Forms Designer.</remarks>
        public bool CanExtend(object target) => target is Control && target.GetType() != this.GetType();

        /// <summary>Retrieves the ToolTip message text associated with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> for which to retrieve the ToolTip text. </param>
        /// <returns>A <see cref="T:System.String" /> containing the ToolTip text for the specified control.</returns>
        /// <remarks>Used by the Forms Designer.</remarks>
        [Category("ToolTip")]
        [DefaultValue(""), Localizable(true), Description("Set the tooltip message. If this text does not contain newlines, it is autowrapped to fit within a box that has a 3x2 ratio. If empty, there is no message.")]
        [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
        public string GetMessage(Control control)
        {
            var tp = GetTipProp(control);
            if (tp == null) return string.Empty;
            return tp.Message;
        }

        /// <summary>Associates ToolTip message text with the specified control.  If this text does not contain newlines, it is autowrapped to fit within a box that has a 3x2 ratio. If empty, there is no message.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> to associate the ToolTip text with.</param>
        /// <param name="message">The ToolTip text to display when the pointer is on the control. </param>
        /// <remarks>Used by the Forms Designer.</remarks>
        public void SetMessage(Control control, string message)
        {
            message = message?.Trim() ?? string.Empty;
            var tp = GetTipProp(control);
            if (tp == null)
            {
                if (message.Length == 0) return;
                TipProps[control] = new TipProp(this, control, message, string.Empty, ToolTipIcon.None, TipProp.CreationSource.Designer);
            }
            else
            {
                if (message == tp.Message) return;
                if (tp.ToolTipIcon == ToolTipIcon.None && message.Length == 0 && tp.Title.Length == 0)
                {
                    TipProps.Remove(control);
                    return;
                }

                TipProps[control] = new TipProp(this, control, message, tp.Title, tp.ToolTipIcon, TipProp.CreationSource.Designer);
                tp.Dispose();
            }
        }

        /// <summary>Retrieves the ToolTip title text associated with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> for which to retrieve the ToolTip text. </param>
        /// <returns>A <see cref="T:System.String" /> containing the ToolTip text for the specified control.</returns>
        /// <remarks>Used by the Forms Designer.</remarks>
        [Category("ToolTip")]
        [DefaultValue(""), Localizable(true), Description("Set an optional title/caption on the tooltip. It is a single line and will not wrap. If empty, there is no title.")]
        public string GetTitle(Control control)
        {
            var tp = GetTipProp(control);
            if (tp == null) return string.Empty;
            return tp.Title;
        }

        /// <summary>Associates ToolTip title text with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> to associate the ToolTip text with. </param>
        /// <param name="title">The ToolTip title text to display when the pointer is on the control. If empty, no no title will be shown.</param>
        /// <remarks>Used by the Forms Designer.</remarks>
        public void SetTitle(Control control, string title)
        {
            title = Squeeze(title);
            var tp = GetTipProp(control);
            if (tp == null)
            {
                if (title.Length == 0) return;
                TipProps[control] = new TipProp(this, control, string.Empty, title, ToolTipIcon.None, TipProp.CreationSource.Designer);
            }
            else
            {
                if (title == tp.Title) return;
                if (tp.ToolTipIcon == ToolTipIcon.None && tp.Message.Length == 0 && title.Length == 0)
                {
                    TipProps.Remove(control);
                    return;
                }

                TipProps[control] = new TipProp(this, control, tp.Message, title, tp.ToolTipIcon, TipProp.CreationSource.Designer);
                tp.Dispose();
            }
        }

        /// <summary>Retrieves the ToolTip icon associated with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> for which to retrieve the ToolTip text. </param>
        /// <returns>A <see cref="T:System.String" /> containing the ToolTip text for the specified control.</returns>
        /// <remarks>Used by the Forms Designer.</remarks>
        [Category("ToolTip")]
        [DefaultValue(ToolTipIcon.None), Localizable(true), Description("Add a severity icon to the tooltip.")]
        public ToolTipIcon GetIcon(Control control)
        {
            var tp = GetTipProp(control);
            if (tp == null) return ToolTipIcon.None;
            return tp.ToolTipIcon;
        }

        /// <summary>Associates ToolTip icon with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> to associate the ToolTip text with. </param>
        /// <param name="icon">The severity icon to display when the pointer is on the control. </param>
        /// <remarks>Used by the Forms Designer.</remarks>
        public void SetIcon(Control control, ToolTipIcon icon)
        {
            var tp = GetTipProp(control);
            if (tp == null)
            {
                if (icon == ToolTipIcon.None) return;
                TipProps[control] = new TipProp(this, control, string.Empty, string.Empty, icon, TipProp.CreationSource.Designer);
            }
            else
            {
                if (icon == tp.ToolTipIcon) return;
                if (icon == ToolTipIcon.None && tp.Message.Length == 0 && tp.Title.Length == 0)
                {
                    TipProps.Remove(control);
                    return;
                }

                TipProps[control] = new TipProp(this, control, tp.Message, tp.Title, icon, TipProp.CreationSource.Designer);
                tp.Dispose();
            }
        }

        #region Prototype: Using single Get/SetTooltip with properties struct. Can't get setter to work...
        #if PROTOTYPE
        //https://docs.microsoft.com/en-us/dotnet/api/system.componentmodel?view=net-5.0
        //https://www.codeproject.com/Articles/4683/Getting-to-know-IExtenderProvider
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        [Category("ToolTip"), Description("Set tooltip properties.")]
        public ToolTipProps GetToolTip(Control control)
        {
            var tp = GetTipProp(control);
            if (tp == null) return new ToolTipProps() { Message = "", Title = "", Icon = 0 };
            return new ToolTipProps() { Message = tp.Message, Title = tp.Title, Icon = tp.ToolTipIcon };
        }

        /// <summary>Associates ToolTip icon with the specified control.</summary>
        /// <param name="control">The <see cref="T:System.Windows.Forms.Control" /> to associate the ToolTip text with. </param>
        /// <param name="icon">The ToolTip icon to display when the pointer is on the control. </param>
        /// <remarks>Used by the Forms Designer.</remarks>
        public void SetToolTip(Control control, ToolTipProps p) => RegisterTooltip(control, p.Message, p.Title, p.Icon);
        private bool ShouldSerializeToolTip(Control c) => GetTipProp(c) != null;
        private void ResetToolTip(Control c) => UnregisterTooltip(c);

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public struct ToolTipProps
        {
            [Localizable(true), Description("Set a tooltip message. This text is autowrapped to fit within a box that has a 3x2 ratio.")]
            [Editor("System.ComponentModel.Design.MultilineStringEditor, System.Design, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a", typeof(UITypeEditor))]
            public string Message { get; set; }
            [Localizable(true), Description("Set a title/caption on the tooltip. It is a single line and does not wrap.")]
            public string Title { get; set; }
            [Description("Set a severity icon on the tooltip.")]
            public ToolTipIcon Icon { get; set; }

            public override string ToString() => $"({Truncate(Message)}), ({Truncate(Title)}), {Icon}";
            private static string Truncate(string s)
            {
                const int maxLen = 24;
                if (s == null) return string.Empty;
                if (s.Length < maxLen) return s;
                return s.Substring(0, maxLen - 1) + 0x2026; //ellipsis
            }
        }
        #endif
        #endregion Prototype: Using single Get/SetTooltip with properties struct
        #endregion IExtenderProvider

        // Properties and actions for a single tooltip
        private class TipProp: IDisposable
        {
            public enum CreationSource
            {
                Auto = 0,   //Created by AddAllToolTips()
                Designer,   //Created by SetMessage()/SetTitle()/SetIcon()
                Registered, //Created by RegisterTooltip()
                Custom      //Created by Show()
            }

            public Control Control { get; private set; }
            public string Message { get; private set; } = "";
            public string Title { get; private set; } = "";
            public ToolTipIcon ToolTipIcon { get; private set; }
            public CreationSource CreatedBy { get; private set; } //Determines HOW tooltip was added. Via RegisterToolTip() or AddAllToolTips()

            // Require resources from the ToolTipManager that may not have been initialized at time of this object creation,
            //so we cannot create private copies of them in the constructor.
            private readonly ToolTipManager TTParent;

            Image __toolTipImage = null;
            private Image ToolTipImage
            {
                get
                {
                    if (__toolTipImage == null) __toolTipImage = TTParent.Resources.CreateToolTipImage(Message, Title, ToolTipIcon);
                    return __toolTipImage;
                }
            }

            public TipProp(ToolTipManager ttParent, Control control, string msg, string title = "", ToolTipIcon icon = ToolTipIcon.None, CreationSource createdBy = 0)
            {
                TTParent = ttParent;

                Control = control;
                Message = msg;
                Title = title;
                ToolTipIcon = icon;
                CreatedBy = createdBy;
            }

            public void Dispose()
            {
                if (__toolTipImage != null) { __toolTipImage.Dispose(); __toolTipImage = null; }
            }

            //Compute location (top-left corner) for the tooltip panel in screen coordinates.
            private Point GetLocation(Control c)
            {
                if (CreatedBy == CreationSource.Custom)
                {
                    int y = 0;
                    int charHeight = 0;
                    using (var g = Graphics.FromHwnd(c.Handle))
                        charHeight = Size.Ceiling(g.MeasureString("Wg", c.Font)).Height;

                    if (c.Height < 2 * charHeight)
                    {
                        y = (int)(c.Height * 0.75 + 0.5);
                    }
                    else
                    {
                        y = (int)(charHeight * 1.25 + 0.5);
                    }

                    return c.PointToScreen(new Point(10, y));
                }

                //** Get location as a slight offset from the current mouse position

                var mousePosition = System.Windows.Forms.Control.MousePosition;

                // control.Cursor is an Arrow for these two control types, but internally (somewhere), it's an IBeam!
                Cursor cur;
                if (c is NumericUpDown || c is ComboBox) cur = Cursors.IBeam;
                else cur = c.Cursor;

                var rc = TTResources.GetCursorDimensions(cur);
                mousePosition.Offset(rc.X - cur.HotSpot.X, rc.Bottom - cur.HotSpot.Y + 1);
                return mousePosition;
            }

            public void Show(bool useFading = true)
            {
                //Cursor no longer over this control, so don't show the tooltip.
                if (!this.Control.ClientRectangle.Contains(this.Control.PointToClient(Control.MousePosition))) return;

                TTParent.TTPanel.SuspendLayout();
                TTParent.TTPanel.Image = ToolTipImage;
                TTParent.TTPanel.Location = GetLocation(Control);
                TTParent.TTPanel.Opacity = 0;
                TTParent.TTPanel.ResumeLayout(true);
                TTParent.TTPanel.Show();

                TTParent.ShowTimer.TipProp = this;

                if (!useFading)
                {
                    TTParent.ShowTimer.Interval = 1;
                    TTParent.TTPanel.Opacity = 1;
                }
                TTParent.ShowTimer.Start();
            }

            public void Hide(bool useFading = true)
            {
                if (!TTParent.TTPanel.Visible) return;
                TTParent.AutoPopTimer.Stop();
                TTParent.ShowTimer.Stop();

                if (!useFading)
                {
                    TTParent.HideTimer.Interval = 1;
                    TTParent.TTPanel.Opacity = 0;
                }
                TTParent.HideTimer.Start();
            }
        }

        // Lightweight form that simply displays a floating image.
        private class PopupPanel : Form
        {
            private Image __image;
            public Image Image
            {
                get => __image;
                set
                {
                    __image = value;
                    if (__image != null)
                    {
                        this.Size = __image.Size;
                    }
                }
            }

            /// <summary>
            /// Add a simple drop-shadow to to this panel
            /// </summary>
            public bool UseWindowDropShadow { get; set; }

            public PopupPanel(Form owner, bool useWindowDropShadow = false)
            {
                UseWindowDropShadow = useWindowDropShadow;

                this.SuspendLayout();
                this.Owner = owner;
                this.AutoScaleDimensions = new SizeF(6F, 13F);
                this.AutoScaleMode = AutoScaleMode.Font;
                this.Location = new Point(10, 10);
                this.ClientSize = new Size(10, 10);
                this.FormBorderStyle = FormBorderStyle.None;
                this.MaximizeBox = false;
                this.MinimizeBox = false;
                this.Name = "ToolTipPanel";
                //this.Padding = new Padding(5);
                //this.Margin = new Padding(5);
                this.ShowIcon = false;
                this.ShowInTaskbar = false;
                this.StartPosition = FormStartPosition.Manual;
                this.Text = "ToolTipPanel";
                this.Enabled = false;
                this.Visible = false;
                this.BackColor = Color.FromArgb(1, 254, 2);
                this.TransparencyKey = Color.FromArgb(1, 254, 2);
                this.AllowTransparency = true;
                this.Opacity = 0d;
                this.ResumeLayout(false);
                base.CreateHandle();  //need to do this so set_Image() will also set the Win32 size not just the .net size.
                base.Hide();
            }

            protected override CreateParams CreateParams
            {
                get
                {
                    const int CS_DROPSHADOW = 0x00020000;
                    const int WS_EX_TRANSPARENT = 0x00000020;
                    const int WS_EX_NOPARENTNOTIFY = 0x00000004;
                    const int WS_EX_NOACTIVATE = 0x08000000;
                    const int WS_EX_LAYERED = 0x00080000;

                    CreateParams cp = base.CreateParams;

                    if (UseWindowDropShadow)  cp.ClassStyle |= CS_DROPSHADOW;

                    cp.ExStyle |= WS_EX_TRANSPARENT;
                    cp.ExStyle |= WS_EX_NOPARENTNOTIFY;
                    cp.ExStyle |= WS_EX_NOACTIVATE;
                    cp.ExStyle |= WS_EX_LAYERED;
                    return cp;
                }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (this.Image == null) return;

                if (this.Image.PixelFormat == PixelFormat.Undefined) //Image disposed!
                {
                    var rc = e.ClipRectangle;
                    e.Graphics.DrawRectangle(Pens.Crimson, rc.X, rc.Y, rc.Width - 1, rc.Height - 1);
                    e.Graphics.FillRectangle(Brushes.Pink, rc);
                    var sf = new StringFormat() { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                    e.Graphics.DrawString(this.Image == null ? "(null)" : "(disposed)", SystemFonts.IconTitleFont, Brushes.Black, rc, sf);
                    return;
                }

                e.Graphics.DrawImage(this.Image, 0, 0);
            }

            protected override void OnClosing(CancelEventArgs e)
            {
                //this.Image.Dispose(); //disposed by class TipProp that owns the image.
                this.Image = null;
            }

            protected override bool ShowWithoutActivation => true;

            protected override void WndProc(ref Message m)
            {
                const int WM_NCHITTEST = 0x0084;

                if (m.Msg == WM_NCHITTEST)
                    m.Result = (IntPtr)(-1);  //Ignore all mouse input
                else
                    base.WndProc(ref m);
            }
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
#endregion
        }

        // Handy overload to associate TipProp associated with the timer and also debugging purposes.
        private class MyTimer : System.Windows.Forms.Timer
        {
            public MyTimer() : base() { }
            public MyTimer(IContainer container) : base(container) { }

            public string Name { get; set; } = "UNKNOWN";

            public TipProp TipProp { get; set; }

            public override bool Enabled
            {
                get => base.Enabled;
                set
                {
                    if (base.Enabled == value) return;
                    base.Enabled = value;
                    Debug.WriteLine($"{Name}.Enabled=={value}");
                }
            }
        }

        // Tooltip image resources
        private class TTResources : IDisposable
        {
            //Used to get display properties for new image because Control.Handle may not have been created yet.
            [DllImport("user32.dll")] private static extern IntPtr GetDesktopWindow();

            [DllImport("shlwapi.dll")] private static extern int ColorHLSToRGB(int H, int L, int S);  //All HLS values range from 0-240
            [DllImport("shlwapi.dll")] private static extern void ColorRGBToHLS(int RGB, out int H, out int L, out int S);
            private static void RGBtoHLS(Color color, out int hue, out int luminosity, out int saturation) => ColorRGBToHLS(color.ToArgb(), out hue, out luminosity, out saturation);
            private static Color HLStoRGB(Color color, int hue, int luminosity, int saturation) => Color.FromArgb(color.A, Color.FromArgb(ColorHLSToRGB(hue, luminosity, saturation)));
            private static int NewLuminosity(int l, int s)
            {
                if (s < -240) s = -240; //restrict range to ±240.
                else if (s > 240) s = 240;
                int v = l + s;          //Rollover on 0-240 range.
                if (v < 0) return 240 + s;
                if (v > 240) return s - 240;
                return v;
            }

            private Brush MessageTextBrush;
            private Brush TitleTextBrush;
            private Brush FillBrush;
            private Pen BorderPen;
            private Pen DividerPen;

            public Font MessageFont { get; set; }
            public Font TitleFont { get; set; }

            private Color __messageTextColor;
            public Color MessageTextColor
            {
                get => __messageTextColor;
                set
                {
                    if (__messageTextColor == value) return;
                    __messageTextColor = value;
                    if (MessageTextBrush != null) { MessageTextBrush.Dispose(); MessageTextBrush = null; }
                    if (__messageTextColor == Color.Empty) return;
                    MessageTextBrush = new SolidBrush(__messageTextColor);
                }
            }

            private Color __titleTextColor;
            public Color TitleTextColor
            {
                get => __titleTextColor;
                set
                {
                    if (__titleTextColor == value) return;
                    __titleTextColor = value;
                    if (TitleTextBrush != null) { TitleTextBrush.Dispose(); TitleTextBrush = null; }
                    if (__titleTextColor == Color.Empty) return;
                    TitleTextBrush = new SolidBrush(__titleTextColor);
                }
            }

            private Color __fillColor;
            public Color FillColor
            {
                get => __fillColor;
                set
                {
                    if (__fillColor == value) return;
                    __fillColor = value;
                    if (__fillColor == Color.Empty)
                    {
                        if (FillBrush != null) { FillBrush.Dispose(); FillBrush = null; }
                        if (BorderPen != null) { BorderPen.Dispose(); BorderPen = null; }
                        if (DividerPen != null) { DividerPen.Dispose(); DividerPen = null; }
                        return;
                    }

                    if (FillBrush != null) { FillBrush.Dispose(); FillBrush = null; }
                    FillBrush = new SolidBrush(__fillColor);

                    //border and divider colors are related to the fill color.
                    RGBtoHLS(__fillColor, out int h, out int l, out int s);

                    var borderColor = HLStoRGB(__fillColor, h, NewLuminosity(l, -150), s); //much darker than fill color
                    if (BorderPen != null) { BorderPen.Dispose(); BorderPen = null; }
                    BorderPen = new Pen(borderColor, 1.0f);

                    var dividerColor = HLStoRGB(__fillColor, h, NewLuminosity(l, -40), s); //slightly darker than fill color
                    if (DividerPen != null) { DividerPen.Dispose(); DividerPen = null; }
                    DividerPen = new Pen(dividerColor, 1.0f);
                }
            }

            public TTResources()
            {
            }

#region IDisposable
            public void Dispose()
            {
                if (__errorIcon != null) { __errorIcon.Dispose(); __errorIcon = null; }
                if (__warningIcon != null) { __warningIcon.Dispose(); __warningIcon = null; }
                if (__infoIcon != null) { __infoIcon.Dispose(); __infoIcon = null; }
                MessageTextColor = Color.Empty;
                TitleTextColor = Color.Empty;
                FillColor = Color.Empty;
            }
#endregion

            public Bitmap CreateToolTipImage(string message, string title, ToolTipIcon toolTipIcon)
            {
                //To add simple rectanglular dropshadow, use CreateParams:CS_DROPSHADOW.
                //For other shapes we will have to simulate our own dropshadow.

                Graphics g = Graphics.FromHwnd(GetDesktopWindow()); //Cannot use Owner or Control as Win32 window handle may not be instaniated yet.

                Rectangle iconBounds = new Rectangle(3, 3, 0, 0);
                Rectangle titleBounds = iconBounds;
                Rectangle msgBounds = iconBounds;

                if (toolTipIcon != ToolTipIcon.None)
                {
                    iconBounds.Width = 16;
                    iconBounds.Height = 16;
                }

                if (title.Length > 0)
                {
                    titleBounds = new Rectangle(new Point(iconBounds.Right, iconBounds.Top), ComputeTextDimensions(g, title, TitleFont, int.MaxValue));
                    if (message.Length > 0)
                    {
                        var msgSize = ComputeTextDimensions(g, message, MessageFont);
                        if (iconBounds.Width + titleBounds.Width >= msgSize.Width && !message.Any(c => c == '\n')) msgSize = ComputeTextDimensions(g, message, MessageFont, iconBounds.Width + titleBounds.Width);
                        msgBounds = new Rectangle(new Point(iconBounds.Left, titleBounds.Bottom), msgSize); //below icon
                    }
                }
                else if (message.Length > 0)
                {
                    msgBounds = new Rectangle(new Point(iconBounds.Right, iconBounds.Top), ComputeTextDimensions(g, message, MessageFont)); //right of icon
                }

                var bounds = Rectangle.Union(Rectangle.Union(iconBounds, titleBounds), msgBounds); //finds the largest box that fits all 3 objects.
                bounds.Width += iconBounds.Left * 2;  //put the border areas back
                bounds.Height += iconBounds.Top * 2;

                var bmp = new Bitmap(bounds.Width, bounds.Height, g);   //Use Desktop properties for new image
                g.Dispose();
                g = Graphics.FromImage(bmp);

                var rc = new Rectangle(0, 0, bmp.Width, bmp.Height);
                g.FillRectangle(FillBrush, rc);
                g.DrawRectangle(BorderPen, rc.X, rc.Y, rc.Width - 1, rc.Height - 1); //draw on inside edge of rectangle

                Image ico = null;
                switch (toolTipIcon)
                {
                    case ToolTipIcon.Error: ico = ErrorIcon; break;
                    case ToolTipIcon.Warning: ico = WarningIcon; break;
                    case ToolTipIcon.Info: ico = InfoIcon; break;
                }

                if (title != null && title.Length > 0 && message != null && message.Length > 0)
                {
                    g.DrawLine(DividerPen, titleBounds.Left + 1, titleBounds.Bottom - 1, Math.Max(titleBounds.Right, msgBounds.Right) - 2, titleBounds.Bottom - 1);
                }

                if (ico != null)
                {
                    g.DrawImageUnscaled(ico, iconBounds);
                    //g.DrawRectangle(Pens.Red, iconBounds.X, iconBounds.Y, iconBounds.Width - 1, iconBounds.Height - 1);
                }

                if (title != null && title.Length > 0)
                {
                    g.DrawString(title, TitleFont, TitleTextBrush, titleBounds);
                    //g.DrawRectangle(Pens.Green, titleBounds.X, titleBounds.Y, titleBounds.Width - 1, titleBounds.Height - 1);
                }

                if (message != null && message.Length > 0)
                {
                    g.DrawString(message, MessageFont, MessageTextBrush, msgBounds);
                    //g.DrawRectangle(Pens.Blue, msgBounds.X, msgBounds.Y, msgBounds.Width - 1, msgBounds.Height - 1);
                }

                g.Dispose();

                return bmp;
            }

            /// <summary>
            /// Compute the dimensions of 3x2 rectangle box that will fit the supplied text.
            /// </summary>
            /// <param name="graphics">Graphics object used to measure string. Or NULL to use legacy TextRenderer (necessary to mirror Forms.Label control wrapping).</param>
            /// <param name="s">Supplied text. If string contains newlines, autowrap is disabled.</param>
            /// <param name="font">Font to use for measuring.</param>
            /// <param name="maxWidth"> If 0, then autowrap and fit text into a 3x2 rectangle. Should not contain newlines. Else autowrap at this width.</param>
            /// <returns>size of fitted box.</returns>
            private static Size ComputeTextDimensions(Graphics graphics, string s, Font font, int maxWidth = 0)
            {
                const TextFormatFlags flags = TextFormatFlags.HidePrefix | TextFormatFlags.TextBoxControl | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
                Func<float, int> ceil = (f) => (int)(f > (int)f ? f + 1 : f);

                if (s == null && s.Length == 0) return Size.Empty;

                if (s.Any(c => c == '\n')) maxWidth = int.MaxValue; //We don't autowrap if the string contains newlines.

                if (maxWidth > 0)
                {
                    if (graphics != null) return Size.Ceiling(graphics.MeasureString(s, font, new SizeF(maxWidth, int.MaxValue), (StringFormat)null));
                    else return TextRenderer.MeasureText(s, font, new Size(maxWidth, int.MaxValue), flags);
                }

                int width = 50;
                SizeF sizef = SizeF.Empty;
                SizeF prevsizef = SizeF.Empty;
                double ratio = 0;

                while (ratio < 3)
                {
                    if (graphics != null) sizef = graphics.MeasureString(s, font, width);
                    else sizef = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                    if (sizef == prevsizef) break;
                    ratio = sizef.Width / (double)sizef.Height;
                    width += 25;
                    prevsizef = sizef;
                }

                width = ceil(sizef.Width);
                for (int i = 0; i < 50; i++)
                {
                    width--;
                    SizeF size2;
                    if (graphics != null) size2 = graphics.MeasureString(s, font, width);
                    else size2 = TextRenderer.MeasureText(s, font, new Size(width, 99999), flags);
                    if (size2.Height > sizef.Height) break;
                }

                return new Size(width + 1, ceil(sizef.Height));
            }

            //Cache dimensions forever since cursors are system-wide. No need to recompute every time.
            private static readonly Dictionary<Cursor, Rectangle> CursorDimensions = new Dictionary<Cursor, Rectangle>();
            /// <summary>
            /// A cursor is 32x32 pixels consisting of a lot of transparent area. This returns the containing box of the visible cursor area.
            /// This is used to assist in getting the location for the popup relative to the current cursor position.
            /// </summary>
            /// <param name="cur">Cursor object to get dimensions from</param>
            /// <returns>Visible dimensions within Cursor.</returns>
            public static Rectangle GetCursorDimensions(Cursor cur)
            {
                if (!CursorDimensions.TryGetValue(cur,out var rc))
                {
                    var source = new Bitmap(cur.Size.Width, cur.Size.Height, PixelFormat.Format32bppArgb);
                    using (var g = Graphics.FromImage(source))
                        cur.Draw(g, new Rectangle(0, 0, cur.Size.Width, cur.Size.Height));
                    rc = GetVisibleRectangle(source);
                    source.Dispose();
                    CursorDimensions[cur] = rc;
                }
                return rc;
            }
            private static Rectangle GetVisibleRectangle(Bitmap source)
            {
                //https://stackoverflow.com/questions/4820212/automatically-trim-a-bitmap-to-minimum-size
                BitmapData data = null;
                try
                {
                    data = source.LockBits(new Rectangle(0, 0, source.Width, source.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
                    byte[] buffer = new byte[data.Height * data.Stride];
                    Marshal.Copy(data.Scan0, buffer, 0, buffer.Length);

                    int xMin = int.MaxValue,
                        xMax = int.MinValue,
                        yMin = int.MaxValue,
                        yMax = int.MinValue;

                    bool foundPixel = false;

                    // Find xMin
                    for (int x = 0; x < data.Width; x++)
                    {
                        bool stop = false;
                        for (int y = 0; y < data.Height; y++)
                        {
                            byte alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha != 0)
                            {
                                xMin = x;
                                stop = true;
                                foundPixel = true;
                                break;
                            }
                        }
                        if (stop)
                            break;
                    }

                    // Image is empty...
                    if (!foundPixel)
                        return new Rectangle(0, 0, source.Width, source.Height);

                    // Find yMin
                    for (int y = 0; y < data.Height; y++)
                    {
                        bool stop = false;
                        for (int x = xMin; x < data.Width; x++)
                        {
                            byte alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha != 0)
                            {
                                yMin = y;
                                stop = true;
                                break;
                            }
                        }
                        if (stop)
                            break;
                    }

                    // Find xMax
                    for (int x = data.Width - 1; x >= xMin; x--)
                    {
                        bool stop = false;
                        for (int y = yMin; y < data.Height; y++)
                        {
                            byte alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha != 0)
                            {
                                xMax = x;
                                stop = true;
                                break;
                            }
                        }
                        if (stop)
                            break;
                    }

                    // Find yMax
                    for (int y = data.Height - 1; y >= yMin; y--)
                    {
                        bool stop = false;
                        for (int x = xMin; x <= xMax; x++)
                        {
                            byte alpha = buffer[y * data.Stride + 4 * x + 3];
                            if (alpha != 0)
                            {
                                yMax = y;
                                stop = true;
                                break;
                            }
                        }
                        if (stop)
                            break;
                    }

                    return Rectangle.FromLTRB(xMin, yMin, xMax, yMax);
                }
                finally
                {
                    if (data != null)
                        source.UnlockBits(data);
                }
            }

#region Icon Image Resources
            private Image __errorIcon = null;
            private Image __warningIcon = null;
            private Image __infoIcon = null;

            public  Image ErrorIcon
            {
                get
                {
                    if (__errorIcon == null) __errorIcon = Base64StringToBitmap(TT_ErrorBase64);
                    return __errorIcon;
                }
            }

            public Image WarningIcon
            {
                get
                {
                    if (__warningIcon == null) __warningIcon = Base64StringToBitmap(TT_WarningBase64);
                    return __warningIcon;
                }
            }

            public Image InfoIcon
            {
                get
                {
                    if (__infoIcon == null) __infoIcon = Base64StringToBitmap(TT_InfoBase64);
                    return __infoIcon;
                }
            }

            private static Bitmap Base64StringToBitmap(string base64String)
            {
                byte[] byteBuffer = Convert.FromBase64String(base64String);
                MemoryStream memoryStream = new MemoryStream(byteBuffer);
                memoryStream.Position = 0;
                Bitmap bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream, false, true);
                memoryStream.Close();
                memoryStream = null;
                byteBuffer = null;
                return bmpReturn;
            }

            // Compressed 16x16 png's with Photoshop 'Save for Web'
            const string TT_ErrorBase64 = @"
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8
        YQUAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADZ0lEQVQ4T0WSfUxTZxTGz73c
        23vbUr7BxsxgKhgYOj+ixoU/DAsE/ChuMVEYseJQKSqiJjK2qVGcggglAhqM4MYWNTMbaeKm28xmpqjr
        BhUUTEwF0lqVobNgBZwxeTy3YnaT5/045/ec+977HqojXUj1JFGrqKd2NYYuz1qY5s5fV+qy5ja4llkb
        XJlLt/08LWn2t8w1Uxg5mK2d9IUGB+sgEbWpkZb+1QXtE43HgSsdgPM88N0PwFffYOxgNW5bVzmbBTn5
        S2brSPm/wD4OtETHLgrYtwTAIEp3Alu3AyWlgJ21dQdQ/hlQVYN/1q4fPWo0pVeyp56LUA0fqVYxmgPW
        VePIW4eRGWnoTk7Dy5mzgIwcYEk2Xs1djN5FS/A8mWN5Nng/yHlRKemmVbGX9vNwIzHFiXnvw6NGwPVx
        IXqu/4nOjGwEiTDG6kzPQO81F/62FcGrmoA5C/GjOfFiOQl8AlGfPBg7FbcYvJRowX948ww/+RcdCVNw
        NSYWQ4+GQrEXrA5LEjzM9kTE4QtRTaUmyVjymxyOeg62GPS4W304BGvPkP8hHnn9kzug/0gtzhoNOM7s
        edKhRlTLqFo21B2V9SgjCYc50cTqLC7Gs6cBTLBJ0+jIKNwlm9HCuWOsSpLRLOhQLSoNtE9ndOzWqcjk
        xDYKw+c8n4yKxuDAIAYfP8bA8DAGfD60xU9BVcgsoJBEVIgS9ohKI+2SDJtPKAbYOLmedShpJrp7bqPn
        vg999/244/ej2+dFd18fGt+dgx3M5HGRA1ygXFTKqECQk84aTKHvaphuQdddDy55POjwetG1qRjuDRtx
        zevDT3d6MfA0COeCdNQweypMgU2QU6mA23K3YnT+JSs4FxUJ17EmXA0G0VVRgV8Z/IXl3lWOK8Hn6Gpt
        RXt8PP7gt++V1Av5WiN9wkMuyea28MjgA0HCZTbcXL4Cbp7vsfpZ2hXfWmHF75P7r3WGiY9IN1Xzhgqs
        5XbOFeR5p03RT4J8tGcMgf804sxAwjuApMc4x0b4ps6opsBKQV5sY9+GtwW0RT7JlElhiXv04ec6o+Iw
        lvIesPxDIMeK8dnz4Y5OwH7F8H0WSZY8Zjeyp+htAW1RyKfQElncntkkpWxSTfaKGLPj08h4h12JsC8j
        OTWLRFrDjMZqniJS6DW2y9hcWEfK0wAAAABJRU5ErkJggg==";

            const string TT_WarningBase64 = @"
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8
        YQUAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAACTElEQVQ4T6WRW0iUURSF14w2
        3q3AchyLIixEK9IIH0xLuugMmZIUoS9pwhgZmZIlFIEilRVRhhGZET1EEulbEUQIYd4mU8tSAotGlCK6
        YE4XarVsfshpopcWfPxs9l77rHN+dG2CP9nAYA6CXRnYEm1BGP6lHhn+ZGqJy46ST058qYxHhTH6dz3S
        aT5sBjpXI3x4Jzx8bOL7UnyPBmKNcX9167TpdGRpwUo0sttB8is55OD5BbhhjPtrenSXEnSkYOnYQRNd
        fafY0HiP3X0nyFoTN1iQZlh81a/Iv8jRgvXA0yw84KsddFZVMzDIxvzdleRoPrvWYcCw+Gr6gq4k5L27
        EKno/aw6fITz58VzT/kB1Z3ktQgWhsFp2H5r6t49it6eCvOTPLymp0KGNzxXv59WawLrjpepdpPfSvhy
        Gz5GAuGG1atemR869HDJqPlw26bhW+IO29uPMTExhW1t1apbRBN5P4q1VjQYVq8GcpViFWKe78UPsliD
        V8VJUcfBwVJ9a8QhUSscnKgCFwEJhl2nbxRpaPk8HKuBfWKXcHJ8vIjNzQUcGdmueqvI9uKezdYVuGvY
        9duSkeY+a1ZzucgQqSKdp88kEbCxrHyJ0Vss4kQMecXEXAv07NKLQuiFgtWYShDtHaCVYxNzebR+Dp+5
        Z6mOEKHCIoKEmX0FeBsAzMRQMUZ5PZCey6H0NIUI75fNolPctHCyMZCTFwOEmZ5LUA/sLcKkBViIuBBE
        2QOQmQ7Y15iQ6Qd8WQtkZZpht83AMl0gSOv4HxA/Ae3CsBWmeVz5AAAAAElFTkSuQmCC";

            const string TT_InfoBase64 = @"
        iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAYAAAAf8/9hAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8
        YQUAAAAZdEVYdFNvZnR3YXJlAEFkb2JlIEltYWdlUmVhZHlxyWU8AAADuklEQVQ4T0WTf1ATZBjHnzwt
        uewuLk6xOiy4rkKFMxsjJCnFbAPvQM/JQY6fc/04BGQwk7kBDYhtYIzBiJ3IHBDgdHDEr0GEXehpJnim
        OyQFiUEHA8YYWMn17Q2v6/nnee75vu/n+37/eGn0tykaHZ+kBksfZSurKUmiTXzjPcnZ5JPVrqPJX7ri
        kopdwqyvXa+FpJmyCgxHRJ+XkUhaTstuJ9mGfyV6wC673W4yt/Vt9N6R0p2hbEbPwD1MTLvxX03OuHGh
        fRBiWT08NkcZFCW1xNZkt9uJpqZnafC2bV3wodzbcp0V8+7HsE8vYOThDOYWlnF/3IEhmx1zriXMOP9C
        sqQeO/knr1g6esnScZloadFJwVGnO2OYcGtkGudbb0DfdBWuxUdP7FkZW67DYL6Gps4h3Howhw8TquAX
        JGpTlhmJcorO7Q8WlKCi+WfkVlohr+xBhroD5t47cC4uo+vHOzihaYOi8jsoDX3QNV6BpvEGXo8ownGZ
        dgsdEGsbAmN0SC3+Fh/JzBAqLiJeYcGBzG+QomzFYWkTEvJaIGK64FQzuLF6+IQq8YxPOt7mSXKIE6dZ
        3BpnQGjKWewS1eDNmCpklPdi+KEDtjEH7o3PYvcnRmx6XwXyzwF5p4G8PgN5HAM/SfsLPRWY+uiFSB28
        +F/BK6IMnrwybNithu7SzdX8KyuP8TQ3D7Q5C7T1NChAAdrGuo8UgdGqJfIIOA4KVIKCCkDBRaBdzMk7
        Cx+kNa8CZhfcWBvCtG1fgLhM57B5Rz7IT4adAg1ob7J2hHxlDMDEEDUotATkK8cRRfsqYN7lxvpwtuMw
        7d1SZsA6txj0Yib4x7TjlK+35JOPBBSmBYVrsT6iCvSWCrGF1icApwvPR2qxZp8e63iVoD3sXFgFe0E2
        VFWWcupq79+4KZK579HDM8qALbFGNuuQU3NtFQD8jSBxHTwP1eLlGCOe5TMIi8kR6zE7NfkSzTkcZGi0
        Sjfs18A30YxXYk2Ilnfhp7sTGB77HaMTM6jruIntiSb4CRvhn2CEV5gUH0tK9v257P73L0yR7e4IpebW
        GoPFJoRLrUhUDyCeReBLLiEyuwXxBZ04qLBir7Qb3JRzeC4oiydMVZFzfp7o/pidun8Yogst35NcZRJE
        p9esxOd1IULSCZHqKhILB3DwlBWfqvshltf98Y6gkLcm4AQJ00v/B3T1D9J5cx8pShtIXVHv39rRnycv
        vXg9R9Pqkp9pW8hQNl6uNrXl9lp7XuUcLiLanknCtBIGmKd/AADlYlgrmuE4AAAAAElFTkSuQmCC";
#endregion // Icon Image Resources
        }
    }
}

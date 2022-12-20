//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Threading.cs" company="Chuck Hill">
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
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ChuckHill2.Forms
{
    /// <summary>
    /// Tools to perform on forms in a multithreaded environment.
    /// </summary>
    public static class Threading
    {
        /// <summary>
        /// Used to pause current action but still allow Windows message pump to continue.
        /// </summary>
        /// <param name="ms">Time in milliseconds to wait.</param>
        /// <param name="cancel">Optional cancellation to return immediately.</param>
        public static void ControlSleep(int ms, CancellationToken cancel = default(CancellationToken))
        {
            SpinWait.SpinUntil(() =>
            {
                Application.DoEvents();
                return !cancel.IsCancellationRequested;
            }, ms);
        }

        /// <summary>
        /// Invoke an action in the context of specified form's message pump thread.
        /// Action does not get executed if the the window is minimized unless it is forced.
        /// </summary>
        /// <param name="owner">Form the action is to be performed upon.</param>
        /// <param name="command">Action to perform, usually to update form.</param>
        /// <param name="force">False to ignore action if Form window is minimized. Default is True to perform the action even though this may be a minimized window.</param>
        private static void CallForm(Form owner, Action command, bool force = true)
        {
            if (owner == null || command == null || owner.Disposing) return;
            if (force || owner.WindowState != FormWindowState.Minimized)
            {
                if (owner.InvokeRequired)
                {
                    //Hide ThreadAbortException and ObjectDisposedException. Occurs when application exiting.
                    try { owner.Invoke(command); } catch { }
                }
                else
                {
                    command();
                }
            }
        }

        /// <summary>
        /// Invoke an action in the context of current form's message pump thread.
        /// Action does not get executed if the the window is minimized unless it is forced.
        /// </summary>
        /// <remarks>
        /// Assumes action is to be performed on the the active or current form.
        /// </remarks>
        /// <param name="command">Action to perform, usually to update form.</param>
        /// <param name="force">False to ignore action if Form window is minimized. Default is True to perform the action even though this may be a minimized window.</param>
        private static void CallForm(Action command, bool force = true) //Invoke takes a delegate. Pre-cast to Action type
        {
            Form owner = Form.ActiveForm != null ? Form.ActiveForm : (Application.OpenForms.Count > 0 ? Application.OpenForms[Application.OpenForms.Count - 1] : null);
            if (owner == null || command==null || owner.Disposing) return;
            if (force || owner.WindowState != FormWindowState.Minimized)
            {
                if (owner.InvokeRequired)
                {
                    //Hide ThreadAbortException and ObjectDisposedException. Occurs when application exiting.
                    try { owner.Invoke(command); } catch { }
                }
                else
                {
                    command();
                }
            }
        }
    }
}

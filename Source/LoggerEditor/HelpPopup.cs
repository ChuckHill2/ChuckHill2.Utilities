//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="HelpPopup.cs" company="Chuck Hill">
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
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ChuckHill2.Extensions;

namespace ChuckHill2.LoggerEditor
{
    public partial class HelpPopup : Form
    {
        public enum HelpItem
        {
            Main,
            Trace,
            Switches,
            Sources,
            SharedListeners
        }

        public static void Show(IWin32Window owner, HelpItem helpItem)
        {
            var stream = typeof(HelpPopup).GetManifestResourceStream(helpItem.ToString() + ".rtf");
            if (stream == null) return; //should never happen
            using(var dlg = new HelpPopup())
            {
                using (var sr = new StreamReader(stream))
                    dlg.m_rtfHelp.Rtf = sr.ReadToEnd();

                dlg.ShowDialog(owner);
            }
        }

        private HelpPopup()
        {
            InitializeComponent();
        }
    }
}

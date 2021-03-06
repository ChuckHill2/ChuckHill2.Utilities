//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ConnectionStringEditor.cs" company="Chuck Hill">
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
using System.ComponentModel;
using System.Drawing.Design;
using ChuckHill2.Extensions.Reflection;
using System.Windows.Forms;

namespace ChuckHill2.LoggerEditor
{
    public class ConnectionStringEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            //Control owner = ((System.Windows.Forms.PropertyGridInternal.GridEntry)context).OwnerGrid.Parent;
            var owner = (IWin32Window)context.GetReflectedValue("OwnerGrid").GetReflectedValue("Parent");
            string cs = ChuckHill2.Forms.ConnectionStringDlg.Show(owner, value.ToString());
            return string.IsNullOrWhiteSpace(cs) ? value : cs;
        }
    }
}

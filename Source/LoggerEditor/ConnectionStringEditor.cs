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

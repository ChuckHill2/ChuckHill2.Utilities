using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.Windows.Forms;
using ChuckHill2.Extensions.Reflection;

namespace ChuckHill2.LoggerEditor
{
    public class FormatEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            //Control owner = ((System.Windows.Forms.PropertyGridInternal.GridEntry)context).OwnerGrid.Parent;
            var owner = (IWin32Window)context.GetReflectedValue("OwnerGrid").GetReflectedValue("Parent");
            string cs = FormatEditorForm.Show(owner, value.ToString());
            return string.IsNullOrWhiteSpace(cs) ? value : cs;
        }
    }
}

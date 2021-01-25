using System;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace ChuckHill2.LoggerEditor
{
    /// <summary>Provides a user interface for selecting a file name.</summary>
    /// <remarks>Based upon decompiled System.Windows.Forms.Design.FileNameEditor</remarks>
    public class SaveLogNameEditor : UITypeEditor
    {
        private SaveFileDialog dlg;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context) => UITypeEditorEditStyle.Modal;
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (provider != null && (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService)) != null)
            {
                var initialDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                if (this.dlg == null)
                {
                    this.dlg = new SaveFileDialog();
                    dlg.InitialDirectory = initialDirectory;
                    dlg.ValidateNames = true;
                    dlg.Title = "Select a Log filename";
                    dlg.Filter = "Text Log (*.log)|*.log|CSV Log (*.csv)|*.csv|XML Log (*.xml)|*.xml|Plain Text (*.txt)|*.txt|All Files (*.*)|*.*";
                    dlg.FilterIndex = 1;
                    dlg.DefaultExt = "*.log";
                    dlg.Title = "Select a log file to Save into";
                }

                string fn = value as string ?? "";

                if (string.IsNullOrWhiteSpace(fn)) dlg.FilterIndex = 1;
                else
                {
                    switch (Path.GetExtension(fn).ToLower())
                    {
                        case ".log": dlg.FilterIndex = 1; break;
                        case ".csv": dlg.FilterIndex = 2; break;
                        case ".xml": dlg.FilterIndex = 3; break;
                        case ".txt": dlg.FilterIndex = 4; break;
                        default: dlg.FilterIndex = 5; break;
                    }
                }

                this.dlg.FileName = fn;

                if (this.dlg.ShowDialog() == DialogResult.OK)
                {
                    value = this.dlg.FileName.Replace(initialDirectory+"\\","");
                }
            }

            return value;
        }
    }
}

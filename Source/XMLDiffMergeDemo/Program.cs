using System;
using System.IO;
using System.Reflection;
using System.Xml;
using ChuckHill2;
using ChuckHill2.Extensions;

namespace XmlDiffMergeDemo
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            if (args.Length != 3 || !File.Exists(args[0]) || !File.Exists(args[1]) || !File.Exists(args[2]))
            {
                Console.WriteLine(@"
The commandline must contain 3 files: (1) the original xml, (2) the modified original,
and (3) the new XML to merge the differences into.
Defaulting to built-in example.\r\n");

                if (!Directory.Exists("Resources")) Directory.CreateDirectory("Resources");

                using (var sw = new FileStream(@"Resources\xoriginal.config", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    typeof(Program).GetManifestResourceStream("xoriginal.config").CopyTo(sw);

                using (var sw = new FileStream(@"Resources\xmodified.config", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    typeof(Program).GetManifestResourceStream("xmodified.config").CopyTo(sw);

                using (var sw = new FileStream(@"Resources\xnew.config", FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
                    typeof(Program).GetManifestResourceStream("xnew.config").CopyTo(sw);

                args = new string[]  //Example Test
                {
                    @"Resources\xoriginal.config",
                    @"Resources\xmodified.config",
                    @"Resources\xnew.config"
                };
            }

            //Assign variables
            var originalFile = args[0];
            var modifiedFile = args[1];
            var targetFile = args[2];
            var dir = Path.GetDirectoryName(targetFile);
            var name = Path.GetFileNameWithoutExtension(targetFile);
            var ext = Path.GetExtension(targetFile);
            var diffFile = string.Concat(dir, "\\", name, ".diff.xml"); //this is the file containing the differences for debugging.
            var targetMerged = string.Concat(dir, "\\", name, ".merged", ext); //this is the file to contain the merged result

            var xdiffmerge = new XmlDiffMerge(originalFile, modifiedFile, PreprocessSourceFiles); //by key
            //var xdiffmerge = new XmlDiffMerge(originalFile, modifiedFile); //by index
            //var xdiffmerge = XmlDiffMerge.Deserialize(File.ReadAllText(diffFile));

            var xd = xdiffmerge.Serialize(true); //save differences for debugging.
            File.WriteAllText(diffFile, xd);

            File.Copy(targetFile, targetMerged, true); //apply merge to a copy

            bool success = xdiffmerge.ApplyTo(targetMerged, InsertNameAttributes, RemoveNameAttributes); //by key
            //bool success = xdiffmerge.ApplyTo(result); //by index

            Console.WriteLine("[Done]");
        }

        /// In the example config files, there are 2 sibling elements ("system.webServer") with no uniquely identifying key.
        /// In this example it is not really necessary to assign a unique key because index is sufficient to identify
        /// the elements. That is because the elements are neither added or removed or order changed. However, there are many
        /// cases where it is necessary to identify an element by unique key irrespective of order. The following is example
        /// code that shows how to implement this.

        private static void PreprocessSourceFiles(XmlDocument original, XmlDocument modified)
        {
            InsertNameAttributes(original);
            InsertNameAttributes(modified);
            //Plus do some other stuff like removing potentially changed elements or
            //attributes from both DOM's so they won't be added to the merged result.
        }

        private static void InsertNameAttributes(XmlDocument xdoc)
        {
            foreach (XmlElement dt in xdoc.GetElementsByTagName("system.webServer"))
            {
                var value = dt.FirstChild.FirstChild.Name;  //use child info as unique identifier

                // pick a temp key name to use: 'name', 'id', or 'key'

                var a = dt.Attributes["name"]; //in case it already exists.
                if (a == null) a = dt.Attributes.Prepend(dt.OwnerDocument.CreateAttribute("name"));
                a.Value = value;
            }
        }

        private static void RemoveNameAttributes(XmlDocument xdoc)
        {
            //Remove the temporary keys from the elements before saving the results.
            foreach (XmlElement n in xdoc.GetElementsByTagName("system.webServer"))
            {
                n.RemoveAttribute("name");
            }
        }
    }
}

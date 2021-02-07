// --------------------------------------------------------------------------
// <copyright file="XmlDiffMerge.cs" company="Chuck Hill">
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
// <author>Chuck Hill</author>
// --------------------------------------------------------------------------
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace ChuckHill2
{
    /// <summary>
    /// Creates and contains the differences between 2 similar XML files. The results may
    /// be applied to a third similar XML file. Each element MUST be unique at its current
    /// depth/level. Repeating array elements at a given depth should have a unique identifier
    /// attribute. Known identifier attribute names are: 'name', 'id', or 'key'
    /// </summary>
    public class XmlDiffMerge
    {
        private XmlDocument xOriRoot; //root node for original unmodified xml file
        private readonly List<XmlNode> MatchedNodes = new List<XmlNode>(); //list of nodes found in original xml file.
        private static readonly string[] Identifiers = new string[]  // known unique node identifier-names. add more if you want.
        {
            "name",   //default unique identifier
            "id",     //unique identifier
            "key"     //unique identifier used by app.config <appSettings>
        };

        public string OriginalFile { get; set; }  //save for serialization
        public string ModifiedFile { get; set; }
        public List<Diff> Adds { get; } = new List<Diff>();
        public List<Diff> Removes { get; } = new List<Diff>();
        public List<Diff> Changes { get; } = new List<Diff>();
        public bool IsDifferent => ((Adds.Count + Removes.Count + Changes.Count) > 0);

        private XmlDiffMerge() { } //need parameterless constructor for XmlSerialize deserialization

        /// <summary>
        /// Find the differences between 2 similar XML files.
        /// </summary>
        /// <param name="originalFile">Original unmodified XML file</param>
        /// <param name="modifiedFile">Modified XML file</param>
        /// <param name="preProcess">
        /// Change the original and modified open XmlDocuments to hide any known potential changes by removing the elements.
        /// Or assign temporary attribute names (based upon unique content) to array elements that have no name id's for diff comparison.
        /// These temporary attribute names are then removed within ApplyTo() pre and post merge steps.
        /// Note: temporary attributes are unecessary if the element path is unique.
        /// </param>
        public XmlDiffMerge(string originalFile, string modifiedFile, Action<XmlDocument, XmlDocument> preProcess = null)
        {
            this.OriginalFile = originalFile;
            this.ModifiedFile = modifiedFile;

            XmlReader reader;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.CloseInput = true;
            //settings.IgnoreComments = true;
            settings.IgnoreWhitespace = true;
            settings.IgnoreProcessingInstructions = true;

            XmlDocument xOri = new XmlDocument();
            reader = XmlReader.Create(originalFile, settings);
            xOri.Load(reader);
            reader.Dispose();

            XmlDocument xMod = new XmlDocument();
            reader = XmlReader.Create(modifiedFile, settings);
            xMod.Load(reader);
            reader.Dispose();
            xOriRoot = xOri;

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xMod.NameTable);

            preProcess?.Invoke(xOri, xMod); //fixup values that we don't want to notice that were changed.
            MatchedNodes.Clear();
            CompareNodes(xMod, nsmgr); //When there is a match, the node(or attribute) is deleted from xOri
            DeletedNodes(xOri, nsmgr); //This is now used to populate the' Removes' list
        }

        private void CompareNodes(XmlNode node, XmlNamespaceManager nsmgr)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType != XmlNodeType.Element) continue;
                if (n.Attributes.Count > 0)
                    foreach (var a in n.Attributes.Cast<XmlAttribute>().Where(a => IsNamespace(a)))
                        nsmgr.AddNamespace(a.LocalName, a.Value);  //We need to declare the namespaces BEFORE we unwind the call stack.
                CompareNodes(n, nsmgr);
            }
            if (node.NodeType != XmlNodeType.Element) return; //the very first node is not type Element

            string xp;
            List<XmlAttribute> matchedAttr = new List<XmlAttribute>();
            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in node.Attributes)
                {
                    XmlAttribute xa;
                    if (IsNamespace(a))//SelectSingleNode does not work on namespace attributes!
                    {
                        xp = GetXPath(a.OwnerElement);
                        XmlNode n = xOriRoot.SelectSingleNode(xp, nsmgr);
                        if (n == null) continue;
                        xa = n.Attributes.Cast<XmlAttribute>().FirstOrDefault(m => m.Name == a.Name);
                        xp = GetXPath(a);
                    }
                    else
                    {
                        xp = GetXPath(a);
                        xa = xOriRoot.SelectSingleNode(xp, nsmgr) as XmlAttribute;
                    }
                    if (xa == null)
                    {
                        if (string.IsNullOrWhiteSpace(a.Value)) continue;
                        if (!Identifiers.Contains(a.Name)) Adds.Add(new Diff(xp, a.Value));
                        //else Adds.Add(new Diff(GetXPath(a.OwnerElement), null)); //don't need to create an empty element.
                        continue;
                    }
                    if (!xa.Value.EqualsI(a.Value) && !Identifiers.Contains(a.Name)) Changes.Add(new Diff(xp, a.Value, xa.Value));
                    MatchedNodes.Add(xa);
                }
            }

            xp = GetXPath(node);
            XmlElement xn = (XmlElement)xOriRoot.SelectSingleNode(xp, nsmgr);
            string nodeValue = node.GetValue();

            if (xn == null) { if (!string.IsNullOrWhiteSpace(nodeValue)) Adds.Add(new Diff(xp, nodeValue)); return; }
            string xnValue = xn.GetValue();
            if (!xnValue.EqualsI(nodeValue)) Changes.Add(new Diff(xp, nodeValue, xnValue));
            MatchedNodes.Add(xn);
        }

        private void DeletedNodes(XmlNode node, XmlNamespaceManager nsmgr)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType != XmlNodeType.Element) continue;
                DeletedNodes(n, nsmgr);
            }
            if (node.NodeType != XmlNodeType.Element) return;

            int matchIndex = -1;
            int leftoverAttr = 0;
            var parentXp = new List<string>();
            if (node.Attributes != null && node.Attributes.Count > 0)
            {
                foreach (XmlAttribute a in node.Attributes)
                {
                    matchIndex = MatchedNodes.IndexOf(a);
                    if (matchIndex != -1) { MatchedNodes.RemoveAt(matchIndex); continue; }
                    leftoverAttr++;
                    if (Identifiers.Contains(a.Name))
                    {
                        string xp = GetXPath(a.OwnerElement);
                        parentXp.Add(xp);
                        for (int i = Removes.Count - 1; i >= 0; i--)
                        {
                            var d = Removes[i];
                            if (d.XPath.StartsWith(xp)) Removes.RemoveAt(i);
                        }
                        Removes.Add(new Diff(GetXPath(a.OwnerElement), null, null));
                    }
                    else
                    {
                        string xp = GetXPath(a);
                        if (parentXp.FirstOrDefault(p => xp.StartsWith(p)) != null) continue;
                        Removes.Add(new Diff(xp, null, a.Value));
                    }
                }
            }
            matchIndex = MatchedNodes.IndexOf(node);
            if (matchIndex != -1) MatchedNodes.RemoveAt(matchIndex);
            if (leftoverAttr > 0 || node.HasChildNodes || node.ParentNode.HasChildNodes) return;
            string xpn = GetXPath(node);
            if (parentXp.FirstOrDefault(p => xpn.StartsWith(p)) != null) return;
            Removes.Add(new Diff(xpn, null, node.GetValue()));
        }

        private string GetXPath(XmlNode xmlNode)
        {
            //http://msdn.microsoft.com/en-us/library/ms256086%28v=vs.110%29.aspx
            List<string> basenames = new List<string>();
            XmlNode n = xmlNode;

            if (n is XmlAttribute)
            {
                XmlAttribute attr = (XmlAttribute)n;
                basenames.Add("@" + attr.Name);
                n = attr.OwnerElement;
            }

            while (n != null && n.NodeType != XmlNodeType.Document)
            {
                string name = null;

                if (n.Attributes != null)
                {
                    for (int i = 0; i < Identifiers.Length && name == null; i++)
                    {
                        string id = Identifiers[i];
                        if (n.Attributes[id] != null)
                            name = string.Format("{0}[@{1}='{2}']", n.Name, id, n.Attributes[id].Value);
                    }
                }
                if (name == null && n.ParentNode != null && n.ParentNode.ChildNodes.Count > 1) //there are NO unique identifiers. Use index
                {
                    int sindex = SiblingIndex(n); //xPath indices start at 1, not 0
                    if (sindex == 0) name = n.Name; //don't need an index because the node name is unique among the siblings
                    else name = string.Format("{0}[{1}]", n.Name, sindex);
                }
                if (name == null) name = n.Name;

                basenames.Add(name);
                n = n.ParentNode;
            }

            // iterate the lists backwards and construct the XPath
            StringBuilder xpath = new StringBuilder();
            for (int i = basenames.Count - 1; i >= 0; i--)
            {
                xpath.Append("/" + basenames[i]);
            }
            return xpath.ToString();
        }

        private int SiblingIndex(XmlNode n)
        {
            if (n.ParentNode == null) return 0;
            int sindex = 0;
            int dupes = 0;
            foreach (XmlNode sibling in n.ParentNode.ChildNodes)
            {
                if (sibling.Name != n.Name) continue;
                dupes++;
            }

            if (dupes <= 1) return 0; //don't need an index because the node name is unique among the siblings

            foreach (XmlNode sibling in n.ParentNode.ChildNodes)
            {
                if (sibling.Name != n.Name) continue;
                sindex++;
                if (sibling == n) break;
            }
            return sindex;
        }

        private static readonly PropertyInfo pi = typeof(XmlAttribute).GetProperty("IsNamespace", BindingFlags.Instance | BindingFlags.NonPublic);
        private static bool IsNamespace(XmlAttribute a) => (bool)pi.GetValue(a);

        private void NamespaceBuilder(XmlNode node, XmlNamespaceManager nsmgr)
        {
            foreach (XmlNode n in node.ChildNodes)
            {
                if (n.NodeType != XmlNodeType.Element) continue;
                if (n.Attributes.Count > 0)
                    foreach (var a in n.Attributes.Cast<XmlAttribute>().Where(a => IsNamespace(a)))
                        nsmgr.AddNamespace(a.LocalName, a.Value);  //We need to declare the namespaces BEFORE we unwind the call stack.
                NamespaceBuilder(n, nsmgr);
            }
        }

        /// <summary>
        /// Merge the differences into new XML file.
        /// </summary>
        /// <param name="filename">Destination XML file.</param>
        /// <param name="preProcess">Make changes to XML document BEFORE the differences are applied. Only necessary if temporary attribute names were assigned in the constructor pre-processor step.</param>
        /// <param name="postProcess">Make changes to XML document AFTER the differences are applied.</param>
        /// <returns>True if successful</returns>
        public bool ApplyTo(string filename, Action<XmlDocument> preProcess = null, Action<XmlDocument> postProcess = null)
        {
            if (!this.IsDifferent) return true;

            XmlReaderSettings rs = new XmlReaderSettings();
            rs.CloseInput = true;
            rs.IgnoreComments = false; //leave the comments in the file
            rs.IgnoreWhitespace = true;
            rs.IgnoreProcessingInstructions = true;
            var reader = XmlReader.Create(filename, rs);
            XmlDocument xdoc = new XmlDocument();
            xdoc.Load(reader);
            reader.Dispose();

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(xdoc.NameTable);
            NamespaceBuilder(xdoc, nsmgr);
            preProcess?.Invoke(xdoc);
            foreach (var d in Removes)
            {
                XmlNode node = xdoc.SelectSingleNode(d.XPath, nsmgr);
                if (node == null) continue;
                if (node is XmlAttribute)
                {
                    var attr = (XmlAttribute)node;
                    attr.OwnerElement.RemoveAttribute(attr.Name);
                    continue;
                }

                node.ParentNode.RemoveChild(node);
            }

            foreach (var d in Changes)
            {
                XmlNode node = xdoc.SelectSingleNode(d.XPath, nsmgr);
                if (node == null) continue;
                node.SetValue(d.NewValue);
            }

            foreach (var d in Adds)
            {
                xdoc.GetNode(d.XPath).SetValue(d.NewValue);
            }

            postProcess?.Invoke(xdoc);

            XmlWriterSettings ws = new XmlWriterSettings();
            ws.CloseOutput = true;
            ws.Indent = true;
            ws.IndentChars = "  ";
            ws.NewLineChars = Environment.NewLine;
            ws.NewLineHandling = NewLineHandling.Replace;
            ws.NewLineOnAttributes = false;
            XmlWriter writer = XmlWriter.Create(filename, ws);
            xdoc.Save(writer);
            writer.Dispose();

            return true;
        }

        /// <summary>
        /// Serialize this (the differences) to an XML string.
        /// </summary>
        /// <param name="beautify">True to apply formatting and indenting to string.</param>
        /// <returns>Serialized object as an xml string.</returns>
        public string Serialize(bool beautify = false)
        {
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = Encoding.UTF8;
            if (beautify)
            {
                settings.Indent = true;
                settings.IndentChars = "  ";
                settings.NewLineChars = Environment.NewLine;
                settings.NewLineHandling = NewLineHandling.Replace;
                settings.OmitXmlDeclaration = true;
                settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
                settings.DoNotEscapeUriAttributes = true;
            }
            else
            {
                settings.Indent = false; //disable all XML formatting
                settings.IndentChars = string.Empty;
                settings.NewLineChars = string.Empty;
                settings.NewLineHandling = NewLineHandling.None;
                settings.OmitXmlDeclaration = false;
                settings.NamespaceHandling = NamespaceHandling.OmitDuplicates;
                settings.DoNotEscapeUriAttributes = false;
            }

            StringBuilder sb = new StringBuilder();
            using (var writer = XmlWriter.Create(sb, settings))
            {
                var serializer = new XmlSerializer(typeof(XmlDiffMerge));
                serializer.Serialize(writer, this);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Deserialize XmlDiff xml string into new XmlDiff object. Then one can use ApplyTo().
        /// Will throw exception if not a valid XmlDiff xml string.
        /// </summary>
        /// <param name="xml">XmlDiff xml string</param>
        /// <returns>new XmlDiff object</returns>
        public static XmlDiffMerge Deserialize(string xml)
        {
            var settings = new XmlReaderSettings();
            settings.CloseInput = true;
            settings.IgnoreComments = true;
            settings.IgnoreProcessingInstructions = true;
            settings.IgnoreWhitespace = true;
            using (var reader = XmlReader.Create(new StringReader(xml), settings))
            {
                var serializer = new XmlSerializer(typeof(XmlDiffMerge));
                return (XmlDiffMerge)serializer.Deserialize(reader);
            }
        }

        /// <summary>
        /// Represents a single row of Adds/Changes/Removes arrays when this is serialized.
        /// </summary>
        public class Diff
        {
            [XmlAttribute] public string XPath { get; set; }
            [XmlAttribute] public string NewValue { get; set; }
            [XmlAttribute] public string OldValue { get; set; }

            public Diff()
            {
                XPath = string.Empty; 
                NewValue = string.Empty; 
                OldValue = string.Empty;
            }

            public Diff(string xpath, string newvalue = null, string originalvalue = null)
            {
                XPath = xpath ?? string.Empty; 
                NewValue = newvalue ?? string.Empty; 
                OldValue = originalvalue ?? string.Empty;
            }
        }
    }

    internal static class XmlExtensions
    {
        //Also used by AppConfig.cs

        /// <summary>
        /// Node values are stored/retrieved differently for different node types. We just make it the same here.
        /// </summary>
        /// <param name="node">Node to retrieve value from</param>
        /// <returns>String value</returns>
        internal static string GetValue(this XmlNode node)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Attribute:
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Text:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                case XmlNodeType.XmlDeclaration: return node.Value;
                case XmlNodeType.Element:
                    XmlNode text = node.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.NodeType == XmlNodeType.CDATA || n.NodeType == XmlNodeType.Text);
                    if (text == null) return null;
                    return text.Value;
            }
            return null;
        }

        /// <summary>
        /// Node values are stored/retrieved differently for different node types. We just make it the same here.
        /// </summary>
        /// <param name="node">Node to set value to</param>
        /// <param name="value">Value to set</param>
        /// <returns>The same node. Useful for daisy-chaining.</returns>
        internal static XmlNode SetValue(this XmlNode node, string value)
        {
            switch (node.NodeType)
            {
                case XmlNodeType.Attribute:
                case XmlNodeType.CDATA:
                case XmlNodeType.Comment:
                case XmlNodeType.Text:
                case XmlNodeType.ProcessingInstruction:
                case XmlNodeType.SignificantWhitespace:
                case XmlNodeType.Whitespace:
                case XmlNodeType.XmlDeclaration:
                    node.Value = value ?? string.Empty;
                    break;

                case XmlNodeType.Element:
                    XmlNode text = node.ChildNodes.Cast<XmlNode>().FirstOrDefault<XmlNode>(n => n.NodeType == XmlNodeType.CDATA || n.NodeType == XmlNodeType.Text);
                    if (string.IsNullOrWhiteSpace(value)) { if (text != null) node.RemoveChild(text); }
                    else
                    {
                        if (text != null) text.Value = value;
                        else node.InsertBefore(node.OwnerDocument.CreateTextNode(value), node.FirstChild);
                    }
                    break;
            }
            return node;
        }

        /// <summary>
        /// Return the node refered to by the xPath. If any elements in the node do not exist, they are created.
        /// </summary>
        /// <param name="node">node to start of xPath. if xPath is an absolute path, this node is set to the document root</param>
        /// <param name="xpath">relative or absolute xPath to node</param>
        /// <returns>Node referred to by the xPath</returns>
        internal static XmlNode GetNode(this XmlNode node, string xpath, XmlNamespaceManager nsmgr = null)
        {
            char[] delimiters = new char[] { '/', '[', ']', '=', '"', '\'' };
            // xpath == /configuration/node1/node2[subnode1/subnode2[@attr="str"]/@attr2
            //book[/bookstore/@specialty=@style]
            //author[last-name = "Bob"]

            if (xpath == null) return node;
            xpath = xpath.Trim();
            if (xpath[0] == '/') node = node.OwnerDocument ?? node;
            xpath = xpath.Trim('/');
            if (xpath.Length == 0) return node;
            XmlNode n = SelectNode(node, xpath, nsmgr);
            if (n != null) return n;
            char leadingDelimiter = '\0';
            char trailingDelimiter = '\0';
            while (xpath.Length > 0)
            {
                int delimiterIndex = xpath.IndexOfAny(delimiters);
                if (delimiterIndex == -1)
                {
                    if (xpath[0] == '@') return node.Attributes.Append(node.OwnerDocument.CreateAttribute(xpath.TrimStart('@')));
                    else return node.AppendChild(node.OwnerDocument.CreateElement(xpath));
                }

                leadingDelimiter = trailingDelimiter;
                trailingDelimiter = xpath[delimiterIndex];
                string item = xpath.Substring(0, delimiterIndex).Trim();
                xpath = xpath.Substring(delimiterIndex + 1, xpath.Length - delimiterIndex - 1).Trim();

                if (trailingDelimiter == '[')
                {
                    int bracketCount = 1;
                    for (delimiterIndex = 0; delimiterIndex < xpath.Length; delimiterIndex++)
                    {
                        if (xpath[delimiterIndex] == '[') { bracketCount++; continue; }
                        if (xpath[delimiterIndex] == ']') { bracketCount--; if (bracketCount > 0) continue; else break; }
                    }
                    n = node.SelectSingleNode(item + "[" + xpath.Substring(0, delimiterIndex + 1), nsmgr);
                    if (n == null)
                    {
                        n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                        n.GetNode(xpath.Substring(0, delimiterIndex));
                    }
                    leadingDelimiter = trailingDelimiter;
                    trailingDelimiter = xpath[delimiterIndex];
                    if ((delimiterIndex + 2) > xpath.Length) xpath = string.Empty;
                    else xpath = xpath.Substring(delimiterIndex + 2, xpath.Length - delimiterIndex - 2);
                    node = n;
                    continue;
                }

                if (trailingDelimiter == '/')
                {
                    if (item.Length == 0 && trailingDelimiter == '/') { n = node.OwnerDocument; continue; }
                    n = node.SelectSingleNode(item, nsmgr);
                    if (n == null)
                    {
                        if (item[0] == '@') n = node.Attributes.Append(node.OwnerDocument.CreateAttribute(item.TrimStart('@')));
                        else n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                    }
                    node = n;
                    continue;
                }

                if (trailingDelimiter == '=')
                {
                    n = node.SelectSingleNode(item, nsmgr);
                    if (n == null)
                    {
                        if (item[0] == '@') n = node.Attributes.Append(node.OwnerDocument.CreateAttribute(item.TrimStart('@')));
                        else n = node.AppendChild(node.OwnerDocument.CreateElement(item));
                    }
                    node = n;
                    continue;
                }

                if (trailingDelimiter == '"' || trailingDelimiter == '\'')
                {
                    delimiterIndex = xpath.IndexOf(trailingDelimiter);
                    if (delimiterIndex == -1) throw new FormatException("Invalid XPath format. Missing trailing quote");
                    leadingDelimiter = trailingDelimiter;
                    trailingDelimiter = xpath[delimiterIndex];
                    item = xpath.Substring(0, delimiterIndex).Trim();
                    xpath = xpath.Substring(delimiterIndex + 1, xpath.Length - delimiterIndex - 1).Trim();
                    node.SetValue(item);
                    continue;
                }
            }
            return node;
        }

        private static XmlNode SelectNode(XmlNode node, string path, XmlNamespaceManager nsmgr = null)
        {
            //Used exclusively by GetNode(), above.
            XmlNode n = null;
            int equalIndex = path.IndexOf('=');
            if (equalIndex != 1)
            {
                int bracketIndex = path.IndexOf('[');
                if (bracketIndex == -1) bracketIndex = int.MaxValue;
                if (equalIndex < bracketIndex) return null;
            }
            try { n = node.SelectSingleNode(path, nsmgr); } catch { }
            return n;
        }

        /// <summary>
        /// Returns a value indicating whether the specified case-insensitive string is equal to this string. Safely handles null values
        /// </summary>
        /// <param name="s">String to operate upon</param>
        /// <param name="value">The case-insensitive string to compare.</param>
        /// <returns>true if the value parameter equals this string or if both the string to search and the value to seek are both null.</returns>
        internal static bool EqualsI(this string s, string value)
        {
            if (s == null && value == null) return true;
            return (s != null && value != null && s.Equals(value, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}

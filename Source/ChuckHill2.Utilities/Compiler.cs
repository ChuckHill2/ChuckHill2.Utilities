//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="Compiler.cs" company="Chuck Hill">
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
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ChuckHill2.Extensions;
using Microsoft.CSharp;

namespace ChuckHill2
{
    /// <summary>
    /// Compile a C# source code file or source code string into a loaded dynamic assembly.
    /// </summary>
    public class Compiler : IDisposable
    {
        private CompilerResults _results = null;
        private CSharpCodeProvider csCompiler = null;

        /// <summary>
        /// Returns the successfully compiled and loaded assembly
        /// </summary>
        public Assembly CompiledAssembly
        {
            get
            {
                if (_results == null || _results.Errors.HasErrors) return null;
                return _results.CompiledAssembly;
            }
        }

        /// <summary>
        /// Gets the path of the compiled assembly or null if the assembly was generated in memory.
        /// </summary>
        public string AssemblyPath
        {
            get
            {
                if (_results == null) return null;
                return _results.PathToAssembly;
            }
        }

        /// <summary>
        /// Gets the compiler's exit/return value (e. g. zero==success)
        /// </summary>
        public int CompilerStatus
        {
            get
            {
                if (_results == null) return -1;
                return _results.NativeCompilerReturnValue;
            }
        }

        /// <summary>
        ///  Gets the collection of compiler errors and warnings, if any, as a multi-line string.
        /// </summary>
        public string ErrorString
        {
            get
            {
                if (_results == null) return null;
                var sb = new StringBuilder();
                foreach (var error in _results.Errors) { sb.AppendLine(error.ToString()); }
                return sb.ToString();
            }
        }

        /// <summary>
        /// Gets the collection of compiler errors and warnings, if any, as an array.
        /// </summary>
        public System.CodeDom.Compiler.CompilerError[] Errors
        {
            get
            {
                if (_results == null) return null;
                CompilerError[] errors = new CompilerError[_results.Errors.Count];
                for(int i=0; i<errors.Length; i++) errors[i] = _results.Errors[i];
                return errors;
            }
        }

        /// <summary>
        /// Dynamically compile monolithic source code string or file into a loaded assembly.
        /// If this assembly is compiled in DEBUG mode then the returned compiled assembly will 
        /// also be compiled in DEBUG mode with matching source code and PDB files. 
        /// </summary>
        /// <param name="sourceCode">
        /// source code string or a CS file containing the source code. MUST be fully defined source code with usings and namespace keywords. 
        /// If this is not a filename AND not debugging, no dll file will be created...strictly in-memory.
        /// </param>
        /// <param name="references">
        /// Array of dll references required to execute sourcecode. Must include filename.extension.
        /// The built-in default references are:
        ///     Microsoft.CSharp.dll
        ///     System.dll
        ///     System.Core.dll
        ///     System.Data.dll
        ///     System.Data.DataSetExtensions.dll
        ///     System.Xml.dll
        ///     System.Xml.Linq.dll
        ///     (assembly containing this class. not the caller)
        /// Adding the same reference more than once, does nothing.
        /// </param>
        /// <param name="debug">True to create source file (if it does not already exist), associated PDB, and compile as NOT optimized.</param>
        /// <returns>compiled and loaded assembly.</returns>
        public Assembly Compile(string sourceCode, IList<string> references = null, bool debug=false)
        {
            this._results = null;

            string dllFile = null;
            string csFile = null;
            bool saveResult = true; //only relevent if debug==false

            if (sourceCode.IsNullOrEmpty()) throw new ArgumentNullException("sourceCode");
            if (sourceCode.IsFileName())  //must be a filename
            {
                csFile = sourceCode;
                string ext = Path.GetExtension(csFile).Substring(1).ToUpper();
                if (!ext.Equals("CS")) throw new InvalidDataException("Only CS source files are supported.");
                if (!File.Exists(csFile)) throw new FileNotFoundException(string.Format("File {0} not found.",csFile),csFile);
                sourceCode = File.ReadAllText(csFile);
                dllFile = Path.ChangeExtension(csFile,"dll");
                if (File.Exists(dllFile)) File.Delete(dllFile);
            }

            if (csFile==null) //create a name
            {
                saveResult = false; //only relevent if debug==false
                string ns = Regex.Match(sourceCode, @"^\s*namespace +(?<NAMESPACE>[\w.]+)", RegexOptions.Multiline).Groups["NAMESPACE"].Value;
                string cls = Regex.Match(sourceCode, @"^\s*(?:public\s+|private\s+interface\s+|sealed\s+|abstract\s+|abstract\s+|static\s+)*class\s+(?<CLASS>\w+)", RegexOptions.Multiline).Groups["CLASS"].Value;
                string dir = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location);
                csFile = string.Format(@"{0}\{1}.{2}.cs", dir, ns, cls);
                if (File.Exists(csFile)) File.Delete(csFile);
            }

            #region AssemblyInfo - populate string assemblyInfo
            Assembly aiAsm = Assembly.GetCallingAssembly();
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("//------------------------------------------------------------------------------");
            sb.AppendLine("// <auto-generated>");
            sb.AppendLine("//     This code was automatically generated from " + Path.GetFileName(csFile));
            sb.AppendLine("//     Changes to this file are ignored and will");
            sb.AppendLine("//     be lost when this code is regenerated.");
            sb.AppendLine("//     This file is only needed for debugging.");
            sb.AppendLine("// </auto-generated>");
            sb.AppendLine("//------------------------------------------------------------------------------");
            sb.AppendLine("[assembly: System.Security.AllowPartiallyTrustedCallers()]");
            sb.AppendLine("[assembly: System.Security.SecurityTransparent()]");
            sb.AppendLine("[assembly: System.Security.SecurityRules(System.Security.SecurityRuleSet.Level1)]");
            sb.AppendLine("[assembly: System.CLSCompliantAttribute(true)]");
            sb.AppendFormat("[assembly: System.Reflection.AssemblyTitleAttribute(\"{0}\")]\r\n",Path.GetFileNameWithoutExtension(csFile));
            sb.AppendLine("[assembly: System.Reflection.AssemblyDescriptionAttribute(\"Dynamically Generated Code\")]");
            sb.AppendFormat("[assembly: System.Reflection.AssemblyConfigurationAttribute(\"{0}\")]\r\n",debug?"DEBUG":"RELEASE");
            sb.AppendFormat("[assembly: System.Reflection.AssemblyCompanyAttribute(\"{0}\")]\r\n", aiAsm.Attribute<AssemblyCompanyAttribute>());
            sb.AppendFormat("[assembly: System.Reflection.AssemblyProductAttribute(\"{0}\")]\r\n", aiAsm.Attribute<AssemblyProductAttribute>());
            sb.AppendFormat("[assembly: System.Reflection.AssemblyCopyrightAttribute(\"{0}\")]\r\n", aiAsm.Attribute<AssemblyCopyrightAttribute>());
            sb.AppendFormat("[assembly: System.Reflection.AssemblyTrademarkAttribute(\"{0}\")]\r\n", aiAsm.Attribute<AssemblyTrademarkAttribute>());
            sb.AppendFormat("[assembly: System.Reflection.AssemblyCultureAttribute(\"{0}\")]\r\n", aiAsm.Attribute<AssemblyCultureAttribute>());
            sb.AppendLine("[assembly: System.Runtime.InteropServices.ComVisibleAttribute(false)]"); //COM interface not supported
            //sb.AppendLine("[assembly: System.Runtime.InteropServices.GuidAttribute(\"580E2058-6FA3-4879-A80E-341303C3E851\")]");  //COM interface not supported
            string ver = aiAsm.Attribute<AssemblyFileVersionAttribute>();
            if (ver.IsNullOrEmpty()) ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            sb.AppendFormat("[assembly: System.Reflection.AssemblyFileVersionAttribute(\"{0}\")]\r\n",ver);
            ver = aiAsm.Attribute<AssemblyVersionAttribute>();
            if (ver.IsNullOrEmpty()) ver = Assembly.GetExecutingAssembly().GetName().Version.ToString();
            sb.AppendFormat("[assembly: System.Reflection.AssemblyVersionAttribute(\"{0}\")]\r\n", ver);

            string assemblyInfo = sb.ToString();
            #endregion

            #region Compile source code
            if (csCompiler==null) csCompiler = new CSharpCodeProvider();
            CompilerParameters compilerParams = new CompilerParameters();
            //Add 'new class library project' default references.
            compilerParams.ReferencedAssemblies.Add("Microsoft.CSharp.dll");
            compilerParams.ReferencedAssemblies.Add("System.dll");
            compilerParams.ReferencedAssemblies.Add("System.Core.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.dll");
            compilerParams.ReferencedAssemblies.Add("System.Data.DataSetExtensions.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.dll");
            compilerParams.ReferencedAssemblies.Add("System.Xml.Linq.dll");
            //Also add this utility assembly as a reference.
            compilerParams.ReferencedAssemblies.Add(Path.GetFileName(Assembly.GetAssembly(typeof(Compiler)).Location));
            //Now add the user-specified references
            if (references!=null) compilerParams.ReferencedAssemblies.AddRange(references.ToArray());
            compilerParams.GenerateExecutable = false;
            if (debug)
            {
                string assemblyInfoCS = Path.ChangeExtension(Path.GetTempFileName(),"cs");
                File.WriteAllText(assemblyInfoCS,assemblyInfo);
                if (!File.Exists(csFile)) File.WriteAllText(csFile, sourceCode);
                compilerParams.OutputAssembly = dllFile;
                compilerParams.CompilerOptions = "/debug";
                compilerParams.IncludeDebugInformation = true;
                compilerParams.GenerateInMemory = false;
                this._results = csCompiler.CompileAssemblyFromFile(compilerParams, assemblyInfoCS, csFile);
                File.Delete(assemblyInfoCS);
            }
            else
            {
                compilerParams.IncludeDebugInformation = false;
                compilerParams.CompilerOptions = "/optimize";
                if (saveResult)
                {
                    compilerParams.OutputAssembly = dllFile;
                    compilerParams.GenerateInMemory = false;

                }
                else
                {
                    compilerParams.GenerateInMemory = true;
                }
                this._results = csCompiler.CompileAssemblyFromSource(compilerParams, assemblyInfo, sourceCode);
            }
            #endregion
            return this.CompiledAssembly;
        }

        /// <summary>
        /// Releases all the resources used by this object.
        /// </summary>
        public void Dispose()
        {
            if (csCompiler != null)
            {
                csCompiler.Dispose();
                csCompiler = null;
            }
        }
    }
}

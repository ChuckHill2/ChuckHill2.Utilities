using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ChuckHill2.Utilities.Extensions;
using Microsoft.Win32;

namespace ChuckHill2.Utilities
{
    /// <summary>
    /// Principally locates product files and assemblies that CLR loader 
    /// can't find. This effectively flattens the directory heriarchy 
    /// creating a virtual directory where all files reside.
    /// </summary>
    public static class Resolver
    {
        /// <summary>
        /// Delegate for search and locate the product root directory. This is
        /// the directory heirarchy root where all files are resolved from.
        /// </summary>
        /// <param name="productName">Returns the name of root directory component.</param>
        /// <param name="productBase">Returns the root directory path where the product principially resides.</param>
        /// <param name="additionalSearchDirs">Returns an array of fully qualified directory paths that will also be searched...if the directory exists.</param>
        public delegate void InitializeDelegate(out string productName, out string productBase, out string[] additionalSearchDirs);
        /// <summary>
        /// Property for getting/setting delegate for search and locate the product root 
        /// directory. This is the directory heirarchy root where all files are resolved from.
        /// </summary>
        public static InitializeDelegate InitializeResolver { get { return _InitializeResolver; } set { _InitializeResolver = value; ReInitialize(); } }
        private static InitializeDelegate _InitializeResolver = Initialize_Default;

        //First-time initialization of static readonly directory base strings
        static Resolver() { ReInitialize(); }

        /// <summary>
        /// Reinitialize base directories when they when they get changed. (aka installation)
        /// </summary>
        public static void ReInitialize() 
        {
            string[] additionalDirs;
            InitializeResolver(out _ProductName, out _ProductBase, out additionalDirs);
            SearchDirs = InitSearchDirs(_ProductName, _ProductBase, additionalDirs);
        }

        /// <summary>
        /// Product directory name
        /// </summary>
        public static string ProductName { get { return _ProductName; } }
        private static string _ProductName;

        /// <summary>
        /// Path where this executable resides. This will never change for the life of the application.
        /// </summary>
        private static readonly string ApplicationBase = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

        /// <summary>
        /// Root directory where this product resides
        /// </summary>
        public static string ProductBase { get { return _ProductBase; } }
        private static string _ProductBase;

        private static int ResolveHandlerKount = 0;   //There can only be one Domain_AssemblyResolver() within the AppDomain

        /// <summary>
        /// Last chance assembly resolver for current domain when the CLR cannot find a Product assembly to load.
        /// Note:
        /// For this to work on the Main program AppDomain, the following MUST be on the first line of the startup static class.
        /// static Program() { Resolver.Subscribe(); }
        /// </summary>
        public static void Subscribe() { Subscribe(AppDomain.CurrentDomain); }
        /// <summary>
        /// Last chance assembly resolver for specified domain when the CLR cannot find a Product assembly to load.
        /// </summary>
        /// <param name="ad">appdomain initialize resolver for</param>
        public static void Subscribe(AppDomain ad)
        {
            if (ad == AppDomain.CurrentDomain)
            {
                if (ResolveHandlerKount == 0)
                    ad.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolver);
                ResolveHandlerKount++;
            }
            else
                ad.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolver);
        }
        /// <summary>
        /// Unsubscribe this Resolver class assembly resolver.
        /// </summary>
        public static void Unsubscribe() { Unsubscribe(AppDomain.CurrentDomain); }
        /// <summary>
        /// Unsubscribe the specified appdomain's Resolver class assembly resolver.
        /// </summary>
        /// <param name="ad"></param>
        public static void Unsubscribe(AppDomain ad)
        {
            if (ad == AppDomain.CurrentDomain)
            {
                ResolveHandlerKount--;
                if (ResolveHandlerKount < 0) ResolveHandlerKount = 0;
                if (ResolveHandlerKount == 0)
                    ad.AssemblyResolve -= new ResolveEventHandler(Domain_AssemblyResolver);
            }
            else
                ad.AssemblyResolve -= new ResolveEventHandler(Domain_AssemblyResolver);

        }

        /// <summary>
        /// Finds full file path to the specified Product filename. 
        /// If we have to hunt for the file, directories and/or files marked with the Hidden attribute will be ignored.
        /// If the file contains wildcards ('*' or '?') only the first, most recently modified instance will be returned.
        /// </summary>
        /// <param name="filename">filename (e.g. ABC.xml) or relative filename (e.g. systems\abc.xml)</param>
        /// <param name="t">if filename not found, extract from embedded resources in the assembly contining this type</param>
        /// <returns>Full filepath or null if not found.</returns>
        public static string FindFile(string filename, Type t=null)
        {
            string[] files = FindFiles(filename);
            if (files.Length != 0) return files[0];
            if (filename.Any(m => m == '*' || m == '?')) return null; //no wildcards
            try
            {
                filename = Path.GetFullPath(filename);
                if (!DirectoryEx.IsWritable(Path.GetDirectoryName(filename)))
                {
                    string dir = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\" + ProductName;
                    if (!Directory.Exists(dir)) try { Directory.CreateDirectory(dir); } catch { }
                    filename = Path.Combine(dir, Path.GetFileName(filename));
                    if (!DirectoryEx.IsWritable(dir))
                    {
                        if (!Directory.Exists(dir)) try { Directory.CreateDirectory(dir); } catch { }
                        filename = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\" + ProductName, Path.GetFileName(filename));
                    }
                }

                if (t != null && ExtractResourceFile(Assembly.GetAssembly(t), filename)) return filename;

                string an = Assembly.GetExecutingAssembly().GetName().Name;
                List<Assembly> assemblyStack = (from f in new StackTrace().GetFrames() select f.GetMethod().ReflectedType.Assembly)
                                 .Distinct()
                                 .FindAll(m => (m.GetName().Name.StartsWith(Resolver.ProductName,StringComparison.InvariantCultureIgnoreCase) && !m.GetName().Name.Equals(an)));
                foreach (var a in assemblyStack)
                {
                    if (ExtractResourceFile(a, filename)) return filename;
                }
            }
            catch (Exception ex)
            {
                Log("[Could not write resource {0}] {1}", filename, ex.Message);
            }
            return null;
        }

        public static string FindFile(string filename)
        {
            return FindFile(filename, null);
        }

        private static bool ExtractResourceFile(Assembly asm, string filepath)
        {
            if (filepath.IsNullOrEmpty()) return false;
            if (asm==null) return false;
            string filename = Path.GetFileName(filepath);
            var resname = asm.GetManifestResourceNames().FirstOrDefault(s => s.EndsWith(filename, StringComparison.InvariantCultureIgnoreCase));
            if (resname != null)
            {
                if (!Directory.Exists(Path.GetDirectoryName(filepath))) Directory.CreateDirectory(Path.GetDirectoryName(filepath));
                using (var fs = new FileStream(filepath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite, 4096, FileOptions.SequentialScan))
                    asm.GetManifestResourceStream(resname).CopyTo(fs);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Finds full file path to the specified Product filename. 
        /// If we have to hunt for the file, directories and/or files marked with the Hidden attribute will be ignored.
        /// If the file contains wildcards ('*' or '?') ALL most recently modified instances will be returned.
        /// </summary>
        /// <param name="filename">filename (e.g. ABC.xml) or relative filename (e.g. systems\abc.xml)</param>
        /// <returns>array of unique found filenames. returned value is never null.</returns>
        public static string[] FindFiles(string filename)
        {
            string originalFilename = filename;
            int fileCount = 0;
            try
            {
                if (!filename.Any(m => m == '*' || m == '?'))
                {
                    if (!Path.IsPathRooted(filename) && File.Exists(filename))
                    {
                        filename = Path.GetFullPath(filename);  //if file contains path relative to CWD and is found just return it.
                        fileCount = 1;
                        return new string[]{ filename };
                    }
                    filename = Path.GetFileName(filename);
                    if (File.Exists(filename))
                    {
                        filename = Path.GetFullPath(filename);  //if file exists in CWD just return it.
                        fileCount = 1;
                        return new string[] { filename };
                    }
                }

                string[] files = GetFiles(filename);
                fileCount = files.Length;
                if (fileCount > 0) filename = files[0];
                return files;
            }
            finally
            {
                if (fileCount == 0) Log("[Cannot find file: \"{0}\"]", originalFilename);
                else if (fileCount == 1) Log("[Found file: \"{0}\"]", filename);
                else Log("[Found {0} instances of file: \"{1}\"]", fileCount, originalFilename);
            }
        }

        /// <summary>
        /// Get full names of all object types within a specified assembly, for an optionally 
        /// specified base type WITHOUT actually having to load the assembly.
        /// </summary>
        /// <param name="dllname">DLL/Assembly to search</param>
        /// <param name="baseType">
        /// Full type name of object base type to search for 
        /// or NULL to retrieve all types in DLL/Assembly.
        /// </param>
        /// <returns>array of full type names</returns>
        public static string[] GetTypes(string dllname, string baseType)
        {
            AppDomain domain = null;
            try
            {
                domain = AppDomain.CreateDomain("TypeFinder");
                TypeFinder tf = (TypeFinder)domain.CreateInstanceFromAndUnwrap(Assembly.GetAssembly(typeof(TypeFinder)).Location, typeof(TypeFinder).FullName);
                return tf.FindTypes(dllname, baseType);
            }
            catch { return new string[0]; }
            finally { if (domain != null) AppDomain.Unload(domain); }
        }

        /// <summary>
        /// Get attributes within a specified assembly WITHOUT actually having to load the assembly.
        /// </summary>
        /// <param name="dllname">DLL/Assembly to search</param>
        /// <param name="member">
        /// Full (aka namespace.class.member) or relative (class.member or just member) membername 
        /// or Null or Empty to return all assembly attributes
        /// </param>
        /// <returns>array of found attributes</returns>
        public static Attribute[] GetAttributes(string dllname, string member)
        {
            AppDomain domain = null;
            try
            {
                domain = AppDomain.CreateDomain("TypeFinder");
                TypeFinder tf = (TypeFinder)domain.CreateInstanceFromAndUnwrap(Assembly.GetAssembly(typeof(TypeFinder)).Location, typeof(TypeFinder).FullName);
                return tf.FindAttributes(dllname, member);
            }
            catch { return new Attribute[0]; }
            finally { if (domain != null) AppDomain.Unload(domain); }
        }

        /// <summary>
        /// Get static Fields/Properties within a specified assembly WITHOUT actually having to load the assembly.
        /// </summary>
        /// <param name="dllname">DLL/Assembly to search</param>
        /// <param name="member">
        /// Full (aka namespace.class.member) or relative (class.member or just member) membername 
        /// Null will return an empty array.
        /// </param>
        /// <returns>array of found attributes</returns>
        public static Object[] GetStaticValues(string dllname, string member)
        {
            AppDomain domain = null;
            try
            {
                domain = AppDomain.CreateDomain("TypeFinder");
                TypeFinder tf = (TypeFinder)domain.CreateInstanceFromAndUnwrap(Assembly.GetAssembly(typeof(TypeFinder)).Location, typeof(TypeFinder).FullName);
                return tf.FindStaticValues(dllname, member);
            }
            catch { return new Object[0]; }
            finally { if (domain != null) AppDomain.Unload(domain); }
        }

        /// <summary>
        /// Loads the contents of an assembly by filename or assembly reference.
        /// If it is already loaded, it is returned.
        /// Ignores directory part and attempts to find the file with Resolver.FindFile().
        /// </summary>
        /// <param name="filename">The file or assembly reference to load.</param>
        /// <returns>Loaded assembly</returns>
        /// <exception cref="System.ArgumentNullException">The path parameter is null.</exception>
        /// <exception cref="System.IO.FileLoadException"> A file that was found could not be loaded.</exception>
        /// <exception cref="System.IO.FileNotFoundException">The path parameter is an empty string ("") or does not exist.</exception>
        /// <exception cref="System.BadImageFormatException">path is not a valid assembly.</exception>
        public static Assembly LoadAssembly(string filename)
        {
            string fname = null;
            if (!filename.EndsWith(".dll", StringComparison.InvariantCultureIgnoreCase) &&
                !filename.EndsWith(".exe", StringComparison.InvariantCultureIgnoreCase))
            {
                fname = (new AssemblyName(filename)).Name + ".dll";
            }
            else fname = Path.GetFileName(filename);
            Assembly asm = Resolver.GetLoadedAssemblyByName(fname);
            if (asm != null)
            {
                Log("[Assembly \"{0}\" already loaded]", filename);
                return asm;
            }
            string path = Resolver.FindFile(fname);
            if (path == null) throw new FileNotFoundException(string.Format("File {0} not found.", fname), filename);
            return Assembly.LoadFrom(path);
        }

        /// <summary>
        /// Get previously loaded assembly by file name.
        /// Works similarly to Assembly.GetAssembly(Type.GetType(typeName,false)) except
        /// Type.GetType() will ALWAYS return null if the assembly was previously loaded
        /// via Assembly.LoadFrom(filepath).
        /// </summary>
        /// <param name="filename">name of underlying file of assembly to retrieve</param>
        /// <returns>loaded assembly or null if not found</returns>
        public static Assembly GetLoadedAssemblyByName(string filename)
        {
            filename = Path.GetFileNameWithoutExtension(filename);
            return AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(a =>
            {   //some dynamic assemblies throw an exception when referencing Location!
                try { if (a.IsDynamic) return false; return (string.Compare(Path.GetFileNameWithoutExtension(a.Location), filename, true) == 0); }
                catch { return false; }
            });
        }
        /// <summary>
        /// Get previously loaded assembly by full type name.
        /// Works similarly to Assembly.GetAssembly(Type.GetType(typeName,false)) except
        /// Type.GetType() will ALWAYS return null if the assembly was previously loaded
        /// via Assembly.LoadFrom(filepath).
        /// </summary>
        /// <param name="fulltypename">full name of type to retrieve</param>
        /// <returns>loaded assembly or null if not found</returns>
        public static Assembly GetLoadedAssemblyByType(string fulltypename)
        {
            Assembly[] asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly asm in asms)
            {
                try { if (asm.GetType(fulltypename, false, true) != null) return asm; }
                catch { }
            }
            return null;
        }

        /// <summary>
        /// Get path of file relative to Current Working Directory.
        /// </summary>
        /// <param name="path">full or relative path</param>
        /// <returns>relative path name</returns>
        public static string GetRelativePath(String path)
        {
            path = Path.GetFullPath(path);
            string[] cwd = Path.GetFullPath(".").Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);
            string[] paths = path.Split(new string[] { "\\" }, StringSplitOptions.RemoveEmptyEntries);

            int i = 0;
            for (; i < cwd.Length && i < paths.Length && cwd[i].EqualsI(paths[i]); i++) { }
            if (i == 0) return path;  //nothing in common! return fully qualified name.
            string newpath = string.Join("\\", paths, i, paths.Length - i);
            if (i == cwd.Length) return newpath; //directly under CWD
            StringBuilder sb = new StringBuilder(path.Length);
            for (int j = 0; j < cwd.Length-i; j++) sb.Append(@"..\");
            return sb.Append(newpath).ToString();  //parent dir(s) not common
        }

        #region Private Methods
        /// <summary>
        /// Search and locate the product root directory. This is the 
        /// directory heirarchy root where all files are resolved from.
        /// 
        /// The logic in this function is GENERIC TO ANY PRODUCT and 
        /// should be replaced with something more product-specific.
        /// See example: class MyProductResolver, below.
        /// This is not sophisticated enough for most uses.
        /// This should be be able to handle the installed product 
        /// location(s) as well as the developer environment.
        /// </summary>
        /// <param name="productName">Returns the name of root directory component.</param>
        /// <param name="productBase">Returns the root directory path where the product principially resides ("e.g. "C:\Program Files\[productname]").</param>
        /// <param name="additionalSearchDirs">Returns an array of fully qualified directory paths that will also be searched...if the directory exists, yet.</param>
        private static void Initialize_Default(out string productName, out string productBase, out string[] additionalSearchDirs)
        {
            productBase = null;
            additionalSearchDirs = new string[0];
            string path;

            Log("[Using default resolver initializer]"); //We *should* be using a product-specific initializer.

            //Identify THIS product name.
            productName = Path.GetFileNameWithoutExtension(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName)); //use the executable name as the product sub-directory name.
            var attribute = Assembly.GetEntryAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false).FirstOrDefault() as AssemblyTitleAttribute;
            if (attribute != null) productName = attribute.Title; //use the main executable assembly title as the product sub-directory name.

            //Check for posssible install paths. Not very effective, but 
            //this is best we can do without any additional information.
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), productName);
            if (productBase == null && Directory.Exists(path)) productBase = path;
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), productName);
            if (productBase == null && Directory.Exists(path)) productBase = path;
            if (productBase == null) productBase = Resolver.ApplicationBase; //the default.

            //List all possible extra paths to search. These directories 
            //may be created upon demand, so they may not exist...yet.
            List<string> dirs = new List<string>();
            path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
            path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
            path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
            if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
            path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
            if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
            path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
            if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
            additionalSearchDirs = dirs.ToArray();
        }

        /// <summary>
        /// Last chance assembly resolver when the CLR cannot find a Product assembly to load.
        /// Usage:
        /// AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(Domain_AssemblyResolver);
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns>Found assembly or NULL</returns>
        private static Assembly Domain_AssemblyResolver(object sender, ResolveEventArgs args)
        {
            //This function can ONLY use THIS assembly or assemblies from the GAC because if this routine or attendant  
            //subroutines needs to use an assembly that needs to be "found" then recursion will occur!
            AssemblyName asmName = new AssemblyName(args.Name);
            Exception ex = null;
            Assembly asm = null;
            string fullpath = null;

            asm = GetLoadedAssemblyByName(asmName.Name+".dll");
            if (asm != null) return asm;

            try
            {
                fullpath = FindAssembly(asmName);
                if (fullpath != null) asm = Assembly.LoadFrom(fullpath);
            }
            catch (Exception e) { ex = e; }
            finally
            {
                if (asm == null)
                {
                    if (!asmName.Name.EndsWith(".resources")) Log("[Unresolved assembly: \"{0}\"]", asmName.Name);
                    if (ex != null) Log("Assembly Resolver Error: \"{0}\"", ex);
                }
                else
                {
                    string loc = asm.ToString();
                    try { if (!string.IsNullOrEmpty(asm.Location)) loc = asm.Location; } catch { }
                    Log("[Resolved assembly: \"{0}\"]", loc);
                }
            }
            return asm;
        }

        private static string FindAssembly(AssemblyName reference)
        {
            string[] files = new string[0];
            string filename = Path.GetFileName(reference.Name + ".dll");
            string originalFilename = filename;

            files = GetFiles(filename);
            if (files.Length == 0) //maybe load an EXE as a regular assembly?
            {
                filename = Path.GetFileName(reference.Name + ".exe");
                originalFilename = filename;
                files = GetFiles(filename);
            }
            if (files.Length == 0) return null;
            string parentdirname = "\\"+Path.GetFileNameWithoutExtension(filename)+"\\";
            filename = null;
            filename = files.FirstOrDefault(f => (Path.GetDirectoryName(f).Contains(parentdirname,true) && ValidateAssembly(f, reference))); //We use the one found in dir with the same name as the assembly
            if (filename == null) filename = files.FirstOrDefault(f => (ValidateAssembly(f, reference)));
            if (filename == null) return null;

            //Update PATH so we don't have to search so hard next time.
            #pragma warning disable 618 //warning CS0618: 'AppDomain.AppendPrivatePath(string)' is obsolete: 'AppDomain.AppendPrivatePath has been deprecated. Please investigate the use of AppDomainSetup.PrivateBinPath instead.
            string dirpath = Path.GetDirectoryName(filename);
            //AppDomain dir path must be a subdirectory of the BaseDirectory or else it will be ignored.
            if (dirpath.Contains(AppDomain.CurrentDomain.BaseDirectory,true))
            {
                //Don't append new path if it already exists.
                string searchpath = AppDomain.CurrentDomain.RelativeSearchPath;
                if (searchpath == null || !searchpath.Contains(dirpath,true))
                    AppDomain.CurrentDomain.AppendPrivatePath(dirpath);
            }
            return filename;
        }

        private static bool ValidateAssembly(string f, AssemblyName reference)
        {
            //Validate that the assembly will load successfully 
            System.Reflection.AssemblyName newName = System.Reflection.AssemblyName.GetAssemblyName(f);
            if (!System.Reflection.AssemblyName.ReferenceMatchesDefinition(reference, newName)) return false;
            //if the requested assembly is signed, then the one we found better have the same signature
            byte[] token = newName.GetPublicKeyToken();
            if (token == null) token = new byte[0];
            byte[] refToken = reference.GetPublicKeyToken();
            if (refToken == null) refToken = new byte[0];
            if (refToken.Length > 0 && !refToken.SequenceEqual(token)) return false;
            return true;
        }

        private static string[] SearchDirs; //list of root directories that GetFiles() searches.
        private static string[] GetFiles(string filename)
        {
            //Locate matching files (1 or more) based upon:
            // (1) The the directory tree starting where this running EXE resides.
            // (2) If not found, search the directory tree starting in the root Product directory from where this EXE is running from (aka C:\Program Files\[productname] OR dev environment root, ex. C:\Local\ProductMain).
            // (3) If not found, search the following until found.
            //     Environment.SpecialFolder.LocalApplicationData = %LOCALAPPDATA% = C:\Users\charlesh\AppData\Local\[Product]            
            //     Environment.SpecialFolder.ApplicationData = %APPDATA% = C:\Users\charlesh\AppData\Roaming\[Product]
            //     Environment.SpecialFolder.CommonApplicationData = %ProgramData% = C:\ProgramData\[Product] 
            //     Environment.SpecialFolder.CommonProgramFiles = %CommonProgramFiles% = C:\Program Files\Common Files\[Product]
            //     Environment.SpecialFolder.CommonProgramFilesX86 = %CommonProgramFiles(x86)% = C:\Program Files (x86)\Common Files\[Product]
            // (4) If not found, just return a zero-length array of strings.
            // Note: Directories and/or files marked with the Hidden attribute will be ignored and not in this returned list.
            // Note: File list is sorted with the most recently modified first.
            try
            {
                string[] files = null;
                foreach (string dir in SearchDirs)
                {
                    if (!Directory.Exists(dir)) continue;
                    files = GetFiles(dir, filename, SearchOption.AllDirectories);
                    if (files.Length > 0) break;
                }
                if (files==null || files.Length == 0) return files; //we're done. file not found

                return files;
            }
            catch { return new string[0]; }
        }

        #region static string[] GetFiles(string path, string searchPattern, SearchOption searchOption)
        //Cannot use System.IO.Directory.GetFiles() because it does not honor the Hidden attribute.
        //Plus it also throws System.UnauthorizedAccessException if any of the subdirs are not accessable.

        /// <summary>
        ///    Returns the names of files (including their paths) in the specified directory
        ///    that match the specified search pattern, using a value to determine whether
        ///    to search subdirectories.
        /// </summary>
        /// <param name="dir">The directory to search.</param>
        /// <param name="searchPattern">
        ///    The search string to match against the names of files in path. May contain 
        ///    traditional DOS filename wildcards '*' and '?'. This search string cannot 
        ///    contain any Regex special characters except for '*' and '?'.
        /// </param>
        /// <param name="searchOption">
        ///    One of the System.IO.SearchOption values that specifies whether the search
        ///    operation should include all subdirectories or only the current directory.
        /// </param>
        /// <exception>There are none</exception>
        /// <remarks>
        ///    Files and/or directories with the Hidden attribute are ignored/skipped.
        /// </remarks>
        /// <returns>
        ///    A String array containing the names of files in the specified directory that
        ///    match the specified search pattern. File names include the full path. Never null. 
        ///    Will return a zero-length string array if nothing is found.
        /// </returns>
        private static string[] GetFiles(string dir, string searchPattern, SearchOption searchOption)
        {
            if (dir.IsNullOrEmpty()) return new string[0];
            List<FileItem> fileItems = new List<FileItem>();
            Win32.WIN32_FIND_DATA fd = new Win32.WIN32_FIND_DATA();  //pass as ref (aka ptr) to avoid filling up the stack
            Regex mask = CreatePatternMask(searchPattern);
            GetFiles(ref fd, ref fileItems, dir, mask, searchOption);

            if (fileItems.Count == 0) return new string[0];
            if (fileItems.Count == 1) return new string[]{ fileItems[0].FullPath };
            fileItems.Sort();
            List<string> files = new List<string>();
            string prevF = string.Empty;
            foreach (var f in fileItems)
            {
                if (f.FileName.Equals(prevF)) continue;
                files.Add(f.FullPath);
                prevF = f.FileName;
            }
            return files.ToArray();
        }
        private static Regex CreatePatternMask(string searchPattern)
        {
            StringBuilder sb = new StringBuilder();
            bool containsDir = false;
            bool hasExtension = false;
            foreach (char c in searchPattern)
            {
                if (c == '\\'){ sb.Append(@"\\"); containsDir = true; hasExtension = false; continue;  }
                if (c == '.') { sb.Append(@"\."); hasExtension = true; continue; }
                if (c == '*') { sb.Append(".*"); continue; }
                if (c == '?') { sb.Append('.'); continue; }
                if (c == '[' || c == ']' || 
                    c == '(' || c == ')' || 
                    c == '^' || c == '$' || 
                    c == '{' || c == '}') sb.Append('\\');
                sb.Append(c);
            }
            if (!containsDir) sb.Insert(0, @"\\");
            if (hasExtension) sb.Append('$');
            return new Regex(sb.ToString(), RegexOptions.IgnoreCase);
        }

        //Used exclusivly by above GetFiles(string, string, SearchOption) for recursion.
        private static void GetFiles(ref Win32.WIN32_FIND_DATA fd, ref List<FileItem> files, string dir, Regex mask, SearchOption searchOption)
        {
            IntPtr hFind = Win32.FindFirstFile(Path.Combine(dir, "*"), out fd);
            if (hFind != Win32.INVALID_HANDLE_VALUE)
            {
                do
                {
                    if (fd.cFileName == "." || fd.cFileName == "..") continue;   //pseudo-directory
                    if ((fd.dwFileAttributes & FileAttributes.Hidden) != 0) continue;
                    string path = Path.Combine(dir, fd.cFileName);
                    if ((fd.dwFileAttributes & FileAttributes.Directory) != 0)
                    {
                        if (searchOption != SearchOption.AllDirectories) continue;
                        GetFiles(ref fd, ref files, path, mask, searchOption);
                        continue;
                    }
                    if (!mask.IsMatch(path)) continue;
                    files.Add(new FileItem(path,fd.cFileName,fd.ftLastWriteTime));
                } while (Win32.FindNextFile(hFind, out fd));
                Win32.FindClose(hFind);
            }
        }

        #region struct FileItem - for sorting to get the most recent version from list
        public struct FileItem : IComparable<FileItem>
        {
            public readonly string FullPath;
            public readonly string FileName;
            private readonly string HashString;
            public FileItem(string path, string fileName, ulong lastWriteTime)
            {
                FullPath = path;
                FileName = fileName.ToLower();
                HashString = string.Format("{0},{1}", FileName, lastWriteTime.ToString());
            }
            public int CompareTo(FileItem other) { return string.CompareOrdinal(other.HashString, this.HashString); } //sort decending
            public override string ToString() { return HashString; }
            public override bool Equals(object obj) { return CompareTo((FileItem)obj) == 0; }
            public override int GetHashCode() { return HashString.GetHashCode(); }
        }
        #endregion
        #endregion -------------------------------------------------------------

        /// <summary>
        /// Create a unique list of directories to search. 
        /// The output is used exclusively by GetFiles(string filename);
        /// Directory paths that are already contained in another are excluded from the returned search list.
        /// </summary>
        /// <param name="productName">trailing product directory elemement to append to the windows special folders that will also be searched, if they exist.</param>
        /// <param name="productBase">path of root product directory (e.g. "C:\Program Files\[productname]" or "C:\Local\ProductName\bin\Debug")."</param>
        /// <param name="baseDirs">Array of directory paths that may or may not exist.</param>
        /// <returns>Array of 0 or more directories. never returns null</returns>
        private static string[] InitSearchDirs(string productName, string productBase, params string[] baseDirs)
        {
            string dir;
            List<string> dirs = new List<string>();

            if (!ApplicationBase.IsNullOrEmpty() && Directory.Exists(ApplicationBase)) dirs.Add(ApplicationBase);
            if (!productBase.IsNullOrEmpty() && Directory.Exists(productBase)) dirs.Add(productBase);

            if (baseDirs != null && baseDirs.Length > 0)
            {
                //These do not necessarily exist yet. They are usually created upon demand by the caller.
                //GetFiles() searching will ignore directories that do not exist.
                foreach (var d in baseDirs)
                {
                    if (!d.IsNullOrEmpty()) dirs.Add(d);
                }
            }

            if (dirs.Count > 1)
            {
                //Remove any directory path that is already contained in another.
                for (int j = dirs.Count - 1; j >= 0; j--)
                {
                    dir = dirs[j];
                    for (int i = dirs.Count - 1; i >= 0; i--)
                    {
                        if (i == j) continue;
                        if (dirs[i].Length >= dir.Length && dirs[i].StartsWith(dir)) { dirs.RemoveAt(i); break; }
                    }
                }
            }

            return dirs.ToArray();
        }

        [Serializable] private class TypeFinder : MarshalByRefObject
        {
            public TypeFinder() { }
            public string[] FindTypes(string assemblyfile, string baseType)
            {
                List<string> list = new List<string>();
                try
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(assemblyfile);
                    Type[] ts = asm.GetTypes();
                    foreach (Type t in ts)
                    {
                        if (baseType != null)
                        {
                            Type b = t.BaseType;
                            if (string.Compare(b.FullName, baseType, true) == 0) { list.Add(t.FullName); continue; }
                            Type[] interfaces = t.FindInterfaces(null,null);
                            foreach (Type i in interfaces)
                            {
                                if (string.Compare(i.FullName, baseType) != 0) continue;
                                list.Add(t.FullName);
                                break;
                            }
                        }
                        else list.Add(t.FullName);
                    }
                }
                catch { }
                return list.ToArray();
            }

            public Attribute[] FindAttributes(string assemblyfile, string member)
            {
                List<Attribute> list = new List<Attribute>();
                try
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(assemblyfile);

                    if (member.IsNullOrEmpty())
                    {
                        return asm.GetCustomAttributes().ToArray();
                    }
                    string xclass = string.Empty;
                    int start = member.LastIndexOf('.');
                    if (start > -1)
                    {
                        xclass = member.Substring(0, start);
                        member = member.Substring(start + 1, member.Length-start-1);
                    }

                    Type[] ts;
                    if (xclass.Length == 0)
                    {
                        ts = asm.GetTypes();
                    }
                    else
                    {
                        Type t = asm.GetType(xclass, false, true);
                        ts = (t == null ? new Type[0] : new Type[] { t });
                    }

                    foreach (Type t in ts)
                    {
                        MemberInfo[] mis = t.GetMember(member, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                        foreach (var mi in mis)
                        {
                            list.AddRange(mi.GetCustomAttributes());
                        }
                    }
                }
                catch { }
                return list.ToArray();
            }

            public Object[] FindStaticValues(string assemblyfile, string member)
            {
                List<Object> list = new List<Object>();
                try
                {
                    AppDomain.CurrentDomain.ReflectionOnlyAssemblyResolve += new ResolveEventHandler(CurrentDomain_ReflectionOnlyAssemblyResolve);
                    Assembly asm = Assembly.ReflectionOnlyLoadFrom(assemblyfile);

                    if (member.IsNullOrEmpty()) return list.ToArray();

                    string xclass = string.Empty;
                    int start = member.LastIndexOf('.');
                    if (start > -1)
                    {
                        xclass = member.Substring(0, start);
                        member = member.Substring(start + 1, member.Length - start - 1);
                    }

                    Type[] ts;
                    if (xclass.Length == 0)
                    {
                        ts = asm.GetTypes();
                    }
                    else
                    {
                        Type t = asm.GetType(xclass, false, true);
                        ts = (t == null ? new Type[0] : new Type[] { t });
                    }

                    foreach (Type t in ts)
                    {
                        MemberInfo[] mis = t.GetMember(member, MemberTypes.Field | MemberTypes.Property, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.IgnoreCase);
                        foreach (var mi in mis)
                        {
                            if (mi.MemberType == MemberTypes.Field)
                                list.Add(((FieldInfo)mi).GetValue(null));
                            if (mi.MemberType == MemberTypes.Property && ((PropertyInfo)mi).CanRead)
                                list.Add(((PropertyInfo)mi).GetValue(null));
                        }
                    }
                }
                catch { }
                return list.ToArray();
            }

            private Assembly CurrentDomain_ReflectionOnlyAssemblyResolve(object sender, ResolveEventArgs args)
            {
                Assembly asm = null;
                try { asm = Assembly.ReflectionOnlyLoad(args.Name); return asm; }
                catch { }
                string fullpath = FindAssembly(new AssemblyName(args.Name));
                if (fullpath != null) asm = Assembly.ReflectionOnlyLoadFrom(fullpath);
                return asm;
            }
        }

        private static Log _log = new Log("General");
        private static void Log(string format, params object[] args)
        {
            //We cannot use regular logging because we cannot use 
            //assemblies we may have to load first. This could cause recursion!
            //See notes in Domain_AssemblyResolver()
            DBG.RawWrite(string.Format(format, args)+Environment.NewLine);
            //This now works only because logging and resolver are in the same assembly.
            _log.Information(format, args);
        }
        #endregion
    }

    #region Example Custom Resolver
    /// <summary>
    /// This is example product-specific code to ChuckHill2.Utilities.Resolver initializer delegate. FOR EXAMPLE ONLY.
    /// </summary>
    internal static class MyProductResolver
    {
        /// <summary>
        /// Search and locate the product root directory. This is the 
        /// directory heirarchy root where all files are resolved from.
        /// The logic in this function is entirely unique to a specific product.
        /// </summary>
        /// <param name="productName">Returns the name of root directory component ("e.g. "MyProduct").</param>
        /// <param name="productBase">Returns the root directory path where the product principially resides.</param>
        /// <param name="additionalSearchDirs">Returns an array of fully qualified directory paths that will also be searched...if the directory exists.</param>
        internal static void InitializeProduct(out string productName, out string productBase, out string[] additionalSearchDirs)
        {
            productName = "MyProduct";
            productBase = null;
            additionalSearchDirs = new string[0];
            string path;

            // locate directory where the MyProduct.Services.exe Windows service resides.
            string serviceBase = null;
            try
            {
                serviceBase = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\MyProduct Service", "ImagePath", null) as String;
                if (!serviceBase.IsNullOrEmpty())
                {
                    serviceBase = Path.GetDirectoryName(serviceBase);
                    if (!Directory.Exists(serviceBase)) serviceBase = null;
                }
            }
            catch { }

            string applicationBase = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            string startfilename = Path.GetFileName(applicationBase);
            applicationBase = Path.GetDirectoryName(applicationBase);

            //Check if running in Development Environment
            //Dev Heirarchy: BaseDirectory==C:\SourceCode\MyProduct\bin\Debug - If running from here, we do NOT want to use any production files.
            bool isDevEnv = (applicationBase.EndsWith(@"\bin\Debug", StringComparison.InvariantCultureIgnoreCase) || applicationBase.EndsWith(@"\bin\Release", StringComparison.InvariantCultureIgnoreCase));
            //Dev Test Harness Environment: C:\SourceCode\MyTestHarness\Regex2Identifier\bin\Debug - If running from here, we will allow using production dlls for missing components we are not explicitly testing.
            if (isDevEnv && Directory.EnumerateFiles(applicationBase + @"\..\..", "MyProduct.Services.exe", SearchOption.AllDirectories).FirstOrDefault() == null) isDevEnv = false;
            if (isDevEnv)
            {
                productBase = applicationBase;
                serviceBase = null;  //When running in DEV envronment we assume the Service is NOT installed.
            }
            else
            {
                //OK. We are NOT running in Development Environment
                //Official Release Environment: C:\Program Files\MyProduct\[MyProductService\MyProduct.Services.exe]
                // or "%LOCALAPPDATA%\Apps\2.0\V2D48X3D.EZA\L7ZQPJLK.MG8\pand..tion_e64fe86bf178e344_0013.0000_c0cdf30059a1af52\[MyProduct.exe] (click-once)
                // or C:\Program Files\MyProduct\Client\[MyProduct.exe] (MSI install)
                //Note: The client MyProduct.exe lives in a single monolithic directory.

                if (startfilename.EqualsI("MyProduct.Services.exe") || startfilename.EqualsI("MyProduct.Services.vshost.exe"))
                {
                    //Ignore, for now. We will get to this after we have searched for the client.
                }
                else if (startfilename.EqualsI("MyProduct.exe") || startfilename.EqualsI("MyProduct.vshost.exe"))
                {
                    path = Directory.EnumerateFiles(applicationBase, "MyProduct.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (path != null) { productBase = Path.GetDirectoryName(path); serviceBase = null; }
                }
                else if (applicationBase.StartsWith(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), StringComparison.InvariantCultureIgnoreCase))
                {
                    //Click-Once == client installation. Some other MyProduct client app is running this code.
                    path = Directory.EnumerateFiles(applicationBase, "MyProduct.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (path != null) { productBase = Path.GetDirectoryName(path); serviceBase = null; }
                }
                else if (applicationBase.ContainsI(@"\MyProduct\WSSetup"))
                {
                    //Running directly from WSSetup folder on server? Some other MyProduct client app is running this code.
                    path = Directory.EnumerateFiles(applicationBase, "MyProduct.exe", SearchOption.AllDirectories).FirstOrDefault();
                    if (path != null) { productBase = Path.GetDirectoryName(path); serviceBase = null; }
                }

                //Ok. If productBase is not defined yet, then we assume that "MyProduct.Services.exe or some other app is running on the MyProduct server.

                if (productBase == null) productBase = ConfigurationManager.AppSettings["ApplicationBase"];
                if (productBase != null && !Directory.Exists(productBase)) productBase = null;
                if (productBase == null) productBase = Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Omnicell\MyProduct", "InstallDir", null) as String;
                if (productBase != null && !Directory.Exists(productBase)) productBase = null;
                if (productBase == null && serviceBase != null) //Retrieve it directly from the App.Config
                {
                    path = Path.Combine(serviceBase, "MyProduct.Services.exe");
                    if (File.Exists(path + ".config"))
                    {
                        Configuration conf = ConfigurationManager.OpenExeConfiguration(path);
                        var setting = conf.AppSettings.Settings["ApplicationBase"];
                        if (setting != null) productBase = setting.Value;
                        if (productBase != null) productBase = Path.GetFullPath(productBase);
                        if (!Directory.Exists(productBase)) productBase = null;
                    }
                }
                if (productBase == null && serviceBase != null)
                {
                    path = serviceBase + "\\";
                    int index = path.IndexOf(@"\MyProduct\", StringComparison.InvariantCultureIgnoreCase);
                    if (index != -1) productBase = path.Substring(0, index + 8);
                }
                if (productBase == null)
                {
                    path = applicationBase + "\\";
                    int index = path.IndexOf(@"\MyProduct\", StringComparison.InvariantCultureIgnoreCase);
                    if (index != -1) productBase = path.Substring(0, index + 8);
                    else productBase = applicationBase;
                }

                List<string> dirs = new List<string>();
                //These directories may be created upon demand, so they may not exist...yet.
                path = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
                if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
                path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);
                if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
                path = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFilesX86);
                if (!path.IsNullOrEmpty() && Directory.Exists(path)) dirs.Add(Path.Combine(path, productName));
                additionalSearchDirs = dirs.ToArray();
            }
        }
    }
    #endregion
}

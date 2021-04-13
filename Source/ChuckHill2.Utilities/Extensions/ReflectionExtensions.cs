//--------------------------------------------------------------------------
// <summary>
//   
// </summary>
// <copyright file="ReflectionExtensions.cs" company="Chuck Hill">
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;

/// <summary>
/// Extensions that simplify access of non-public objects and members
/// </summary>
namespace ChuckHill2.Extensions.Reflection
{
    public static class ReflectionExtensions
    {
        /// <summary>
        /// Duplicate the entire graph of any class object. Duplicates nested objects as well. Properly handles recursion. 
        /// Class and all nested classes <b>must</b> be marked as [Serializable] or an exception will occur.
        /// </summary>
        /// <typeparam name="T">Type of this object</typeparam>
        /// <param name="obj">Object to copy</param>
        /// <returns>A completely independent copy of object</returns>
        public static T DeepClone<T>(this T obj) where T : class
        {
            if (obj == null) return null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                ms.Seek(0, SeekOrigin.Begin);
                return (T)bf.Deserialize(ms);
            }
        }

        /// <summary>
        /// Create a shallow copy of any object. Nested class objects
        /// are not duplicated. They are just referenced again.
        /// Unlike DeepClone(), this object does not need to be marked as [Serializable].
        /// </summary>
        /// <typeparam name="T">Type of this object</typeparam>
        /// <param name="obj">Object to copy</param>
        /// <returns>Shallow copy of object</returns>
        public static T ShallowClone<T>(this T obj)
        {
            if (obj == null) return default(T);
            MethodInfo mi = typeof(Object).GetMethod("MemberwiseClone", BindingFlags.Instance | BindingFlags.NonPublic, null, new Type[0], null);
            return (T)mi.Invoke(obj, new object[0]);
        }

        /// <summary>
        /// Test if value or type is an implementation of the specified type.
        /// </summary>
        /// <param name="value">Value or type to check</param>
        /// <param name="t">Type may be a class, interface, or generic interface with or without parameters.</param>
        /// <returns>True if type is implemented</returns>
        public static bool IsImplementationOf(this object value, Type t)
        {
            if (!(value is Type))
            {
                return IsImplementationOf(value.GetType(), t);
            }

            var v = (Type)value;

            if (t.IsGenericType)
            {
                if (t.IsInterface)
                {
                    if (t.GenericTypeArguments.Length > 0)
                    {
                        return v.GetInterfaces().Where(i => i.IsGenericType).Any(i => i == t);
                    }

                    return v.GetInterfaces().Where(i => i.IsGenericType).Any(i => i.GetGenericTypeDefinition() == t);
                }

                if (t.GenericTypeArguments.Length > 0)
                {
                    return v == t;
                }

                return v.GetGenericTypeDefinition() == t;
            }

            if (t.IsInterface)
            {
                return v.GetInterfaces().Where(i => !i.IsGenericType).Any(i => i == t);
            }

            return v.IsSubclassOf(t) || v == t;
        }

        /// <summary>
        /// Get value by reflection from an object.
        /// It may be a field or property, public or private, instance or static.
        /// This function may be chained together to get a nested value.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to retrieve value from. Must not be null.</param>
        /// <param name="membername">Case-sensitive field or property name.</param>
        /// <param name="indices">Optional index for indexed properties.</param>
        /// <returns>Retrieved value or null if field or property not found and readable.</returns>
        public static object GetReflectedValue(this Object obj, string membername, params object[] indices)
        {
            try
            {
                MemberInfo[] mis = GetAllMembers(obj.GetType(), membername, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static);
                if (mis.Length == 0) return null;
                foreach (var mi in mis)
                {
                    if (mi is FieldInfo)
                    {
                        FieldInfo fi = mi as FieldInfo;
                        return fi.GetValue(obj);
                    }
                    else if (mi is PropertyInfo)
                    {
                        PropertyInfo pi = mi as PropertyInfo;
                        if (!pi.CanRead) continue;
                        var iparams = pi.GetIndexParameters();
                        if (iparams.Length != indices.Length) continue;
                        if (iparams.Length == 0) return pi.GetValue(obj);
                        bool match = true;
                        for (int i = 0; i < iparams.Length; i++)
                        {
                            if (iparams[i].ParameterType != indices[i].GetType())
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        return pi.GetValue(obj, indices);
                    }
                }
                return null;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Set value by reflection to an object.
        /// It may be a field or property, public or private, instance or static.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to retrieve value from. Must not be null.</param>
        /// <param name="membername">Case-sensitive field or property name.</param>
        /// <param name="value">value to set</param>
        /// <param name="indices">Optional index for indexed properties.</param>
        /// <returns>True if value successfully set or false if field or property not found or writeable.</returns>
        public static bool SetReflectedValue(this Object obj, string membername, object value, params object[] indices)
        {
            try
            {
                MemberInfo[] mis = GetAllMembers(obj.GetType(), membername, MemberTypes.Field | MemberTypes.Property, BindingFlags.Instance | BindingFlags.Static);
                if (mis.Length == 0) return false;
                foreach (var mi in mis)
                {
                    if (mi is FieldInfo)
                    {
                        FieldInfo fi = mi as FieldInfo;
                        fi.SetValue(obj, value);
                        return true;
                    }
                    else if (mi is PropertyInfo)
                    {
                        PropertyInfo pi = mi as PropertyInfo;
                        if (!pi.CanWrite) continue;
                        var iparams = pi.GetIndexParameters();
                        if (iparams.Length != indices.Length) continue;
                        if (iparams.Length == 0)
                        {
                            pi.SetValue(obj, value);
                            return true;
                        }
                        bool match = true;
                        for (int i = 0; i < iparams.Length; i++)
                        {
                            if (iparams[i].ParameterType != indices[i].GetType())
                            {
                                match = false;
                                break;
                            }
                        }
                        if (!match) continue;
                        pi.SetValue(obj, value, indices);
                        return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Invoke a static method or invoke a constructor to return a constructed object or invoke a static method.
        /// It may be a static method or non-static constructor, public or private.
        /// To handle 'ref' or 'out' arguments, one must explicitly pass a single initialized 
        /// object[] array for the args argument instead of multiple single args. The object[]
        /// array will contain the results upon return.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="typename">Case-sensitive full type name of the object to invoke OR fully qualified assembly name just in case the assembly has not already been loaded into the domain.</param>
        /// <param name="membername">Name of object member to invoke</param>
        /// <param name="args">arguments to pass to method or constructor.</param>
        /// <returns>the constructed object or method return value.</returns>
        public static object InvokeReflectedMethod(string typename, string membername, params object[] args)
        {
            try
            {
                Type t = GetReflectedType(typename);
                if (t == null) return null;
                return InvokeReflectedMethod(t, membername, args);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Invoke a public or private static method or constructor.
        /// To handle 'ref' or 'out' arguments, one must explicitly pass a single initialized 
        /// object[] array for the args argument instead of multiple single args. The object[]
        /// array will contain the results upon return.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="t">Type execute static method on. Must not be null.</param>
        /// <param name="membername">Case-sensitive static method name. If null, must be a constructor to create the object. If the same name as the type, it must be a STATIC constructor.</param>
        /// <param name="args">arguments to pass to method.</param>
        /// <returns>value returned from method or created object if a constructor</returns>
        public static object InvokeReflectedMethod(this Type t, string membername, params object[] args)
        {
            if (t == null) return null;
            try
            {
                var types = GetReflectedArgTypes(args);
                var containsNull = types.Any(m => m == null);
                if (string.IsNullOrEmpty(membername)) //must be a constructor. Not a static constructor.
                {
                    ConstructorInfo ci = null;
                    if (containsNull)
                    {
                        ci = t.GetConstructors(BindingFlags.Instance).FirstOrDefault(m =>
                        {
                            var p = m.GetParameters();
                            if (p.Length != types.Length) return false;
                            for (int i = 0; i < types.Length; i++)
                            {
                                if (types[i] == null) continue;
                                if (types[i] == p[i].ParameterType) continue;
                                return false;
                            }
                            return true;
                        });
                    }
                    else
                    {
                        ci = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                    }
                    if (ci != null) return ci.Invoke(args);
                    return null;
                }

                MethodInfo mi = null;
                if (containsNull) //an argument was null so couldn't determine an arg type.  We just find the nearest match based upon the other args and arg count.
                {
                    mi = (MethodInfo)GetAllMembers(t, null, MemberTypes.Method, BindingFlags.Static).FirstOrDefault(m =>
                    {
                        if (!m.Name.Equals(membername)) return false;
                        var p = ((MethodInfo)m).GetParameters();
                        if (p.Length != types.Length) return false;
                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i] == null) continue;
                            if (types[i] == p[i].ParameterType) continue;
                            return false;
                        }
                        return true;
                    });
                }
                else
                {
                    mi = (MethodInfo)t.GetMethod(membername, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                }
                if (mi != null) return mi.Invoke(null, args);
                return null;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
            catch { return null; }
        }

        /// <summary>
        /// Invoke a public or private instance method.
        /// To handle 'ref' or 'out' arguments, one must explicitly pass a single initialized 
        /// object[] array for the args argument instead of multiple single args. The object[]
        /// array will contain the results upon return.
        /// This function should not be used in tight loops because reflection is expensive.
        /// </summary>
        /// <param name="obj">Object to execute method on. Must not be null.</param>
        /// <param name="membername">Case-sensitive method name.</param>
        /// <param name="args">arguments to pass to method</param>
        /// <returns>value returned from method</returns>
        public static object InvokeReflectedMethod(this Object obj, string membername, params object[] args)
        {
            try
            {
                Type t = obj.GetType();
                var types = GetReflectedArgTypes(args);
                var containsNull = types.Any(m => m == null);

                MethodInfo mi = null;
                if (containsNull) //an argument was null so couldn't determine and arg type. We just find the nearest match based upon the other args and arg count.
                {
                    mi = (MethodInfo)(GetAllMembers(t, null, MemberTypes.Method, BindingFlags.Instance)).FirstOrDefault(m =>
                    {
                        if (!m.Name.Equals(membername)) return false;
                        var p = ((MethodInfo)m).GetParameters();
                        if (p.Length != types.Length) return false;
                        for (int i = 0; i < types.Length; i++)
                        {
                            if (types[i] == null) continue;
                            if (types[i] == p[i].ParameterType) continue;
                            return false;
                        }
                        return true;
                    });
                }
                else
                {
                    mi = (MethodInfo)t.GetMethod(membername, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, types, null);
                }

                if (mi != null) return mi.Invoke(obj, args);
                return null;
            }
            catch (TargetInvocationException ex)
            {
                throw ex.InnerException ?? ex;
            }
            catch { return null; }
        }

        /// <summary>
        /// Determine if the object is of the specified type. Functionally equivalant to (myobject is IEnumerable).
        /// </summary>
        /// <param name="tMain">Type that may be parent of 'typename'</param>
        /// <param name="typename">Assembly-qualified type name.
        ///    Case-insensitive, full type name of the object to invoke OR fully 
        ///    qualified assembly name just in case the assembly has not already 
        ///    been loaded into the domain.
        /// </param>
        /// <returns>True if 'typename' is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool MemberIs(this Type tMain, string typename)
        {
            if (tMain == null) return false;
            Type t = GetReflectedType(typename);
            if (t == null) return false;
            return (t.IsAssignableFrom(tMain));
        }

        /// <summary>
        /// Determine if the object is of the specified type. Functionally equivalant to (myobject is IEnumerable).
        /// </summary>
        /// <param name="tMain">Type that may be parent of 'typename'</param>
        /// <param name="isType">Type of object it is.</param>
        ///    Case-insensitive, full type name of the object to invoke OR fully 
        ///    qualified assembly name just in case the assembly has not already 
        ///    been loaded into the domain.
        /// </param>
        /// <returns>True if 'isType' is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool MemberIs(this Type tMain, Type isType)
        {
            if (tMain == null || isType == null) return false;
            return (isType.IsAssignableFrom(tMain));
        }

        /// <summary>
        /// Determine if the object is of the specified type. Functionally equivalant to (myobject is IEnumerable) except type is determined at runtime, not compile type.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <param name="typename">Assembly-qualified type name.
        ///    Case-insensitive, full type name of the object to invoke OR fully 
        ///    qualified assembly name just in case the assembly has not already 
        ///    been loaded into the domain.
        /// </param>
        /// <returns>True if 'typename' is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool MemberIs(this Object obj, string typename)
        {
            if (obj == null) return false;
            Type t = GetReflectedType(typename);
            if (t == null) return false;
            return (t.IsAssignableFrom(obj.GetType()));
        }

        /// <summary>
        /// Determine if the object is of the specified type. Functionally equivalant to (myobject is IEnumerable) except type is determined at runtime, not compile type.
        /// </summary>
        /// <param name="obj">Object to test.</param>
        /// <param name="isType">Type of object it is.</param>
        /// <returns>True if 'isType' is the class or contains the base class or interface of. False if not, or typename cannot be found</returns>
        public static bool MemberIs(this Object obj, Type isType)
        {
            if (obj == null || isType == null) return false;
            return (isType.IsAssignableFrom(obj.GetType()));
        }

        /// <summary>
        /// Get specified public or private type.
        /// </summary>
        /// <param name="typename">
        ///    Case-sensitive full name of the type to get.<br />
        ///    example: "System.Windows.Forms.Layout.TableLayout+ContainerInfo"
        /// </param>
        /// <param name="relatedType">Optional known public type that is in the same assembly as 'typename'.</param>
        /// <returns>Found type or null if not found</returns>
        /// <remarks>
        ///    When 'relatedType' is defined, this method is effectively the same as:
        ///    \code{.cs}
        ///    Type t = Type.GetType("System.Windows.Forms.Layout.TableLayout+ContainerInfo, " + typeof(TableLayoutPanel).Assembly.FullName, false, false);
        ///    \endcode
        ///    If 'relatedType' is undefined, this method will search the currently loaded assemblies for a match.
        /// </remarks>
        public static Type GetReflectedType(string typename, Type relatedType = null)
        {
            if (typename.IsNullOrEmpty()) return null;

            Type t = Type.GetType(typename, false, false);

            if (t == null && relatedType != null)
            {
                t = Type.GetType($"{typename}, {relatedType.Assembly.FullName}", false, false);
            }

            if (t == null) //Ok. Hunt for it the hard way, assuming the assembly is already loaded.
            {
                var elements = typename.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                if (elements.Length > 1) return null;
                typename = elements[0];
                t = AppDomain.CurrentDomain.GetAssemblies()
                    .Where(a => !a.IsDynamic)
                    .SelectMany(a => a.GetTypes())
                    .FirstOrDefault(m => m.FullName.Equals(typename, StringComparison.InvariantCultureIgnoreCase));
                if (t == null) return null;
            }

            return t;
        }

        /// <summary>
        /// Handy utility for getting array of arg types for invoking methods
        /// </summary>
        /// <param name="args">array of args</param>
        /// <returns>Always returns a Type array</returns>
        private static Type[] GetReflectedArgTypes(params object[] args)
        {
            Type[] types;
            if (args == null || args.Length == 0) return new Type[0];
            types = new Type[args.Length];
            for (int i = 0; i < types.Length; i++) { types[i] = (args[i] == null ? null : args[i].GetType()); }
            return types;
        }

        /// <summary>
        /// Retrieve ALL members public/private from the entire heirarchy. Type.GetMembers() 
        /// does not traverse the type heirarchy for private or static members. Go figure....
        /// </summary>
        /// <param name="t">The type to retrieve the members from</param>
        /// <param name="membername">Name of member to retrieve. There may be more than one in the heirarchy. OR null to get all members.</param>
        /// <param name="membertypes">The specified types of members to retrieve</param>
        /// <param name="bf">The binding flags used to retrieve the members</param>
        /// <returns></returns>
        private static MemberInfo[] GetAllMembers(Type t, string membername, MemberTypes membertypes, BindingFlags bf)
        {
            var members = new List<MemberInfo>();

            if (membername == string.Empty) membername = null;
            bool declaredOnly = ((bf & BindingFlags.DeclaredOnly) != 0); //in case it is a 'new' property so don't dive deep into the heirarchy.
            bf = ~(~bf | BindingFlags.FlattenHierarchy) | BindingFlags.DeclaredOnly; //remove these flages because we recurse the heirarchy.
            bf |= BindingFlags.Public | BindingFlags.NonPublic;  //always search all public, private, protected, and internal members 
            while (t != null)
            {
                var m1 = t.GetMembers(bf);
                var m2 = m1.Where(m => membername != null
                                       ? (membername=="Item"
                                            ? m.Name.EndsWith(".Item") && (m.MemberType & membertypes) != 0
                                            : m.Name == membername && (m.MemberType & membertypes) != 0
                                         )
                                       : (m.MemberType & membertypes) != 0
                                 );
                members.AddRange(m2);
                t = declaredOnly ? null : t.BaseType;
            }

            return members.ToArray();
        }

        #region Debugging tool: public static string[] GetReflectedObjectMembers(this Object obj)
        /// <summary>
        /// Get formatted list of members of a specified type; Great for debugging.
        /// </summary>
        /// <param name="typename">Type name as string</param>
        /// <returns>Formatted list of members</returns>
        public static string[] GetReflectedObjectMembers(string typename)
        {
            Type t = GetReflectedType(typename);
            if (t == null) return null;
            return GetReflectedObjectMembers(t);
        }

        /// <summary>
        /// Get list of All members in the object heirarchy displayed in C# format.
        /// </summary>
        /// <param name="obj">Object to enumerate</param>
        /// <returns>Array of members with their current values</returns>
        public static string[] GetReflectedObjectMembers(this Object obj)
        {
            return GetReflectedObjectMembers(obj.GetType(), obj);
        }

        /// <summary>
        /// Get list of All members in the type heirarchy with object values displayed in C# format.
        /// </summary>
        /// <param name="t">Type of object to enumerate</param>
        /// <param name="obj">Object containing values to display</param>
        /// <returns>Array of members with their current values</returns>
        public static string[] GetReflectedObjectMembers(this Type t, Object obj = null)
        {

            MemberInfo[] mis = GetAllMembers(t, null, MemberTypes.All, BindingFlags.Instance | BindingFlags.Static);
            List<string> members = new List<string>(mis.Length);
            foreach (var mi in mis)
            {
                switch (mi.MemberType)
                {
                    case MemberTypes.Constructor: members.Add(GetConstructorDeclaration(mi)); break;
                    case MemberTypes.Event: members.Add(GetEventDeclaration(mi)); break;
                    case MemberTypes.Field: members.Add(GetFieldDeclaration(mi, obj)); break;
                    case MemberTypes.Method: members.Add(GetMethodDeclaration(mi)); break;
                    case MemberTypes.Property: members.Add(GetPropertyDeclaration(mi, obj)); break;
                    case MemberTypes.TypeInfo: members.Add(GetTypeDeclaration(mi)); break;
                    default: members.Add(GetUnknownDeclaration(mi)); break;
                }
            }
            return members.ToArray();
        }
        #region GetReflectedObjectMembers(Type) private methods
        private static string GetConstructorDeclaration(MemberInfo mi)
        {
            var m = mi as ConstructorInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            //if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.DeclaringType.Name); //sb.Append(m.Name);
            sb.Append('(');
            string comma = "";
            foreach (ParameterInfo p in m.GetParameters())
            {
                sb.Append(comma);
                if (p.IsIn && p.IsOut) sb.Append("ref ");
                if (!p.IsIn && p.IsOut) sb.Append("out ");
                sb.Append(MakeName(p.ParameterType));
                if (p.HasDefaultValue)
                {
                    sb.Append(" = ");
                    sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                }
                comma = ", ";
            }
            sb.Append(')');
            return sb.ToString();
        }
        private static string GetEventDeclaration(MemberInfo mi)
        {
            var m = mi as EventInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            MethodInfo ac1 = m.GetAddMethod(true);
            MethodInfo ac2 = m.GetRemoveMethod(true);
            if (ac1 == null) ac1 = ac2;
            if (ac2 == null) ac2 = ac1;
            if (ac1.IsPublic && ac2.IsPublic) sb.Append("public ");
            else if (ac1.IsAssembly && ac2.IsAssembly) sb.Append("internal ");
            else if (ac1.IsFamily && ac2.IsFamily) sb.Append("protected ");
            else if (ac1.IsPrivate && ac2.IsPrivate) sb.Append("private ");
            if (ac1.IsStatic && ac2.IsStatic) sb.Append("static ");
            if ((ac1.IsVirtual && !ac1.IsFinal) || (ac2.IsVirtual && !ac2.IsFinal)) sb.Append("virtual ");
            if ((ac1.IsVirtual && ac1.IsFinal) || (ac2.IsVirtual && ac2.IsFinal)) sb.Append("override ");

            sb.Append("event ");
            sb.Append(MakeName(m.EventHandlerType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            return sb.ToString();
        }
        private static string GetFieldDeclaration(MemberInfo mi, Object obj = null)
        {
            var m = mi as FieldInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            //if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            //if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(MakeName(m.FieldType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            if (obj != null)
            {
                sb.Append(" = ");
                object value = m.GetValue(obj);
                if (value == null) sb.Append("null");
                else
                {
                    string quote = (m.FieldType == typeof(string) ? "\"" : "");
                    sb.Append(quote);
                    try { sb.Append(m.GetValue(obj).ToString()); }
                    catch { }
                    sb.Append(quote);
                }
            }
            return sb.ToString();
        }
        private static string GetMethodDeclaration(MemberInfo mi)
        {
            var m = mi as MethodInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            if (m.IsSpecialName) sb.Append("[special] ");
            if (m.IsPublic) sb.Append("public ");
            else if (m.IsAssembly) sb.Append("internal ");
            else if (m.IsFamily) sb.Append("protected ");
            else if (m.IsPrivate) sb.Append("private ");
            if (m.IsStatic) sb.Append("static ");
            if (m.IsVirtual && !m.IsFinal) sb.Append("virtual ");
            if (m.IsVirtual && m.IsFinal) sb.Append("override ");
            sb.Append(MakeName(m.ReturnType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);
            sb.Append('(');
            string comma = "";
            foreach (ParameterInfo p in m.GetParameters())
            {
                sb.Append(comma);
                if (p.IsIn && p.IsOut) sb.Append("ref ");
                if (!p.IsIn && p.IsOut) sb.Append("out ");
                sb.Append(MakeName(p.ParameterType));
                if (p.HasDefaultValue)
                {
                    sb.Append(" = ");
                    sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                }
                comma = ", ";
            }
            sb.Append(')');
            return sb.ToString();
        }
        private static string GetPropertyDeclaration(MemberInfo mi, Object obj = null)
        {
            var m = mi as PropertyInfo;
            if (m == null) return GetUnknownDeclaration(mi);
            var sb = new StringBuilder();

            MethodInfo ac1 = m.GetGetMethod(true);
            MethodInfo ac2 = m.GetSetMethod(true);
            if (ac1 == null) ac1 = ac2;
            if (ac2 == null) ac2 = ac1;
            if (ac1.IsPublic && ac2.IsPublic) sb.Append("public ");
            else if (ac1.IsAssembly && ac2.IsAssembly) sb.Append("internal ");
            else if (ac1.IsFamily && ac2.IsFamily) sb.Append("protected ");
            else if (ac1.IsPrivate && ac2.IsPrivate) sb.Append("private ");
            if (ac1.IsStatic && ac2.IsStatic) sb.Append("static ");
            if ((ac1.IsVirtual && !ac1.IsFinal) || (ac2.IsVirtual && !ac2.IsFinal)) sb.Append("virtual ");
            if ((ac1.IsVirtual && ac1.IsFinal) || (ac2.IsVirtual && ac2.IsFinal)) sb.Append("override ");

            sb.Append(MakeName(m.PropertyType));
            sb.Append(' ');
            sb.Append(m.DeclaringType.Name);
            sb.Append('.');
            sb.Append(m.Name);

            ParameterInfo[] parameters = m.GetIndexParameters();
            if (parameters.Length > 0)
            {
                sb.Append("[");
                string comma = "";
                foreach (ParameterInfo p in m.GetIndexParameters())
                {
                    sb.Append(comma);
                    if (p.IsIn && p.IsOut) sb.Append("ref ");
                    if (!p.IsIn && p.IsOut) sb.Append("out ");
                    sb.Append(MakeName(p.ParameterType));
                    if (p.HasDefaultValue)
                    {
                        sb.Append(" = ");
                        sb.Append(p.DefaultValue.GetType() == typeof(string) ? "\"" + p.DefaultValue.ToString() + "\"" : p.DefaultValue.ToString());
                    }
                    comma = ", ";
                }
                sb.Append(']');
            }

            sb.Append(" { ");
            if (m.CanRead) sb.Append("get; ");
            if (m.CanWrite) sb.Append("set; ");
            sb.Append("}");

            if (obj != null && m.CanRead && parameters.Length == 0)
            {
                sb.Append(" = ");

                object value = null;
                try { value = m.GetValue(obj); }
                catch { }
                if (value == null) sb.Append("null");
                else
                {
                    string quote = (m.PropertyType == typeof(string) ? "\"" : "");
                    sb.Append(quote);
                    try { sb.Append(m.GetValue(obj).ToString()); }
                    catch { }
                    sb.Append(quote);
                }
            }

            return sb.ToString();
        }
        private static string GetTypeDeclaration(MemberInfo mi)
        {
            var m = mi as TypeInfo;
            return GetUnknownDeclaration(mi);
        }
        private static string GetUnknownDeclaration(MemberInfo mi)
        {
            return string.Format("[{0}] {1}.{2}", mi.MemberType, mi.DeclaringType.Name, mi.Name);
        }
        private static string MakeName(Type t)
        {
            if (t == null) return "void";
            var sb = new StringBuilder();
            int i = t.Name.IndexOf('`');
            sb.Append(i >= 0 ? t.Name.Substring(0, i) : t.Name);
            if (t.IsGenericType)
            {
                sb.Append('<');
                string comma = "";
                foreach (Type arg in t.GetGenericArguments())
                {
                    sb.Append(comma);
                    sb.Append(MakeName(arg));
                    comma = ", ";
                }
                sb.Append('>');
            }
            return sb.ToString();
        }
        #endregion
        #endregion
    }
}

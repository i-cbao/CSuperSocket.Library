
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Linq;
using Microsoft.Extensions.DependencyModel;
using System.IO;

namespace Dynamic.Core.Extensions
{
    public static class ReflectionExtensions
    {
        public static bool IsSubclassOf(this Type subtype, Type baseType)
        {
            bool isSubClass = false;
            isSubClass = subtype.GetTypeInfo().IsSubclassOf(baseType);
            return isSubClass;
        }
        public static bool HaveCustomAttributes<A>(this PropertyInfo info) where A : Attribute
        {
            var attr = info.GetCustomAttribute<A>();
            return attr != null;
        }
        public static object[] GetCustomAttributes(this Type t, Type mustHasAttrType, bool inherit)
        {
            var attArr = t.GetTypeInfo().GetCustomAttributes(mustHasAttrType, inherit);
            return attArr;
        }
        public static bool IsInterface(this Type t)
        {
            var issInterface = t.GetTypeInfo().IsInterface;
            return issInterface;
        }
        public static bool IsEnum(this Type t)
        {
            var IsEnum = t.GetTypeInfo().IsEnum;
            return IsEnum;
        }
        public static bool IsClass(this Type t)
        {
            var IsClass = t.GetTypeInfo().IsClass;
            return IsClass;
        }
        public static bool IsPrimitive(this Type t)
        {
            var IsPrimitive = t.GetTypeInfo().IsPrimitive;
            return IsPrimitive;
        }
        public static PropertyInfo[] GetProperties(this Type t, string assemblyFileName)
        {
            return t.GetTypeInfo().GetProperties();
        }
        public static bool IsValueType(this Type t)
        {
            var IsValueType = t.GetTypeInfo().IsValueType;
            return IsValueType;
        }
        public static bool IsGenericType(this Type t)
        {
            var IsGenericType = t.GetTypeInfo().IsGenericType;
            return IsGenericType;
        }

        public static Assembly LoadFile(string assemblyFileFullName)
        {
            // var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFileFullName)
            var rtnAssembly = AssemblyLoader.Default.LoadFromAssemblyPath(assemblyFileFullName);
            return rtnAssembly;
        }
        public static Assembly LoadFrom(string assemblyFileName)
        {
            // var myAssembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyFileFullName)
            var rtnAssembly = AssemblyLoader.Default.LoadFromAssemblyName(new AssemblyName(assemblyFileName));
            return rtnAssembly;
        }
    }
    public class AssemblyLoader : AssemblyLoadContext
    {
        protected override Assembly Load(AssemblyName assemblyName)
        {
            var deps = DependencyContext.Default;
            var res = deps.CompileLibraries.Where(d => d.Name.Contains(assemblyName.Name)).ToList();
            var assembly = Assembly.Load(new AssemblyName(res.First().Name));
            return assembly;
        }
        /// <summary>
        /// 动态加载dllpath
        /// </summary>
        /// <param name="dllPath"></param>
        public static Assembly TryLoadAssembly(string dllPath)
        {
            Assembly entry = Assembly.GetEntryAssembly();          
            string entryName = entry.GetName().Name;
            if (entryName.Equals(Path.GetFileNameWithoutExtension(dllPath))) return null;
            return  AssemblyLoadContext.Default.LoadFromAssemblyPath(dllPath);
        }
        public static T GetInstance<T>(string assemblyName, string typeName)
        {
            Assembly assembly = Assembly.Load(new AssemblyName(assemblyName));
            Type type = assembly.GetType(string.Format("{0}.{1}", assemblyName, typeName));
            var instance = (T)Activator.CreateInstance(type);
            return instance;
        }
        /// <summary>
        /// 动态加载，当前执行目录下的所有dll（解决dotnet core依赖dll无法加载的bug）
        /// </summary>
        public static void TryCurrentExeAssembly()
        {
            Assembly entry = Assembly.GetEntryAssembly();
            //找到当前执行文件所在路径
            string dir = Path.GetDirectoryName(entry.Location);
            string entryName = entry.GetName().Name;
            //获取执行文件同一目录下的其他dll
            foreach (string dll in Directory.GetFiles(dir, "*.dll"))
            {
                if (entryName.Equals(Path.GetFileNameWithoutExtension(dll))) { continue; }
                //非程序集类型的关联load时会报错
                try
                {
                    AssemblyLoadContext.Default.LoadFromAssemblyPath(dll);
                }
                catch (Exception ex)
                {
                    
                }
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Dynamic.Core.Runtime
{
    /// <summary>
    /// 反射辅助类
    /// </summary>
    public static class ReflectionHelper
    {
        /// <summary>
        /// 从指定程序集中获取类型
        /// </summary>
        /// <param name="assemblyFile">程序集路径</param>
        /// <param name="baseType">基类</param>
        /// <param name="mustHasAttrType">必须具有的特性</param>
        /// <param name="addtionalCheck">自定义的附加检查</param>
        /// <param name="useLoadFile">
        /// 是否使用Assembly.LoadFile载入程序集，使用LoadFile时可以载入相同标识但路径不同的程序集，
        /// 否则使用LoadFrom载入程序集，此时在载入多个相同标识的程序集时只会载入第一个程序集
        /// </param>
        /// <returns>类型列表</returns>
        public static IEnumerable<Type> GetTypeFromAssembly(string assemblyFile, Type baseType, Type mustHasAttrType, Func<Type, bool> addtionalCheck, bool useLoadFile)
        {
            List<Type> types = new List<Type>();

            if (!File.Exists(assemblyFile))
            {
                return types;
            }

            Assembly assembly = null;
            if (useLoadFile)
            {
                assembly = Assembly.LoadFile(assemblyFile);
            }
            else
            {
                assembly = Assembly.LoadFrom(assemblyFile);
            }
            assembly.ModuleResolve += new ModuleResolveEventHandler(assembly_ModuleResolve);

            return GetTypeFromAssembly(assembly, baseType, mustHasAttrType, addtionalCheck);

        }


        public static IEnumerable<Type> GetTypeFromAssembly(Assembly assembly, Type baseType, Type mustHasAttrType, Func<Type, bool> addtionalCheck)
        {
            List<Type> types = new List<Type>();



            #region 类型检查
            Func<Type, bool> checkType = null;
            if (baseType == null)
            {
                checkType = new Func<Type, bool>((t) =>
                {
                    return true;
                });
            }
            else if (baseType.IsInterface)
            {
                checkType = new Func<Type, bool>((t) =>
                {
                    return baseType.IsAssignableFrom(t);
                });
            }
            else
            {
                checkType = new Func<Type, bool>((t) =>
                {
                    return t.IsSubclassOf(baseType);
                });
            }
            #endregion

            #region 特性检查
            Func<Type, bool> checkAttr = null;
            if (mustHasAttrType == null)
            {
                checkAttr = new Func<Type, bool>((t) =>
                {
                    return true;
                });
            }
            else
            {
                checkAttr = new Func<Type, bool>((t) =>
                {
                    return t.GetCustomAttributes(mustHasAttrType, false).Any();
                });
            }
            #endregion

            if (addtionalCheck == null)
            {
                addtionalCheck = new Func<Type, bool>((t) =>
                {
                    return true;
                });
            }

            try
            {
               
                Type[] allType = assembly.GetTypes();
                foreach (Type type in allType)
                {
                    if (checkType(type) && checkAttr(type) && addtionalCheck(type))
                    {
                        types.Add(type);
                    }
                }
            }
            catch
            {

            }


            return types;
        }

        static Module assembly_ModuleResolve(object sender, ResolveEventArgs e)
        {
            return null;
        }
    }
}

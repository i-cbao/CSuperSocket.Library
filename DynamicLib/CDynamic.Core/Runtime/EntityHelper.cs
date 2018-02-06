using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Dynamic.Core
{
    public static class EntityHelper 
    {
        public static TEntity CloneEntity<TEntity>(this TEntity obj)
        {
            if (obj == null)
            {
                return obj;
            }

            TEntity target = (TEntity)CopyFrom(obj.GetType(), obj, new Dictionary<object, object>());

            return target;
        }

        public static TEntity CopyFrom<TEntity>(this TEntity target, object source)
        {
            Type t = typeof(TEntity);
            if (target != null)
            {
                t = target.GetType();
            }
            if (source == null)
                return target;
            Dictionary<object, object> mapper = new Dictionary<object, object>();
            mapper.Add(source, target);

            Type st = source.GetType();
            PropertyInfo[] pis = t.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            PropertyInfo[] spis = st.GetProperties(BindingFlags.Instance | BindingFlags.Public);

            foreach (PropertyInfo p in pis)
            {
                if (p.CanRead && p.CanWrite)
                {
                    PropertyInfo sp = spis.FirstOrDefault(x => x.Name == p.Name);
                    if (sp != null)
                    {
                        object val = CopyFrom(p.PropertyType, sp.GetValue(source, null), mapper);
                        p.SetValue(target, val, null);
                    }
                }
            }

            return target;
        }


        private static object CopyFrom(Type targetType, object source, Dictionary<object, object> mapper )
        {
            object target = null;
            if (source == null)
            {
                return target;
            }

            if (mapper.ContainsKey(source))
            {
                return mapper[source];
            }

            Type st = source.GetType();
            if (typeof(ICloneable).IsAssignableFrom(targetType) && st == targetType)
            {
                target = (source as ICloneable).Clone();
            }
            else if (targetType.IsPrimitive || targetType.IsValueType)
            {
                target = Activator.CreateInstance(targetType);
                if (st == targetType)
                {
                    target = source;
                }
                else if (targetType.IsEnum)
                {
                    string enumStr = "";
                    if (st.IsEnum)
                    {
                        enumStr = source.ToString();
                    }
                    else if (source is String)
                    {
                        enumStr = source as string;
                    }
                    if (!String.IsNullOrEmpty(enumStr))
                    {
                        try
                        {
                            target = Enum.Parse(targetType, enumStr);
                        }
                        catch { }
                    }
                }
            }
            else
            {


                PropertyInfo lenProperty = null;
                if (targetType.IsArray)
                {
                    int len = GetArrayOrListLength(source);
                    ConstructorInfo ci = targetType.GetConstructor(new Type[] { typeof(int) });
                    target = ci.Invoke(new object[] { len });
                    CopyArray(target, targetType.GetElementType(), source, mapper);
                }
                else if (typeof(IList).IsAssignableFrom(targetType))
                {
                    Type elementType = null;
                    if (targetType.IsGenericType)
                    {
                        elementType = targetType.GetGenericArguments()[0];
                    }

                    target = Activator.CreateInstance(targetType);
                    IEnumerable e = source as IEnumerable;
                    foreach (object item in e)
                    {
                        object newItem = null;
                        if (item != null)
                        {
                            if (elementType == null)
                            {
                                newItem = CopyFrom(item.GetType(), item, mapper);
                            }
                            else
                            {
                                newItem = CopyFrom(elementType, item, mapper);
                            }
                        }

                        (target as IList).Add(newItem);

                    }
                }
                else
                {
                    target = Activator.CreateInstance(targetType);
                    PropertyInfo[] pis = targetType.GetProperties(BindingFlags.Public | BindingFlags.Instance);
                    PropertyInfo[] spis = st.GetProperties(BindingFlags.Instance | BindingFlags.Public);

                    foreach (PropertyInfo p in pis)
                    {
                        if (p.CanRead && p.CanWrite)
                        {
                            PropertyInfo sp = spis.FirstOrDefault(x => x.Name == p.Name);
                            if (sp != null)
                            {
                                object val = CopyFrom(p.PropertyType, sp.GetValue(source, null), mapper);
                                p.SetValue(target, val, null);
                            }
                        }
                    }

                }
            }
            mapper.Add(source, target);
            return target;
        }

        private static int GetArrayOrListLength(object source)
        {
            IEnumerable e = source as IEnumerable;
            if (e != null)
            {
                int len = 0;
                foreach (object i in e)
                {
                    len++;
                }
                return len;
            }
            return 0;
        }

        private static void CopyArray(object target, Type elementType, object source, Dictionary<object, object> mapper)
        {
            IEnumerable e = source as IEnumerable;
            if (e != null)
            {
                int idx = 0;
                foreach (object item in e)
                {
                    object itemVal = CopyFrom(elementType, item, mapper);
                    (target as Array).SetValue(itemVal, idx);

                    idx++;
                }
            }
        }

        

       
    }
}

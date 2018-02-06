using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using Dynamic.Core.Runtime;
using System.Collections;

namespace Dynamic.Core.Runtime
{
    public class PropertyCollection<TEntity> : IEnumerable<RuntimePropertyInfo>
       where TEntity : class
    {
        private List<RuntimePropertyInfo> propertyList = new List<RuntimePropertyInfo>();

        private static List<Type> ignoreTypes = new List<Type>()
        {
            typeof(String), typeof(Nullable),typeof(IEnumerable)
        };

        public PropertyCollection()
        {
        }

        public void Add<TMember>(Expression<Func<TEntity, TMember>> field)
        {
            if (field != null)
            {
                RuntimePropertyInfo pi = getPropertyInfo(field);
                propertyList.Add(pi);
            }
        }

        public void Add(RuntimePropertyInfo propertyInfo)
        {
            if (propertyInfo != null)
            {
                propertyList.Add(propertyInfo);
            }
        }

        public void Remove<TMember>(Expression<Func<TEntity, TMember>> field)
        {
            if (field != null)
            {
                RuntimePropertyInfo pi = getPropertyInfo(field);
                propertyList.Remove(pi);
            }
        }

        public void Remove(RuntimePropertyInfo propertyInfo)
        {
            if (propertyInfo != null)
            {
                propertyList.Remove(propertyInfo);
            }
        }


        public void Clear()
        {
            propertyList.Clear();
        }

        public int Count
        {
            get { return propertyList.Count; }
        }

        public RuntimePropertyInfo this[int index]
        {
            get
            {
                return propertyList[index];
            }
        }

        /// <summary>
        /// 将实例中符合指定条件的属性放入属性列表
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="checkProc"></param>
        /// <returns></returns>
        public static PropertyCollection<T> GetProperties<T>(T obj, Func<RuntimePropertyInfo, object, bool> checkProc)
            where T : class, new()
        {
            PropertyCollection<T> propertyList = new PropertyCollection<T>();

            getProperties<T>(typeof(T), obj, checkProc, propertyList, null, new List<object>());

            return propertyList;
        }

        /// <summary>
        /// 获取值不为空的属性列表
        /// </summary>
        /// <param name="obj">对象实例</param>
        /// <returns></returns>
        public static PropertyCollection<T> GetNotNullProperties<T>(T obj)
            where T: class, new()
        {
            return GetProperties<T>(obj, (p, o) => { return o != null; });
        }

        /// <summary>
        /// 递归检查符合条件的属性
        /// </summary>
        /// <param name="entityType">实例类型</param>
        /// <param name="obj">实例</param>
        /// <param name="checkProc">检查委托</param>
        /// <param name="collection">属性集合</param>
        /// <param name="parentProperty">上级属性</param>
        /// <param name="refObjects">已检查的实例列表，防止无限递归</param>
        private static void getProperties<T>(Type entityType, object obj, Func<RuntimePropertyInfo, object, bool> checkProc, PropertyCollection<T> collection, RuntimePropertyInfo parentProperty, List<object> refObjects)
            where T: class, new()
        {
            refObjects.Add(obj);

            PropertyInfo[] pis = entityType.GetProperties();

            Func<RuntimePropertyInfo, object, bool> actualCheckProc = checkProc;
            if (actualCheckProc == null)
            {
                actualCheckProc = (p, o) => { return true; };
            }

            bool isPass = false;
            foreach (PropertyInfo pi in pis)
            {
                if (!pi.CanRead)
                {
                    continue;
                }
                object val = null;
                if (obj != null)
                {
                    val = pi.GetValue(obj, null);
                }

                RuntimePropertyInfo curProperty = new RuntimePropertyInfo(pi);
                curProperty.ParentProperty = parentProperty;

                isPass = actualCheckProc(curProperty, val);

                if (isPass)
                {
                    collection.Add(curProperty);
                }

                //递归子级对象
                if (pi.PropertyType.IsClass && val != null && !refObjects.Any(x => x == val)
                    )
                {
                    bool goOn = true;
                    foreach (Type t in ignoreTypes)
                    {
                        if (pi.PropertyType == t ||
                            pi.PropertyType.IsSubclassOf(t))
                        {
                            goOn = false;
                            break;
                        }

                        if (t.IsInterface && t.IsAssignableFrom(pi.PropertyType))
                        {
                            goOn = false;
                            break;
                        }
                    }
                    if (goOn)
                    {
                        getProperties(pi.PropertyType, val, actualCheckProc, collection, curProperty, refObjects);
                    }
                }

            }

            

           
        }


        private RuntimePropertyInfo getPropertyInfo<TMember>(Expression<Func<TEntity, TMember>> selector)
        {
            return RuntimePropertyInfo.GetPropertyInfo(selector);
        }

        #region IEnumerable<PropertyInfo> 成员

        public IEnumerator<RuntimePropertyInfo> GetEnumerator()
        {
            return propertyList.GetEnumerator();
        }

        #endregion

        #region IEnumerable 成员

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return propertyList.GetEnumerator();
        }

        #endregion
    }
}

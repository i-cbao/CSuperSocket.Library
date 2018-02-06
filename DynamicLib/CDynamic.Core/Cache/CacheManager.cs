using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Microsoft.Extensions.Caching.Memory;
using CacheManager.Core;

namespace Dynamic.Core.Cache
{
    /// <summary>
    /// 缓存管理类
    /// </summary>
    public static class CacheManagerUnity
    {
        private static readonly string defaultCacheName = "defaultCache";

        private static ICacheManager<object> innerCacheManager = null;

        static CacheManagerUnity()
        {
            
            init();
        }

        private static void init()
        {
            innerCacheManager= CacheFactory.Build(defaultCacheName, settings => {
                settings.WithMicrosoftMemoryCacheHandle();
            });
        }

        #region 添加对象到缓存
        /// <summary>
        /// 将对象添加到缓存中，使用此方法所添加的对象将不会过期，除非你显示第从缓存中移除掉
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        public static void Add(string cacheKey, object cacheObject)
        {
            Add(null, cacheKey, cacheObject);
        }


        /// <summary>
        /// 将对象添加到缓存中，缓存中的对象将在指定的毫秒后过期
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="persistMilliseconds">过期时间(毫秒)</param>
        public static void Add(string cacheKey, object cacheObject, int persistMilliseconds)
        {
            Add(null, cacheKey, cacheObject, persistMilliseconds);
        }


        /// <summary>
        /// 将对象添加到缓存中，缓存中的对象将在指定的时间后过期
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="persistTime">过期时间</param>
        public static void Add(string cacheKey, object cacheObject, TimeSpan persistTime)
        {
            Add(null, cacheKey, cacheObject, persistTime);
        }


        /// <summary>
        /// 将对象添加到缓存中，缓存中的对象将在指定时间点后过期
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="expiredTime">过期时间点</param>
        public static void Add(string cacheKey, object cacheObject, DateTime expiredTime)
        {
            Add(null, cacheKey, cacheObject, expiredTime);
        }


        /// <summary>
        /// 将对象添加到缓存中，该对象与指定的文件关联，如果指定的文件被修改，缓存将失效
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="dependenceFileName">与缓存对象关联的文件路径</param>
        public static void Add(string cacheKey, object cacheObject, string dependenceFileName)
        {
            Add(null, cacheKey, cacheObject, dependenceFileName);
        }


        /// <summary>
        /// 将对象添加到缓存中，该对象与指定的文件关联，如果指定的文件被修改，缓存将失效
        /// 或者缓存到达指定的时间后亦将失效
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="dependenceFileName">关联的文件路径</param>
        /// <param name="maxPersistTime">缓存可被保留的最长时间</param>
        public static void Add(string cacheKey, object cacheObject, string dependenceFileName, TimeSpan maxPersistTime)
        {
            Add(null, cacheKey, cacheObject, dependenceFileName, maxPersistTime);
        }


        /// <summary>
        /// 将对象添加到缓存中，对象将不会自动失效，除非你显示地从缓存中移除对象
        /// </summary>
        /// <param name="cacheClassification">缓存的类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject)
        {
            innerCacheManager.Add(getCacheKey(cacheClassification, cacheKey),
                cacheObject);
        }

        /// <summary>
        /// 将对象添加到缓存中,缓存将在指定的毫秒时间后失效
        /// </summary>
        /// <param name="cacheClassification">缓存的类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="persistMilliseconds">缓存时间(毫秒)</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject, int persistMilliseconds)
        {
            Add(cacheClassification,cacheKey,cacheObject, TimeSpan.FromMilliseconds(persistMilliseconds));
        }


        /// <summary>
        /// 将对象添加到缓存中,缓存将在指定秒时间后失效
        /// </summary>
        /// <param name="cacheClassification">缓存的类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="persistTime">缓存时间</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject, TimeSpan persistTime)
        {
            innerCacheManager.Add(getCacheKey(cacheClassification, cacheKey),
                cacheObject);
        }


        /// <summary>
        /// 将对象添加到缓存中,缓存将在指定时间点后失效
        /// </summary>
        /// <param name="cacheClassification">缓存的类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="expiredTime">缓存终止的时间点</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject, DateTime expiredTime)
        {
            innerCacheManager.Add(getCacheKey(cacheClassification, cacheKey),
                cacheObject);
        }


        /// <summary>
        /// 将对象添加到缓存中，该对象与指定的文件关联，如果指定的文件被修改，缓存将失效
        /// </summary>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="dependenceFileName">与缓存对象关联的文件路径</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject, string dependenceFileName)
        {
            string newCacheKey = getCacheKey(cacheClassification, cacheKey);
          //  NeverExpired neverExpired = new NeverExpired();
          //  FileDependency fileDependence = new FileDependency(dependenceFileName);
          //  ICacheItemExpiration[] expirations = new ICacheItemExpiration[] { fileDependence, neverExpired };
            innerCacheManager.Add(newCacheKey, cacheObject);
            
        }


        /// <summary>
        /// 将对象添加到缓存中，该对象与指定的文件关联，如果指定的文件被修改，缓存将失效
        /// 或者缓存到达指定的时间后亦将失效
        /// </summary>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="cacheObject">缓存对象</param>
        /// <param name="dependenceFileName">关联的文件路径</param>
        /// <param name="maxPersistTime">缓存可被保留的最长时间</param>
        public static void Add(string cacheClassification, string cacheKey, object cacheObject, string dependenceFileName, TimeSpan maxPersistTime)
        {
            string newCacheKey = getCacheKey(cacheClassification, cacheKey);
          //  SlidingTime expiredTime = new SlidingTime(maxPersistTime);
          //  FileDependency fileDependence = new FileDependency(dependenceFileName);
          //  ICacheItemExpiration[] expirations = new ICacheItemExpiration[] { fileDependence, expiredTime };
            innerCacheManager.Add(newCacheKey, cacheObject);
        }

        #endregion

        #region 移除缓存
        /// <summary>
        /// 从缓存中移除缓存对象
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        public static void Remove(string cacheKey)
        {
            Remove(null, cacheKey);
        }


        /// <summary>
        /// 从缓存中移除缓存对象
        /// </summary>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        public static void Remove(string cacheClassification, string cacheKey)
        {
            innerCacheManager.Remove(getCacheKey(cacheClassification, cacheKey));
        }


        /// <summary>
        /// 清除当前缓存的所有对象
        /// </summary>
        public static void Clear()
        {
            innerCacheManager.Clear();
        }
        #endregion

        #region 获取缓存


        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回null</returns>
        public static Object Get(string cacheKey)
        {
            return Get(null, cacheKey);
        }


        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回默认值</returns>
        public static Object Get(string cacheKey, Object defaultValue)
        {
            return Get(null, cacheKey, defaultValue);
        }


        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <typeparam name="TValue">缓存对象的类型</typeparam>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回默认值</returns>
        public static TValue Get<TValue>(string cacheKey, TValue defaultValue)
        {
            return Get<TValue>(null, cacheKey, defaultValue);
        }

        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回null</returns>
        public static Object Get(string cacheClassification, string cacheKey)
        {
            return innerCacheManager.Get(getCacheKey(cacheClassification, cacheKey));
        }


        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回默认值</returns>
        public static Object Get(string cacheClassification, string cacheKey, Object defaultValue)
        {
            string newCacheKey = getCacheKey(cacheClassification, cacheKey);
            if (!innerCacheManager.Exists(newCacheKey))
            {
                return defaultValue;
            }

            return innerCacheManager.Get(newCacheKey);
        }


        /// <summary>
        /// 获取缓存对象
        /// </summary>
        /// <typeparam name="TValue">缓存对象的类型</typeparam>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="defaultValue">默认值</param>
        /// <returns>缓存对象，如果给定的键值不存在将返回默认值</returns>
        public static TValue Get<TValue>(string cacheClassification, string cacheKey, TValue defaultValue)
        {
            string newCacheKey = getCacheKey(cacheClassification, cacheKey);
            if (!innerCacheManager.Exists(newCacheKey))
            {
                return defaultValue;
            }

            Object cacheObject = innerCacheManager.Get(newCacheKey);
            if (cacheObject == null )
                return defaultValue;

            return (TValue)cacheObject;           
        }
        #endregion

        #region 检查
        /// <summary>
        /// 检查指定的缓存对象是否存在
        /// </summary>
        /// <remarks>
        /// 此检查并不十分精确， 可能会包括一些已经过期但是还没被删除的对象。
        /// 如果需要精确检查，请通过Get方法获取缓存对象，如果返回null说明该对象在缓存中不存在
        /// </remarks>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>存在返回true，否则返回false</returns>
        public static bool IsExist(string cacheKey)
        {
            return IsExist(null, cacheKey);
        }


        /// <summary>
        /// 检查指定的缓存对象是否存在
        /// </summary>
        /// <remarks>
        /// 此检查并不十分精确， 可能会包括一些已经过期但是还没被删除的对象。
        /// 如果需要精确检查，请通过Get方法获取缓存对象，如果返回null说明该对象在缓存中不存在
        /// </remarks>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>存在返回true，否则返回false</returns>
        public static bool IsExist(string cacheClassification, string cacheKey)
        {
            return innerCacheManager.Exists(getCacheKey(cacheClassification, cacheKey));
        }


        /// <summary>
        /// 缓存对象数量
        /// </summary>
        /// <remarks>
        /// 此数量并不是十分精确， 可能会包括一些已经过期但是还没被删除的对象
        /// </remarks>
        //public static int Count
        //{
        //    get
        //    {
        //        return innerCacheManager;
        //    }
        //}
        #endregion

        /// <summary>
        /// 获取缓存键
        /// </summary>
        /// <remarks>此方法将会对给定的缓存键进行验证，如果给定的键值不符合要求将引发异常</remarks>
        /// <param name="cacheClassification">缓存类别</param>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>实际缓存键</returns>
        private static string getCacheKey(string cacheClassification, string cacheKey)
        {
            if (String.IsNullOrEmpty(cacheKey))
            {
                throw new NullReferenceException("cacheKey");
            }
            if (cacheKey.Contains("."))
            {
                throw new NotSupportedException("缓存键中不能包含字符(.)");
            }
            if (String.IsNullOrEmpty(cacheClassification))
                return cacheKey;

            return cacheClassification + "." + cacheKey;
        }


    }
}

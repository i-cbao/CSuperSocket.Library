using Newtonsoft.Json;
using StackExchange.Redis;
using System;
using System.Collections.Generic;

namespace Dynamic.Core.Cache
{
    /// <summary>
    /// Redis操作类
    /// </summary>
    public class SERedisHelper
    {
        public static string _conn { get;protected set; }//"127.0.0.1:6379";
        public SERedisHelper(string connStr)
        {
            _conn = connStr;
        }
        public SERedisHelper()
        {

        }
        public static string SetRedisConn(string connStr)
        {
            if(!string.IsNullOrEmpty(connStr))
            {
                _conn = connStr;
                return _conn;
            }
            throw new NullReferenceException("连接字符串不能为空！");
        }


        #region string类型
        /// <summary>
        /// 根据Key获取值
        /// </summary>
        /// <param name="key">键值</param>
        /// <returns>System.String.</returns>
        public static string StringGet(string key)
        {
            try
            {
                using (var client = ConnectionMultiplexer.Connect(_conn))
                {
                    return client.GetDatabase().StringGet(key);
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// 批量获取值
        /// </summary>
        public static string[] StringGetMany(string[] keyStrs)
        {
            var count = keyStrs.Length;
            var keys = new RedisKey[count];
            var addrs = new string[count];

            for (var i = 0; i < count; i++)
            {
                keys[i] = keyStrs[i];
            }
            try
            {
                using (var client = ConnectionMultiplexer.Connect(_conn))
                {
                    var values = client.GetDatabase().StringGet(keys);
                    for (var i = 0; i < values.Length; i++)
                    {
                        addrs[i] = values[i];
                    }
                    return addrs;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }


        /// <summary>
        /// 单条存值
        /// </summary>
        /// <param name="key">key</param>
        /// <param name="value">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool StringSet(string key, string value)
        {

            using (var client = ConnectionMultiplexer.Connect(_conn))
            {
                return client.GetDatabase().StringSet(key, value);
            }
        }


        /// <summary>
        /// 批量存值
        /// </summary>
        /// <param name="keysStr">key</param>
        /// <param name="valuesStr">The value.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool StringSetMany(string[] keysStr, string[] valuesStr)
        {
            var count = keysStr.Length;
            var keyValuePair = new KeyValuePair<RedisKey, RedisValue>[count];
            for (int i = 0; i < count; i++)
            {
                keyValuePair[i] = new KeyValuePair<RedisKey, RedisValue>(keysStr[i], valuesStr[i]);
            }
            using (var client = ConnectionMultiplexer.Connect(_conn))
            {
                return client.GetDatabase().StringSet(keyValuePair);
            }
        }

        #endregion

        #region 泛型
        /// <summary>
        /// 存值并设置过期时间
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">key</param>
        /// <param name="t">实体</param>
        /// <param name="ts">过期时间间隔</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool Set<T>(string key, T t, TimeSpan ts)
        {
            var str = JsonConvert.SerializeObject(t);
            using (var client = ConnectionMultiplexer.Connect(_conn))
            {
                return client.GetDatabase().StringSet(key, str, ts);
            }
        }

        /// <summary>
        /// 
        /// 根据Key获取值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key">The key.</param>
        /// <returns>T.</returns>
        public static T Get<T>(string key) where T : class
        {
            using (var client = ConnectionMultiplexer.Connect(_conn))
            {
                var strValue = client.GetDatabase().StringGet(key);
                return string.IsNullOrEmpty(strValue) ? null : JsonConvert.DeserializeObject<T>(strValue);
            }
        }
        #endregion
    }
}

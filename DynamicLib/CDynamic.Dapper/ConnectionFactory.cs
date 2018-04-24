using CDynamic.Dapper.Adapters;
using Dynamic.Core.Comm;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using Dynamic.Core.Config;
using Timer = System.Timers.Timer;

namespace CDynamic.Dapper
{
    /// <summary> 数据库连接管理 </summary>
    public class ConnectionFactory : IDbConnectionProvider
    {
        private const string Prefix = "dapper:";
        private const string DefaultConfigName = "dapperDefault";
        private const string DefaultName = "default";

        private static readonly ConcurrentDictionary<Thread, Dictionary<string, ConnectionStruct>> ConnectionCache= new ConcurrentDictionary<Thread, Dictionary<string, ConnectionStruct>>();
        private static readonly object LockObj = new object();
        private int _removeCount;
        private int _cacheCount;
        private int _clearCount;
        private readonly Timer _clearTimer;
        private bool _clearTimerRun;

        private ConnectionFactory()
        {
            _clearTimer = new Timer(1000 * 60);
            _clearTimer.Elapsed += ClearTimerElapsed;
            _clearTimer.Enabled = true;
            _clearTimer.Stop();
            _clearTimerRun = false;
        }

        private void ClearTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _clearCount++;
            ClearDict();
            if (ConnectionCache.Count == 0)
            {
                _clearTimerRun = false;
                _clearTimer.Stop();
            }
        }

        /// <summary> 单例 </summary>
        public static ConnectionFactory Instance
            =>
                Singleton<ConnectionFactory>.Instance ??
                (Singleton<ConnectionFactory>.Instance = new ConnectionFactory());


        /// <summary> 清理失效的线程级缓存 </summary>
        private void ClearDict()
        {
            if (ConnectionCache.Count == 0)
                return;
            foreach (var key in ConnectionCache.Keys)
            {
                if (!ConnectionCache.TryGetValue(key, out var connDict))
                    continue;
                foreach (var name in connDict.Keys)
                {
                    if (key.IsAlive && connDict[name].IsAlive())
                        continue;
                    if (connDict.Remove(name))
                    {
                        _removeCount++;
                        connDict[name]?.Dispose();
                    }
                }
                if (connDict.Count == 0)
                    ConnectionCache.TryRemove(key, out connDict);
            }
        }
        /// <summary> 创建数据库连接 </summary>
        /// <param name="connectionName"></param>
        /// <returns></returns>
        private static IDbConnection Create(DBConfig dbConfig)
        {
            var adapter = DbConnectionManager.Create(dbConfig.DbType);
            var connection = adapter.Create();
            if (connection == null)
                throw new Exception("创建数据库连接失败");
            connection.ConnectionString = dbConfig.ConnetionString;
            return connection;
        }


        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName">连接名称</param>
        /// <param name="threadCache">是否启用线程缓存</param>
        /// <returns></returns>
        public IDbConnection Connection(DBConfig dbConfig, bool threadCache = true)
        {
            string cacheKey = dbConfig.ConnetionString;
            lock (LockObj)
            {
                if (!threadCache)
                    return Create(dbConfig);
                var connectionKey = Thread.CurrentThread;

                if (!ConnectionCache.TryGetValue(connectionKey, out var connDict))
                {
                    connDict = new Dictionary<string, ConnectionStruct>();
                    if (!ConnectionCache.TryAdd(connectionKey, connDict))
                    {
                        throw new Exception("Can not set db connection!");
                    }
                }
                if (connDict.ContainsKey(cacheKey))
                {
                    _cacheCount++;
                    return connDict[cacheKey].GetConnection();
                }

                connDict.Add(cacheKey, new ConnectionStruct(Create(dbConfig)));

                if (!_clearTimerRun)
                {
                    _clearTimer.Start();
                    _clearTimerRun = true;
                }
                return connDict[cacheKey].GetConnection();
            }
        }
        /// <summary> 缓存总数/// </summary>
        public int Count => ConnectionCache.Sum(t => t.Value.Count);

        /// <summary> 连接缓存信息 </summary>
        /// <returns></returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var proc = Process.GetCurrentProcess();
            sb.AppendLine($"专用工作集内存：{proc.PrivateMemorySize64 / 1024.0}kb");
            sb.AppendLine($"工作集内存：{proc.WorkingSet64 / 1024.0}kb");
            sb.AppendLine($"最大内存：{proc.PeakWorkingSet64 / 1024.0}kb");
            sb.AppendLine($"线程数：{proc.Threads.Count}");
            foreach (var connectionStruct in ConnectionCache)
            {
                foreach (var item in connectionStruct.Value)
                {
                    sb.AppendLine(item.ToString());
                }
            }
            sb.AppendLine($"total:{Count},useCache:{_cacheCount},clear:{_clearCount},remove:{_removeCount}");
            return sb.ToString();
        }
    }
}

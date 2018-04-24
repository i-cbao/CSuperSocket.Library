
using Dynamic.Core;
using Dynamic.Core.Comm;
using System;
using System.Data;

using Dynamic.Core.Config;
using Dynamic.Core.ViewModel;

namespace CDynamic.Dapper.Domain
{
    public abstract class DRepository
    {
        private readonly DBConfig _defaultConnectionDBConfig;
        /// <summary> 获取默认连接 </summary>
        protected IDbConnection Connection => GetConnection(_defaultConnectionDBConfig);

        protected DRepository(DBConfig dBConfig = null)
        {
            _defaultConnectionDBConfig = dBConfig;
        }
        protected DRepository(DBCfgViewModel dBCfgViewModel)
        {
            _defaultConnectionDBConfig = DBConfig.GetConfig(dBCfgViewModel);
        }

        public static TRepository Instance<TRepository>()
            where TRepository : DRepository, new()
        {
            return Singleton<TRepository>.Instance ?? (Singleton<TRepository>.Instance = new TRepository());
        }


        

        /// <summary> 获取数据库连接 </summary>
        /// <param name="connectionName"></param>
        /// <param name="threadCache"></param>
        /// <returns></returns>
        protected IDbConnection GetConnection(DBConfig dbConfig = null, bool threadCache = true)
        {
            
            return ConnectionFactory.Instance.Connection(dbConfig, threadCache);
        }

        /// <summary> 执行数据库事务 </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="action"></param>
        /// <param name="connectionName"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected TResult Transaction<TResult>(Func<IDbConnection, IDbTransaction, TResult> action, DBConfig dbConfig = null,
            IsolationLevel? level = null)
        {
            var conn = GetConnection(dbConfig ?? _defaultConnectionDBConfig, false);
            using (conn)
            {
                conn.Open();
                var transaction = level.HasValue ? conn.BeginTransaction(level.Value) : conn.BeginTransaction();
                using (transaction)
                {
                    try
                    {
                        var result = action.Invoke(conn, transaction);
                        transaction.Commit();
                        return result;
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        /// <summary> 执行数据库事务 </summary>
        /// <param name="action"></param>
        /// <param name="connectionName"></param>
        /// <param name="level"></param>
        /// <returns></returns>
        protected DResult Transaction(Action<IDbConnection, IDbTransaction> action, DBConfig dBConfig = null,
            IsolationLevel? level = null)
        {
            var result = Transaction((conn, trans) =>
            {
                action.Invoke(conn, trans);
                return DResult.Success;
            }, dBConfig, level);
            return result ?? DResult.Error("事务执行失败");
        }

        /// <summary> 更新数量 </summary>
        /// <param name="conn"></param>
        /// <param name="column"></param>
        /// <param name="key"></param>
        /// <param name="keyColumn"></param>
        /// <param name="count"></param>
        /// <param name="trans"></param>
        /// <returns></returns>
        protected int Increment<T>(string column, object key, string keyColumn = "id",
            int count = 1, IDbConnection conn = null, IDbTransaction trans = null)
        {
            return (conn ?? Connection).Increment<T>(column, key, keyColumn, count, trans);
        }
    }
}

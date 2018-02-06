using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Dynamic.Net.Base
{
    /// <summary>
    /// 网路应用接口
    /// </summary>
    public interface INetApplication
    {
        event EventHandler SessionStarted;
        event EventHandler SessionClosed;

        void SessionCreated(INetSession session);

        /// <summary>
        /// 应用名称
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// 应用唯一标识，应该在初始化应用时赋值
        /// </summary>
        string AppGuid { get; }

        /// <summary>
        /// 配置应用
        /// </summary>
        /// <param name="appName">应用名称</param>
        /// <returns>是否成功</returns>
        bool Setup(string  appName);

        /// <summary>
        /// 启动应用
        /// </summary>
        /// <returns></returns>
        bool Start();

        /// <summary>
        /// 停止应用
        /// </summary>
        /// <returns></returns>
        bool Stop();

        /// <summary>
        /// 服务列表
        /// </summary>
        IEnumerable<INetServer> ServerList { get; }

        /// <summary>
        /// 获取指定的Session
        /// </summary>
        /// <param name="sessionID">会话ID</param>
        /// <returns>对应的INetSession对象</returns>
        INetSession GetSession(string sessionID);


        /// <summary>
        /// 会话总数
        /// </summary>
        int SessionCount { get; }

        /// <summary>
        /// 获取指定的缓存对象
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <returns>缓存值</returns>
        object GetCache(string cacheKey);

        /// <summary>
        /// 设置缓存对象
        /// </summary>
        /// <param name="cacheKey">缓存键</param>
        /// <param name="value">缓存值</param>
        /// <returns>是否成功</returns>
        bool SetCache(string cacheKey, Object value);
    }
}

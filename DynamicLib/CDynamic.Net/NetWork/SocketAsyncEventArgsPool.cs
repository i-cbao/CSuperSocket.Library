using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Dynamic.Net.KcpSharp.NetWork
{
    /// <summary>
    /// 后进先出SocketAsyncEventArgs集合，即Stack集合
    /// 这种集合类型没有排序，操作加入或取出对象内存操作速度极快
    /// </summary>
    class SocketAsyncEventArgsPool
    {
        Stack<SocketAsyncEventArgs> m_pool;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="capacity">初始容量大小</param>
        public SocketAsyncEventArgsPool(int capacity)
        {
            m_pool = new Stack<SocketAsyncEventArgs>(capacity);
        }

        /// <summary>
        /// 将异步套接字操作加入队列
        /// </summary>
        /// <param name="item">异步套接字操作</param>
        public void Push(SocketAsyncEventArgs item)
        {
            if (item == null) { throw new ArgumentNullException("Items added to a SocketAsyncEventArgsPool cannot be null"); }
            lock (m_pool)
            {
                m_pool.Push(item);
            }
        }

        /// <summary>
        /// 返回最后进入队列的异步套接字操作，并将之移除队列
        /// </summary>
        /// <returns>最后进入队列的异步套接字操作</returns>
        public SocketAsyncEventArgs Pop()
        {
            lock (m_pool)
            {
                return m_pool.Pop();
            }
        }

        // 数量
        public int Count
        {
            get { return m_pool.Count; }
        }


    }
}

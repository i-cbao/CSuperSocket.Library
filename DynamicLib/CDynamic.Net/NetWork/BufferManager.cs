using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace Dynamic.Net.KcpSharp.NetWork
{
    /// <summary>
    /// 内存缓冲管理器
    /// </summary>
    class BufferManager
    {
        int m_numBytes;                 // 缓冲区字节总数
        byte[] m_buffer;                // 底层缓冲数据
        Stack<int> m_freeIndexPool;     // 缓冲区块
        int m_currentIndex;
        int m_bufferSize;
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="totalBytes">缓冲区大小</param>
        /// <param name="bufferSize">缓冲区块大小</param>
        public BufferManager(int totalBytes, int bufferSize)
        {
            m_numBytes = totalBytes;
            m_currentIndex = 0;
            m_bufferSize = bufferSize;
            m_freeIndexPool = new Stack<int>();
        }

        /// <summary>
        ///  初始化缓冲空间
        /// </summary>
        public void InitBuffer()
        {
            m_buffer = new byte[m_numBytes];
        }


        //
        /// <summary>
        /// 从缓冲区中分配一块区域到指定的异步套接字操作
        /// </summary>
        /// <param name="args">目标套接字操作</param>
        /// <returns>是否分配成功</returns>
        public bool SetBuffer(SocketAsyncEventArgs args)
        {

            if (m_freeIndexPool.Count > 0)
            {
                args.SetBuffer(m_buffer, m_freeIndexPool.Pop(), m_bufferSize);
            }
            else
            {
                if ((m_numBytes - m_bufferSize) < m_currentIndex)
                {
                    return false;
                }
                args.SetBuffer(m_buffer, m_currentIndex, m_bufferSize);
                m_currentIndex += m_bufferSize;
            }
            return true;
        }

        /// <summary>
        /// 将使用完成的缓冲区释放到缓冲块中
        /// </summary>
        /// <param name="args">目标套接字操作</param>
        public void FreeBuffer(SocketAsyncEventArgs args)
        {
            m_freeIndexPool.Push(args.Offset);
            args.SetBuffer(null, 0, 0);
        }

    }

}

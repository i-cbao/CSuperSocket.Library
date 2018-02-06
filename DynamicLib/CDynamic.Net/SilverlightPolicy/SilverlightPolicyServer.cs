using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using Dynamic.Core.Log;
using NLog;

namespace Dynamic.Net.SilverlightPolicy
{
    class PolicyConnection
    {
        private Socket m_connection;

        private byte[] m_buffer;
        private int m_received;

        private byte[] m_policy;

        private static string s_policyRequestString = "<policy-file-request/>";

        protected ILogger Logger = LoggerManager.GetLogger("SilverlightPolicy");

        public PolicyConnection(Socket client, byte[] policy)
        {
            m_connection = client;
            m_policy = policy;

            m_buffer = new byte[s_policyRequestString.Length];
            m_received = 0;
            Logger.Trace("收到请求：{0}", client.RemoteEndPoint);
            try
            {
                m_connection.BeginReceive(m_buffer, 0, s_policyRequestString.Length, SocketFlags.None, new AsyncCallback(OnReceive), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }

        private void OnReceive(IAsyncResult res)
        {
            try
            {
                m_received += m_connection.EndReceive(res);

                if (m_received < s_policyRequestString.Length)
                {
                    m_connection.BeginReceive(m_buffer, m_received, s_policyRequestString.Length - m_received, SocketFlags.None, new AsyncCallback(OnReceive), null);
                    return;
                }

                string request = System.Text.Encoding.UTF8.GetString(m_buffer, 0, m_received);
                if (StringComparer.InvariantCultureIgnoreCase.Compare(request, s_policyRequestString) != 0)
                {
                    Logger.Warn("非安全策略请求：{0}", request);
                    m_connection.Close();
                    return;
                }

                m_connection.BeginSend(m_policy, 0, m_policy.Length, SocketFlags.None, new AsyncCallback(OnSend), null);
            }
            catch (SocketException)
            {
                m_connection.Close();
            }
        }

        public void OnSend(IAsyncResult res)
        {
            try
            {
                m_connection.EndSend(res);
            }
            finally
            {
                m_connection.Close();
            }
        }
    }

    public class SilverlightPolicyServer
    {
        private Socket m_listener;
        private byte[] m_policy;

        protected ILogger Logger = LoggerManager.GetLogger("SilverlightPolicy");

        protected bool IsRunning { get; set; }

        public SilverlightPolicyServer(string policyFile)
        {
            Stream policyStream = null;

            if (policyFile.StartsWith("res://"))
            {
                Assembly exeAssembly = Assembly.GetEntryAssembly();

                

                string resName = policyFile.Substring(6);
                policyStream = exeAssembly.GetManifestResourceStream(resName);

                Logger.Trace("使用资源策略文件：{0}", resName);
            }
            else
            {
                if (File.Exists(policyFile))
                {
                    policyStream = new FileStream(policyFile, FileMode.Open);
                }
            }

            if (policyStream != null)
            {
                m_policy = new byte[policyStream.Length];
                policyStream.Read(m_policy, 0, m_policy.Length);

                policyStream.Close();
            }
            else
            {
                m_policy = new byte[1];
            }

            Logger.Trace("策略文件长度：{0}", m_policy.Length);

            m_listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            m_listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            m_listener.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, true);

            m_listener.Bind(new IPEndPoint(IPAddress.Any, 943));
            m_listener.Listen(0);
            IsRunning = true;
            
            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }

        public void OnConnection(IAsyncResult res)
        {
            Socket client = null;

            try
            {
                client = m_listener.EndAccept(res);
            }
            catch (SocketException)
            {
                if (IsRunning)
                {
                    m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
                }
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }

            PolicyConnection pc = new PolicyConnection(client, m_policy);

            m_listener.BeginAccept(new AsyncCallback(OnConnection), null);
        }

        public void Close()
        {
            IsRunning = false;
            m_listener.Close();
        }
    }


}
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Dynamic.Core.Log;

namespace Dynamic.Net.SilverlightPolicy
{
    public class CrossDomainPolicyServer 
    {
        protected ILogger Logger = LoggerManager.GetLogger("CrossDomainPolicyServer");
        private HttpListener httpServer = null;
        private Thread thread = null;

        public CrossDomainPolicyServer(string prefix)
        {
            httpServer = new HttpListener();
            UriBuilder ub = new UriBuilder(prefix);
            ub.Host = "_HOSTPLACEHOLDER_";
            string baseUrl = ub.Uri.ToString().ToLower();
        
            baseUrl = baseUrl.Replace("_hostplaceholder_", "*");
            httpServer.Prefixes.Add(baseUrl);
        }

        public String PolicyText { get; set; }

        public bool Start()
        {
            if (httpServer != null && !httpServer.IsListening)
            {
                httpServer.Start();


            }

            if (thread != null)
            {
                try
                {
                    thread.Abort();
                    
                }
                catch { }
                thread = null;
            }
           
            thread = new Thread(pro);
            thread.IsBackground = true;
            thread.Name = "HTTP监听线程";
            thread.Start(null);

            return true;
        }

        private void pro(object ctx)
        {
            
            while (true)
            {
                HttpListenerContext httpCtx = null;

                try
                {
                    httpCtx = httpServer.GetContext();
                }
                catch { }
                if (httpCtx != null)
                {
                    ThreadPool.QueueUserWorkItem(processRequest, httpCtx);
                }

            }
            
        }

        private void processRequest(object httpCtx)
        {
            try
            {

                HttpListenerContext httpContext = httpCtx as HttpListenerContext;

                byte[] strData = Encoding.UTF8.GetBytes((PolicyText) ?? "");
                httpContext.Response.OutputStream.Write(strData, 0, strData.Length);
                httpContext.Response.OutputStream.Close();
            }
            catch (Exception e)
            {
                Logger.Error("处理请求失败：\r\n{0}", e.ToString());
            }
        }

        public bool Stop()
        {
            

            if (httpServer != null && httpServer.IsListening)
            {
                try
                {
                    httpServer.Abort();
                }
                catch (ObjectDisposedException)
                {
                }
            }

            if (thread != null)
            {
                try
                {
                    thread.Abort();
                    
                }
                catch { }
                thread = null;
            }


            return true;
        }

        public bool IsRunning
        {
            get { return httpServer.IsListening; }
        }

      
    }
}

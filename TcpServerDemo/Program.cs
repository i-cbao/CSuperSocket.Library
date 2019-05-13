using CSuperSocket.SocketBase.Config;
using CSuperSocket.SocketEngine;
using CSuperSocket.SocketEngine.Extions;
using Dynamic.Core.Service;
using System;
using System.Configuration;
using System.Diagnostics;

using System.Linq;

namespace TcpServerDemo
{
    class Program
    {
        protected static  Service _server { get; set; }
        private static bool _setConsoleColor;
        static void Main(string[] args)
        {
            Startup.Init();
            
            InitSocket();
          //  InitServer();
            Console.WriteLine("初始化成功!");
            while (Console.ReadLine()!="exit")
            {
                foreach (var item in _server.GetAllSessions())
                {
                    Console.WriteLine(item.SessionID);
                }
            }
        }
        public static void InitServer()
        {
            Service appServer = new Service();
            if (!appServer.Setup(7008)) //开启的监听端口
            {
                return;
            }
            if (!appServer.Start())
            {
                return;
            }
            appServer.SessionClosed += Service_SessionClosed;
            appServer.NewSessionConnected += Service_NewSessionConnected;
          
          
            
        }

        private static void Service_NewSessionConnected(Session session)
        {
            Console.WriteLine(session.LocalEndPoint.Address);
        }

        private static void Service_SessionClosed(Session session, CSuperSocket.SocketBase.CloseReason value)
        {
            Console.WriteLine(session.SessionID+"断开");
        }

        static void InitSocket()
        {
            var simpleCfg = IocUnity.Get<SimpleSocketConfig>();

            Stopwatch startWatch = new Stopwatch();
            startWatch.Start();
            var bootstrap = BootstrapFactory.CreateBootstrapFromServerCfg(simpleCfg);
            
            startWatch.Stop();
            Console.WriteLine("CSpuerTcp初始化工厂耗时：{0}ms", startWatch.ElapsedMilliseconds);

            startWatch.Reset();
            startWatch.Start();
            var isSuccess = bootstrap.Initialize();
            startWatch.Stop();
            Console.WriteLine("CSpuerTcp启动耗时：{0}ms", startWatch.ElapsedMilliseconds);
            if (!isSuccess)
            {
                SetConsoleColor(ConsoleColor.Red);
                Console.WriteLine("Failed to initialize CSuperSocket ServiceEngine! Please check error log for more information!");
                Console.ReadKey();
                return;
            }
            _server = bootstrap.AppServers.ToList().FirstOrDefault() as Service;
            Console.WriteLine("Starting...");
            var result = bootstrap.Start();
       
            Console.WriteLine("监听端口:{0}", simpleCfg.Port);
            Console.WriteLine("-------------------------------------------------------------------");
            SetConsoleColor(ConsoleColor.Green);
            //Console.WriteLine($"上位机唯一Id:{ConfigurationManager.AppSettings["Pid"].ToString()}");
        }
        private static void SetConsoleColor(ConsoleColor color)
        {
            if (_setConsoleColor)
                Console.ForegroundColor = color;
        }

    }
}

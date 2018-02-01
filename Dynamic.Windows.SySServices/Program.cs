using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Dynamic.Windows.SySServices.Common;
using System.Threading;

namespace Statistics.WindowService
{
    internal static class Program
    {
        public static void WriteLineColor(string content, ConsoleColor color = ConsoleColor.Green)
        {
            var oldColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(content);
            Console.ForegroundColor = oldColor;
        }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static void Main(string[] args)
        {
            Console.Title = "NWF Windows服务代理工具！";
            if (!IdentityHelper.IsRunAsAdmin())
            {

                WriteLineColor("注：本程序，需用管理员身份运行！", ConsoleColor.Red);
                Console.Beep();
                while (Console.ReadLine() != "exit")
                {
                    WriteLineColor("注：本程序，需用管理员身份运行！", ConsoleColor.Red);
                    Console.Beep();
                }
            }

            var serviceCfg = ConfigHelper.LoadSysServiceCfg();
            serviceCfg.Filter();



            SysServiceHelper.IsExites(serviceCfg.ServiceName, true);

            Console.WriteLine("请选择对{0}的服务操作！", serviceCfg.ExeFileName);
            Console.WriteLine("请选择，[1]安装服务 [2]卸载服务 [3]重装 [4]退出");
            var rs = int.Parse(Console.ReadLine());


            switch (rs)
            {
                case 1:
                    while (SysServiceHelper.IsExites(serviceCfg.ServiceName, true))
                    {
                        Console.ReadLine();
                        return;
                    }
                    SysServiceHelper.InstallConsoleService(serviceCfg);
                    //取当前可执行文件路径，加上"s"参数，证明是从windows服务启动该程序
                    if (SysServiceHelper.IsExites(serviceCfg.ServiceName))
                    {
                        WriteLineColor("安装成功 输入s运行服务！");
                        var inputStr = Console.ReadLine();
                        if (inputStr.Equals("s", StringComparison.OrdinalIgnoreCase))
                        {
                            SysServiceHelper.StartService(serviceCfg.ServiceName);
                        }
                    }
                    Console.Read();
                    break;
                case 2:
                    SysServiceHelper.UnInstall(serviceCfg);
                    if (!SysServiceHelper.IsExites(serviceCfg.ServiceName))
                    {
                        WriteLineColor("卸载成功！");
                    }
                    Console.Read();
                    break;
                case 3:
                    SysServiceHelper.UnInstall(serviceCfg);
                    if (!SysServiceHelper.IsExites(serviceCfg.ServiceName))
                    {
                        SysServiceHelper.InstallConsoleService(serviceCfg);
                    }
                    if (SysServiceHelper.IsExites(serviceCfg.ServiceName))
                    {
                        WriteLineColor("安装成功 输入s运行服务！");
                        var inputStr = Console.ReadLine();
                        if (inputStr.Equals("s", StringComparison.OrdinalIgnoreCase))
                        {
                            SysServiceHelper.StartService(serviceCfg.ServiceName);
                        }
                    }
                    Console.Read();
                    break;
                case 4: break;
            }
        }
    }
}
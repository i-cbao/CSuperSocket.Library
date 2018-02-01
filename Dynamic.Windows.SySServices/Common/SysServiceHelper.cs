using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ServiceProcess;
using System.ComponentModel;

using Statistics.WindowService;
using Dynamic.Windows.SySServices.Config;
using System.IO;
using System.Diagnostics;

namespace Dynamic.Windows.SySServices.Common
{
    public class SysServiceHelper
    {
        public static bool IsExites(string serverName, bool isShowMsg = false)
        {
            ServiceController sc = new ServiceController(serverName);

            try
            {
                string servicesName = sc.ServiceName;
                sc.Refresh();
                if (sc.CanStop)
                {
                    sc.Stop();
                }
                sc.Start();

                if (isShowMsg) Program.WriteLineColor(string.Format("{0}服务已经安装！", serverName), ConsoleColor.Red);
                return true;
            }
            catch (Exception wex)
            {
                if (isShowMsg) Program.WriteLineColor(wex.Message, ConsoleColor.Red);
                return false;
            }

        }
        public static bool InstallConsoleService(SysServicesInstallCfg serviceCfg)
        {
            var exePath = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), serviceCfg.ExeFileName);
            var serviceProxyCfg = serviceCfg.Clone();
            serviceProxyCfg.ExeFileName = "serviceProxy.exe";
            var installProxyServiceStatus = Install(serviceProxyCfg);
            //            reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ServiceName\Parameters /v Application /t REG_SZ /d "这里填入你要作为服务运行的程序地址比如c:\xxx.exe" /f
            //reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ServiceName\Parameters /v AppParameters /t REG_SZ /d "如果程序需要参数则填在这里，如果不需要，清空这段文字或者整行" /f
            //reg add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\ServiceName\Parameters /v Application /t REG_SZ /d "这里填入程序运行时所在文件夹（作为环境变量），如果不填，则清除这段内容或者直接删除本行" /f
            var reg1 = string.Format(@" add HKEY_LOCAL_MACHINE\SYSTEM\CurrentControlSet\Services\{0}\Parameters /v Application /t REG_SZ /d {1} /f", serviceCfg.ServiceName, "\"" + exePath + "\"");
            File.AppendAllText("Dynamic.Windows.SySServices.log", reg1);
            Proxy regProxy = new Proxy("reg", reg1, true);
            return installProxyServiceStatus && regProxy.Start();


        }
        public static bool StartService(string serviceName)
        {
            string startProxyStr = string.Format("start {0}", serviceName);
            Proxy scProxy = new Proxy("sc", startProxyStr, true);
            return scProxy.Start();
        }
        public static bool StopService(string serviceName)
        {
            string stopProxyStr = string.Format("start {0}", serviceName);
            Proxy scProxy = new Proxy("sc", stopProxyStr, true);
            return scProxy.Start();
        }
        public static bool Install(SysServicesInstallCfg serviceCfg)
        {
            var path = Path.Combine(Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName), serviceCfg.ExeFileName);
            if (!File.Exists(path))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("{0}路径无法识别,按任意键结束！", path);
                Console.ReadLine();
                return false;
            }
            //path = path + " s";
            string installStr = string.Format("create {0} binpath= \"{1}\" DisplayName= \"{2}\" start= auto", serviceCfg.ServiceName, path, serviceCfg.DisplayName);
            string updateDesStr = string.Format("description {0} \"{1}\" ", serviceCfg.ServiceName, serviceCfg.Description);
            File.AppendAllText("Dynamic.Windows.SySServices.log", installStr);
            Proxy scProxy = new Proxy("sc", installStr, true);
            Proxy scProxyUpdate = new Proxy("sc", updateDesStr, true);
            return scProxy.Start() & scProxyUpdate.Start();

        }
        public static bool UnInstall(SysServicesInstallCfg serviceCfg)
        {
            string unInstallStr = "delete " + serviceCfg.ServiceName + "";
            Console.WriteLine(unInstallStr);
            Proxy scProxy = new Proxy("sc", unInstallStr, true);
            return scProxy.Start();
        }
    }
}

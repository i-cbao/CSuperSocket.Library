using Dynamic.Core.Log;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Dynamic.Core.Config
{
    public class LogConfigSetting
    {
        public static readonly Log.ILogger _GlobLogger = LoggerManager.GetLogger("GlobLogger");
        public static void InitConfig(LogConfig logConfig)
        {
            if (logConfig == null)
            {
                logConfig = new LogConfig() {
                    LogBaseDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Logs"),
                    LogContentTemplate = LogLayoutTemplates.SimpleLayout,
                    LogFileTemplate = LogFileTemplates.PerDayDirAndLogger,
                    LogLevels = LogLevels.All
                };
            }
            LoggerManager.InitLogger(logConfig);
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            try
            {
                Exception ex = e.ExceptionObject as Exception;
                //这儿记录全局为处理的异常
                _GlobLogger.Error(ex.ToString());
            }
            catch {

            }
           
        }
    }
}

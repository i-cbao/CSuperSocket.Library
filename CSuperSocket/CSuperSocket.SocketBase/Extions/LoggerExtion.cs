using Dynamic.Core.Log;
using System;
using System.Collections.Generic;
using System.Text;

namespace CSuperSocket.SocketBase
{
    public static class LoggerExtion
    {
       
        public static void Info(this ILogger Logger, IAppSession session, string strContext)
        {
            Logger.Info("session={0}=>"+strContext, session.SessionID);
        }
        public static void Error(this ILogger Logger, IAppSession session, string strContext)
        {
            Logger.Error("session={0}=>" + strContext, session.SessionID);
        }
        public static void InitLog(this LogConfig logConfig)
        {
            LoggerManager.InitLogger(logConfig);
        }
        public static void Error(this ILogger Logger,Exception ex)
        {
            Logger.Error(ex.ToString());
        }
    }
}

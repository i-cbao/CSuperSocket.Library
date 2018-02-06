using Dynamic.Core.Cache;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Core.Config
{
    public class DBConfig
    {
        public static void InitConfig(IServiceCollection services)
        {
            var ioptions_dbconfig = services.BuildServiceProvider().GetService<IOptions<DBConfig>>();
            DBConfig._Install = ioptions_dbconfig.Value;

            SERedisHelper.SetRedisConn(DBConfig._Install.RedisConnectionString);
        }
        public static DBConfig _Install { get; set; }
        public string DbType { get; set; }
        public string ConnetionString { get; set; }

        public string RedisConnectionString { get; set; }

        public string GetConetionString(int index)
        {
            var conStrs = ConnetionString.Split('|');
            if (index < 0)
            {
                index = 0;
            }
            if (index >= conStrs.Length)
            {
                index = conStrs.Length - 1;
            }
            return conStrs[index];
        }
    }
}

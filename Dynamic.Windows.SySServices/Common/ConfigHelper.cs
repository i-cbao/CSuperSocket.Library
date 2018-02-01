using Dynamic.Windows.SySServices.Config;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Windows.SySServices.Common
{
    public class ConfigHelper
    {
        public static SysServicesInstallCfg LoadSysServiceCfg(string filePath= "SysServicesInstallCfg.json")
        {
            var objStr=File.ReadAllText(filePath);
            SysServicesInstallCfg cfg = Newtonsoft.Json.JsonConvert.DeserializeObject<SysServicesInstallCfg>(objStr);
            return cfg;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamic.Windows.SySServices.Config
{
    public  class SysServicesInstallCfg
    {
        public SysServicesInstallCfg()
        {
            this.AccountName = "LocalSystem";
        }
        public SysServicesInstallCfg Clone()
        {
            SysServicesInstallCfg cfg = new SysServicesInstallCfg();
            cfg.ServiceName = this.ServiceName;
            cfg.DisplayName = this.DisplayName;
            cfg.ExeFileName = this.ExeFileName;
            cfg.Description = this.Description;
            cfg.AccountName = this.AccountName;
            return cfg;
        }
        public SysServicesInstallCfg Filter()
        {
            this.ServiceName = this.ServiceName.Replace(".","_").ToLower();
            this.DisplayName = this.ServiceName.Replace(".","_").ToLower();
            return this;
        }
        public string ExeFileName { get; set; }
        public string ServiceName { get; set; }
        public string Description { get; set; }

        public string AccountName { get; set; }

        public string DisplayName { get; set; }
    }
}

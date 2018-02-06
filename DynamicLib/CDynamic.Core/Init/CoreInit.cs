using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Core.Init
{
    public sealed class CoreInit
    {
        private static bool _IsInit = false;
        public static void Init()
        {
            if (_IsInit)
            {
                throw new TypeInitializationException("Dynamic.Core.CoreInit", new Exception("CoreInit.Init()已经调用，重复初始化！"));
            }
            //注册EncodingProvider实现对中文编码的支持
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            _IsInit = true;
        }
     
        public void ResIoc(IServiceCollection services)
        {
             
            //  services.Scan(scan => scan.FromAssemblyOf<this>()
              //.AddClasses().UsingAttributes());
        }
      
    }
}

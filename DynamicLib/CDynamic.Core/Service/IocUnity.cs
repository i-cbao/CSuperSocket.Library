using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Dynamic.Core.Service
{
    public class IocUnity
    {
        public readonly static string Please_visit_by_Instance = "请通过Instance单例模式访问";
        protected static IServiceCollection _Services { get; private set; }
        private static IocUnity _instance;
        public static IocUnity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _Services = new ServiceCollection();
                    _instance = new IocUnity(_Services);
                }
                return _instance;
            }
        }

        public IocUnity(IServiceCollection services)
        {
            _Services = services;
            _Services.AddOptions();
        }
        /// <summary>
        /// 通过build方式创建，为外部留插入空间（如可以和aspnetcore中共用注入容器），如故不手动调用build将默认通过new初始化容器创建
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IocUnity Build(IServiceCollection services)
        {
            _instance = new IocUnity(services);
            return Instance;

        }
        public  IServiceCollection AddSingleton<T>(T t) where T : class
        {
            return _Services.AddSingleton<T>(t);
        }
        public  IServiceCollection AddTransient<T>(T t) where T : class
        {
            return _Services.AddTransient<T>();
        }
        public  IServiceCollection AddScoped<T>(T t) where T : class
        {
            return _Services.AddScoped<T>();
        }
        public  T Get<T>() where T : class
        {
            return _Services.BuildServiceProvider().GetService<T>();
        }

    }
}

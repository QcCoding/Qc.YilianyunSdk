using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加易联云SDK，根据配置读取应用配置，保存到文本文件中
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddYilianyunSdk<T>(this IServiceCollection services, Action<YilianyunConfig> optionsAction = null) where T : IYilianyunSdkHook
        {
            if (optionsAction != null)
                services.Configure(optionsAction);
            services.AddHttpClient();
            services.AddScoped<IYilianyunSdkHook, DefaultYilianyunSdkHook>();
            services.AddScoped<YilianyunService, YilianyunService>();
            return services;
        }
        /// <summary>
        /// 添加易联云SDK，注入自定义实现的IYilianyunSdkHook
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <param name="optionsAction"></param>
        /// <returns></returns>
        public static IServiceCollection AddYilianyunSdk(this IServiceCollection services, Action<YilianyunConfig> optionsAction)
        {
            services.AddYilianyunSdk<DefaultYilianyunSdkHook>(optionsAction);

            return services;
        }
    }
}

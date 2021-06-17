using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Qc.YilianyunSdk.SqlServer.Infrastructure;
using Qc.YilianyunSdk.SqlServer.Models;
using Qc.YilianyunSdk.SqlServer.Services;
using System;

namespace Qc.YilianyunSdk.SqlServer.Builder
{
    public static class ServiceCollectionExtensions
    {
        public static void AddSqlServerYilianyunSdk(this IServiceCollection services, Action<YilianyunSqlServerConfig> optionsAction)
        {
            services.AddMemoryCache();
            if (optionsAction != null)
                services.Configure(optionsAction);
            services.AddHttpClient();
            services.AddScoped<IYilianyunSdkHook, SqlServerYilianyunSdkHook>();
            services.AddScoped(typeof(YilianyunService));

            services.AddDbContext<YilianyunContext>(
                options => options.UseSqlServer());
        }
    }
}

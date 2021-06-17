using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Qc.YilianyunSdk.SqlServer.Infrastructure;
using Qc.YilianyunSdk.SqlServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qc.YilianyunSdk.SqlServer.Services
{
    public class SqlServerYilianyunSdkHook : IYilianyunSdkHook
    {
        private readonly YilianyunSqlServerConfig _yilianyunConfig;
        private readonly IMemoryCache _cache;

        public SqlServerYilianyunSdkHook(IOptions<YilianyunSqlServerConfig> configOptions, IMemoryCache cache)
        {
            _yilianyunConfig = configOptions?.Value;
            _cache = cache;
        }

        /// <summary>
        /// 获取应用配置
        /// </summary>
        /// <returns></returns>
        public YilianyunConfig GetClientConfig()
        {
            return _yilianyunConfig;
        }

        public AccessTokenOutputModel GetAccessToken(string machine_code)
        {
             return _cache.GetOrCreate($"{GetType().FullName}_{machine_code}", entry =>
            {
                entry.SlidingExpiration = _yilianyunConfig.TokenSlidingExpiration;
                using var context = new YilianyunContext(_yilianyunConfig.SaveConnection);
                return context.AccessTokenOutputModels.AsNoTracking().FirstOrDefault(f => f.Machine_Code == machine_code);
            });
        }

        public YilianyunBaseOutputModel<AccessTokenOutputModel> SaveToken(AccessTokenOutputModel input)
        {
            using var context = new YilianyunContext(_yilianyunConfig.SaveConnection);
            context.Add(input);
            context.SaveChanges();
            _cache.Set($"{GetType().FullName}_{input.Machine_Code}", input);
            return new YilianyunBaseOutputModel<AccessTokenOutputModel>("授权成功", "0") { Body = input };
        }
    }
}

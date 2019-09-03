using Microsoft.Extensions.Options;
using Qc.YilianyunSdk.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class DefaultYilianyunSdkHook : IYilianyunSdkHook
    {
        private readonly YilianyunConfig _yilianyunConfig;
        public DefaultYilianyunSdkHook(IOptions<YilianyunConfig> configOptions)
        {
            _yilianyunConfig = configOptions?.Value;
        }
        /// <summary>
        /// 获取应用配置
        /// </summary>
        /// <returns></returns>
        public YilianyunConfig GetClientConfig()
        {
            return _yilianyunConfig;
        }
        /// <summary>
        /// 获取指定打印机的 AccessToken
        /// </summary>
        /// <returns></returns>
        public AccessTokenOutputModel GetAccessToken(string machine_code)
        {
            string readPath = Path.Combine(Path.GetFullPath(_yilianyunConfig.SaveTokenDirPath), machine_code + ".txt");
            if (!File.Exists(readPath))
                return null;
            var accessResult = File.ReadAllText(readPath);
            var result = JsonHelper.Deserialize<AccessTokenOutputModel>(accessResult);
            return result;
        }
        /// <summary>
        /// 保存TOKEN
        /// </summary>
        /// <param name="accessToken"></param>
        /// <param name="refreshToken"></param>
        /// <param name="expires_in"></param>
        /// <param name="machine_code"></param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> SaveToken(AccessTokenOutputModel input)
        {
            if (!Directory.Exists(_yilianyunConfig.SaveTokenDirPath))
            {
                Directory.CreateDirectory(_yilianyunConfig.SaveTokenDirPath);
            }
            string savePath = Path.Combine(_yilianyunConfig.SaveTokenDirPath, input.Machine_Code + ".txt");
            System.IO.File.WriteAllText(savePath, JsonHelper.Serialize(input));
            return new YilianyunBaseOutputModel<AccessTokenOutputModel>("授权成功", "0") { Body = input };
        }
    }
}

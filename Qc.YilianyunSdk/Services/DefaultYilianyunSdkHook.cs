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
            // 自有应用需要根据clientId获取
            string code = machine_code;
            if (_yilianyunConfig.YilianyunClientType == YilianyunClientType.自有应用)
            {
                code = _yilianyunConfig.ClientId;
            }
            string readPath = Path.Combine(Path.GetFullPath(_yilianyunConfig.SaveTokenDirPath), code + ".txt");
            if (!File.Exists(readPath))
                return null;
            var accessResult = File.ReadAllText(readPath);
            var result = JsonHelper.Deserialize<AccessTokenOutputModel>(accessResult);
            return result;
        }
        /// <summary>
        /// 保存TOKEN
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> SaveToken(AccessTokenOutputModel input)
        {
            if (!Directory.Exists(_yilianyunConfig.SaveTokenDirPath))
            {
                Directory.CreateDirectory(_yilianyunConfig.SaveTokenDirPath);
            }
            // 自有应用需要根据clientId获取
            string code = _yilianyunConfig.YilianyunClientType == YilianyunClientType.自有应用 ? _yilianyunConfig.ClientId : input.Machine_Code;
            string savePath = Path.Combine(Path.GetFullPath(_yilianyunConfig.SaveTokenDirPath), code + ".txt");
            if (_yilianyunConfig.YilianyunClientType == YilianyunClientType.自有应用)
            {
                // 可以根据 Machine_Code 保存 终端名称,手机号
                // 自有应用Token永久有效，所以保存一次即可
                if (File.Exists(savePath))
                    return new YilianyunBaseOutputModel<AccessTokenOutputModel>("授权成功", "0") { Body = input };
            }
            //保存
            System.IO.File.WriteAllText(savePath, JsonHelper.Serialize(input));
            return new YilianyunBaseOutputModel<AccessTokenOutputModel>("授权成功", "0") { Body = input };
        }
    }
}

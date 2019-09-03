using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{

    public class YilianyunBaseOutputModel<T>
    {
        public YilianyunBaseOutputModel() { }
        /// <summary>
        /// 自定义错误 error=-1,success:0
        /// </summary>
        /// <param name="customErrorDesc"></param>
        public YilianyunBaseOutputModel(string customErrorDesc, string errorCode = "-1")
        {
            Error = errorCode;
            Error_Description = customErrorDesc;
        }
        public string Error { get; set; }
        public string Error_Description { get; set; }
        public T Body { get; set; }
    }
    public class YilianyunBaseOutputModel : YilianyunBaseOutputModel<object>
    {
        public YilianyunBaseOutputModel() : base()
        { }
        public YilianyunBaseOutputModel(string customErrorDesc) : base(customErrorDesc)
        { }
        public YilianyunBaseOutputModel(string customErrorDesc, string errorCode) : base(customErrorDesc, errorCode)
        { }
    }


    public enum YilianyunClientType
    {
        开放应用,
        自有应用
    }
    public class YilianyunConfig
    {
        /// <summary>
        /// 应用ID
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 应用密钥
        /// </summary>
        public string ClientSecret { get; set; }
        /// <summary>
        /// 应用类型
        /// </summary>
        public YilianyunClientType YilianyunClientType { get; set; }
        /// <summary>
        /// token保存目录 默认 ./AppData
        /// </summary>
        public string SaveTokenDirPath { get; set; } = "./AppData";
        /// <summary>
        /// 接口地址 默认 https://open-api.10ss.net
        /// </summary>
        public string ApiUrl { get; set; } = "https://open-api.10ss.net";
        /// <summary>
        /// 接口超时时间 默认 30s
        /// </summary>
        public int? Timeout { get; set; } = 30;
    }
}

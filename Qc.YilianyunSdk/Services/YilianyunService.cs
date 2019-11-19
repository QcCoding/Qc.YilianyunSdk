using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Qc.YilianyunSdk
{

    public class YilianyunService
    {
        private readonly HttpClient _httpClient;
        private readonly IYilianyunSdkHook _yilianyunSdkHook;
        private YilianyunConfig _yilianyunConfig;
        public YilianyunService(IHttpClientFactory _httpClientFactory
            , IOptions<YilianyunConfig> configOptions
            , IYilianyunSdkHook yilianyunSdkHook)
        {
            _yilianyunConfig = configOptions?.Value;
            if (yilianyunSdkHook != null)
            {
                _yilianyunSdkHook = yilianyunSdkHook;
                _yilianyunConfig = _yilianyunSdkHook.GetClientConfig();
            }
            if (_yilianyunConfig == null)
                throw new Exception("打印机配置错误");
            _httpClient = _httpClientFactory.CreateClient("yilianyun");
            _httpClient.BaseAddress = new Uri(_yilianyunConfig.ApiUrl ?? "https://open-api.10ss.net");
            _httpClient.Timeout = TimeSpan.FromSeconds(_yilianyunConfig.Timeout ?? 30);
        }
        /// <summary>
        /// 获取初始化参数
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetInitPostData()
        {
            var timestamp = DateTime.Now.GetDateTimeStamp();
            Dictionary<string, object> dicData = new Dictionary<string, object>
            {
                { "client_id", _yilianyunConfig.ClientId },
                { "timestamp", timestamp },
                { "id", Guid.NewGuid().ToString() },
                { "scope", "all" },
                { "sign", (_yilianyunConfig.ClientId + timestamp + _yilianyunConfig.ClientSecret).ToMD5().ToLower() }
            };
            return dicData;
        }

        #region 授权接口

        /// <summary>
        /// 获取授权链接
        /// </summary>
        /// <param name="redirect_uri"></param>
        /// <returns></returns>
        public string GetAuthorizeUrl(string redirect_uri)
        {
            redirect_uri = System.Web.HttpUtility.UrlEncode(redirect_uri);
            var authUrl = $"{_yilianyunConfig.ApiUrl}/oauth/authorize?response_type=code&client_id={_yilianyunConfig.ClientId}&redirect_uri={redirect_uri}&state=1";
            return authUrl;
        }
        /// <summary>
        /// 获取 AccessToken
        /// </summary>
        /// <param name="code">开放应用需要code</param>
        /// <param name="skipSave">跳过保存</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> GetAccessToken(string code, bool skipSave = false)
        {
            Dictionary<string, object> dicData = GetInitPostData();
            if (_yilianyunConfig.YilianyunClientType == YilianyunClientType.开放应用)
            {
                if (string.IsNullOrEmpty(code))
                    return new YilianyunBaseOutputModel<AccessTokenOutputModel>("开放应用 code 不能为空");
                dicData.Add("grant_type", "authorization_code");
                dicData.Add("code", code);
            }
            else if (_yilianyunConfig.YilianyunClientType == YilianyunClientType.自有应用)
            {
                dicData.Add("grant_type", "client_credentials");
            }
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<AccessTokenOutputModel>>("/oauth/oauth", dicData);
            if (responseResult.IsError())
            {
                return new YilianyunBaseOutputModel<AccessTokenOutputModel>(responseResult.Error_Description, responseResult.Error);
            }
            if (responseResult.Body.Expires_In.HasValue)
                responseResult.Body.ExpiressEndTime = DateTime.Now.AddSeconds(responseResult.Body.Expires_In.Value);
            if (skipSave)
            {
                return responseResult;
            }
            return _yilianyunSdkHook.SaveToken(responseResult.Body);
        }
        /// <summary>
        /// 刷新 AccessToken
        /// </summary>
        /// <param name="machine_code">设备号，用于保存</param>
        /// <param name="refresh_token"></param>
        /// <param name="skipSave">跳过保存</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> RefreshToken(string machine_code, string refresh_token, bool skipSave = false)
        {
            if (string.IsNullOrEmpty(refresh_token))
                return new YilianyunBaseOutputModel<AccessTokenOutputModel>("refresh_token 不能为空");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("grant_type", "refresh_token");
            dicData.Add("refresh_token", refresh_token);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<AccessTokenOutputModel>>("/oauth/oauth", dicData);
            if (responseResult.IsError())
            {
                return new YilianyunBaseOutputModel<AccessTokenOutputModel>(responseResult.Error_Description, responseResult.Error);
            }
            responseResult.Body.Machine_Code = machine_code;
            if (responseResult.Body.Expires_In.HasValue)
                responseResult.Body.ExpiressEndTime = DateTime.Now.AddSeconds(responseResult.Body.Expires_In.Value);
            if (skipSave)
            {
                return responseResult;
            }
            return _yilianyunSdkHook.SaveToken(responseResult.Body);
        }
        /// <summary>
        /// 终端授权 (永久授权) 仅支持自有应用,将自动调用 GetAccessToken 授权
        /// </summary>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="msign">易联云终端密钥</param>
        /// <param name="phone">手机卡号码(可填)</param>
        /// <param name="print_name">自定义打印机名称(可填)</param>
        /// <param name="skipSave">跳过保存(可填)</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> AuthTerminal(string machine_code, string msign, string phone = null, string print_name = null, bool skipSave = true)
        {
            var accessModel = _yilianyunSdkHook.GetAccessToken(machine_code);
            if (accessModel == null || string.IsNullOrEmpty(accessModel.Access_Token))
            {
                var accessTokenResult = GetAccessToken(null, true);
                if (accessTokenResult.IsError())
                    return accessTokenResult;
                accessTokenResult.Body.PrinterName = print_name;
                accessTokenResult.Body.Phone = phone;
                accessTokenResult.Body.Machine_Code = machine_code;
                if (!skipSave)
                {
                    var saveTokenResult = _yilianyunSdkHook.SaveToken(accessTokenResult.Body);
                    if (saveTokenResult.IsError())
                        return saveTokenResult;
                }
                accessModel = accessTokenResult.Body;
            }
            Dictionary<string, object> dicData = GetInitPostData();
            //终端号
            dicData.Add("machine_code", machine_code);
            //终端密钥
            dicData.Add("msign", msign);
            //授权token
            dicData.Add("access_token", accessModel.Access_Token);
            //手机卡号
            if (!string.IsNullOrEmpty(phone))
                dicData.Add("phone", phone);
            //打印机名称
            if (!string.IsNullOrEmpty(print_name))
                dicData.Add("print_name", print_name);

            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<AccessTokenOutputModel>>("/printer/addprinter", dicData);
            if (responseResult.IsSuccess())
            {
                responseResult.Body = accessModel;
            }
            return responseResult;
        }
        /// <summary>
        /// 极速授权 仅支持开放应用 k4、w1、k6机型支持
        /// </summary>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="qr_key">特殊密钥(有效期为300秒)</param>
        /// <param name="printerName">自定义打印机名称</param>
        /// <param name="skipSave">跳过保存</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<AccessTokenOutputModel> AuthFast(string machine_code, string qr_key, string printerName = null, bool skipSave = false)
        {
            if (string.IsNullOrEmpty(machine_code)
                || string.IsNullOrEmpty(qr_key))
                return new YilianyunBaseOutputModel<AccessTokenOutputModel>("密钥不正确");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("machine_code", machine_code);
            dicData.Add("qr_key", qr_key);

            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<AccessTokenOutputModel>>("/oauth/scancodemodel", dicData);
            if (responseResult.IsError())
            {
                return new YilianyunBaseOutputModel<AccessTokenOutputModel>(responseResult.Error_Description, responseResult.Error);
            }
            if (responseResult.Body.Expires_In.HasValue)
                responseResult.Body.ExpiressEndTime = DateTime.Now.AddSeconds(responseResult.Body.Expires_In.Value);
            responseResult.Body.Machine_Code = machine_code;
            responseResult.Body.PrinterName = printerName;
            if (skipSave)
            {
                return responseResult;
            }
            return _yilianyunSdkHook.SaveToken(responseResult.Body);
        }

        /// <summary>
        /// 删除终端授权
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterDeleteprinter(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            //终端号
            dicData.Add("machine_code", machine_code);
            //授权token
            dicData.Add("access_token", access_token);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/deleteprinter", dicData);
            return responseResult;
        }
        #endregion

        #region 打印接口

        /// <summary>
        /// 打印文本
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="content">打印内容,无需进行URL编码</param>
        /// <param name="origin_id">商户系统内部订单号，要求32个字符内，只能是数字、大小写字母 ，且在同一个client_id下唯一</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrintText(string access_token, string machine_code, string content, string origin_id = null)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            if (string.IsNullOrEmpty(machine_code)
                || string.IsNullOrEmpty(content))
                return new YilianyunBaseOutputModel("打印内容不能为空");

            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("content", System.Web.HttpUtility.UrlEncode(content));
            dicData.Add("origin_id", origin_id ?? Guid.NewGuid().ToString("N"));
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/print/index", dicData);
            return responseResult;
        }
        /// <summary>
        /// 打印图形 不支持机型: k4-wh, k4-wa, m1
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="picture_url">线上图片地址,格式为 jpg，jpeg，png ， K4图片宽度不能超过384像素。理论上图片 （像素宽/8）*像素高 不能超过 100*1024。K5图片宽度不能超过108*8像素。理论上图片 （像素宽/8）*像素高 不能超过 200*1024。</param>
        /// <param name="origin_id">商户系统内部订单号，要求32个字符内，只能是数字、大小写字母 ，且在同一个client_id下唯一</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrintPicture(string access_token, string machine_code, string picture_url, string origin_id)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("picture_url", picture_url);
            dicData.Add("origin_id", origin_id);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/pictureprint/index", dicData);
            return responseResult;
        }

        /// <summary>
        /// 打印面单 不支持机型: k4-wh, k4-wa, m1 (k4系列机型不建议使用不干胶热敏纸)
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="origin_id">商户系统内部订单号，要求32个字符内，只能是数字、大小写字母 ，且在同一个client_id下唯一</param>
        /// <param name="contentJson">json字符串，详细说明：http://doc2.10ss.net/631855 </param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrintExpress(string access_token, string machine_code, string origin_id, string contentJson)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("origin_id", origin_id);
            dicData.Add("content", contentJson);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/expressprint/index", dicData);
            return responseResult;
        }

        #endregion

        #region 打印机设置

        /// <summary>
        /// 设置内置语音接口 仅支持K4-WA、K4-GAD、K4-WGEAD、k6型号
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="contentJson">播报内容 , 音量(1~9) , 声音类型(0,1,3,4) 组成json ! 示例 ["测试",9,0] 或者是在线语音链接! 语音内容请小于24kb</param>
        /// <param name="is_online_file">判断content是否为在线语音链接</param>
        /// <param name="aid">0~9 , 定义需设置的语音编号,若不提交,默认升序</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSetVoice(string access_token, string machine_code, string contentJson, bool is_online_file, int? aid = null)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("content", contentJson);
            dicData.Add("is_file", is_online_file);
            if (aid.HasValue)
                dicData.Add("aid", aid);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/setvoice", dicData);
            return responseResult;
        }
        /// <summary>
        /// 删除内置语音接口 仅支持K4-WA、K4-GAD、K4-WGEAD、k6型号
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="aid">	0~9 , 定义需删除的语音编号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterDeleteVoice(string access_token, string machine_code, string aid)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("aid", aid);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/setvoice", dicData);
            return responseResult;
        }
        /// <summary>
        /// 添加应用菜单  仅支持除k4-WA，k4-WH外的k4或w1机型
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="contentJson">json格式的应用菜单（其中url和菜单名称需要urlencode)</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrintMenuAdd(string access_token, string machine_code, string contentJson)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("content", contentJson);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printmenu/addprintmenu", dicData);
            return responseResult;
        }
        /// <summary>
        /// 关机重启接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="isRestart">是否重启/关机</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterRestart(string access_token, string machine_code, bool isRestart)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("response_type", isRestart ? "restart" : "shutdown");
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/shutdownrestart", dicData);
            return responseResult;
        }

        /// <summary>
        /// 声音调节接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="isBuzzer">是否蜂鸣器/喇叭</param>
        /// <param name="voice">[0,1,2,3] 4种音量设置</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSetSound(string access_token, string machine_code, bool isBuzzer, int voice)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("response_type", isBuzzer ? "buzzer" : "horn");
            dicData.Add("voice", voice);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/setsound", dicData);
            return responseResult;
        }
        /// <summary>
        /// 获取机型打印宽度接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterPrintinfo(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/printinfo", dicData);
            return responseResult;
        }

        /// <summary>
        /// 获取机型软硬件版本接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterGetVersion(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/getversion", dicData);
            return responseResult;
        }
        /// <summary>
        /// 设置logo接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="img_url">图片地址,图片宽度最大为350px,文件大小不能超过40Kb</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSeticon(string access_token, string machine_code, string img_url)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("img_url", img_url);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/seticon", dicData);
            return responseResult;
        }
        /// <summary>
        /// 取消logo接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterDeleteicon(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/deleteicon", dicData);
            return responseResult;
        }
        /// <summary>
        /// 设置按键打印（打印方式接口）
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="isOpen">是否开启按键打印</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSetBtnprint(string access_token, string machine_code, bool isOpen)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("response_type", isOpen ? "btnopen" : "btnclose");
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/btnprint", dicData);
            return responseResult;
        }
        /// <summary>
        /// 接单拒单设置接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="isOpen">是否开启接单拒单</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSetGetorder(string access_token, string machine_code, bool isOpen)
        {
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("response_type", isOpen ? "btnopen" : "btnclose");
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/getorder", dicData);
            return responseResult;
        }
        /// <summary>
        /// 设置推送url接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="cmd">打印完成标识oauth_finish，接单拒单标识oauth_getOrder，终端状态标识oauth_printStatus， 按键请求标识oauth_request</param>
        /// <param name="url">推送地址填写必须以http://或https://开头的地址。推送地址需要支持GET访问，当GET请求访问时，请直接返回{"data":"OK"}，用于推送地址的可用性测试</param>
        /// <param name="isOpen">是否开启接单拒单</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterSetPushurl(string access_token, string machine_code, string cmd, string url, bool isOpen)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("cmd", cmd);
            dicData.Add("url", url);
            dicData.Add("status", isOpen ? "open" : "close");
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/setpushurl", dicData);
            return responseResult;
        }

        /// <summary>
        /// 获取终端状态接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<PrinterStatusOutputModel> PrinterGetStatus(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel<PrinterStatusOutputModel>("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<PrinterStatusOutputModel>>("/printer/getprintstatus", dicData);
            return responseResult;
        }

        #endregion

        #region 订单接口

        /// <summary>
        /// 取消所有未打印订单
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterOrderCancelall(string access_token, string machine_code)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/cancelall", dicData);
            return responseResult;
        }
        /// <summary>
        /// 取消单条未打印订单
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel PrinterOrderCancelone(string access_token, string machine_code, string order_id)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("order_id", order_id);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel>("/printer/cancelone", dicData);
            return responseResult;
        }
        /// <summary>
        /// 获取订单状态接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="isOpen">是否开启接单拒单</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<PrinterOrderStatusOutputModel> PrinterOrderGetStatus(string access_token, string machine_code, string order_id)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel<PrinterOrderStatusOutputModel>("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("order_id", order_id);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<PrinterOrderStatusOutputModel>>("/printer/getorderstatus", dicData);
            return responseResult;
        }
        /// <summary>
        /// 获取订单列表接口
        /// </summary>
        /// <param name="access_token">授权的token 为null将查询hook中的AccessToken</param>
        /// <param name="machine_code">易联云打印机终端号</param>
        /// <param name="page_index">查询条件—当前页码,暂只提供前3页数据</param>
        /// <param name="page_size">查询条件—每页显示条数,每页最大条数100</param>
        /// <returns></returns>
        public YilianyunBaseOutputModel<List<PrinterOrderListOutputModel>> PrinterOrderGetPageingList(string access_token, string machine_code, int page_index, int page_size)
        {
            access_token = access_token ?? _yilianyunSdkHook.GetAccessToken(machine_code)?.Access_Token;
            if (string.IsNullOrEmpty(access_token))
                return new YilianyunBaseOutputModel<List<PrinterOrderListOutputModel>>("打印机未授权");
            Dictionary<string, object> dicData = GetInitPostData();
            dicData.Add("access_token", access_token);
            dicData.Add("machine_code", machine_code);
            dicData.Add("page_index", page_index);
            dicData.Add("page_size", page_size);
            var responseResult = _httpClient.HttpPost<YilianyunBaseOutputModel<List<PrinterOrderListOutputModel>>>("/printer/getorderpaginglist", dicData);
            return responseResult;
        }
        #endregion
    }
}

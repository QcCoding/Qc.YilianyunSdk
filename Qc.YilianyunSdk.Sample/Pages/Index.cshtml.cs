using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Qc.YilianyunSdk.Sample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly YilianyunService _yilianyunService;
        private readonly IYilianyunSdkHook _yilianyunSdkHook;
        public IndexModel(YilianyunService yilianyunService
            , IYilianyunSdkHook yilianyunSdkHook)
        {
            _yilianyunService = yilianyunService;
            _yilianyunSdkHook = yilianyunSdkHook;
        }
        #region 页面模型数据
        public string Message { get; set; }

        /// <summary>
        /// 授权token
        /// </summary>
        [BindProperty]
        public string AccessToken { get; set; }
        /// <summary>
        /// 刷新Token
        /// </summary>
        [BindProperty]
        public string RefreshToken { get; set; }
        /// <summary>
        /// 打印内容
        /// </summary>
        [BindProperty]
        public string PrintContent { get; set; }
        /// <summary>
        /// 设备号
        /// </summary>
        [BindProperty]
        public string MachineCode { get; set; }
        /// <summary>
        /// 特殊密钥
        /// </summary>
        [BindProperty]
        public string QrKey { get; set; }

        /// <summary>
        /// 终端密钥
        /// </summary>
        [BindProperty]
        public string Msign { get; set; }
        /// <summary>
        /// 打印机名称
        /// </summary>
        [BindProperty]
        public string PrinterName { get; set; }
        /// <summary>
        /// 手机卡号
        /// </summary>
        [BindProperty]
        public string Phone { get; set; }
        /// <summary>
        /// 终端状态
        /// </summary>
        [BindProperty]
        public string PrinterStatus { get; set; }
        #endregion
        public void OnGet()
        {

        }
        /// <summary>
        /// 开放应用授权认证回调
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        public IActionResult OnGetAuthCallback(string code)
        {
            var result = _yilianyunService.GetAccessToken(code);
            Message = result.IsSuccess() ? "授权成功" : ("错误信息：" + result.Error_Description);
            return Page();
        }
        /// <summary>
        /// 开放应用授权跳转
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostAuthRedirect()
        {
            var redirectUrl = Request.Scheme + "://" + Request.Host + "/AuthCallback";
            var oauthUrl = _yilianyunService.GetAuthorizeUrl(redirectUrl);
            return Redirect(oauthUrl);
        }
        /// <summary>
        /// 获取AccessToken
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostHistoryAccessToken()
        {
            var result = _yilianyunSdkHook.GetAccessToken(MachineCode);
            if (string.IsNullOrEmpty(result?.Access_Token))
            {
                Message = "未存储AccessToken";
            }
            else
            {
                Message = "获取AccessToken成功,到期时间：" + result.ExpiressEndTime;
            }
            AccessToken = result?.Access_Token;
            RefreshToken = result?.Refresh_Token;
            return Page();
        }
        /// <summary>
        /// 终端授权
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostAuthTerminal()
        {
            var result = _yilianyunService.AuthTerminal(MachineCode, Msign, Phone, PrinterName);
            Message = result.IsSuccess() ? "极速授权成功" : ("错误信息：" + result.Error_Description);
            AccessToken = null;
            RefreshToken = null;
            if (result.IsSuccess())
            {
                AccessToken = result.Body.Access_Token;
                RefreshToken = result.Body.Refresh_Token;
            }
            return Page();
        }
        /// <summary>
        /// 极速授权
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostAuthFast()
        {
            var result = _yilianyunService.AuthFast(MachineCode, QrKey);
            Message = result.IsSuccess() ? "极速授权成功" : ("错误信息：" + result.Error_Description);
            return Page();
        }
        /// <summary>
        /// 刷新token
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostRefreshToken()
        {
            var result = _yilianyunService.RefreshToken(MachineCode, RefreshToken);
            Message = result.IsSuccess() ? "刷新token成功" : ("错误信息：" + result.Error_Description);
            AccessToken = null;
            RefreshToken = null;
            if (result.IsSuccess())
            {
                AccessToken = result.Body.Access_Token;
                RefreshToken = result.Body.Refresh_Token;
            }
            return Page();
        }
        /// <summary>
        /// 打印机状态
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostPrinterStatus()
        {
            var result = _yilianyunService.PrinterGetStatus(AccessToken, MachineCode);
            if (result.IsSuccess())
            {
                PrinterStatus = result.Body.State.ToString();
            }
            Message = result.IsSuccess() ? "获取终端状态成功" : ("错误信息：" + result.Error_Description);
            return Page();
        }
        /// <summary>
        /// 打印文本
        /// </summary>
        /// <returns></returns>
        public IActionResult OnPostPrintText()
        {
            var result = _yilianyunService.PrintText(AccessToken, MachineCode, PrintContent);
            Message = result.IsSuccess() ? "打印文本成功" : ("错误信息：" + result.Error_Description);
            return Page();
        }
    }
}

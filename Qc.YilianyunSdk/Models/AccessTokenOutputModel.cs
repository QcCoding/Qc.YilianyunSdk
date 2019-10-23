using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class AccessTokenOutputModel
    {
        /// <summary>
        /// 易联云打印机终端号
        /// </summary>
        public string Machine_Code { get; set; }
        /// <summary>
        /// 打印机名称
        /// </summary>
        public string PrinterName { get; set; }
        /// <summary>
        /// 手机卡号码(自由应用保存时返回)
        /// </summary>
        public string Phone { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public int? Expires_In { get; set; }
        public DateTime? ExpiressEndTime { get; set; }
    }
}

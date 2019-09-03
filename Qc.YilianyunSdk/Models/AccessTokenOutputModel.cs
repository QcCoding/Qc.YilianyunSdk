using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class AccessTokenOutputModel
    {
        /// <summary>
        /// 打印机名称
        /// </summary>
        public string PrinterName { get; set; }
        public string Access_Token { get; set; }
        public string Refresh_Token { get; set; }
        public int? Expires_In { get; set; }
        public string Machine_Code { get; set; }
        public DateTime? ExpiressEndTime { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class PrinterStatusOutputModel
    {
        /// <summary>
        /// 0离线 1在线 2缺纸
        /// </summary>
        public PrinterStatusType State { get; set; }
    }
    public enum PrinterStatusType
    {
        离线,
        在线,
        缺纸
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class PrinterOrderStatusOutputModel
    {
        /// <summary>
        /// 订单号
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 商户系统内部订单号
        /// </summary>
        public string Origin_Id { get; set; }
        /// <summary>
        /// 1已打印 0未打印或已取消
        /// </summary>
        public int Status { get; set; }
    }
}

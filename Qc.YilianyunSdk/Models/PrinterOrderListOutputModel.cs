using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk
{
    public class PrinterOrderListOutputModel
    {
        /// <summary>
        /// 易联云订单id
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// 接收打印数据时间
        /// </summary>
        public string receiving_time { get; set; }
        /// <summary>
        /// 打印完成时间,若等于0且status也为0, 说明该订单已被取消打印
        /// </summary>
        public string print_time { get; set; }
        /// <summary>
        /// 商户内部系统订单号
        /// </summary>
        public string origin_id { get; set; }
        /// <summary>
        /// 1已打印 0未打印
        /// </summary>
        public string status { get; set; }
        /// <int>
        /// 打印内容
        /// </summary>
        public string content { get; set; }


    }
}

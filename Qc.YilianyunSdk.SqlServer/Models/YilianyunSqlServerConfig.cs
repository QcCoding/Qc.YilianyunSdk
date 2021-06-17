using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk.SqlServer.Models
{
    public class YilianyunSqlServerConfig : YilianyunConfig
    {
        /// <summary>
        /// token保存数据库链接
        /// </summary>
        public string SaveConnection { get; set; }
        public TimeSpan? TokenSlidingExpiration { get; set; } = TimeSpan.FromMinutes(2);

    }
}

using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Qc.YilianyunSdk
{
    public static class DateTimeExtension
    {
        public static int GetDateTimeStamp(this DateTime dt)
        {
            DateTime dateStart = new DateTime(1970, 1, 1, 0, 0, 0);
            int timeStamp = Convert.ToInt32((dt.ToUniversalTime() - dateStart).TotalSeconds);
            return timeStamp;
        }
    }
}

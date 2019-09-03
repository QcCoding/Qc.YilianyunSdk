using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Qc.YilianyunSdk.Utils
{
    public class JsonHelper
    {
        public static string Serialize(object o)
        {
            var setting = new JsonSerializerSettings()
            {
                ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
            };
            return JsonConvert.SerializeObject(o, setting);
        }

        public static T Deserialize<T>(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                    return JsonConvert.DeserializeObject<T>(input);
            }
            catch
            {
            }
            return default(T);
        }

        public static dynamic Deserialize(string input)
        {
            try
            {
                if (!string.IsNullOrEmpty(input))
                    return JsonConvert.DeserializeObject(input);
            }
            catch
            {
            }
            return null;
        }
    }
}

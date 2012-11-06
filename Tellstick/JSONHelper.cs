using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
//using System.Runtime.Serialization.Json;
//using System.Web.Script.Serialization;
using Newtonsoft.Json;

namespace Tellstick
{
    public class JSONHelper
    {
        public static T Deserialise<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json);


            //JavaScriptSerializer serializer = new JavaScriptSerializer();
            //DeviceContainer test = serializer.Deserialize<DeviceContainer>(json);
            //return serializer.Deserialize<T>(json);


            //T obj = Activator.CreateInstance<T>();
            //MemoryStream ms = new MemoryStream(Encoding.Unicode.GetBytes(json));
            //DataContractJsonSerializer serialiser = new DataContractJsonSerializer(obj.GetType());
            //obj = (T)serialiser.ReadObject(ms);
            //ms.Close();
            //return obj;
        }
    }
}

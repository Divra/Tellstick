using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using System.IO;

namespace Tellstick
{
    [DataContract]
    public class Sensor
    {
        public static Dictionary<int, DateTime> sensorLastUpdate = new Dictionary<int, DateTime>();

        [DataMember]
        string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        int id;
        public int Id
        {
            get { return id; }
            set { id = value; }
        }

        [DataMember]
        int lastupdated;
        [DataMember]
        int ignored;
        [DataMember]
        int client;
        [DataMember]
        string clientName;
        [DataMember]
        int online;
        [DataMember]
        int editable;

        [DataMember]
        List<SensorData> data;
        [DataMember]
        string protocol;
        [DataMember]
        int sensorId;
        public int SensorId
        {
            get { return sensorId; }
            set { sensorId = value; }
        }
        [DataMember]
        int timezoneOffset;


        public DateTime myLastUpdated;

        public List<SensorData> Data
        {
            get { return data; }
            set { data = value; }
        }

        public void SaveToFile()
        {
            if (!sensorLastUpdate.ContainsKey(sensorId))
                sensorLastUpdate.Add(sensorId, DateTime.Now);
            else if (DateTime.Now.Subtract(sensorLastUpdate[sensorId]).TotalSeconds<60)
                return;

            FileStream fs = File.Open("Sensor_" + sensorId + ".txt", FileMode.Append);
            byte[] text = System.Text.Encoding.ASCII.GetBytes(myLastUpdated.ToString("yyyyMMdd hh:mm:ss") + ";" + Data[0].Value + Environment.NewLine);
            fs.Write(text, 0, text.Length);
            fs.Close();

            sensorLastUpdate[sensorId] = DateTime.Now;
        }

    }
    [DataContract]
    public class SensorData
    {
        [DataMember]
        string name;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        [DataMember]
        string value;

        public string Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }

    [DataContract]
    public class SensorContainer
    {
        [DataMember]
        List<Sensor> sensor;

        public List<Sensor> Sensors
        {
            get { return sensor; }
            set { sensor = value; }
        }
    }

}

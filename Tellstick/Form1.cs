using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Net;
using OAuth;
using System.IO;
using System.Xml;
using System.Runtime.Serialization;
using System.Configuration;

namespace Tellstick
{
    public partial class Form1 : Form
    {
        Model model = new Model();
        Manager m;
        List<Device> devices = new List<Device>();
        public Form1()
        {
            InitializeComponent();

            m = new Manager();
            m["consumer_key"] = ConfigurationManager.AppSettings["consumer_key"];
            m["consumer_secret"] = ConfigurationManager.AppSettings["consumer_secret"];

            m["token"] = ConfigurationManager.AppSettings["token"];
            m["token_secret"] = ConfigurationManager.AppSettings["token_secret"];

            UpdateDevices();
            tmrUpdateDevices.Start();
        }

        void UpdateDevices()
        {
            string url = "https://api.telldus.com/json/devices/list";
            string deviceJson = "";
            try
            {
                deviceJson = GetResponse(url);
            }
            catch (Exception)
            {
                return;
            }
            // Get all devices from JSON
            DeviceContainer allDevices = JSONHelper.Deserialise<DeviceContainer>(deviceJson);

            flowLayoutPanel1.Controls.Clear();

            foreach (Device dev in allDevices.Devices)
            {
                Button btnOn = new Button();
                btnOn.Text = dev.Name + " (på)";
                btnOn.Tag = dev.Id;
                btnOn.Click += new EventHandler(btn_Click);
                btnOn.Width = (flowLayoutPanel1.Width / 2) - 10;
                flowLayoutPanel1.Controls.Add(btnOn);

                Button btnOff = new Button();
                btnOff.Text = dev.Name + " (av)";
                btnOff.Tag = dev.Id;
                btnOff.Click += new EventHandler(btnOff_Click);
                btnOff.Width = (flowLayoutPanel1.Width / 2) - 10;
                flowLayoutPanel1.Controls.Add(btnOff);
            }

            url = "https://api.telldus.com/json/sensors/list";
            string sensorJson = "";
            try
            {
                sensorJson = GetResponse(url);
            }
            catch (Exception)
            {
                return;
            }
            SensorContainer allSensors = JSONHelper.Deserialise<SensorContainer>(sensorJson);

            for (int i = 0; i < allSensors.Sensors.Count; i++)
            {
                Sensor sensor = allSensors.Sensors[i];
                UpdateSensor(ref sensor);
                allSensors.Sensors[i] = sensor;
                Label lbl = new Label();
                lbl.Text = sensor.Name + ": " + sensor.Data[0].Value;
                flowLayoutPanel1.Controls.Add(lbl);

                //sensor.SaveToFile();
                model.SaveSensorValue(sensor.SensorId, double.Parse(sensor.Data[0].Value.Replace(".", ",")));
            }
            List<Trigger> triggers = model.GetTriggers().ToList();
            CheckTriggers(triggers, allSensors);
        }

        private void UpdateSensor(ref Sensor sensor)
        {
            string url = "https://api.telldus.com/json/sensor/info?id=" + sensor.Id;
            string sensorJson = "";
            try
            {
                sensorJson = GetResponse(url);
            }
            catch (Exception)
            {
                return;
            }

            sensor = JSONHelper.Deserialise<Sensor>(sensorJson);
            sensor.myLastUpdated = DateTime.Now;
        }

        void btn_Click(object sender, EventArgs e)
        {
            TurnOn((int)((Button)sender).Tag);
        }
        void btnOff_Click(object sender, EventArgs e)
        {
            TurnOff((int)((Button)sender).Tag);
        }

        void TurnOn(int deviceId)
        {
            GetResponse("https://api.telldus.com/json/device/command?id=" + deviceId + "&method=1&value=1");
        }

        void TurnOff(int deviceId)
        {
            GetResponse("https://api.telldus.com/json/device/command?id=" + deviceId + "&method=2&value=2");
        }

        string GetResponse(string url)
        {
            string authzHeader = m.GenerateAuthzHeader(url, "GET");

            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";

            req.ServicePoint.Expect100Continue = false;
            req.ContentType = "application/x-www-form-urlencoded";

            req.Headers["Authorization"] = authzHeader;
            req.KeepAlive = false;

            WebResponse response = req.GetResponse();

            Stream ms = response.GetResponseStream();

            Encoding encode = System.Text.Encoding.GetEncoding("utf-8");

            // Pipe the stream to a higher level stream reader with the required encoding format. 
            StreamReader readStream = new StreamReader(ms, encode);
            Char[] read = new Char[256];

            // Read 256 charcters at a time.    
            int count = readStream.Read(read, 0, 256);

            string respText = "";

            while (count > 0)
            {
                // Dump the 256 characters on a string and display the string onto the console.
                respText += new String(read, 0, count);
                count = readStream.Read(read, 0, 256);
            }

            // Release the resources of stream object.
            readStream.Close();

            // Release the resources of response object.
            response.Close();

            if (respText.StartsWith("Could not select database"))
                throw new Exception("Telldus said: " + respText);

            return respText;
        }

        private void tmrUpdateDevices_Tick(object sender, EventArgs e)
        {
            UpdateDevices();
        }

        private void CheckTriggers(IEnumerable<Trigger> triggers, SensorContainer sensors)
        {
            foreach (Trigger t in triggers)
            {
                Sensor sensor = sensors.Sensors.FirstOrDefault(s => s.SensorId == t.sensorId);
                if (sensor == null)
                {
                    model.AddTriggerLog(t, "Error: Sensor " + t.sensorId + " not found");
                    continue;
                }
                double sensorValue = double.Parse(sensor.Data[0].Value.Replace(".", ","));
                bool valueMatch = ((t.triggerAbove && sensorValue > t.limit) || (!t.triggerAbove && sensorValue < t.limit));
                if (t.canTrigger && valueMatch)
                {
                    model.AddTriggerLog(t, "Triggered, value was " + sensor.Data[0].Value);
                    t.lastTriggered = DateTime.Now;
                    t.canTrigger = false;

                    //if (t.command == "on")
                    //    TurnOn(t.deviceId);
                    //else if (t.command == "off")
                    //    TurnOff(t.deviceId);

                    model.UpdateTrigger(t);
                }
                else if (!t.canTrigger && !valueMatch)
                {
                    // Already triggered, now value is outside the limit, reset canTrigger
                    model.AddTriggerLog(t, "Reset, value was " + sensor.Data[0].Value);
                    t.canTrigger = true;
                    model.UpdateTrigger(t);
                }
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.SqlClient;

namespace Tellstick
{
    public class Model
    {
        SqlConnection connection = new SqlConnection("Server=(local);Database=hornon;Trusted_Connection=True;");

        public IEnumerable<Trigger> GetTriggers()
        {
            List<Trigger> triggers = new List<Trigger>();
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT [triggerId], [triggerName], [sensorId], [limit], [triggerAbove], [lastTriggered], [deviceId], [command], [canTrigger] FROM [hornon].[dbo].[Triggers]", connection);
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                DateTime? lastTriggered = null;
                if (reader["lastTriggered"] != DBNull.Value)
                    lastTriggered = (DateTime)reader["lastTriggered"];
                triggers.Add(new Trigger((int)reader["triggerId"], (string)reader["triggerName"], (int)reader["sensorId"], (double)reader["limit"], (bool)reader["triggerAbove"], lastTriggered, (int)reader["deviceId"], (string)reader["command"], (bool)reader["canTrigger"]));
            }

            connection.Close();
            return triggers;
        }

        public void AddTriggerLog(Trigger t, string text)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand cmd = new SqlCommand("INSERT INTO [hornon].[dbo].[TriggerLog] ([triggerId], [logText]) VALUES (" + t.triggerId + ", '" + text.Replace("'", "''") + "')", connection);
            cmd.ExecuteNonQuery();

            connection.Close();
        }


        public void SaveSensorValue(int sensorId, double data)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            SqlCommand cmd = new SqlCommand("SELECT TOP 2 value FROM [hornon].[dbo].[SensorValues] WHERE sensorId = " + sensorId + " ORDER BY timeRead DESC", connection);
            double oldValue1 = double.NegativeInfinity;
            double oldValue2 = double.NegativeInfinity;
            SqlDataReader reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                oldValue1 = (double)reader["value"];
                if (reader.Read())
                {
                    oldValue2 = (double)reader["value"];
                    if (oldValue1 == oldValue2 && oldValue1 == data)
                    {
                        reader.Close();
                        cmd = new SqlCommand("DELETE FROM [hornon].[dbo].[SensorValues] WHERE sensorId = " + sensorId + " AND timeRead = (SELECT TOP 1 timeRead FROM [hornon].[dbo].[SensorValues] WHERE sensorId = " + sensorId + " ORDER BY timeRead DESC)", connection);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            if (!reader.IsClosed)
                reader.Close();


            //if ((double)cmd.ExecuteScalar() != data)
            //{
                cmd = new SqlCommand("INSERT INTO [hornon].[dbo].[SensorValues] ([sensorId], [timeRead], [value]) VALUES (" + sensorId + ", '" + DateTime.Now.ToString("yyyyMMdd HH:mm:ss") + "', " + data.ToString().Replace(",", ".") + ") ", connection);
                cmd.ExecuteNonQuery();
            //}
            connection.Close();
        }


        public void UpdateTrigger(Trigger t)
        {
            if (connection.State != System.Data.ConnectionState.Open)
                connection.Open();

            string dateTime = "NULL";
            if (t.lastTriggered.HasValue)
                dateTime = "'" + t.lastTriggered.Value.ToString("yyyyMMdd HH:mm:ss") + "'";

            SqlCommand cmd = new SqlCommand("UPDATE [hornon].[dbo].[Triggers] SET lastTriggered = " + dateTime + ", canTrigger = '" + (t.canTrigger ? "1" : "0") + "' WHERE triggerId = " + t.triggerId, connection);
            cmd.ExecuteNonQuery();

            connection.Close();
        }
    }



    public class Trigger
    {
        public int triggerId;
        public string triggerName;
        public int sensorId;
        public double limit;
        public bool triggerAbove;
        public DateTime? lastTriggered;
        public int deviceId;
        public string command;
        public bool canTrigger;

        public Trigger(int triggerId, string triggerName, int sensorId, double limit, bool triggerAbove, DateTime? lastTriggered, int deviceId, string command, bool canTrigger)
        {
            this.triggerId = triggerId;
            this.triggerName = triggerName;
            this.sensorId = sensorId;
            this.limit = limit;
            this.triggerAbove = triggerAbove;
            this.lastTriggered = lastTriggered;
            this.deviceId = deviceId;
            this.command = command;
            this.canTrigger = canTrigger;
        }
    }
}

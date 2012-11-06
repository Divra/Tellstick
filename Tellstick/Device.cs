using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace Tellstick
{
    [DataContract]
    public class Device
    {
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
        int state;
        [DataMember]
        int? statevalue;
        [DataMember]
        int methods;
        [DataMember]
        int client;
        [DataMember]
        string clientName;
        [DataMember]
        int online;
        [DataMember]
        int editable;
    }

    [DataContract]
    public class DeviceContainer
    {
        [DataMember]
        List<Device> device;

        public List<Device> Devices
        {
            get { return device; }
            set { device = value; }
        }

    }

}

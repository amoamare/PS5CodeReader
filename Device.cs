using System.Xml.Linq;

namespace PS5CodeReader
{
    internal class Device
    {
        internal string Port { get; }
        internal string FriendlyName { get; }
        internal string InstanceId { get; }
        internal string DeviceLocationPath { get; }
        internal bool IsProductIdSet { get; }
        internal bool IsVenderIdSet { get; }
        internal bool HasProductAndVendorId => IsProductIdSet && IsVenderIdSet;
        internal int ProductId { get; }
        internal int VendorId { get; }
        internal string DeviceParent { get; }
        internal string DeviceSerialNumber { get; }
        internal Device(string port, string deviceDescription)
        {
            Port = port;
            FriendlyName = deviceDescription;
        }


        internal Device(string port, string deviceDescription, string deviceLocationPath) :this(port, deviceDescription)
        {
            DeviceLocationPath = deviceLocationPath;
        }

        internal Device(string port, string deviceDescription, string instanceId, string deviceLocationPath, string deviceParent) : this(port, deviceDescription, deviceLocationPath)
        {
            InstanceId = instanceId;
            DeviceParent = deviceParent;
            //USB\\VID_04E8&PID_6860\\4200BF9BE4B315BF
            var split = DeviceParent.Split('\\');
            if (split != null && split.Length > 0)
                DeviceSerialNumber = split.LastOrDefault();
           
        }

        public override string ToString()
        {
            return $"{FriendlyName} ({Port})";
        }
    }
}

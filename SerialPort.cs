using System.Runtime.InteropServices;
using System.Text;
using static Vanara.PInvoke.SetupAPI;
using static Vanara.PInvoke.AdvApi32;
using System.Security.AccessControl;
using Vanara.InteropServices;
using Vanara.PInvoke;
using Vanara.Extensions;

namespace PS5CodeReader
{
    internal class SerialPort : System.IO.Ports.SerialPort
    {
        internal static IEnumerable<Device> SelectSerial(bool isSorted = true, Func<Device, bool>? filter = null)
        {
            var guid = GetGuidFromClassName(@"Ports");
            var autoDevice = new Device("Detect Device Automatically", "Auto");
            var deviceList = new List<Device>();
            deviceList.AddRange(GetDeviceByGuid(guid, filter));
            if (isSorted)
            {
                deviceList = deviceList.OrderBy(x => x.Port.Length).ThenBy(x => x.Port).ToList();
            }
            deviceList.Insert(0, autoDevice);
            return deviceList;
        }

        private static IEnumerable<Device> GetDeviceByGuid(Guid guid, Func<Device, bool>? filter = null)
        {
            var hDevInfo = SetupDiGetClassDevs(guid, Flags: DIGCF.DIGCF_PRESENT);
            if (hDevInfo == IntPtr.Zero)
            {
                throw new Exception("Failed to get device information set for the Modem ports");
            }

            try
            {
                var devices = new List<Device>();
                SetupDiEnumDeviceInfo(hDevInfo).ToList()?.ForEach(hDevInfoData =>
                {
                    var flag = GetDeviceName(hDevInfo, hDevInfoData, out var name);
                  //  if (flag) return;
                     GetDeviceDescription(hDevInfo, hDevInfoData, out var description);
                    devices.Add(new Device(name, description));

                });
                return devices;
            }
            finally
            {
                SetupDiDestroyDeviceInfoList(hDevInfo);
            }
        }


        internal static Guid GetGuidFromClassName(string name)
        {
            var guidArray = new Guid[20];
            var status = SetupDiClassGuidsFromName(name, guidArray, 20, out var size);
            if (status)
            {
                if (1 >= size) return guidArray.First();
                guidArray = new Guid[size];
                SetupDiClassGuidsFromName(name, guidArray, size, out size);
                if (size >= 1) return guidArray.First();
            }
            else
            {
                // throw new Exception("Failed to get device information set for the COM ports");
            }
            return Guid.Empty;
        }

        internal static string GetClassNameFromGuid(Guid guid)
        {
            var size = 0;
            SetupDiClassNameFromGuid(guid, out var className);
            return className;
        }

        private static bool GetDeviceName(SafeHDEVINFO hDevInfo, SP_DEVINFO_DATA deviceInfoData, out string? name)
        {
            name = null;
            const string portName = @"PortName";
            var hDeviceRegistryKey = SetupDiOpenDevRegKey(hDevInfo, deviceInfoData, DICS_FLAG.DICS_FLAG_GLOBAL, 0, DIREG.DIREG_DEV, RegistryRights.QueryValues);
            if (hDeviceRegistryKey.IsInvalid)
            {
                // Debug.WriteLine("Failed to open a registry key for device-specific configuration information");
                return false;
            }
            try
            {
                using var mem = new SafeHGlobalHandle(1024);
                var memSz = (uint)mem.Size;
                RegQueryValueEx(hDeviceRegistryKey, portName, default, out _, mem, ref memSz);
                var success = RegQueryValueEx(hDeviceRegistryKey, portName, default, out _, mem, ref memSz);
                if (success == Win32Error.ERROR_SUCCESS)
                {
                    name = mem.ToString(-1, CharSet.Auto);
                    return true;
                }
                return false;
            }
            finally
            {
                RegCloseKey(hDeviceRegistryKey);
            }
        }
        private static string GetDeviceDescription(SafeHDEVINFO hDevInfo, SP_DEVINFO_DATA hDevInfoData, out string? description)
        {
            GetValue(hDevInfo, hDevInfoData, SPDRP.SPDRP_DEVICEDESC, out object value);
            description = value.ToString();
            return description;

        }

        private static Win32Error GetValue(SafeHDEVINFO hDevInfo, SP_DEVINFO_DATA hDevInfoData, SPDRP propKey, out object value)
        {
            value = null;
            if (!SetupDiGetDeviceRegistryProperty(hDevInfo, hDevInfoData, propKey, out _, default, 0, out var bufSz))
            {
                if (bufSz == 0)
                    return Win32Error.ERROR_NOT_FOUND;
                using var mem = new SafeCoTaskMemHandle(bufSz);
                if (!SetupDiGetDeviceRegistryProperty(hDevInfo, hDevInfoData, propKey, out var propType, mem, mem.Size, out bufSz))
                    return Win32Error.GetLastError();
                value = GetRegValue(propKey, mem, propType);
            }
            return Win32Error.ERROR_SUCCESS;
        }

        internal static object GetRegValue<T>(T key, SafeAllocatedMemoryHandle mem, REG_VALUE_TYPE propType) where T : Enum =>
                        GetRegValue(mem, propType, CorrespondingTypeAttribute.GetCorrespondingTypes(key).FirstOrDefault());

        internal static object GetRegValue(SafeAllocatedMemoryHandle mem, REG_VALUE_TYPE propType, Type cType = null) => propType switch
        {
            REG_VALUE_TYPE.REG_DWORD when cType is not null => ((IntPtr)mem).Convert(mem.Size, cType),
            REG_VALUE_TYPE.REG_BINARY when cType is not null && cType != typeof(byte[]) => ((IntPtr)mem).Convert(mem.Size, cType),
            _ => propType.GetValue(mem, mem.Size),
        };

        //private static string GetDeviceLocationPath(IntPtr deviceInfoSet, SpDevInfoData deviceInfoData)
        //{
        //    var flag = SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SpDrp.SPDRP_LOCATION_PATHS, out _, null, 0, out var size);
        //    if (!flag) return string.Empty;
        //    var description = new StringBuilder(size);
        //    var success = SetupDiGetDeviceRegistryProperty(deviceInfoSet, ref deviceInfoData, SpDrp.SPDRP_LOCATION_PATHS, out _, description, size, out size);
        //    return success ? description.ToString() : string.Empty;
        //}

        //internal static string GetDeviceInstanceId(IntPtr deviceInfoSet, SpDevInfoData deviceInfoData)
        //{
        //    var flag = SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, null, 0, out var size);
        //    if (!flag) return string.Empty;
        //    var id = new StringBuilder(size);
        //    var success = SetupDiGetDeviceInstanceId(deviceInfoSet, ref deviceInfoData, id, size, out size);
        //    return success ? id.ToString() : string.Empty;
        //}

        //internal static string GetDeviceParent(IntPtr deviceInfoSet, SpDevInfoData deviceInfoData)
        //{
        //    var error = CM_Get_Parent(out var ptrPrevious, deviceInfoData.devInst, 0);
        //    if (error != CfgMgrErrorCode.CR_SUCCESS || error == CfgMgrErrorCode.CR_NO_SUCH_VALUE)
        //        return string.Empty;
        //    CM_Get_Device_ID_Size(out var size, ptrPrevious);
        //    var buffer = new StringBuilder(size);
        //    var errorCode = CM_Get_Device_ID(ptrPrevious, buffer, buffer.Capacity, 0);
        //    buffer.Length -= 1; //resize to account for null termination
        //    return errorCode == CfgMgrErrorCode.CR_SUCCESS ? buffer.ToString() : string.Empty;
        //}


        private static byte CalculateChecksum(string data)
        {
            var checksum = 0;
            checksum = Encoding.ASCII.GetBytes(data).Sum(x => x);
            var test = checksum + 256;
            return (byte)(test % 256);
        }

        internal new void Write(string command)
        {
            var checkSum = CalculateChecksum(command);
            var formattedCommand = $"{command}:{checkSum:X2}\r\n";
            var commandBytes = Encoding.ASCII.GetBytes(formattedCommand);
            Write(commandBytes, 0 , commandBytes.Length);
        }

    }
}

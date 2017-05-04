using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using DataBridge.Extensions;
using DataBridge.Helper;

namespace DataBridge.Services
{
    public class UsbDeviceWatcher : IDisposable
    {
        private ManagementEventWatcher insertWatcher = new ManagementEventWatcher();

        private ManagementEventWatcher removeWatcher = new ManagementEventWatcher();

        private bool enableRaisingEvents = true;

        public event EventHandler<EventArgs<UsbDeviceInfo>> OnDeviceInserted;

        public event EventHandler<EventArgs<UsbDeviceInfo>> OnDeviceRemoved;

        public UsbDeviceWatcher()
        {
            this.insertWatcher.Query = new WqlEventQuery("SELECT * FROM __InstanceCreationEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            this.insertWatcher.EventArrived += this.Device_Inserted;

            this.removeWatcher.Query = new WqlEventQuery("SELECT * FROM __InstanceDeletionEvent WITHIN 2 WHERE TargetInstance ISA 'Win32_USBHub'");
            this.removeWatcher.EventArrived += this.Device_Removed;
        }

        public void Start()
        {
            this.insertWatcher.Start();
            this.removeWatcher.Start();
        }

        public void Stop()
        {
            this.insertWatcher.Stop();
            this.removeWatcher.Stop();
        }

        public bool EnableRaisingEvents
        {
            get { return this.enableRaisingEvents; }
            set { this.enableRaisingEvents = value; }
        }

        private void Device_Inserted(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            var deviceInfo = new UsbDeviceInfo(
                            instance.GetPropertyValue("DeviceID").ToStringOrEmpty(),
                            instance.GetPropertyValue("PNPDeviceID").ToStringOrEmpty(),
                            instance.GetPropertyValue("Description").ToStringOrEmpty()
                            );
            if (this.OnDeviceInserted != null && this.EnableRaisingEvents)
            {
                this.OnDeviceInserted(this, new EventArgs<UsbDeviceInfo>(deviceInfo));
            }
        }

        private void Device_Removed(object sender, EventArrivedEventArgs e)
        {
            var instance = (ManagementBaseObject)e.NewEvent["TargetInstance"];

            var deviceInfo = new UsbDeviceInfo(
                            instance.GetPropertyValue("DeviceID").ToStringOrEmpty(),
                            instance.GetPropertyValue("PNPDeviceID").ToStringOrEmpty(),
                            instance.GetPropertyValue("Description").ToStringOrEmpty()
                            );
            if (this.OnDeviceRemoved != null && this.EnableRaisingEvents)
            {
                this.OnDeviceRemoved(this, new EventArgs<UsbDeviceInfo>(deviceInfo));
            }
        }

        public void Dispose()
        {
            if (this.insertWatcher != null)
            {
                this.insertWatcher.Stop();
                this.insertWatcher.EventArrived -= this.Device_Inserted;
                this.insertWatcher.Dispose();
                this.insertWatcher = null;
            }

            if (this.removeWatcher != null)
            {
                this.removeWatcher.Stop();
                this.removeWatcher.EventArrived -= this.Device_Removed;
                this.removeWatcher.Dispose();
                this.removeWatcher = null;
            }
        }

        //public UsbDiskCollection GetAvailableDisks()
        //{
        //    UsbDiskCollection disks = new UsbDiskCollection();

        //    // browse all USB WMI physical disks
        //    foreach (ManagementObject drive in
        //        new ManagementObjectSearcher(
        //            "select DeviceID, Model from Win32_DiskDrive " +
        //             "where InterfaceType='USB'").Get())
        //    {
        //        // associate physical disks with partitions
        //        ManagementObject partition = new ManagementObjectSearcher(String.Format(
        //            "associators of {{Win32_DiskDrive.DeviceID='{0}'}} " +
        //                  "where AssocClass = Win32_DiskDriveToDiskPartition",
        //            drive["DeviceID"])).First();

        //        if (partition != null)
        //        {
        //            // associate partitions with logical disks (drive letter volumes)
        //            ManagementObject logical = new ManagementObjectSearcher(String.Format(
        //                "associators of {{Win32_DiskPartition.DeviceID='{0}'}} " +
        //                    "where AssocClass= Win32_LogicalDiskToPartition",
        //                partition["DeviceID"])).First();

        //            if (logical != null)
        //            {
        //                // finally find the logical disk entry
        //                ManagementObject volume = new ManagementObjectSearcher(String.Format(
        //                    "select FreeSpace, Size, VolumeName from Win32_LogicalDisk " +
        //                     "where Name='{0}'",
        //                    logical["Name"])).First();

        //                UsbDisk disk = new UsbDisk(logical["Name"].ToString());
        //                disk.Model = drive["Model"].ToString();
        //                disk.VolumeName = volume["VolumeName"].ToString();
        //                disk.FreeSpace = (ulong)volume["FreeSpace"];
        //                disk.Size = (ulong)volume["Size"];

        //                disks.Add(disk);
        //            }
        //        }
        //    }

        //    return disks;
        //}

        public class UsbDeviceInfo
        {
            public UsbDeviceInfo(string deviceID, string pnpDeviceID, string description)
            {
                this.DeviceID = deviceID;
                this.PnpDeviceID = pnpDeviceID;
                this.Description = description;
                var usbDisk = this.GetUsbDisks().FirstOrDefault();
                if (usbDisk != null)
                {
                    this.DriveLetter = usbDisk.DriveLetter;
                    this.VolumeName = usbDisk.VolumeName;
                }
            }

            public string DeviceID { get; private set; }

            public string PnpDeviceID { get; private set; }

            public string Description { get; private set; }

            public string DriveLetter { get; private set; }

            public string VolumeName { get; private set; }

            private IEnumerable<UsbDisk> GetUsbDisks()
            {
                Thread.Sleep(50);

                var usbDisks = new List<UsbDisk>();
                using (Device device = Device.Get(this.PnpDeviceID))
                {
                    // get children devices
                    foreach (string childDeviceId in device.ChildrenPnpDeviceIds)
                    {
                        // get the drive object that correspond to this id (escape the id)
                        var driveSearcher = new ManagementObjectSearcher("SELECT DeviceID FROM Win32_DiskDrive WHERE PNPDeviceID='" + childDeviceId.Replace(@"\", @"\\") + "'");
                        using (var drives = driveSearcher.Get())
                        {
                            foreach (var drive in drives)
                            {
                                var partitionSearcher = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskDrive.DeviceID='" + drive["DeviceID"] + "'} WHERE AssocClass=Win32_DiskDriveToDiskPartition");
                                using (var partitions = partitionSearcher.Get())
                                {
                                    // associate physical disks with partitions
                                    foreach (var partition in partitions)
                                    {
                                        var diskSearcher = new ManagementObjectSearcher("ASSOCIATORS OF {Win32_DiskPartition.DeviceID='" + partition["DeviceID"] + "'} WHERE AssocClass=Win32_LogicalDiskToPartition");
                                        using (var disks = diskSearcher.Get())
                                        {
                                            // associate partitions with logical disks (drive letter volumes)
                                            foreach (var disk in disks)
                                            {
                                                var usbDisk = new UsbDisk();
                                                usbDisk.VolumeName = disk["VolumeName"].ToStringOrEmpty();
                                                usbDisk.DriveLetter = disk["DeviceID"].ToStringOrEmpty();

                                                usbDisks.Add(usbDisk);
                                            }
                                        }
                                        diskSearcher.Dispose();
                                    }
                                }
                                partitionSearcher.Dispose();
                            }
                        }
                        driveSearcher.Dispose();
                    }
                }

                return usbDisks;
            }
        }

        public class UsbDisk
        {
            public string VolumeName { get; set; }

            public string DriveLetter { get; set; }
        }

        public sealed class Device : IDisposable
        {
            private IntPtr _hDevInfo;
            private SP_DEVINFO_DATA _data;

            private Device(IntPtr hDevInfo, SP_DEVINFO_DATA data)
            {
                this._hDevInfo = hDevInfo;
                this._data = data;
            }

            public static Device Get(string pnpDeviceId)
            {
                if (pnpDeviceId == null)
                    throw new ArgumentNullException("pnpDeviceId");

                IntPtr hDevInfo = SetupDiGetClassDevs(IntPtr.Zero, pnpDeviceId, IntPtr.Zero, DIGCF.DIGCF_ALLCLASSES | DIGCF.DIGCF_DEVICEINTERFACE);
                if (hDevInfo == (IntPtr)INVALID_HANDLE_VALUE)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                SP_DEVINFO_DATA data = new SP_DEVINFO_DATA();
                data.cbSize = Marshal.SizeOf(data);
                if (!SetupDiEnumDeviceInfo(hDevInfo, 0, ref data))
                {
                    int err = Marshal.GetLastWin32Error();
                    if (err == ERROR_NO_MORE_ITEMS)
                        return null;

                    throw new Win32Exception(err);
                }

                return new Device(hDevInfo, data) { PnpDeviceId = pnpDeviceId };
            }

            public void Dispose()
            {
                if (this._hDevInfo != IntPtr.Zero)
                {
                    SetupDiDestroyDeviceInfoList(this._hDevInfo);
                    this._hDevInfo = IntPtr.Zero;
                }
            }

            public string PnpDeviceId { get; private set; }

            public string ParentPnpDeviceId
            {
                get
                {
                    if (IsVistaOrHiger)
                        return this.GetStringProperty(DEVPROPKEY.DEVPKEY_Device_Parent);

                    uint parent;
                    int cr = CM_Get_Parent(out parent, this._data.DevInst, 0);
                    if (cr != 0)
                        throw new Exception("CM Error:" + cr);

                    return GetDeviceId(parent);
                }
            }

            private static string GetDeviceId(uint inst)
            {
                IntPtr buffer = Marshal.AllocHGlobal(MAX_DEVICE_ID_LEN + 1);
                int cr = CM_Get_Device_ID(inst, buffer, MAX_DEVICE_ID_LEN + 1, 0);
                if (cr != 0)
                    throw new Exception("CM Error:" + cr);

                try
                {
                    return Marshal.PtrToStringAnsi(buffer);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            public string[] ChildrenPnpDeviceIds
            {
                get
                {
                    if (IsVistaOrHiger)
                        return this.GetStringListProperty(DEVPROPKEY.DEVPKEY_Device_Children);

                    uint child;
                    int cr = CM_Get_Child(out child, this._data.DevInst, 0);
                    if (cr != 0)
                        return new string[0];

                    List<string> ids = new List<string>();
                    ids.Add(GetDeviceId(child));
                    do
                    {
                        cr = CM_Get_Sibling(out child, child, 0);
                        if (cr != 0)
                            return ids.ToArray();

                        ids.Add(GetDeviceId(child));
                    }
                    while (true);
                }
            }

            private static bool IsVistaOrHiger
            {
                get
                {
                    return (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.CompareTo(new Version(6, 0)) >= 0);
                }
            }

            private const int INVALID_HANDLE_VALUE = -1;
            private const int ERROR_NO_MORE_ITEMS = 259;
            private const int MAX_DEVICE_ID_LEN = 200;

            [StructLayout(LayoutKind.Sequential)]
            private struct SP_DEVINFO_DATA
            {
                public int cbSize;
                public Guid ClassGuid;
                public uint DevInst;
                public IntPtr Reserved;
            }

            [Flags]
            private enum DIGCF : uint
            {
                DIGCF_DEFAULT = 0x00000001,
                DIGCF_PRESENT = 0x00000002,
                DIGCF_ALLCLASSES = 0x00000004,
                DIGCF_PROFILE = 0x00000008,
                DIGCF_DEVICEINTERFACE = 0x00000010,
            }

            [DllImport("setupapi.dll", SetLastError = true)]
            private static extern bool SetupDiEnumDeviceInfo(IntPtr DeviceInfoSet, uint MemberIndex, ref SP_DEVINFO_DATA DeviceInfoData);

            [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
            private static extern IntPtr SetupDiGetClassDevs(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, DIGCF Flags);

            [DllImport("setupapi.dll")]
            private static extern int CM_Get_Parent(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

            [DllImport("setupapi.dll")]
            private static extern int CM_Get_Device_ID(uint dnDevInst, IntPtr Buffer, int BufferLen, uint ulFlags);

            [DllImport("setupapi.dll")]
            private static extern int CM_Get_Child(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

            [DllImport("setupapi.dll")]
            private static extern int CM_Get_Sibling(out uint pdnDevInst, uint dnDevInst, uint ulFlags);

            [DllImport("setupapi.dll")]
            private static extern bool SetupDiDestroyDeviceInfoList(IntPtr DeviceInfoSet);

            // vista and higher
            [DllImport("setupapi.dll", SetLastError = true, EntryPoint = "SetupDiGetDevicePropertyW")]
            private static extern bool SetupDiGetDeviceProperty(IntPtr DeviceInfoSet, ref SP_DEVINFO_DATA DeviceInfoData, ref DEVPROPKEY propertyKey, out int propertyType, IntPtr propertyBuffer, int propertyBufferSize, out int requiredSize, int flags);

            [StructLayout(LayoutKind.Sequential)]
            private struct DEVPROPKEY
            {
                public Guid fmtid;
                public uint pid;

                // from devpkey.h
                public static readonly DEVPROPKEY DEVPKEY_Device_Parent = new DEVPROPKEY { fmtid = new Guid("{4340A6C5-93FA-4706-972C-7B648008A5A7}"), pid = 8 };

                public static readonly DEVPROPKEY DEVPKEY_Device_Children = new DEVPROPKEY { fmtid = new Guid("{4340A6C5-93FA-4706-972C-7B648008A5A7}"), pid = 9 };
            }

            private string[] GetStringListProperty(DEVPROPKEY key)
            {
                int type;
                int size;
                SetupDiGetDeviceProperty(this._hDevInfo, ref this._data, ref key, out type, IntPtr.Zero, 0, out size, 0);
                if (size == 0)
                    return new string[0];

                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    if (!SetupDiGetDeviceProperty(this._hDevInfo, ref this._data, ref key, out type, buffer, size, out size, 0))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    List<string> strings = new List<string>();
                    IntPtr current = buffer;
                    do
                    {
                        string s = Marshal.PtrToStringUni(current);
                        if (string.IsNullOrEmpty(s))
                            break;

                        strings.Add(s);
                        current += (1 + s.Length) * 2;
                    }
                    while (true);
                    return strings.ToArray();
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }

            private string GetStringProperty(DEVPROPKEY key)
            {
                int type;
                int size;
                SetupDiGetDeviceProperty(this._hDevInfo, ref this._data, ref key, out type, IntPtr.Zero, 0, out size, 0);
                if (size == 0)
                    return null;

                IntPtr buffer = Marshal.AllocHGlobal(size);
                try
                {
                    if (!SetupDiGetDeviceProperty(this._hDevInfo, ref this._data, ref key, out type, buffer, size, out size, 0))
                        throw new Win32Exception(Marshal.GetLastWin32Error());

                    return Marshal.PtrToStringUni(buffer);
                }
                finally
                {
                    Marshal.FreeHGlobal(buffer);
                }
            }
        }
    }
}
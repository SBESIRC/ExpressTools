using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace THRecordingService
{
    public static class FuncMac
    {
        /// <summary>
        /// 获取所有物理网卡MAC地址
        /// </summary>
        /// <returns></returns>
        private static ConcurrentDictionary<string, string> GetAllPhysicsMac()
        {
            var _rtMac = new ConcurrentDictionary<string, string>();
            EnumerationOptions opt = new EnumerationOptions();
            opt.ReturnImmediately = true;
            opt.Rewindable = false;
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\cimv2", "SELECT NetworkAddresses, MACAddress, PNPDeviceID, NetConnectionStatus, ConfigManagerErrorCode from Win32_NetworkAdapter WHERE PNPDeviceID LIKE 'PCI%'", opt);
            ManagementObjectCollection queryCollection = searcher.Get();
            foreach (ManagementObject obj in queryCollection)
                _rtMac.AddOrUpdate(obj["MACAddress"].ToString(), obj["NetConnectionStatus"].ToString(), (key, oldValue) => string.Empty);
            return _rtMac;
        }

        /// <summary>
        /// 取得设备网卡的MAC地址
        /// </summary>
        public static string GetNetCardMacAddress(string _sIpAddresses = "")
        {
            var macs = GetAllPhysicsMac();
            var _mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            //不存在物理网卡
            if (macs.Count <= 0)
            {
                var _moc1 = _mc.GetInstances();
                foreach (ManagementObject mo in _moc1)
                    return mo["MacAddress"].ToString();
            }
            //存在物理网卡，并且都未链接
            if (macs.ToList().Count(p => p.Value == "2") <= 0)
                return macs.ToList()[0].Key;

            //存在链接，找到对应IP网卡MAC地址
            if (string.IsNullOrEmpty(_sIpAddresses))
                return macs.ToList().First(p => p.Value == "2").Key;
            {
                var adapters = NetworkInterface.GetAllNetworkInterfaces();
                foreach (NetworkInterface _nif in adapters)
                {
                    if (macs.ToList().Count(p => p.Key.Replace(":", "") == _nif.GetPhysicalAddress().ToString()) <= 0) continue;
                    foreach (var _uip in _nif.GetIPProperties().UnicastAddresses)
                    {
                        if (_sIpAddresses == _uip.Address.ToString())
                            return macs.ToList().First(p => p.Key.Replace(":", "") == _nif.GetPhysicalAddress().ToString()).Key;
                    }
                }
            }
            return macs.Count < 0 ? string.Empty : macs.ToList().First(p => p.Value == "2").Key;
        }

        public static string GetIpAddress()
        {
            string _HostName = Dns.GetHostName();   //获取本机名

            IPHostEntry localhost = Dns.GetHostByName(_HostName);     

            IPAddress localaddr = localhost.AddressList[0];

            return localaddr.ToString();
        }

    }
}

using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThIdentity;

namespace ThLicense
{
    public class ThLicenseService
    {
        private static ThLicenseService service;
        private ThLicenseService()
        {
            //
        }
        public static ThLicenseService Service
        {
            get
            {
                if (service == null)
                {
                   service = new ThLicenseService();
                }
                return service;
            }
        }

        public bool IsLicensed()
        {
            ThUserProfile identity = new ThUserProfile();
            return identity.IsDomainUser();
            /*
            if (identity.IsDomainUser())
            {
                //在域中，就认为是授权用户
                return true;
            }
            else
            {
                //不在域中
                RegistryKey autopudate = Registry.CurrentUser.OpenSubKey(@"Software\ThAi\ThAutoUpdate\AutoUpdate", true);
                if (autopudate.GetValue("FirstLoad") == null)
                {
                    //第一次load
                    autopudate.SetValue("FirstLoad", (DateTime.Now.Ticks / 10000000).ToString());
                    return true;
                }
                else
                {
                    //不是第一次load，对比时间如果超过90天即无法使用
                    long registrytime = long.Parse(autopudate.GetValue("FirstLoad").ToString());
                    if ((DateTime.Now.Ticks / 10000000 - registrytime) > 90 * 24 * 60 * 60)
                    {
                        return false;
                    }
                    else return true;
                }
            }
            */
        }
    }
}

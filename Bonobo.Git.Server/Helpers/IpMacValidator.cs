using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Web;
using Bonobo.Git.Server.Configuration;

namespace Bonobo.Git.Server.Helpers
{
    public class IpMacValidator
    {
        public static bool Validate(string macVerify)
        {
#if DEBUG
            return true;
#else
            var userIp = GetWebClientIp();

            var rawIp = userIp.Contains(":") ? userIp.Split(':')[0] : userIp;

            if (string.IsNullOrEmpty(rawIp) || string.IsNullOrEmpty(macVerify))
            {
                return false;
            }

            if (rawIp == "127.0.0.1")
            {
                return true;
            }

            var mac = GetMacAddress(rawIp);

            // ip:mac,ip:mac
            // 如果是由统一网关访问，判断IP
            if (AppSettings.GateWayMacs.Contains(mac))
            {
                return macVerify.Contains(rawIp);
            }
            else
            {
                return macVerify.Contains(mac);
            }
#endif
        }

        [DllImport("Iphlpapi.dll")]
        static extern int SendARP(Int32 destIp, Int32 srcIp, ref Int64 macAddr, ref Int32 phyAddrLen);
        [DllImport("Ws2_32.dll")]
        static extern Int32 inet_addr(string ipaddr);

        ///<summary>  
        /// SendArp获取MAC地址  
        ///</summary>  
        ///<param name="remoteIp">目标机器的IP地址如(192.168.1.1)</param>  
        ///<returns>目标机器的mac 地址</returns>  
        public static string GetMacAddress(string remoteIp)
        {
            StringBuilder macAddress = new StringBuilder();

            try
            {
                Int32 remote = inet_addr(remoteIp);
                Int64 macInfo = new Int64();
                Int32 length = 6;
                SendARP(remote, 0, ref macInfo, ref length);
                string temp = Convert.ToString(macInfo, 16).PadLeft(12, '0').ToUpper();
                int x = 12;
                for (int i = 0; i < 6; i++)
                {
                    if (i == 5)
                    {
                        macAddress.Append(temp.Substring(x - 2, 2));
                    }
                    else
                    {
                        macAddress.Append(temp.Substring(x - 2, 2) + "-");
                    }

                    x -= 2;
                }
                return macAddress.ToString();
            }
            catch
            {
                return macAddress.ToString();
            }
        }

        /// <summary>
        /// 获取web客户端ip
        /// </summary>
        /// <returns></returns>
        public static string GetWebClientIp()
        {

            string userIP = "";

            try
            {
                if (HttpContext.Current == null)
                {
                    return "";
                }

                string customerIp = "";

                //CDN加速后取到的IP simone 090805
                customerIp = HttpContext.Current.Request.Headers["Cdn-Src-Ip"];
                if (!string.IsNullOrEmpty(customerIp))
                {
                    return customerIp;
                }

                customerIp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                if (!String.IsNullOrEmpty(customerIp))
                {
                    return customerIp;
                }

                if (HttpContext.Current.Request.ServerVariables["HTTP_VIA"] != null)
                {
                    customerIp = HttpContext.Current.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];

                    if (customerIp == null)
                    {
                        customerIp = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                    }
                }
                else
                {
                    customerIp = HttpContext.Current.Request.ServerVariables["REMOTE_ADDR"];
                }

                if (String.Compare(customerIp, "unknown", StringComparison.OrdinalIgnoreCase) == 0 || String.IsNullOrEmpty(customerIp))
                {
                    return HttpContext.Current.Request.UserHostAddress;
                }
                return customerIp;
            }
            catch { }

            return userIP;

        }
    }
}
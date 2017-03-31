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
            var userIp = GetWebClientIp();

            if (string.IsNullOrEmpty(userIp) || string.IsNullOrEmpty(macVerify))
            {
                return false;
            }

            if (userIp == "127.0.0.1")
            {
                return true;
            }

            var mac = GetMacAddress(userIp);

            // ip:mac,ip:mac
            // 如果是由统一网关访问，判断IP
            if (AppSettings.GateWayMacs.Contains(mac) && macVerify.Contains(userIp))
            {
                return true;
            }

            return mac.Contains(macVerify);
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
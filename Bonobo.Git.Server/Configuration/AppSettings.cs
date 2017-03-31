using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace Bonobo.Git.Server.Configuration
{
    public static class AppSettings
    {
        public static bool IsPushAuditEnabled
        {
            get
            {
                return bool.Parse(ConfigurationManager.AppSettings["IsPushAuditEnabled"] ?? "false");
            }
        }

        public static string GateWayMacs
        {
            get { return ConfigurationManager.AppSettings["GateWayMacs"]; }
        }
    }
}
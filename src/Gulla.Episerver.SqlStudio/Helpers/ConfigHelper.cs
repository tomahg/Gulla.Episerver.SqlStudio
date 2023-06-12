using System;
using System.Linq;
using System.Web.Configuration;
using EPiServer.Security;

namespace Gulla.Episerver.SqlStudio.Helpers
{
    public static class ConfigHelper
    {
        public static bool Enabled()
        {
            var enabledConfigValue = WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:Enabled"];
            if (!string.IsNullOrEmpty(enabledConfigValue) && enabledConfigValue.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (PrincipalInfo.Current.RoleList.Contains("SqlAdmin", StringComparer.InvariantCultureIgnoreCase))
            {
                return true;
            }

            var usersConfigValue = WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:Users"];
            if (!string.IsNullOrEmpty(usersConfigValue))
            {
                var users = usersConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (users.Contains(PrincipalInfo.Current.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public static bool AutoHintEnabled()
        {
            var value = WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:AutoIntelliSense.Enabled"];
            return value == null || bool.Parse(value);
        }

        public static bool DarkModeEnabled()
        {
            var value = WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:DarkMode.Enabled"];
            return value == null || bool.Parse(value);
        }

        public static string AllowPattern()
        {
            return WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:AllowPattern"];
        }

        public static string AllowMessage()
        {
            return WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:AllowMessage"];
        }


        public static string DenyPattern()
        {
            return WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:DenyPattern"];
        }

        public static string DenyMessage()
        {
            return WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:DenyMessage"];
        }
    }
}

using System.Web.Configuration;
using EPiServer.Security;

namespace Gulla.Episerver.SqlStudio.Helpers
{
    public static class ConfigHelper
    {
        public static bool Enabled()
        {
            if (!PrincipalInfo.Current.RoleList.Contains("SqlAdmin"))
            {
                return false;
            }

            var value = WebConfigurationManager.AppSettings["Gulla.Episerver.SqlStudio:Enabled"];
            return value == null || bool.Parse(value);
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

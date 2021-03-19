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
    }
}

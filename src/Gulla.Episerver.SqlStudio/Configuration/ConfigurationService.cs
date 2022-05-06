using System;
using System.Linq;
using EPiServer.Security;
using Microsoft.Extensions.Configuration;

namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class ConfigurationService
    {
        private IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool Enabled()
        {
            var enabledConfigValue = _configuration.GetValue<string>("Gulla:SqlStudio:Enabled");
            if (!string.IsNullOrEmpty(enabledConfigValue) && enabledConfigValue.Equals("false", StringComparison.InvariantCultureIgnoreCase))
            {
                return false;
            }

            if (PrincipalInfo.CurrentPrincipal.IsInRole("SqlAdmin"))
            {
                return true;
            }

            var usersConfigValue = _configuration.GetValue<string>("Gulla:SqlStudio:Users");
            if (!string.IsNullOrEmpty(usersConfigValue))
            {
                var users = usersConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (users.Contains(PrincipalInfo.CurrentPrincipal.Identity.Name))
                {
                    return true;
                }
            }

            return false;
        }

        public bool AutoHintEnabled()
        {
            var value = _configuration.GetValue<bool?>("Gulla:SqlStudio:AutoIntellisenseEnabled");
            return value == null || value == true;
        }

        public bool DarkModeEnabled()
        {
            var value = _configuration.GetValue<bool?>("Gulla:SqlStudio:DarkModeEnabled");
            return value == null || value == true;
        }

        public string AllowPattern()
        {
            return _configuration.GetValue<string>("Gulla:SqlStudio:AllowPattern");
        }

        public string AllowMessage()
        {
            return _configuration.GetValue<string>("Gulla:SqlStudio:AllowMessage");
        }


        public string DenyPattern()
        {
            return _configuration.GetValue<string>("Gulla:SqlStudio:DenyPattern");
        }

        public string DenyMessage()
        {
            return _configuration.GetValue<string>("Gulla:SqlStudio:DenyMessage");
        }
    }
}

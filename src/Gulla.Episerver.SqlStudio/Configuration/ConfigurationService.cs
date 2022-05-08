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
    }
}

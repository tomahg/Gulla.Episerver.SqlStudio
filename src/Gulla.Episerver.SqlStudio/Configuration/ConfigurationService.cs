using System;
using System.Linq;
using EPiServer.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class ConfigurationService
    {
        private readonly SqlStudioOptions _configuration;

        public ConfigurationService(IConfiguration configuration, IOptions<SqlStudioOptions> options)
        {
            _configuration = options.Value;
        }

        public bool Enabled()
        {
            if (!_configuration.Enabled)
            {
                return false;
            }

            if (PrincipalInfo.CurrentPrincipal.IsInRole("SqlAdmin"))
            {
                return true;
            }

            var usersConfigValue = _configuration.Users;
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

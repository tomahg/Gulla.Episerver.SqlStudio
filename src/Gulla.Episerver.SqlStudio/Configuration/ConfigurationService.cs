using System;
using System.Linq;
using EPiServer.Security;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class ConfigurationService
    {
        private readonly SqlStudioOptions _configuration;

        public ConfigurationService(IOptions<SqlStudioOptions> options)
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

            var groupsConfigValue = _configuration.GroupNames;
            if (!string.IsNullOrEmpty(groupsConfigValue))
            {
                var groups = groupsConfigValue.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && groups.Any(group => PrincipalInfo.CurrentPrincipal.IsInRole(group)))
                {
                    return true;
                }
            }

            var usersConfigValue = _configuration.Users;
            if (!string.IsNullOrEmpty(usersConfigValue))
            {
                var users = usersConfigValue.Split([','], StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && users.Contains(PrincipalInfo.CurrentPrincipal.Identity.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool IsAuditLogEnabled()
        {
            if (_configuration.DisableAuditLog)
            {
                return false;
            }

            return true;
        }

        public bool IsSavedQueriesEnabled()
        {
            if (_configuration.ShowSavedQueries)
            {
                return true;
            }

            return false;
        }

        public bool IsAiGenerationEnabled()
        {
            if (!_configuration.AiEnabled || string.IsNullOrEmpty(_configuration.AiApiKey))
            {
                return false;
            }

            return true;
        }

        public bool CanUserViewAllAuditLogs()
        {
            var auditLogViewAllGroupNamesConfigValue = _configuration.AuditLogViewAllGroupNames;
            if (!string.IsNullOrEmpty(auditLogViewAllGroupNamesConfigValue))
            {
                var groups = auditLogViewAllGroupNamesConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && groups.Any(group => PrincipalInfo.CurrentPrincipal.IsInRole(group)))
                {
                    return true;
                }
            }

            var auditLogViewAllUsersConfigValue = _configuration.AuditLogViewAllUsers;
            if (!string.IsNullOrEmpty(auditLogViewAllUsersConfigValue))
            {
                var users = auditLogViewAllUsersConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && users.Contains(PrincipalInfo.CurrentPrincipal.Identity.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public bool CanUserDeleteAuditLogs()
        {
            var auditLogDeleteGroupNamesConfigValue = _configuration.AuditLogDeleteGroupNames;
            if (!string.IsNullOrEmpty(auditLogDeleteGroupNamesConfigValue))
            {
                var groups = auditLogDeleteGroupNamesConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && groups.Any(group => PrincipalInfo.CurrentPrincipal.IsInRole(group)))
                {
                    return true;
                }
            }

            var auditLogDeleteUsersConfigValue = _configuration.AuditLogDeleteUsers;
            if (!string.IsNullOrEmpty(auditLogDeleteUsersConfigValue))
            {
                var users = auditLogDeleteUsersConfigValue.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim());
                if (PrincipalInfo.CurrentPrincipal.Identity != null && users.Contains(PrincipalInfo.CurrentPrincipal.Identity.Name, StringComparer.InvariantCultureIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        public int GetAuditLogDaysToKeep()
        {
            return _configuration.AuditLogDaysToKeep;
        }
    }
}

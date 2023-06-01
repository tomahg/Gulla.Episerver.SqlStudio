using System.Collections.Generic;
using EPiServer.Shell.Navigation;
using Gulla.Episerver.SqlStudio.Configuration;

namespace Gulla.Episerver.SqlStudio.Menu
{
    namespace WebProject.Redirects
    {
        [MenuProvider]
        public class SqlStudioMenuProvider : IMenuProvider
        {
            readonly ConfigurationService _configurationService;

            public SqlStudioMenuProvider(ConfigurationService configurationService)
            {
                _configurationService = configurationService;
            }

            public IEnumerable<MenuItem> GetMenuItems()
            {
                return new MenuItem[]
                {
                    new UrlMenuItem("SQL", MenuPaths.Global + "/cms/sqlstudio", "/SqlStudio")
                    {
                        IsAvailable = context => _configurationService.Enabled()
                 
                    },
                    new UrlMenuItem("SQL Studio", MenuPaths.Global + "/cms/sqlstudio/sqlstudio", "/SqlStudio")
                    {
                        IsAvailable = context => _configurationService.Enabled(),
                        SortIndex = 1
                    },
                    new UrlMenuItem("Audit log", MenuPaths.Global + "/cms/sqlstudio/auditlog", "/SqlStudio/AuditLog")
                    {
                        IsAvailable = context => _configurationService.Enabled() && _configurationService.IsAuditLogEnabled(),
                        SortIndex = 2
                    },
                    new UrlMenuItem("Admin", MenuPaths.Global + "/cms/sqlstudio/admin", "/SqlStudio/Admin")
                    {
                        IsAvailable = context => _configurationService.Enabled() && _configurationService.CanUserDeleteAuditLogs(),
                        SortIndex = 3
                    },
                };
            }
        }
    }
}
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
                return
                [
                    new UrlMenuItem("SQL", MenuPaths.Global + "/cms/sqlstudio", "/SqlStudio")
                    {
                        IsAvailable = context => _configurationService.Enabled(),
                        AuthorizationPolicy = SqlAuthorizationPolicy.Default

                    },
                    new UrlMenuItem("SQL Studio", MenuPaths.Global + "/cms/sqlstudio/sqlstudio", "/SqlStudio")
                    {
                        IsAvailable = context => _configurationService.Enabled(),
                        AuthorizationPolicy = SqlAuthorizationPolicy.Default,
                        SortIndex = 1
                    },
                    new UrlMenuItem("Saved Queries", MenuPaths.Global + "/cms/sqlstudio/savedqueries", "/SqlStudio/SavedQueries")
                    {
                        IsAvailable = context => _configurationService.Enabled() && _configurationService.IsSavedQueriesEnabled(),
                        AuthorizationPolicy = SqlAuthorizationPolicy.Default,
                        SortIndex = 2
                    },
                    new UrlMenuItem("Audit Log", MenuPaths.Global + "/cms/sqlstudio/auditlog", "/SqlStudio/AuditLog")
                    {
                        IsAvailable = context => _configurationService.Enabled() && _configurationService.IsAuditLogEnabled(),
                        AuthorizationPolicy = SqlAuthorizationPolicy.Default,
                        SortIndex = 2
                    }
                ];
            }
        }
    }
}
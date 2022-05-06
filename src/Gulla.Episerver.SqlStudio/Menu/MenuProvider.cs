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
            ConfigurationService _configurationService;

            public SqlStudioMenuProvider(ConfigurationService configurationService)
            {
                _configurationService = configurationService;
            }

            public IEnumerable<MenuItem> GetMenuItems()
            {
                var foo = _configurationService.Enabled();

                return new MenuItem[]
                {
                    new UrlMenuItem("SQL", "/global/cms/sqlstudio", "/episerver-sql-studio")
                    {
                        IsAvailable = context => _configurationService.Enabled()
                    }
                };
            }
        }
    }
}
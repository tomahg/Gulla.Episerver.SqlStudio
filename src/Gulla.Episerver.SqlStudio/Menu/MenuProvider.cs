using System.Collections.Generic;
using EPiServer.Shell;
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
                var url = Paths.ToResource(GetType(), "container");

                return new MenuItem[]
                {
                    new UrlMenuItem("SQL", MenuPaths.Global + "/cms/sqlstudio", url)
                    {
                        IsAvailable = context => _configurationService.Enabled()
                    }
                };
            }
        }
    }
}
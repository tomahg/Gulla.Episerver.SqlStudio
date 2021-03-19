using System.Collections.Generic;
using EPiServer.Shell.Navigation;
using Gulla.Episerver.SqlStudio.Helpers;

namespace Gulla.Episerver.SqlStudio.Menu
{
    namespace WebProject.Redirects
    {
        [MenuProvider]
        public class SqlStudioMenuProvider : IMenuProvider
        {
            public IEnumerable<MenuItem> GetMenuItems()
            {
                return new MenuItem[]
                {
                    new UrlMenuItem("SQL", "/global/cms/sqlstudio", "/episerver-sql-studio")
                    {
                        IsAvailable = context => ConfigHelper.Enabled()
                    }
                };
            }
        }
    }
}
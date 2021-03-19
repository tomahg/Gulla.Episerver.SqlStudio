using System.Collections.Generic;
using EPiServer.Security;
using EPiServer.Shell.Navigation;

namespace Gulla.Episerver.SqlStudio
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
                        IsAvailable = context => PrincipalInfo.Current.RoleList.Contains("SqlAdmin")
                    }
                };
            }
        }
    }
}
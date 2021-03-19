using System.Web.Mvc;
using System.Web.Routing;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;

namespace Gulla.Episerver.SqlStudio.Initialization
{
    [InitializableModule]
    public class RouteInitialization : IInitializableModule
    {
        public void Initialize(InitializationEngine context)
        {
            RouteTable.Routes.MapRoute(
                null,
                "episerver-sql-studio",
                new {controller = "SqlStudio", action = "Index"}
                );
        }

        public void Uninitialize(InitializationEngine context)
        {

        }
    }
}
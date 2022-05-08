using System;
using System.Linq;
using EPiServer.Shell.Modules;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.DataAccess;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Gulla.Episerver.SqlStudio
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSqlStudio(this IServiceCollection services)
        {
            return AddSqlStudio(services, _ => { });
        }

        public static IServiceCollection AddSqlStudio(this IServiceCollection services, Action<SqlStudioOptions> setupAction)
        {
            AddModule(services);
            services.AddTransient<SqlService, SqlService>();
            services.AddTransient<QueryLoader, QueryLoader>();
            services.AddTransient<ConfigurationService, ConfigurationService>();
            
            services.AddOptions<SqlStudioOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("Gulla:SqlStudio").Bind(options);
            });

            return services;
        }

        private static void AddModule(IServiceCollection services)
        {
            services.Configure<ProtectedModuleOptions>(
                pm =>
                {
                    if (!pm.Items.Any(i => i.Name.Equals(Constants.ModuleName, StringComparison.OrdinalIgnoreCase)))
                    {
                        pm.Items.Add(new ModuleDetails { Name = Constants.ModuleName });
                    }
                });
        }
    }
}
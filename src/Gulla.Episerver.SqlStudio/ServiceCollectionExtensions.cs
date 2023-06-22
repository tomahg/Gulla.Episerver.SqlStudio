using System;
using Gulla.Episerver.SqlStudio.AI;
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
            services.AddTransient<SqlService, SqlService>();
            services.AddTransient<QueryLoader, QueryLoader>();
            services.AddTransient<OpenAiService, OpenAiService>();
            services.AddTransient<ConfigurationService, ConfigurationService>();
            
            services.AddOptions<SqlStudioOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("Gulla:SqlStudio").Bind(options);
            });

            return services;
        }
    }
}
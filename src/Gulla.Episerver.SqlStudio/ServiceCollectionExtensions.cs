using System;
using Gulla.Episerver.SqlStudio.AI;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.DataAccess;
using Microsoft.AspNetCore.Authorization;
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

        public static IServiceCollection AddSqlStudio(
            this IServiceCollection services,
            Action<SqlStudioOptions> setupAction,
            Action<AuthorizationOptions> authorizationOptions = null)
        {
            services.AddTransient<SqlService, SqlService>();
            services.AddTransient<QueryRepository, QueryRepository>();
            services.AddTransient<OpenAiService, OpenAiService>();
            services.AddTransient<ConfigurationService, ConfigurationService>();
            
            services.AddOptions<SqlStudioOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction(options);
                configuration.GetSection("Gulla:SqlStudio").Bind(options);
            });

            // Authorization
            if (authorizationOptions != null)
            {
                services.AddAuthorization(authorizationOptions);
            }
            else
            {
                services.AddAuthorizationBuilder().AddPolicy(SqlAuthorizationPolicy.Default, policy => { policy.RequireRole("SqlAdmin"); });
            }

            return services;
        }
    }
}
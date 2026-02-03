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
        /// <summary>
        /// Adds SQL Studio services with default configuration and default authorization (requires "SqlAdmin" role).
        /// </summary>
        public static IServiceCollection AddSqlStudio(this IServiceCollection services)
        {
            return AddSqlStudio(services, null, null);
        }

        /// <summary>
        /// Adds SQL Studio services with custom options and default authorization (requires "SqlAdmin" role).
        /// </summary>
        public static IServiceCollection AddSqlStudioWithDefaultAuthorization(
            this IServiceCollection services,
            Action<SqlStudioOptions> setupAction)
        {
            return AddSqlStudio(services, setupAction, null);
        }

        /// <summary>
        /// Adds SQL Studio services with default options and custom authorization.
        /// </summary>
        public static IServiceCollection AddSqlStudio(
            this IServiceCollection services,
            Action<AuthorizationOptions> authorizationOptions)
        {
            return AddSqlStudio(services, null, authorizationOptions);
        }

        /// <summary>
        /// Adds SQL Studio services with custom options and custom authorization.
        /// </summary>
        public static IServiceCollection AddSqlStudio(
            this IServiceCollection services,
            Action<SqlStudioOptions> setupAction,
            Action<AuthorizationOptions> authorizationOptions)
        {
            services.AddTransient<SqlService, SqlService>();
            services.AddTransient<QueryRepository, QueryRepository>();
            services.AddTransient<OpenAiService, OpenAiService>();
            services.AddTransient<ConfigurationService, ConfigurationService>();

            services.AddOptions<SqlStudioOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                setupAction?.Invoke(options);
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
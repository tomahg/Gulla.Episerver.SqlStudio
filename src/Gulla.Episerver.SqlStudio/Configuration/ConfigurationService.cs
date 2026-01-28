using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class ConfigurationService
    {
        private readonly SqlStudioOptions _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthorizationService _authorizationService;
        private readonly IAuthorizationPolicyProvider _policyProvider;

        public ConfigurationService(IOptions<SqlStudioOptions> options,
            IHttpContextAccessor httpContextAccessor,
            IAuthorizationService authorizationService,
            IAuthorizationPolicyProvider policyProvider)
        {
            _configuration = options.Value;
            _httpContextAccessor = httpContextAccessor;
            _authorizationService = authorizationService;
            _policyProvider = policyProvider;
        }

        private ClaimsPrincipal CurrentUser => _httpContextAccessor.HttpContext?.User;

        public bool Enabled()
        {
            return _configuration.Enabled;
        }

        public bool IsAuditLogEnabled()
        {
            if (_configuration.DisableAuditLog)
            {
                return false;
            }

            return true;
        }

        public bool IsSavedQueriesEnabled()
        {
            if (_configuration.ShowSavedQueries)
            {
                return true;
            }

            return false;
        }

        public bool IsAiGenerationEnabled()
        {
            if (!_configuration.AiEnabled || string.IsNullOrEmpty(_configuration.AiApiKey))
            {
                return false;
            }

            return true;
        }

        public bool CanUserViewAllAuditLogs()
        {
            return AuthorizeIfPolicyExist(SqlAuthorizationPolicy.AuditLogsViewAll);
        }

        public bool CanUserDeleteAuditLogs()
        {
            return AuthorizeIfPolicyExist(SqlAuthorizationPolicy.AuditLogsDelete);
        }

        private bool AuthorizeIfPolicyExist(string policyName)
        {
            if (CurrentUser == null)
            {
                return false;
            }

            var policy = _policyProvider.GetPolicyAsync(policyName).Result;
            if (policy == null)
            {
                return false;
            }

            return _authorizationService.AuthorizeAsync(CurrentUser, policy).Result.Succeeded;
        }

        public int GetAuditLogDaysToKeep()
        {
            return _configuration.AuditLogDaysToKeep;
        }
    }
}

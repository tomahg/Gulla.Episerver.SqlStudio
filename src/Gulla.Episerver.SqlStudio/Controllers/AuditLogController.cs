using System.Linq;
using System.Net;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.Dds;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    [Route("SqlStudio/AuditLog")]
    [Authorize(Policy = SqlAuthorizationPolicy.Default)]
    public class AuditLogController : Controller
    {
        private readonly ConfigurationService _configurationService;
        private readonly ISqlStudioDdsRepository _sqlStudioDdsRepository;

        public AuditLogController(
            ConfigurationService configurationService, 
            ISqlStudioDdsRepository sqlStudioDdsRepository)
        {
            _configurationService = configurationService;
            _sqlStudioDdsRepository = sqlStudioDdsRepository;
        }

        public ActionResult Index()
        {
            if (!_configurationService.Enabled() || !_configurationService.IsAuditLogEnabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var retentionDays = _configurationService.GetAuditLogDaysToKeep();
            if (retentionDays != 0)
            {
                _sqlStudioDdsRepository.DeleteOldLogEntries(retentionDays);
            }            

            var model = new AuditLogViewModel
            {
                Logs = _configurationService.CanUserViewAllAuditLogs() ? _sqlStudioDdsRepository.ListAll() : _sqlStudioDdsRepository.ListAll(User.Identity.Name),
                LogsCount = _configurationService.CanUserDeleteAuditLogs() ? _sqlStudioDdsRepository.ListAll().Count() : 0,
                ShowDeleteButton = _configurationService.CanUserDeleteAuditLogs()
            };

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string delete)
        {
            if (!_configurationService.Enabled() || !_configurationService.CanUserDeleteAuditLogs())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            _sqlStudioDdsRepository.DeleteAll();
            return RedirectToAction("Index");
        }
    }
}
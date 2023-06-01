using System.Linq;
using System.Net;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.Dds;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    [Route("SqlStudio/Admin")]
    public class AdminController : Controller
    {
        private readonly ConfigurationService _configurationService;
        private readonly ISqlStudioDdsRepository _sqlStudioDdsRepository;

        public AdminController(
            ConfigurationService configurationService, 
            ISqlStudioDdsRepository sqlStudioDdsRepository)
        {
            _configurationService = configurationService;
            _sqlStudioDdsRepository = sqlStudioDdsRepository;
        }

        public ActionResult Index()
        {
            if (!_configurationService.Enabled() || !_configurationService.CanUserDeleteAuditLogs())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var model = new AdminViewModel
            {
                LogsCount = _sqlStudioDdsRepository.ListAll().Count()
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
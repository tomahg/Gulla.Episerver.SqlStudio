using System.Linq;
using System.Net;
using EPiServer.Data;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.DataAccess;
using Gulla.Episerver.SqlStudio.Dds;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    [Route("SqlStudio/SavedQueries")]
    public class SavedQueriesController : BaseSqlController
    {
        private readonly ISqlStudioDdsRepository _sqlStudioDdsRepository;
        private readonly ConfigurationService _configurationService;
        private readonly SqlService _sqlService;
        private readonly QueryRepository _queryRepository;

        public SavedQueriesController(
            ISqlStudioDdsRepository sqlStudioDdsRepository,
            ConfigurationService configurationService,
            SqlService sqlService,
            QueryRepository queryRepository,
            DataAccessOptions dataAccessOptions,
            IOptions<SqlStudioOptions> options) : base(dataAccessOptions, options)
        {
            _sqlStudioDdsRepository = sqlStudioDdsRepository;
            _configurationService = configurationService;
            _sqlService = sqlService;
            _queryRepository = queryRepository;
        }

        public ActionResult Index()
        {
            if (!_configurationService.Enabled() || !_configurationService.IsSavedQueriesEnabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            var model = new SavedQueriesViewModel
            {
                SavedQueries = _sqlService.GetTableNames(connectionString).Contains("SqlQueries") ? _queryRepository.GetQueries(connectionString, keepSortPrefix: true).ToList() : Enumerable.Empty<SqlQueryCategory>()
            };
            return View(model);
        }

        [HttpPost("delete")]
        public IActionResult Delete(string category, string name)
        {
            if (!_configurationService.Enabled() || !_configurationService.IsSavedQueriesEnabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return RedirectToAction(nameof(Index));
            }

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            _queryRepository.DeleteQuery(_configurationService, _sqlStudioDdsRepository, connectionString, category, name);

            return RedirectToAction(nameof(Index));
        }

        [HttpPost("update")]
        public IActionResult Update(string category, string name, string query)
        {
            if (!_configurationService.Enabled() || !_configurationService.IsSavedQueriesEnabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { ok = false, message = "Missing query name." });
            }

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            _queryRepository.UpdateQuery(_configurationService, _sqlStudioDdsRepository, connectionString, category, name, query);

            return Ok(new { ok = true });
        }
    }
}

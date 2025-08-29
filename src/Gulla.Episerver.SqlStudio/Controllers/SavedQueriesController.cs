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
        private readonly QueryLoader _queryLoader;

        public SavedQueriesController(
            ISqlStudioDdsRepository sqlStudioDdsRepository,
            ConfigurationService configurationService,
            SqlService sqlService,
            QueryLoader queryLoader,
            DataAccessOptions dataAccessOptions,
            IOptions<SqlStudioOptions> options) : base(dataAccessOptions, options)
        {
            _sqlStudioDdsRepository = sqlStudioDdsRepository;
            _configurationService = configurationService;
            _sqlService = sqlService;
            _queryLoader = queryLoader;
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
                SavedQueries = _sqlService.GetTableNames(connectionString).Contains("SqlQueries") ? _queryLoader.GetQueries(connectionString, true).ToList() : Enumerable.Empty<SqlQueryCategory>()
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

            _queryLoader.DeleteQuery(_configurationService, _sqlStudioDdsRepository, connectionString, category, name);

            return RedirectToAction(nameof(Index));
        }
    }
}
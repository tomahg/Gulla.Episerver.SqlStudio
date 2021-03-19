using System;
using System.Linq;
using System.Web.Mvc;
using Gulla.Episerver.SqlStudio.DataAccess;
using Gulla.Episerver.SqlStudio.Helpers;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    [Authorize(Roles = "SqlAdmin")]
    public class SqlStudioController : Controller
    {
        private readonly SqlService _sqlService;
        private readonly QueryLoader _queryLoader;

        public SqlStudioController(SqlService sqlService, QueryLoader queryLoader)
        {
            _sqlService = sqlService;
            _queryLoader = queryLoader;
        }

        public ActionResult Index()
        {
            if (!ConfigHelper.Enabled())
            {
                return new HttpUnauthorizedResult();
            }

            var model = new SqlStudioViewModel
            {
                SqlAutoCompleteMetadata = _sqlService.GetMetaData(),
                SavedQueries = _sqlService.GetTableNames().Contains("SqlQueries") ? _queryLoader.GetQueries().ToList() : Enumerable.Empty<SqlQueryCategory>()
            };

            return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
        }

        [HttpPost]
        public ActionResult Index(string query, bool hideEmptyColumns)
        {
            if (!ConfigHelper.Enabled())
            {
                return new HttpUnauthorizedResult();
            }

            var model = new SqlStudioViewModel
            {
                Query = query,
            };

            try
            {
                model.SqlResult = _sqlService.ExecuteQuery(query)?.HideEmptyColumns(hideEmptyColumns)?.ToList();
            }
            catch (Exception e)
            {
                model.Message = e.Message;
            }

            // If the SQL query updates the table 'SqlQueries', or other table columns - this must be called at the very end.
            model.SavedQueries = _sqlService.GetTableNames().Contains("SqlQueries") ? _queryLoader.GetQueries().ToList() : Enumerable.Empty<SqlQueryCategory>();
            model.SqlAutoCompleteMetadata = _sqlService.GetMetaData();

            return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
        }
    }
}
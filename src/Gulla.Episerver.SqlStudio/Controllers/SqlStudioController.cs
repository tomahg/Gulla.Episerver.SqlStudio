using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using EPiServer.Data;
using Gulla.Episerver.SqlStudio.DataAccess;
using Gulla.Episerver.SqlStudio.Extensions;
using Gulla.Episerver.SqlStudio.Helpers;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    public class SqlStudioController : Controller
    {
        private readonly SqlService _sqlService;
        private readonly QueryLoader _queryLoader;
        private readonly DataAccessOptions _dataAccessOptions;

        public SqlStudioController(SqlService sqlService, QueryLoader queryLoader, DataAccessOptions dataAccessOptions)
        {
            _sqlService = sqlService;
            _queryLoader = queryLoader;
            _dataAccessOptions = dataAccessOptions;
        }

        public ActionResult Index()
        {
            if (!ConfigHelper.Enabled())
            {
                return new HttpUnauthorizedResult();
            }

            var connectionstringList = _dataAccessOptions.ConnectionStrings
                .OrderByDescending(x => x.Name == _dataAccessOptions.DefaultConnectionStringName)
                .Select(x => new SelectListItem {Text = x.Name, Value = x.ConnectionString}).ToList();
            var connectionString = connectionstringList.FirstOrDefault()?.Value;

            var model = new SqlStudioViewModel
            {
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                AutoIntelliSense = ConfigHelper.AutoHintEnabled(),
                DarkMode = ConfigHelper.DarkModeEnabled(),
                ConnectionStrings = connectionstringList
            };

            try
            {
                model.SavedQueries = _sqlService.GetTableNames(connectionString).Contains("SqlQueries") ? _queryLoader.GetQueries(connectionString).ToList() : Enumerable.Empty<SqlQueryCategory>();
                model.SqlAutoCompleteMetadata = _sqlService.GetMetaData(connectionString);
                model.SqlTableNameMap = _sqlService.TableNameMap(connectionString);
            }
            catch (Exception e)
            {
                model.Message = e.Message;
            }

            return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
        }

        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Index(string query, bool hideEmptyColumns, 
            int? contentNameIndex, int? contentNameLanguageIndex, int? contentNameInsertIndex, string contentNameHeading,
            int? contentLinkIndex, int? contentLinkLanguageIndex, int? contentLinkInsertIndex, string contentLinkHeading,
            string connectionString)
        {
            if (!ConfigHelper.Enabled())
            {
                return new HttpUnauthorizedResult();
            }

            // Adjusting for indexes, if more than one custom column is used. Not too happy with this.
            int contentNameIndexAdjusted = contentNameIndex ?? -1;
            int contentNameLanguageIndexAdjusted = contentNameLanguageIndex ?? -1;
            int contentNameInsertIndexAdjusted = contentNameInsertIndex ?? -1;
            int contentLinkInsertIndexAdjusted = contentLinkInsertIndex ?? -1;
            int contentLinkLanguageIndexAdjusted = contentLinkLanguageIndex ?? -1;
            int contentLinkIndexAdjusted = contentLinkIndex ?? -1;

            if (contentNameIndexAdjusted != -1 && contentNameInsertIndexAdjusted != -1 && contentLinkIndexAdjusted != -1)
            {
                if (contentNameInsertIndexAdjusted <= contentLinkIndexAdjusted)
                {
                    contentLinkIndexAdjusted++;
                }
                if (contentNameInsertIndexAdjusted <= contentLinkLanguageIndexAdjusted)
                {
                    contentLinkLanguageIndexAdjusted++;
                }
                if (contentNameInsertIndexAdjusted <= contentLinkInsertIndexAdjusted)
                {
                    contentLinkInsertIndexAdjusted++;
                }
            }

            var model = new SqlStudioViewModel
            {
                Query = query,
                ContentNameIndex = contentNameIndexAdjusted,
                ContentNameLanguageIndex = contentNameLanguageIndexAdjusted,
                ContentNameInsertIndex = contentNameInsertIndexAdjusted,
                ContentNameHeading = contentNameHeading,
                ContentLinkIndex = contentLinkIndexAdjusted,
                ContentLinkLanguageIndex = contentLinkLanguageIndexAdjusted,
                ContentLinkInsertIndex = contentLinkInsertIndexAdjusted,
                ContentLinkHeading = contentLinkHeading,
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                ConnectionStrings = _dataAccessOptions.ConnectionStrings.OrderByDescending(x => x.Name == _dataAccessOptions.DefaultConnectionStringName).Select(x => new SelectListItem { Text = x.Name, Value = x.ConnectionString }),
                AutoIntelliSense = ConfigHelper.AutoHintEnabled(),
                DarkMode = ConfigHelper.DarkModeEnabled()
            };

            // Check for configured allow regex pattern
            try
            {
                var allowPattern = ConfigHelper.AllowPattern();
                if (!string.IsNullOrEmpty(allowPattern) && !Regex.Match(query, allowPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = ConfigHelper.AllowMessage() ?? "Query did not match provided allow pattern.";
                    return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
            }

            // Check for configured deny regex pattern
            try
            {
                var denyPattern = ConfigHelper.DenyPattern();
                if (!string.IsNullOrEmpty(denyPattern) && Regex.Match(query, denyPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = ConfigHelper.DenyMessage() ?? "Query matched provided deny pattern.";
                    return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
            }

            if (!string.IsNullOrWhiteSpace(query))
            {
                // Execute query
                try
                {
                    model.SqlResult = _sqlService.ExecuteQuery(query, connectionString)?.HideEmptyColumns(hideEmptyColumns)?.ToList();

                    model.ColumnsContentId = model.SqlResult.FirstOrDefault()?.GetColumnList("[Select column with content id]");
                    model.ColumnsLanguageBranchId = model.SqlResult.FirstOrDefault()?.GetColumnList("[Select column with language branch id (optional)]");
                    model.ColumnsInsertIndex = model.SqlResult.FirstOrDefault()?.GetColumnListForInserting("[Select where to insert column (default last)]");

                    if (model.ContentNameIndex != -1)
                    {
                        model.SqlResult = _sqlService.AddContentName(model.SqlResult, model.ContentNameInsertIndex, model.ContentNameHeading, model.ContentNameIndex, model.ContentNameLanguageIndex);
                    }

                    if (model.ContentLinkIndex != -1)
                    {
                        model.SqlResult = _sqlService.AddContentLink(model.SqlResult, model.ContentLinkInsertIndex, model.ContentLinkHeading, model.ContentLinkIndex, model.ContentLinkLanguageIndex);
                    }
                }
                catch (Exception e)
                {
                    model.Message = e.Message;
                }
            }

            // If the SQL query updates the table 'SqlQueries', or other table columns - this must be called at the very end.
            try
            {
                model.SavedQueries = _sqlService.GetTableNames(connectionString).Contains("SqlQueries") ? _queryLoader.GetQueries(connectionString).ToList() : Enumerable.Empty<SqlQueryCategory>();
                model.SqlAutoCompleteMetadata = _sqlService.GetMetaData(connectionString);
                model.SqlTableNameMap = _sqlService.TableNameMap(connectionString);
            }
            catch (Exception e)
            {
                model.Message = e.Message;
            }


            return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
        }
    }
}
using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using EPiServer.Data;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.DataAccess;
using Gulla.Episerver.SqlStudio.Extensions;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    public class ContainerController : Controller
    {
        private readonly SqlService _sqlService;
        private readonly QueryLoader _queryLoader;
        private readonly DataAccessOptions _dataAccessOptions;
        private readonly ConfigurationService _configurationService;
        private readonly SqlStudioOptions _configuration;

        public ContainerController(SqlService sqlService, 
            QueryLoader queryLoader, 
            DataAccessOptions dataAccessOptions, 
            ConfigurationService configurationService, 
            IOptions<SqlStudioOptions> options)
        {
            _sqlService = sqlService;
            _queryLoader = queryLoader;
            _dataAccessOptions = dataAccessOptions;
            _configurationService = configurationService;
            _configuration = options.Value;
        }

        public ActionResult Index()
        {
            if (!_configurationService.Enabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var connectionStringList = _dataAccessOptions.ConnectionStrings
                .OrderByDescending(x => x.Name == _dataAccessOptions.DefaultConnectionStringName)
                .Select(x => new SelectListItem {Text = x.Name, Value = x.ConnectionString}).ToList();
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            var model = new SqlStudioViewModel
            {
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled,
                ConnectionStrings = connectionStringList
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

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string query, bool hideEmptyColumns, 
            int? contentNameIndex, int? contentNameLanguageIndex, int? contentNameInsertIndex, string contentNameHeading,
            int? contentLinkIndex, int? contentLinkLanguageIndex, int? contentLinkInsertIndex, string contentLinkHeading,
            string connectionString)
        {
            if (!_configurationService.Enabled())
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
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
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled
            };

            // Check for configured allow regex pattern
            try
            {
                var allowPattern = _configuration.AllowPattern;
                if (!string.IsNullOrEmpty(allowPattern) && !Regex.Match(query, allowPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = _configuration.AllowMessage ?? "Query did not match provided allow pattern.";
                    return View(model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                return View(model);
            }

            // Check for configured deny regex pattern
            try
            {
                var denyPattern = _configuration.DenyPattern;
                if (!string.IsNullOrEmpty(denyPattern) && Regex.Match(query, denyPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = _configuration.DenyMessage ?? "Query matched provided deny pattern.";
                    return View(model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                return View(model);
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

            return View(model);
        }
    }
}
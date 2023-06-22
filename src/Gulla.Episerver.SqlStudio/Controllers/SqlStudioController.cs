using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Castle.DynamicProxy.Generators.Emitters.SimpleAST;
using EPiServer.Data;
using EPiServer.Security;
using Gulla.Episerver.SqlStudio.AI;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.DataAccess;
using Gulla.Episerver.SqlStudio.Dds;
using Gulla.Episerver.SqlStudio.Extensions;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace Gulla.Episerver.SqlStudio.Controllers
{
    [Route("SqlStudio")]
    public class SqlStudioController : Controller
    {
        private readonly SqlService _sqlService;
        private readonly QueryLoader _queryLoader;
        private readonly DataAccessOptions _dataAccessOptions;
        private readonly ConfigurationService _configurationService;
        private readonly SqlStudioOptions _configuration;
        private readonly OpenAiService _openAiService;
        private readonly ISqlStudioDdsRepository _sqlStudioDdsRepository;

        public SqlStudioController(SqlService sqlService, 
            QueryLoader queryLoader, 
            DataAccessOptions dataAccessOptions, 
            ConfigurationService configurationService, 
            IOptions<SqlStudioOptions> options,
            OpenAiService openAiService,
            ISqlStudioDdsRepository sqlStudioDdsRepository)
        {
            _sqlService = sqlService;
            _queryLoader = queryLoader;
            _dataAccessOptions = dataAccessOptions;
            _configurationService = configurationService;
            _configuration = options.Value;
            _openAiService = openAiService;
            _sqlStudioDdsRepository = sqlStudioDdsRepository;
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

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            var model = new SqlStudioViewModel
            {
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled,
                ShowCustomColumns = !_configuration.CustomColumnsEnabled,
                ShowAiButtons = _configurationService.IsAiGenerationEnabled(),
                ConnectionStrings = connectionStringList
            };

            FillModelWithTableMetaData(model, connectionString);

            return View(model);
        }

        [HttpPost]
        public ActionResult Index(string submit, string query, bool hideEmptyColumns, 
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

            if (submit.Contains("Generate"))
            {
                return Generate(query);
            }
            if (submit.Contains("Explain"))
            {
                return Explain(query);
            }

            // Dropdown is hidden from GUI, if there is only one connectionstring, or if the connectionstring is set in configuration
            if (string.IsNullOrEmpty(connectionString))
            {
                connectionString = GetConnectionStringList(_dataAccessOptions, _configuration).FirstOrDefault()?.Value;
            }

            // Adjusting for indexes, if more than one custom column is used. Not too happy with this!!!
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
                ConnectionStrings = GetConnectionStringList(_dataAccessOptions, _configuration),
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled,
                ShowCustomColumns = _configuration.CustomColumnsEnabled,
                ShowAiButtons = _configurationService.IsAiGenerationEnabled(),
            };

            // Check for configured allow regex pattern
            try
            {
                var allowPattern = _configuration.AllowPattern;
                if (!string.IsNullOrEmpty(allowPattern) && !Regex.Match(query, allowPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = _configuration.AllowMessage ?? "Query did not match provided allow pattern.";
                    FillModelWithTableMetaData(model, connectionString);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                FillModelWithTableMetaData(model, connectionString);
                return View(model);
            }

            // Check for configured deny regex pattern
            try
            {
                var denyPattern = _configuration.DenyPattern;
                if (!string.IsNullOrEmpty(denyPattern) && Regex.Match(query, denyPattern, RegexOptions.IgnoreCase).Success)
                {
                    model.Message = _configuration.DenyMessage ?? "Query matched provided deny pattern.";
                    FillModelWithTableMetaData(model, connectionString);
                    return View(model);
                }
            }
            catch (Exception e)
            {
                model.Message = e.Message;
                FillModelWithTableMetaData(model, connectionString);
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
            FillModelWithTableMetaData(model, connectionString);

            if (_configurationService.IsAuditLogEnabled())
            {
                _sqlStudioDdsRepository.Log(PrincipalInfo.CurrentPrincipal.Identity.Name, model.Query, model.Message, AnonymizeConnectionString(connectionString));
            }

            return View(model);
        }

        private void FillModelWithTableMetaData(SqlStudioViewModel model, string connectionString)
        {
            try
            {
                model.SavedQueries = _sqlService.GetTableNames(connectionString).Contains("SqlQueries") ? _queryLoader.GetQueries(connectionString).ToList() : Enumerable.Empty<SqlQueryCategory>();
                model.SqlAutoCompleteMetadata = _sqlService.GetMetaData(connectionString);
                model.SqlTableNameMap = _sqlService.TableNameMap(connectionString);
            }
            catch (Exception e)
            {
                model.Message += e.Message;
            }
        }

        private List<SelectListItem> GetConnectionStringList(DataAccessOptions dataAccessOptions, SqlStudioOptions configuration)
        {
            if (!string.IsNullOrEmpty(configuration.ConnectionString))
            {
                return new List<SelectListItem>
                {
                    new SelectListItem
                    {
                        Text = "Default",
                        Value = configuration.ConnectionString
                    }
                };
            }
            else
            {
                return dataAccessOptions.ConnectionStrings
                    .DistinctBy(x => x.Name)
                    .OrderByDescending(x => x.Name == _dataAccessOptions.DefaultConnectionStringName)
                    .Select(x => new SelectListItem { Text = x.Name, Value = x.ConnectionString }).ToList();
            }
        }

        private static string AnonymizeConnectionString(string connectionString)
        {
            string output;
            try
            {
                var connection = new SqlConnectionStringBuilder(connectionString);
                output = connection.UserID + " @ " + connection.InitialCatalog;
            }
            catch 
            {
                // If everything else fails, return the length of the connection string
                output = (connectionString?.Length ?? 0).ToString();
            }
            return output;
        }

        private ActionResult Generate(string query)
        {
            if (!_configurationService.Enabled() && !_configuration.AiEnabled)
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            var model = new SqlStudioViewModel
            {                
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled,
                ShowCustomColumns = !_configuration.CustomColumnsEnabled,
                ShowAiButtons = _configurationService.IsAiGenerationEnabled(),
                ConnectionStrings = connectionStringList
            };

            if (!string.IsNullOrEmpty(_configuration.AiApiKey) && !string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    model.Query = query.ToSqlCommentedLines() + "\r\n" + _openAiService.GenerateSql(query, _sqlService.GetMetaData(connectionString), _configuration.AiApiKey).Result;
                }
                catch (Exception e)
                {
                    model.Message = e.Message;
                }
            }

            FillModelWithTableMetaData(model, connectionString);

            return View(model);
        }

        private ActionResult Explain(string query)
        {
            if (!_configurationService.Enabled() && !_configuration.AiEnabled)
            {
                return new ObjectResult("Unauthorized")
                {
                    StatusCode = (int?)HttpStatusCode.Unauthorized
                };
            }

            var connectionStringList = GetConnectionStringList(_dataAccessOptions, _configuration);
            var connectionString = connectionStringList.FirstOrDefault()?.Value;

            var model = new SqlStudioViewModel
            {
                ColumnsContentId = Enumerable.Empty<Column>(),
                ColumnsLanguageBranchId = Enumerable.Empty<Column>(),
                ColumnsInsertIndex = Enumerable.Empty<Column>(),
                AutoIntelliSense = _configuration.AutoIntellisenseEnabled,
                DarkMode = _configuration.DarkModeEnabled,
                ShowCustomColumns = !_configuration.CustomColumnsEnabled,
                ShowAiButtons = _configurationService.IsAiGenerationEnabled(),
                ConnectionStrings = connectionStringList
            };

            if (!string.IsNullOrEmpty(_configuration.AiApiKey) && !string.IsNullOrWhiteSpace(query))
            {
                try
                {
                    var explaination = _openAiService.ExplainSql(query, _sqlService.GetMetaData(connectionString), _configuration.AiApiKey, _configuration.AiModel).Result;
                    model.Query = explaination.ToSqlCommentedLines() + "\r\n" + query;
                }
                catch (Exception e)
                {
                    model.Message = e.Message;
                }
            }

            FillModelWithTableMetaData(model, connectionString);

            return View(model);
        }
    }
}
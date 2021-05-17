﻿using System;
using System.Linq;
using System.Text.RegularExpressions;
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
                SavedQueries = _sqlService.GetTableNames().Contains("SqlQueries") ? _queryLoader.GetQueries().ToList() : Enumerable.Empty<SqlQueryCategory>(),
                SqlAutoCompleteMetadata = _sqlService.GetMetaData(),
                SqlTableNameMap = _sqlService.TableNameMap(),
                AutoIntelliSense = ConfigHelper.AutoHintEnabled(),
                DarkMode = ConfigHelper.DarkModeEnabled()
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

            // Execute query
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
            model.SqlTableNameMap = _sqlService.TableNameMap();
            model.AutoIntelliSense = ConfigHelper.AutoHintEnabled();
            model.DarkMode = ConfigHelper.DarkModeEnabled();

            return View("/Modules/Gulla.Episerver.SqlStudio/Views/Index.cshtml", model);
        }
    }
}
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.DataAbstraction;
using EPiServer.Web.Routing;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class SqlService
    {
        private readonly IContentLoader _contentLoader;
        private readonly ILanguageBranchRepository _languageBranchRepository;
        private readonly IUrlResolver _urlResolver;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public SqlService(IContentLoader contentLoader, ILanguageBranchRepository languageBranchRepository, IUrlResolver urlResolver, IHttpContextAccessor httpContextAccessor)
        {
            _contentLoader = contentLoader;
            _languageBranchRepository = languageBranchRepository;
            _urlResolver = urlResolver;
            _httpContextAccessor = httpContextAccessor;
        }

        public IEnumerable<IEnumerable<string>> ExecuteQuery(string query, string connectionString)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();
                try
                {
                    return reader.GetAllColumnsStringListList().ToList();
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public IEnumerable<IEnumerable<string>> AddContentName(IEnumerable<IEnumerable<string>> result, int insertNewColumnAtIndex, string heading, int contentIdIndex, int languangeBranchIdIndex)
        {
            var headings = result.First().ToList();
            int insertNewColumnAtIndexAdjusted = insertNewColumnAtIndex == -1 ? headings.Count : insertNewColumnAtIndex;

            headings.Insert(insertNewColumnAtIndexAdjusted, heading);
            yield return headings;

            if (languangeBranchIdIndex == -1)
            {
                foreach (var row in result.Skip(1))
                {
                    var contentName = "";
                    var rowAsList = row.ToList();
                    var contentIdString = rowAsList.ElementAt(contentIdIndex);
                    if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                    {
                        if (_contentLoader.TryGet(new ContentReference(contentId), new LoaderOptions { LanguageLoaderOption.MasterLanguage() }, out IContent content))
                        {
                            contentName = content.Name;
                        }
                    }
                    rowAsList.Insert(insertNewColumnAtIndexAdjusted, contentName);
                    yield return rowAsList;
                }
            }
            else
            {
                var languages = _languageBranchRepository.ListAll();

                foreach (var row in result.Skip(1))
                {
                    var contentName = "";
                    var rowAsList = row.ToList();
                    var contentIdString = rowAsList.ElementAt(contentIdIndex);
                    if (rowAsList.Count > languangeBranchIdIndex && int.TryParse(rowAsList.ElementAt(languangeBranchIdIndex), out int languageBranchId))
                    {
                        var languageBranch = languages.SingleOrDefault(x => x.ID == languageBranchId);
                        if (languageBranch != null)
                        {
                            var language = new CultureInfo(languageBranch.LanguageID);
                            if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                            {
                                if (_contentLoader.TryGet(new ContentReference(contentId), language, out IContent content))
                                {
                                    contentName = content.Name;
                                }
                            }
                        }
                    }
                    rowAsList.Insert(insertNewColumnAtIndexAdjusted, contentName);
                    yield return rowAsList;
                }
            }
        }

        public IEnumerable<IEnumerable<string>> AddContentLink(IEnumerable<IEnumerable<string>> result, int insertNewColumnAtIndex, string heading, int contentIdIndex, int languangeBranchIdIndex)
        {
            var headings = result.First().ToList();
            int insertNewColumnAtIndexAdjusted = insertNewColumnAtIndex == -1 ? headings.Count() : insertNewColumnAtIndex;

            headings.Insert(insertNewColumnAtIndexAdjusted, heading);
            yield return headings;

            if (languangeBranchIdIndex == -1)
            {
                foreach (var row in result.Skip(1))
                {
                    var url = "";
                    var rowAsList = row.ToList();
                    var contentIdString = rowAsList.ElementAt(contentIdIndex);
                    if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                    {
                        url = GetExternalUrl(new ContentReference(contentId));
                    }                    
                    rowAsList.Insert(insertNewColumnAtIndexAdjusted, url);
                    yield return rowAsList;
                }
            }
            else
            {
                var languages = _languageBranchRepository.ListAll();

                foreach (var row in result.Skip(1))
                {
                    var url = "";
                    var rowAsList = row.ToList();
                    if (rowAsList.Count > languangeBranchIdIndex && int.TryParse(rowAsList.ElementAt(languangeBranchIdIndex), out int languageId))
                    {
                        var language = languages.SingleOrDefault(x => x.ID == languageId)?.LanguageID;
                        if (!string.IsNullOrEmpty(language))
                        {
                            var contentIdString = rowAsList.ElementAt(contentIdIndex);
                            if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                            {
                                url = GetExternalUrl(new ContentReference(contentId), language);
                            }
                        }
                    }
                    rowAsList.Insert(insertNewColumnAtIndexAdjusted, url);
                    yield return rowAsList;
                }
            }
        }

        private string GetExternalUrl(ContentReference contentLink)
        {
            var content =_contentLoader.Get<IContent>(contentLink, new LoaderOptions { LanguageLoaderOption.MasterLanguage() });
            var language = (content as ILocalizable)?.Language;

            var result = _urlResolver.GetUrl(contentLink, language?.Name);

            if (!string.IsNullOrEmpty(result) && Uri.TryCreate(result, UriKind.RelativeOrAbsolute, out var relativeUri) && relativeUri.IsAbsoluteUri == false)
            {
                var absoluteUri = new Uri(_httpContextAccessor.HttpContext.Request.Scheme  + "://" + _httpContextAccessor.HttpContext.Request.Host + relativeUri);
                return absoluteUri.AbsoluteUri;
            }

            return result;
        }

        private string GetExternalUrl(ContentReference contentLink, string language)
        {
            var result = _urlResolver.GetUrl(contentLink, language);

            if (Uri.TryCreate(result, UriKind.RelativeOrAbsolute, out var relativeUri) && relativeUri.IsAbsoluteUri == false)
            {
                var absoluteUri = new Uri(_httpContextAccessor.HttpContext.Request.Scheme + "://" + _httpContextAccessor.HttpContext.Request.Host.ToString() + relativeUri);
                return absoluteUri.AbsoluteUri;
            }

            return result;
        }

        public string GetMetaData(string connectionString)
        {
            return string.Join(",\r\n", GetMetaDataRows(connectionString, true));
        }

        public string TableNameMap(string connectionString)
        {
            return string.Join(",\r\n", GetTableNameMapRows(connectionString));
        }

        private IEnumerable<string> GetTableNameMapRows(string connectionString)
        {
            var tableNames = GetTableNames(connectionString);
            foreach (var tableName in tableNames)
            {
                yield return $"\t\t\"{tableName.ToUpper()}\" : \"{tableName}\"";
            }
        }

        private IEnumerable<string> GetMetaDataRows(string connectionString, bool wrap = false)
        {
            var tableNames = GetTableNames(connectionString);
            foreach (var tableName in tableNames)
            {
                yield return $"\t\t\t\t\t\"{tableName}\": [{string.Join(", ", GetColumnNames(connectionString, tableName, wrap))}]";
            }
        }

        public IEnumerable<string> GetTableNames(string connectionString)
        {
            var query = "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME ASC";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();
                try
                {
                    return reader.GetStringList("TABLE_NAME").ToList();
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        private IEnumerable<string> GetColumnNames(string connectionString, string tableName, bool wrap = false)
        {
            var query = "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                command.Parameters.Add(new SqlParameter
                {
                    ParameterName = "@TableName",
                    SqlDbType = SqlDbType.VarChar,
                    Size = 256,
                    Value = tableName
                });
                var reader = command.ExecuteReader();
                try
                {
                    return reader.GetStringList("COLUMN_NAME", wrap).ToList();
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}
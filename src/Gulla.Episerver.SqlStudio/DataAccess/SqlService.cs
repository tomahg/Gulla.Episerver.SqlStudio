using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using EPiServer;
using EPiServer.Core;
using EPiServer.Data;
using EPiServer.DataAbstraction;
using EPiServer.Web;
using EPiServer.Web.Routing;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class SqlService
    {
        private IContentLoader _contentLoader;
        private ILanguageBranchRepository _languageBranchRepository;
        private IUrlResolver _urlResolver;
        private ISiteDefinitionResolver _siteDefinitionResolver;
        private IDatabaseExecutor Executor { get; }

        public SqlService(IDatabaseExecutor databaseExecutor, IContentLoader contentLoader, ILanguageBranchRepository languageBranchRepository, IUrlResolver urlResolver, ISiteDefinitionResolver siteDefinitionResolver)
        {
            Executor = databaseExecutor;
            _contentLoader = contentLoader;
            _languageBranchRepository = languageBranchRepository;
            _urlResolver = urlResolver;
            _siteDefinitionResolver = siteDefinitionResolver;
        }

        public IEnumerable<IEnumerable<string>> ExecuteQuery(string query)
        {
            if (string.IsNullOrEmpty(query))
            {
                return null;
            }

            return Executor.Execute(() =>
            {
                using (var command = Executor.CreateCommand(query, CommandType.Text))
                {
                    return command.ExecuteReader().GetAllColumnsStringListList().ToList();
                }
            });
        }

        public IEnumerable<IEnumerable<string>> AddContentName(IEnumerable<IEnumerable<string>> result, int insertNewColumnAtIndex, string heading, int contentIdIndex, int languangeBranchIdIndex)
        {
            var headings = result.First().ToList();
            int insertNewColumnAtIndexAdjusted = insertNewColumnAtIndex == -1 ? headings.Count() : insertNewColumnAtIndex;

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
                    var language = new CultureInfo(languages.Single(x => x.ID == int.Parse(rowAsList.ElementAt(languangeBranchIdIndex))).LanguageID);
                    if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                    {
                        if (_contentLoader.TryGet(new ContentReference(contentId), language, out IContent content))
                        {
                            contentName = content.Name;
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
                    var language = languages.SingleOrDefault(x => x.ID == int.Parse(rowAsList.ElementAt(languangeBranchIdIndex)))?.LanguageID;
                    var contentIdString = rowAsList.ElementAt(contentIdIndex);
                    if (!string.IsNullOrEmpty(contentIdString) && int.TryParse(contentIdString, out int contentId))
                    {
                        url = GetExternalUrl(new ContentReference(int.Parse(rowAsList.ElementAt(contentIdIndex))), language);
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
                var siteDefinition = _siteDefinitionResolver.GetByContent(contentLink, true, true);
                var baseUri = siteDefinition.GetHosts(language, true).Where(x => x.Url != null).First();
                var absoluteUri = new Uri(baseUri.Url, relativeUri);

                return absoluteUri.AbsoluteUri;
            }

            return result;
        }

        private string GetExternalUrl(ContentReference contentLink, string language)
        {
            var result = _urlResolver.GetUrl(contentLink, language);

            if (Uri.TryCreate(result, UriKind.RelativeOrAbsolute, out var relativeUri) && relativeUri.IsAbsoluteUri == false)
            {
                var siteDefinition = _siteDefinitionResolver.GetByContent(contentLink, true, true);
                var baseUri = siteDefinition.GetHosts(new CultureInfo(language), true).Where(x => x.Url != null).First();
                var absoluteUri = new Uri(baseUri.Url, relativeUri);

                return absoluteUri.AbsoluteUri;
            }

            return result;
        }

        public string GetMetaData()
        {
            return string.Join(",\r\n", GetMetaDataRows(true));
        }

        public string TableNameMap()
        {
            return string.Join(",\r\n", GetTableNameMapRows());
        }

        private IEnumerable<string> GetTableNameMapRows()
        {
            var tableNames = GetTableNames();
            foreach (var tableName in tableNames)
            {
                yield return $"\t\t\"{tableName.ToUpper()}\" : \"{tableName}\"";
            }
        }

        private IEnumerable<string> GetMetaDataRows(bool wrap = false)
        {
            var tableNames = GetTableNames();
            foreach (var tableName in tableNames)
            {
                yield return $"\t\t\t\t\t{tableName}: [{string.Join(", ", GetColumnNames(tableName, wrap))}]";
            }
        }

        public IEnumerable<string> GetTableNames()
        {
            return Executor.Execute(() =>
            {
                using (var command = Executor.CreateCommand(
                    "SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'BASE TABLE' ORDER BY TABLE_NAME ASC",
                    CommandType.Text))
                {
                    return command.ExecuteReader().GetStringList("TABLE_NAME").ToList();
                }
            });
        }

        private IEnumerable<string> GetColumnNames(string tableName, bool wrap = false)
        {
            return Executor.Execute(() =>
            {
                using (var command = Executor.CreateCommand(
                    "SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName",
                    CommandType.Text
                    ))
                {
                    command.Parameters.Add(new SqlParameter
                    {
                        ParameterName = "@TableName",
                        SqlDbType = SqlDbType.VarChar,
                        Size = 256,
                        Value = tableName
                    });
                    command.Prepare();
                    return command.ExecuteReader().GetStringList("COLUMN_NAME", wrap).ToList();
                }
            });
        }
    }
}
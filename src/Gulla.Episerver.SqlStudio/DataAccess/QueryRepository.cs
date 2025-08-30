using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using EPiServer.Security;
using Gulla.Episerver.SqlStudio.Configuration;
using Gulla.Episerver.SqlStudio.Controllers;
using Gulla.Episerver.SqlStudio.Dds;
using Gulla.Episerver.SqlStudio.ViewModels;
using Microsoft.Data.SqlClient;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class QueryRepository
    {
        public IEnumerable<SqlQueryCategory> GetQueries(string connectionString, bool keepSortPrefix)
        {
            const string query = "SELECT Name, Category, Query FROM SqlQueries ORDER BY Category, Name";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();
                try
                {
                    return reader.GetQueryCategoryList(keepSortPrefix).ToList();
                }
                finally
                {
                    reader.Close();
                }
            }
        }

        public void DeleteQuery(ConfigurationService configurationService, ISqlStudioDdsRepository sqlStudioDdsRepository, string connectionString, string category, string name)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string is required.", nameof(connectionString));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                return; // Nothing to delete without a name
            }

            // Normalize category: treat null/whitespace as empty for matching
            var normalizedCategory = string.IsNullOrWhiteSpace(category) ? string.Empty : category;

            int rows = 0;
            string sql = @"DELETE FROM SqlQueries WHERE Name = @name AND Category = @category;";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                using (var cmd = new SqlCommand(sql, connection))
                {
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = name;
                    cmd.Parameters.Add("@category", SqlDbType.NVarChar, 255).Value = normalizedCategory;
                    rows = cmd.ExecuteNonQuery();
                }
            }

            if (configurationService.IsAuditLogEnabled())
            {
                var message = rows + " record " + (rows == 1 ? "" : "s") + " deleted.";
                var replacedSql = sql.Replace("@name", $"'{name}'").Replace("@category", $"'{category}'");
                sqlStudioDdsRepository.Log(PrincipalInfo.CurrentPrincipal.Identity.Name, replacedSql, message, SqlStudioController.AnonymizeConnectionString(connectionString));
            }
        }

        public void UpdateQuery(
            ConfigurationService configurationService,
            ISqlStudioDdsRepository ddsRepository,
            string connectionString,
            string category,
            string name,
            string newQuery)
        {
            // 1) Update the DB
            const string sql = @"
                UPDATE SqlQueries
                SET Query = @query
                WHERE Name = @name AND Category = @category;";

            var normalizedCategory = string.IsNullOrWhiteSpace(category) ? string.Empty : category;

            using (var cn = new SqlConnection(connectionString))
            {
                cn.Open();
                using (var cmd = new SqlCommand(sql, cn))
                {
                    cmd.Parameters.Add("@name", SqlDbType.NVarChar, 255).Value = name;
                    cmd.Parameters.Add("@category", SqlDbType.NVarChar, 255).Value = normalizedCategory;
                    cmd.Parameters.Add("@query", SqlDbType.NVarChar).Value = newQuery ?? string.Empty;
                    cmd.ExecuteNonQuery();
                }
            }
        }
    }
}
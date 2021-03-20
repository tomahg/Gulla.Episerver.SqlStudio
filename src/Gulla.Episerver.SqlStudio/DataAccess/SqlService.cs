using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using EPiServer.Data;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class SqlService
    {
        private IDatabaseExecutor Executor { get; }

        public SqlService(IDatabaseExecutor databaseExecutor)
        {
            Executor = databaseExecutor;
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
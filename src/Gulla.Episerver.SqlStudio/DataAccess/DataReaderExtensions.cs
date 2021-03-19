using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public static class DataReaderExtensions
    {
        public static IEnumerable<string> GetStringList(this IDataReader dataReader, string columnName, bool wrap = false)
        {
            while (true)
            {
                var databaseString = GetString(dataReader, columnName, wrap);
                if (databaseString == null)
                {
                    yield break;
                }

                yield return databaseString;
            }
        }

        private static string GetString(this IDataReader dataReader, string columnName, bool wrap = false)
        {
            if (!dataReader.Read())
            {
                return null;
            }

            var databaseString = (string)dataReader[columnName];
            return wrap ? "\"" + databaseString + "\"" : databaseString;
        }

        public static IEnumerable<SqlQueryCategory> GetQueryCategoryList(this IDataReader dataReader)
        {
            var flatQueryList = new List<SqlQuery>();
            while (true)
            {
                var query = GetQuery(dataReader);
                if (query == null)
                {
                    break;
                }

                flatQueryList.Add(query);
            }

            if (flatQueryList.Count == 0)
            {
                yield break;
            }

            var first = true;
            string category = null;
            var categoryQueries = new List<SqlQuery>();
            foreach (var query in flatQueryList)
            {
                if (first || category != query.Category)
                {
                    if (!first)
                    {
                        yield return new SqlQueryCategory {Name = category, Queries = categoryQueries};
                    }

                    category = query.Category;
                    categoryQueries = new List<SqlQuery>();
                }

                categoryQueries.Add(query);
                first = false;
            }
            yield return new SqlQueryCategory { Name = category, Queries = categoryQueries };
        }

        public static IEnumerable<IEnumerable<string>> GetAllColumnsStringListList(this IDataReader dataReader)
        {
            // TODO: Not optimal...
            if (dataReader.RecordsAffected > 0)
            {
                throw new Exception(dataReader.RecordsAffected + " record" + (dataReader.RecordsAffected != 1 ? "s" : "") + " affected.");
            }

            yield return GetColumnNames(dataReader).ToList();

            while (true)
            {
                var databaseString = GetAllColumnsStringList(dataReader).ToList();
                if (!databaseString.Any())
                {
                    yield break;
                }

                yield return databaseString;
            }
        }

        private static IEnumerable<string> GetColumnNames(IDataReader dataReader)
        {
            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                yield return dataReader.GetName(i);
            }
        }

        private static IEnumerable<string> GetAllColumnsStringList(this IDataReader dataReader)
        {
            if (!dataReader.Read())
            {
                yield break;
            }

            for (var i = 0; i < dataReader.FieldCount; i++)
            {
                yield return Convert.ToString(dataReader.GetValue(i));
            }
        }

        private static SqlQuery GetQuery(this IDataReader dataReader)
        {
            if (!dataReader.Read())
            {
                return null;
            }

            return new SqlQuery
            {
                Name = (string)dataReader[nameof(SqlQuery.Name)],
                Category = (string)dataReader[nameof(SqlQuery.Category)], 
                Query = (string)dataReader[nameof(SqlQuery.Query)] 
            };
        }

        public static IEnumerable<IEnumerable<string>> HideEmptyColumns(this IEnumerable<IEnumerable<string>> result, bool active)
        {
            if (!active || result == null)
            {
                return result;
            }

            // If the result only contains the header, no reason to hide anything
            var resultList = result.ToList();
            if (resultList.Count < 2)
            {
                return resultList;
            }

            // Skip the header
            var numberOfColumns = resultList.First().Count();
            var hideIndexes = new bool[numberOfColumns];
            for (var i = 0; i < hideIndexes.Length; i++)
            {
                hideIndexes[i] = true;
            }

            for (var row = 1; row < resultList.Count; row++)
            {
                for (var column = 0; column < numberOfColumns; column++)
                {
                    if (!string.IsNullOrEmpty(resultList[row].ElementAt(column)))
                    {
                        hideIndexes[column] = false;
                    }
                }
            }

            return HideColumnsWithIndex(resultList, hideIndexes);
        }

        private static IEnumerable<IEnumerable<string>> HideColumnsWithIndex(IEnumerable<IEnumerable<string>> result, IReadOnlyCollection<bool> hideIndexes)
        {
            return result.Select(row => HideColumnsWithIndex(row, hideIndexes));
        }

        private static IEnumerable<string> HideColumnsWithIndex(IEnumerable<string> columns, IReadOnlyCollection<bool> hideIndexes)
        {
            var list = columns.ToList();
            for (var i = list.Count - 1; i >= 0; i--)
            {
                if (hideIndexes.ElementAt(i))
                {
                    list.RemoveAt(i);
                }
            }
            return list;
        }
    }
}
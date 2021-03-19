using System.Collections.Generic;
using System.Data;
using System.Linq;
using EPiServer.Data;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class QueryLoader
    {
        private IDatabaseExecutor Executor { get; }

        public QueryLoader(IDatabaseExecutor databaseExecutor)
        {
            Executor = databaseExecutor;
        }

        public IEnumerable<SqlQueryCategory> GetQueries()
        {
            return Executor.Execute(() =>
            {
                using (var command = Executor.CreateCommand(
                    "SELECT Name, Category, Query FROM SqlQueries ORDER BY Category, Name",
                    CommandType.Text))
                {
                    return command.ExecuteReader().GetQueryCategoryList().ToList();
                }
            });
        }
    }
}
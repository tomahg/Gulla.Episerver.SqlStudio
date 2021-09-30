using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.DataAccess
{
    public class QueryLoader
    {
        public IEnumerable<SqlQueryCategory> GetQueries(string connectionString)
        {
            var query = "SELECT Name, Category, Query FROM SqlQueries ORDER BY Category, Name";

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = new SqlCommand(query, connection);
                var reader = command.ExecuteReader();
                try
                {
                    return reader.GetQueryCategoryList().ToList();
                }
                finally
                {
                    reader.Close();
                }
            }
        }
    }
}
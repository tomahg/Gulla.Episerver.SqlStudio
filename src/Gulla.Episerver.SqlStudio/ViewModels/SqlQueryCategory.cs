using System.Collections.Generic;

namespace Gulla.Episerver.SqlStudio.ViewModels
{
    public class SqlQueryCategory
    {
        public string Name;
        public IEnumerable<SqlQuery> Queries;
    }
}
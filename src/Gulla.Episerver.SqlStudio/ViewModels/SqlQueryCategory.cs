using System.Collections.Generic;

namespace Gulla.Episerver.SqlStudio.ViewModels
{
    public class SqlQueryCategory
    {
        public string Name { get; set; }
        public IEnumerable<SqlQuery> Queries { get; set; }
    }
}
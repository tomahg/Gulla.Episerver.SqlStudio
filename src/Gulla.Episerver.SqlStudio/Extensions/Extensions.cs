using System.Collections.Generic;
using Gulla.Episerver.SqlStudio.ViewModels;

namespace Gulla.Episerver.SqlStudio.Extensions
{
    public static class Extensions
    {
        public static IEnumerable<Column> GetColumnList(this IEnumerable<string> columns, string firstElement)
        {
            int index = 0;

            yield return new Column { Id = -1, Name = firstElement };

            foreach (var column in columns)
            {
                yield return new Column { Id = index++, Name = column };
            }
        }

        public static IEnumerable<Column> GetColumnListForInserting(this IEnumerable<string> columns, string firstElement)
        {
            int index = 0;

            yield return new Column { Id = -1, Name = firstElement };

            yield return new Column { Id = index++, Name = "First" };

            foreach (var column in columns)
            {
                yield return new Column { Id = index++, Name = "After " + column };
            }
        }
    }
}

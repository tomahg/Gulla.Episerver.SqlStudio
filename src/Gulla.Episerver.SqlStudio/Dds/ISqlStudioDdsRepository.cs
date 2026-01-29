using System.Collections.Generic;

namespace Gulla.Episerver.SqlStudio.Dds
{
    public interface ISqlStudioDdsRepository
    {
        public void Log(string username, string query, string message, string connectionstring);

        public void DeleteOldLogEntries(int olderThanDays);

        public void DeleteAll();

        public void DeleteForUser(string username);

        public IEnumerable<SqlStudioDdsLogItem> ListAll(string username = null);
    }
}

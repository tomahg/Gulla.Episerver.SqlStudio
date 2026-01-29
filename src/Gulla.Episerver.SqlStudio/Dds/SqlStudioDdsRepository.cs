using System;
using System.Collections.Generic;
using System.Linq;
using EPiServer.Data.Dynamic;
using EPiServer.ServiceLocation;

namespace Gulla.Episerver.SqlStudio.Dds
{
    [ServiceConfiguration(typeof(ISqlStudioDdsRepository))]
    public class SqlStudioDdsRepository : ISqlStudioDdsRepository
    {
        private readonly DynamicDataStoreFactory _dataStoreFactory;

        public SqlStudioDdsRepository(DynamicDataStoreFactory dataStoreFactory)
        {
            _dataStoreFactory = dataStoreFactory;
        }

        public void Log(string username, string query, string message, string connectionstring)
        {
            var store = GetStore();

            var logItem = new SqlStudioDdsLogItem
            {
                UserName = username,
                Query = query,
                Message = message,
                Timestamp = DateTime.UtcNow,
                ConnectionString = connectionstring
            };

            store.Save(logItem);
        }

        public void DeleteOldLogEntries(int olderThanDays)
        {
            var store = _dataStoreFactory.GetStore(typeof(SqlStudioDdsLogItem));
            if (store != null)
            {
                var itemsToDelete = store.Items<SqlStudioDdsLogItem>().Where(x => x.Timestamp < DateTime.Now.AddDays(-olderThanDays));
                foreach (var item in itemsToDelete)
                {
                    store.Delete(item);
                }
            }
        }

        public void DeleteAll()
        {
            _dataStoreFactory.DeleteStore(typeof(SqlStudioDdsLogItem), true);
        }

        public void DeleteForUser(string username)
        {
            var store = _dataStoreFactory.GetStore(typeof(SqlStudioDdsLogItem));
            if (store != null)
            {
                var itemsToDelete = store.Items<SqlStudioDdsLogItem>().Where(x => x.UserName == username);
                foreach (var item in itemsToDelete)
                {
                    store.Delete(item);
                }
            }
        }

        public IEnumerable<SqlStudioDdsLogItem> ListAll(string username = null)
        {
            return GetStore().LoadAll<SqlStudioDdsLogItem>()
                .Where(x => username == null || x.UserName == username)
                .OrderByDescending(x => x.Timestamp);
        }

        private DynamicDataStore GetStore()
        {
            return _dataStoreFactory.GetStore(typeof(SqlStudioDdsLogItem)) ?? _dataStoreFactory.CreateStore(typeof(SqlStudioDdsLogItem));
        }
    }
}

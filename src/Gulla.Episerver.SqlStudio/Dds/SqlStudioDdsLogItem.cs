using System;
using EPiServer.Data;
using EPiServer.Data.Dynamic;

namespace Gulla.Episerver.SqlStudio.Dds
{
    public class SqlStudioDdsLogItem : IDynamicData
    {
        public Identity Id { get; set; }

        public string UserName { get; set; }

        public DateTime Timestamp { get; set; }

        public string Query { get; set; }

        public string Message { get; set; }

        public string ConnectionString { get; set; }
    }
}

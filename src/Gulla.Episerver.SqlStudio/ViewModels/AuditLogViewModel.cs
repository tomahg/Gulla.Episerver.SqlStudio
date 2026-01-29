using System.Collections.Generic;
using System.Linq;
using Gulla.Episerver.SqlStudio.Dds;

namespace Gulla.Episerver.SqlStudio.ViewModels
{
    public class AuditLogViewModel
    {
        public IEnumerable<SqlStudioDdsLogItem> Logs { get; set; }
        public bool HasResults => Logs?.Any() == true;
        public bool ShowDeleteAllLogsButton { get; set; }
        public bool ShowDeleteMyLogsButton { get; set; }
        public int LogsCount { get; set; }
    }
}
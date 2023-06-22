using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Gulla.Episerver.SqlStudio.ViewModels
{
    public class SqlStudioViewModel
    {
        public string Query;
        public string SqlAutoCompleteMetadata;
        public string SqlTableNameMap;
        public IEnumerable<SqlQueryCategory> SavedQueries;
        public IEnumerable<IEnumerable<string>> SqlResult;
        public bool HasResults => SqlResult?.FirstOrDefault()?.Any() == true;
        public string Message;
        public bool HideEmptyColumns;
        public bool AutoIntelliSense;
        public bool DarkMode;
        public bool ShowCustomColumns;
        public bool ShowAiButtons;
        public IEnumerable<Column> ColumnsContentId;
        public IEnumerable<Column> ColumnsLanguageBranchId;
        public IEnumerable<Column> ColumnsInsertIndex;
        public int ContentNameIndex;
        public int ContentNameLanguageIndex;
        public int ContentNameInsertIndex;
        public string ContentNameHeading;
        public int ContentLinkIndex;
        public int ContentLinkLanguageIndex;
        public int ContentLinkInsertIndex;
        public string ContentLinkHeading;
        public string ConnectionString;
        public IEnumerable<SelectListItem> ConnectionStrings;
    }
}
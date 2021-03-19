﻿using System.Collections.Generic;

namespace Gulla.Episerver.SqlStudio.ViewModels
{
    public class SqlStudioViewModel
    {
        public string Query;
        public string SqlAutoCompleteMetadata;
        public IEnumerable<SqlQueryCategory> SavedQueries;
        public IEnumerable<IEnumerable<string>> SqlResult;
        public string Message;
        public bool HideEmptyColumns;
    }
}
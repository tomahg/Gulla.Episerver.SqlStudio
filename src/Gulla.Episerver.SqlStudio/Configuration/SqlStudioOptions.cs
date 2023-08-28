namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class SqlStudioOptions
    {
        public bool Enabled { get; set; } = true;

        public string Users { get; set; }

        public string GroupNames { get; set; }

        public bool AutoIntellisenseEnabled { get; set; } = true;

        public bool DarkModeEnabled { get; set; } = true;

        public bool CustomColumnsEnabled { get; set; } = true;

        public bool ShowSavedQueries { get; set; } = true;

        public string AllowPattern { get; set; }

        public string AllowMessage { get; set; }

        public string DenyPattern { get; set; }

        public string DenyMessage { get; set; }

        public string ConnectionString { get; set; }

        public bool DisableAuditLog { get; set; }

        public string AuditLogViewAllUsers { get; set; }

        public string AuditLogViewAllGroupNames { get; set; }

        public string AuditLogDeleteUsers { get; set; }

        public string AuditLogDeleteGroupNames { get; set; }

        public int AuditLogDaysToKeep { get; set; } = 30;

        public bool AiEnabled { get; set; } = true;

        public string AiApiKey { get; set; }
        
        public string AiModel { get; set; } = "gpt-4";
    }
}

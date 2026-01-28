namespace Gulla.Episerver.SqlStudio.Configuration
{
    public class SqlStudioOptions
    {
        public bool Enabled { get; set; } = true;

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


        public int AuditLogDaysToKeep { get; set; } = 30;

        public bool AiEnabled { get; set; } = true;

        public string AiApiKey { get; set; }
        
        public string AiModel { get; set; } = "gpt-5";

        public double AiTemperature { get; set; } = 1.0;
    }
}

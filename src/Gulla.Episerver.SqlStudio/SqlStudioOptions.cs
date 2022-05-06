namespace Gulla.Episerver.SqlStudio
{
    public class SqlStudioOptions
    {
        public bool Enabled { get; set; } = true;

        public string Users { get; set; }

        public bool AutoIntellisenseEnabled { get; set; } = false;

        public bool DarkModeEnabled { get; set; } = false;

        public string AllowPattern { get; set; }

        public string AllowMessage { get; set; }

        public string DenyPattern { get; set; }

        public string DenyMessage { get; set; }
    }
}

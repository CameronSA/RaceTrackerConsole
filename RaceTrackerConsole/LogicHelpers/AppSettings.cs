namespace RaceTrackerConsole.LogicHelpers
{
    using System.Configuration;

    public static class AppSettings
    {
        public static string RaceDataDirectory
        { 
            get
            {
                return ConfigurationManager.AppSettings["RaceDataDirectory"];
            }
        }

        public static string RaceDataWebsite
        {
            get
            {
                return ConfigurationManager.AppSettings["RaceDataWebsite"];
            }
        }

        public static char Delimiter
        {
            get
            {
                return ',';
            }
        }
    }
}

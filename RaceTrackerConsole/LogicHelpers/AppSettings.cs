namespace RaceTrackerConsole.LogicHelpers
{
    using System.Configuration;

    public static class AppSettings
    {
        public static string RaceRawDataDirectory
        { 
            get
            {
                return ConfigurationManager.AppSettings["RaceRawDataDirectory"];
            }
        }

        public static string RaceProcessedDataDirectory
        {
            get
            {
                return ConfigurationManager.AppSettings["RaceProcessedDataDirectory"];
            }
        }

        public static string RaceDataWebsite
        {
            get
            {
                return ConfigurationManager.AppSettings["RaceDataWebsite"];
            }
        }

        public static string RaceDataResult
        {
            get
            {
                return ConfigurationManager.AppSettings["RaceDataResult"];
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

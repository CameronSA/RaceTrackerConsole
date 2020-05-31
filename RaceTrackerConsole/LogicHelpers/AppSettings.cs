namespace RaceTrackerConsole.LogicHelpers
{
    using System;
    using System.Configuration;
    using System.Data.SqlClient;

    public static class AppSettings
    {
        public static string[] ProcessedDataColumns
        {
            get
            {
                return new string[] { "Going", "Surface", "Race Type", "Age", "Rated", "Prize", "Distance", "Racetrack", "Date", "Time", "Position", "Draw", "Horse Name", "Horse Age", "Horse Weight", "ISP", "Expectation" };
            }
        }

        public static string[] ReportHeaderFieldList
        {
            get
            {
                return new string[] { "Distance", "Prize", "Rated", "Age", "Race Type", "Surface", "Going" };
            }
        }

        public static string Error
        {
            get
            {
                return "ERROR";
            }
        }

        public static string Null
        {
            get
            {
                return "NULL";
            }
        }

        public static string OldestDateMinedFile
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "OldestDateMined.txt";
            }
        }

        public static string MostRecentDateMinedFile
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + "MostRecentDateMined.txt";
            }
        }

        public static string AcknowledgedRawDataDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"AcknowledgedRawData\";
            }
        }

        public static string FileMonitorsDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"FileMonitors\";
            }
        }

        public static string CompiledDataDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"CompiledData\";
            }
        }

        public static string RaceRawDataDirectory
        { 
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"RawData\";
            }
        }

        public static string RaceProcessedDataDirectory
        {
            get
            {
                return AppDomain.CurrentDomain.BaseDirectory + @"ProcessedData\";
            }
        }

        public static string RawDataFilePrefix
        {
            get
            {
                return "RaceRawData_";
            }
        }

        public static string ProcessedDataFilePrefix
        {
            get
            {
                return "RaceProcessedData_";
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

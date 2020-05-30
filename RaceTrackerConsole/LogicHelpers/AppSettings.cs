﻿namespace RaceTrackerConsole.LogicHelpers
{
    using System;
    using System.Configuration;

    public static class AppSettings
    {
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

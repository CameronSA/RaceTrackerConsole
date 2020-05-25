namespace RaceTrackerConsole
{
    using log4net;
    using RaceTrackerConsole.LogicHelpers;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;

    public class Program
    {
        private static Log log;

        public static void Main(string[] args)
        {
            log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
            ExceptionLogger.Exceptions = new List<Exception>();
            ProcessArgs(args);
            if (ExceptionLogger.Exceptions.Count > 0)
            {
                Console.WriteLine("\nExceptions encountered:\n");
                foreach (var exception in ExceptionLogger.Exceptions)
                {
                    Console.WriteLine(exception.Message);
                }
            }
        }

        private static void ProcessArgs(string[] args)
        {
            if (args.Length >= 1)
            {
                switch(args[0].ToLower())
                {
                    case "-help": PrintHelpMessage(); return;
                    case "-minedata": MineData(args); return;
                    case "-processdata": ProcessData(args); return;
                    default:
                        Console.WriteLine("Invalid input argument");
                        PrintHelpMessage();
                        return;
                }
            }

            Console.WriteLine("Must provide input arguments");
            PrintHelpMessage();
            return;
        }

        private static void ProcessData(string[] args)
        {
            if (args.Length >= 2)
            {
                var dataProcessing = new DataProcessing();
                switch (args[1].ToLower())
                {
                    case "-daterange":
                        if (args.Length < 4)
                        {
                            Console.WriteLine("Not enough arguments");
                            PrintHelpMessage();
                            return;
                        }

                        var dates = GetDateRange(args, 2, 3);
                        if (dates.Count < 1)
                        {
                            return;
                        }

                        dataProcessing.FormatDailyData(dates);

                        return;
                    case "-all":
                        if (Directory.Exists(AppSettings.RaceRawDataDirectory))
                        {
                            foreach (var file in Directory.GetFiles(AppSettings.RaceRawDataDirectory))
                            {
                                try
                                {
                                    dataProcessing.FormatFileData(file);
                                }
                                catch (Exception e)
                                {
                                    log.Error("Error occurred whilst processing data for file '" + file + "': ", ExceptionLogger.LogException(e));
                                }
                            }
                        }

                        return;
                    default:
                        Console.WriteLine("Invalid input");
                        PrintHelpMessage();
                        return;
                }
            }
        }

        private static void MineData(string[] args)
        {
            if (args.Length >= 3)
            {
                var dates = GetDateRange(args, 1, 2);
                if (dates.Count < 1)
                {
                    return;
                }

                var dataMining = new DataMining();
                dataMining.DailyData(dates);
                return;
            }

            Console.WriteLine("Not enough arguments");
            PrintHelpMessage();
            return;
        }

        private static List<DateTime> GetDateRange(string[] args, int startDateIndex, int endDateIndex)
        {
            DateTime startDate, endDate;
            try
            {
                if (args[startDateIndex].ToLower() == "-today")
                {
                    startDate = DateTime.Today;
                }
                else
                {
                    startDate = DateTime.ParseExact(args[startDateIndex], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (args[endDateIndex].ToLower() == "-today")
                {
                    endDate = DateTime.Today;
                }
                else
                {
                    endDate = DateTime.ParseExact(args[endDateIndex], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
            }
            catch
            {
                Console.WriteLine("Failed to process input dates: " + args[startDateIndex] + " " + args[endDateIndex]);
                PrintHelpMessage();
                return new List<DateTime>();
            }

            if (startDate > endDate)
            {
                Console.WriteLine("End date must be later than start date");
                PrintHelpMessage();
                return new List<DateTime>();
            }

            var dates = new List<DateTime>();
            for (int i = 0; startDate.AddDays(i) <= endDate; i++)
            {
                dates.Add(startDate.AddDays(i));
            }

            dates.Reverse();
            return dates;
        }

        private static void PrintHelpMessage()
        {
            Console.WriteLine("\nRaceTrackerConsole - Application for mining horse racing data. Created by C. Simpson-Allsop\n");
            Console.WriteLine("Commands:\n");
            Console.WriteLine("\t-help :=: Prints this page.");
            Console.WriteLine("\t-minedata [startdate] [enddate] :=: Mines all horse racing data within the specified date range, starting from the most recent date (May take a long time depending on the size of the date range).");
            Console.WriteLine("\t-processdata -daterange [startdate] [enddate] :=: Processes raw data obtained via mining into an analysable format, within the specified date range, starting from the most recent date.");
            Console.WriteLine("\t-processdata -all :=: Processes all raw data obtained via mining into an analysable format.");
            Console.WriteLine();
            Console.WriteLine("Fields are represented by square brackets. Dates must be written in the 'yyyy-MM-dd' format. The command '-today' can be used in place of a date to use today's date.");
            Console.WriteLine("Raw data is stored in the 'RawData' directory, in the executable path directory.");
            Console.WriteLine("Processed data is stored in the 'ProcessedData' directory, in the executable path directory.");
            Console.WriteLine("Typical times for mining one days worth of data are ~1-5 minutes, depending on the number of races on that day.");
            Console.WriteLine("In the event of an unplanned or forced closing of the application, be sure to terminate any instances of chromedriver.exe in the task manager");
        }
    }
}

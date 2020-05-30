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
            ExceptionLogger.Exceptions = new List<Tuple<string,Exception>>();
            ProcessArgs(args);
            if (ExceptionLogger.Exceptions.Count > 0)
            {
                Console.WriteLine("\nExceptions encountered:\n");
                foreach (var exception in ExceptionLogger.Exceptions)
                {
                    Console.WriteLine(exception.Item1 + exception.Item2.Message);
                }
            }
        }

        private static void ProcessArgs(string[] args)
        {
            if (args.Length >= 1)
            {
                switch (args[0].ToLower())
                {
                    case "-help": PrintHelpMessage(); return;
                    case "-minedata": MineData(args); return;
                    case "-minedatafor": MineDataFor(args); return;
                    case "-processdata": ProcessData(args, false); return;
                    case "-processverbose": ProcessData(args, true); return;
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

        private static void MineDataFor(string[] args)
        {
            if (args.Length >= 2)
            {
                if (double.TryParse(args[1], out double hours))
                {
                    if(hours<0)
                    {
                        Console.WriteLine("Number of hours cannot be negative");
                        PrintHelpMessage();
                        return;
                    }

                    var dataMining = new DataMining();
                    dataMining.DailyDataFor(hours);
                }
                else
                {
                    Console.WriteLine("Failed to parse '" + args[1] + "' as a valid number of hours");
                    PrintHelpMessage();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Must specify number of hours");
                PrintHelpMessage();
                return;
            }
        }

        private static void ProcessData(string[] args, bool consoleOutput)
        {
            if (args.Length >= 2)
            {
                Output.PrintToConsole = consoleOutput;
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
                                    log.Error("Error occurred whilst processing data for file '" + file + "': ",
                                    ExceptionLogger.LogException(e, file).Item2);
                                }
                            }
                        }

                        return;

                    case "-file":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Not enough arguments");
                            PrintHelpMessage();
                            return;
                        }

                        if (!File.Exists(args[2]))
                        {
                            Console.WriteLine("File does not exist");
                            PrintHelpMessage();
                            return;
                        }

                        dataProcessing.FormatFileData(args[2]);
                        return;
                    case "-compile":
                        if (args.Length < 3)
                        {
                            Console.WriteLine("Not enough arguments");
                            PrintHelpMessage();
                            return;
                        }

                        if (Directory.Exists(AppSettings.RaceProcessedDataDirectory))
                        {
                            string filename = CommonFunctions.RemoveInvalidFilenameChars(args[2], "_");
                            string filepath = AppSettings.CompiledDataDirectory + filename;
                            if (File.Exists(filepath))
                            {
                                Console.WriteLine("File '" + filepath + "' already exists. Please choose a different name or move/delete the existing file");
                            }

                            dataProcessing.CompileData(filename);
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
                List<DateTime> dates = GetDateRange(args, 1, 2); 

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
            bool reverse = false;
            try
            {
                if (args[startDateIndex].ToLower() == "-today")
                {
                    startDate = DateTime.Today;
                }
                else if (args[startDateIndex].ToLower() == "-recursive")
                {
                    startDate = CommonFunctions.GetDateFromFile(AppSettings.MostRecentDateMinedFile, DateTime.Today);
                    reverse = false;
                }
                else
                {
                    startDate = DateTime.ParseExact(args[startDateIndex], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                }

                if (args[endDateIndex].ToLower() == "-today")
                {
                    endDate = DateTime.Today;
                }
                else if (args[endDateIndex].ToLower() == "-recursive")
                {
                    endDate = CommonFunctions.GetDateFromFile(AppSettings.OldestDateMinedFile, DateTime.Today);
                    reverse = true;
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

            if (reverse)
            {
                dates.Reverse();
            }

            return dates;
        }

        private static void PrintHelpMessage()
        {
            Console.WriteLine("\nRaceTrackerConsole - Application for mining horse racing data. Created by C. Simpson-Allsop\n");
            Console.WriteLine("Commands:\n");
            Console.WriteLine("\t-help :=: Prints this page.\n");
            Console.WriteLine("\t-minedata [startdate] [enddate] :=: Mines all horse racing data within the specified date range, starting from the most recent date (May take a long time depending on the size of the date range).\n");
            Console.WriteLine("\t-minedata [startdate] -recursive :=: Mines all horse racing data, starting from the oldest date mined and working backwards until the start date is reached (May take a long time depending on the size of the date range).\n");
            Console.WriteLine("\t-minedata -recursive [enddate] :=: Mines all horse racing data, starting from the most recent date mined and working forwards until the end date is reached (May take a long time depending on the size of the date range).\n");
            Console.WriteLine("\t-minedatafor [hours] :=: Mines all horse racing data, starting from the oldest date mined and working backwards for the given amount of time. Decimal times are accepted. Given time is approximate.\n");
            Console.WriteLine("\t-processdata -daterange [startdate] [enddate] :=: Processes raw data obtained via mining into an analysable format, within the specified date range, starting from the most recent date.\n");
            Console.WriteLine("\t-processdata -all :=: Processes all raw data obtained via mining into an analysable format.\n");
            Console.WriteLine("\t-processdata -file [filepath] :=: Processes the raw data at the given filepath into an analysable format.\n");
            Console.WriteLine("\t-processdata -compile [filename] :=: Compiles all processed data into a single file with the given filename.\n");
            Console.WriteLine();
            Console.WriteLine("Fields are represented by square brackets. Dates must be written in the 'yyyy-MM-dd' format. The command '-today' can be used in place of a date to use today's date.");
            Console.WriteLine("Raw data is stored in the 'RawData' directory, in the executable path directory.");
            Console.WriteLine("Processed data is stored in the 'ProcessedData' directory, in the executable path directory. If the -processverbose tag is used instead of -processdata, the functions are the same but there is a verbose console output");
            Console.WriteLine("Typical times for mining one days worth of data are ~1-5 minutes, depending on the number of races on that day.");
            Console.WriteLine("In the event of an unplanned or forced closing of the application, be sure to terminate any instances of chromedriver.exe in the task manager.");
        }
    }
}

namespace RaceTrackerConsole
{
    using log4net;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Reflection;

    public class Program
    {
        public static void Main(string[] args)
        {
            //var date = new DateTime(2019, 5, 2);
            //new DataMining().DailyData(date);
            //new DataProcessing().FormatDailyData(date);
            ProcessArgs(args);
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
                }
            }
        }

        private static void ProcessData(string[] args)
        {
        }

        private static void MineData(string[] args)
        {
            if (args.Length >= 3)
            {
                DateTime startDate, endDate;
                try
                {
                    if (args[1].ToLower() == "-today")
                    {
                        startDate = DateTime.Today;
                    }
                    else
                    {
                        startDate = DateTime.ParseExact(args[1], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }

                    if (args[2].ToLower() == "-today")
                    {
                        endDate = DateTime.Today;
                    }
                    else
                    {
                        endDate = DateTime.ParseExact(args[2], "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }                   
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to process input dates: " + args[1] + " " + args[2] + e.Message);
                    PrintHelpMessage();
                    return;
                }

                if (startDate > endDate)
                {
                    Console.WriteLine("End date must be later than start date");
                    PrintHelpMessage();
                    return;
                }

                var dataMining = new DataMining();
                var dates = new List<DateTime>();
                for (int i = 0; startDate.AddDays(i) <= endDate; i++)
                {
                    dates.Add(startDate.AddDays(i));
                }

                dataMining.DailyData(dates);
                return;
            }

            Console.WriteLine("Not enough arguments");
            PrintHelpMessage();
            return;
        }

        private static void PrintHelpMessage()
        {

        }
    }
}

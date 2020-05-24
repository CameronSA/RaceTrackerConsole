namespace RaceTrackerConsole
{
    using RaceTrackerConsole.LogicHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;

    public class DataMining
    {
        public static void DailyData(DateTime date)
        {
            var driver = new WebDriver();
            try
            {
                var stopwatch = new Stopwatch();
                stopwatch.Start();
                var overallData = new List<string>();
                Console.WriteLine("Data mine initiated for date: " + date + ". . .");
                try
                {
                    var urls = driver.GetResultsUrls(date);
                    //for (int i = 0; i < urls.Count; i++)
                    for (int i = 0; i < 1; i++)
                    {
                        var urlData = driver.GetRawRaceData(urls[i]);
                        for (int n = 0; n < urlData.Count; n++)
                        {
                            if (n > 0 || (i == 0))
                            {
                                overallData.Add(urls[i] + AppSettings.Delimiter + urlData[n]);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred whilst mining data: ", e);
                }

                if (!Directory.Exists(AppSettings.RaceDataDirectory))
                {
                    Directory.CreateDirectory(AppSettings.RaceDataDirectory);
                }

                using (var file = new StreamWriter(AppSettings.RaceDataDirectory + "RaceData_" + date.Year + "-" + date.Month + "-" + date.Day + ".txt"))
                {
                    foreach (var row in overallData)
                    {
                        file.WriteLine(row);
                    }
                }

                stopwatch.Stop();
                Console.WriteLine("Data mine complete. Time elapsed: " + stopwatch.Elapsed);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                driver.Driver.Quit();
                driver.Driver.Dispose();
            }
        }
    }
}

namespace RaceTrackerConsole
{
    using RaceTrackerConsole.LogicHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Security.Policy;

    public class DataMining
    {
        private readonly Log log;

        public DataMining()
        {
            this.log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void DailyData(List<DateTime> dates)
        {
            var driver = new WebDriver();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var date in dates)
            {
                try
                {
                    var dateStopwatch = new Stopwatch();
                    dateStopwatch.Start();
                    var overallData = new List<string>();
                    this.log.Info("Data mine initiated for date: " + date + ". . .");
                    int urlCount = 0;
                    try
                    {
                        var urls = driver.GetResultsUrls(date);
                        urlCount = urls.Count;
                        if (urls.Count > 0)
                        {
                            bool columnHeadersAdded = false;
                            for (int i = 0; i < urls.Count; i++)
                            //for (int i = 0; i < 1; i++)
                            {
                                var urlData = driver.GetRawRaceData(urls[i]);
                                for (int n = 0; n < urlData.Count; n++)
                                {
                                    if (urlData[n].StartsWith("ReportHeader:"))
                                    {
                                        overallData.Add(urlData[n]);
                                    }
                                    else
                                    {
                                        if (!columnHeadersAdded || n > 0)
                                        {
                                            overallData.Add(urls[i] + AppSettings.Delimiter + urlData[n]);
                                            columnHeadersAdded = true;
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            this.log.Warn("No Urls found, terminating process for date '" + date + "'");
                            return;
                        }
                    }
                    catch (Exception e)
                    {
                        this.log.Error("An error occurred whilst mining data: ", e);
                    }

                    if (!Directory.Exists(AppSettings.RaceRawDataDirectory))
                    {
                        Directory.CreateDirectory(AppSettings.RaceRawDataDirectory);
                    }

                    using (var file = new StreamWriter(AppSettings.RaceRawDataDirectory + "RaceRawData_" + date.Year + "-" + date.Month + "-" + date.Day + ".txt"))
                    {
                        foreach (var row in overallData)
                        {
                            file.WriteLine(row);
                        }
                    }

                    dateStopwatch.Stop();
                    this.log.Info("Data mine for date: " + date + " complete. Time spent on this date: " + dateStopwatch.Elapsed + ". Total time elapsed so far: " + stopwatch.Elapsed);
                }
                catch (Exception e)
                {
                    this.log.Error(e.Message);
                }
            }

            stopwatch.Stop();
            this.log.Info("Data mine complete. Total time elapsed: " + stopwatch.Elapsed);
            driver.Driver.Quit();
            driver.Driver.Dispose();
        }
    }
}

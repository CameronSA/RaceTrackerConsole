namespace RaceTrackerConsole
{
    using RaceTrackerConsole.LogicHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
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
                            continue;
                        }
                    }
                    catch (Exception e)
                    {
                        this.log.Error("An error occurred whilst mining data: ", ExceptionLogger.LogException(e));
                    }

                    if (!Directory.Exists(AppSettings.RaceRawDataDirectory))
                    {
                        Directory.CreateDirectory(AppSettings.RaceRawDataDirectory);
                    }

                    using (var file = new StreamWriter(AppSettings.RaceRawDataDirectory + AppSettings.RawDataFilePrefix + date.Year + "-" + date.Month + "-" + date.Day + ".txt"))
                    {
                        foreach (var row in overallData)
                        {
                            file.WriteLine(row);
                        }
                    }

                    this.UpdateOldestDateMinedFile(date);

                    dateStopwatch.Stop();
                    this.log.Info("Data mine for date: " + date + " complete. Time spent on this date: " + dateStopwatch.Elapsed + ". Total time elapsed so far: " + stopwatch.Elapsed);
                }
                catch (Exception e)
                {
                    this.log.Error(ExceptionLogger.LogException(e).Message);
                }
            }

            stopwatch.Stop();
            this.log.Info("Data mine complete. Total time elapsed: " + stopwatch.Elapsed);
            driver.Driver.Quit();
            driver.Driver.Dispose();
        }

        private void UpdateOldestDateMinedFile(DateTime date)
        {
            DateTime currentDate;
            bool update = false;
            bool fileExists = File.Exists(AppSettings.OldestDateMinedFile);
            if (fileExists)
            {
                try
                {
                    using (var file = new StreamReader(AppSettings.OldestDateMinedFile))
                    {
                        string text = file.ReadToEnd().Trim();
                        currentDate = DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if(currentDate>date)
                        {
                            update = true;
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to Read date from file '" + AppSettings.OldestDateMinedFile + "'. File not updated: " + e.Message);
                    return;
                }
            }

            if(update || !fileExists)
            {
                using(var file = new StreamWriter(AppSettings.OldestDateMinedFile))
                {
                    file.WriteLine(this.FormatStringLength(date.Year.ToString(), 4) + "-" + this.FormatStringLength(date.Month.ToString(), 2) + "-" + this.FormatStringLength(date.Day.ToString(), 2));
                }
            }
        }

        private string FormatStringLength(string str, int length)
        {
            if(str.Length<length)
            {
                str = "0" + str;
                return FormatStringLength(str, length);
            }
            else if(str.Length==length)
            {
                return str;
            }
            else
            {
                throw new Exception("Error in formatting string length");
            }
        }
    }
}

namespace RaceTrackerConsole
{
    using RaceTrackerConsole.LogicHelpers;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public class DataMining
    {
        private readonly Log log;

        public DataMining()
        {
            this.log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void DailyDataFor(double hours)
        {
            var startDate = CommonFunctions.GetDateFromFile(AppSettings.OldestDateMinedFile, DateTime.Today);
            var stopwatch = new Stopwatch();
            var driver = new WebDriver();
            int counter = 0;
            stopwatch.Start();
            do
            {
                var date = startDate.AddDays(counter);
                this.GetData(driver, stopwatch, date, hours);
                counter--;
            } while (stopwatch.Elapsed < TimeSpan.FromHours(hours));

            stopwatch.Stop();
            this.log.Info("Data mine complete. Total time elapsed: " + stopwatch.Elapsed);
            driver.Driver.Quit();
            driver.Driver.Dispose();
        }

        public void DailyData(List<DateTime> dates)
        {
            var driver = new WebDriver();
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var date in dates)
            {
                this.GetData(driver, stopwatch, date, -1);
            }

            stopwatch.Stop();
            this.log.Info("Data mine complete. Total time elapsed: " + stopwatch.Elapsed);
            driver.Driver.Quit();
            driver.Driver.Dispose();
        }

        private void GetData(WebDriver driver, Stopwatch stopwatch, DateTime date, double hours)
        {
            if (!Directory.Exists(AppSettings.RaceRawDataDirectory))
            {
                Directory.CreateDirectory(AppSettings.RaceRawDataDirectory);
            }

            var dateStopwatch = new Stopwatch();
            dateStopwatch.Start();
            this.log.Info("Data mine initiated for date: " + date + ". . .");
            try
            {
                var urls = driver.GetResultsUrls(date);
                if (urls.Count > 0)
                {
                    for (int i = 0; i < urls.Count; i++)
                    //for (int i = 0; i < 1; i++)
                    {
                        var urlData = driver.GetRawRaceData(urls[i]);
                        using (var file = new StreamWriter(AppSettings.RaceRawDataDirectory + AppSettings.RawDataFilePrefix + date.Year + "-" + date.Month + "-" + date.Day + "_" + i + ".txt"))
                        {
                            for (int n = 0; n < urlData.Count; n++)
                            {
                                file.WriteLine(urlData[n]);
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
                this.log.Error("An error occurred whilst mining data: ", ExceptionLogger.LogException(e, date.Year + "-" + date.Month + "-" + date.Day).Item2);
                return;
            }

            if (this.UpdateDateMinedFiles(date, AppSettings.OldestDateMinedFile, false))
            {
                this.log.Info("Updated oldest date mined to '" + date.Year + "-" + date.Month + "-" + date.Day + "'");
            }

            if (this.UpdateDateMinedFiles(date, AppSettings.MostRecentDateMinedFile, true))
            {
                this.log.Info("Updated most recent date mined to '" + date.Year + "-" + date.Month + "-" + date.Day + "'");
            }

            dateStopwatch.Stop();
            string timeRemaining = string.Empty;
            if (hours > 0)
            {
                var remaining = TimeSpan.FromHours(hours) - stopwatch.Elapsed;
                timeRemaining = " Approximate time remaining: " + remaining;
            }

            this.log.Info("Data mine for date: " + date + " complete. Time spent on this date: " + dateStopwatch.Elapsed + ". Total time elapsed so far: " + stopwatch.Elapsed + timeRemaining);            
        }

        private bool UpdateDateMinedFiles(DateTime date,string filePath, bool updateIfMoreRecent)
        {
            DateTime currentDate;
            bool update = false;
            bool fileExists = File.Exists(filePath);
            if (fileExists)
            {
                try
                {
                    using (var file = new StreamReader(filePath))
                    {
                        string text = file.ReadToEnd().Trim();
                        currentDate = DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        if (updateIfMoreRecent)
                        {
                            if (date > currentDate)
                            {
                                update = true;
                            }
                        }
                        else
                        {
                            if (currentDate > date)
                            {
                                update = true;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to Read date from file '" + filePath + "'. File not updated: " + e.Message);
                    return false;
                }
            }

            if(update || !fileExists)
            {
                using(var file = new StreamWriter(filePath))
                {
                    file.WriteLine(this.FormatStringLength(date.Year.ToString(), 4) + "-" + this.FormatStringLength(date.Month.ToString(), 2) + "-" + this.FormatStringLength(date.Day.ToString(), 2));
                }

                return true;
            }

            return false;
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

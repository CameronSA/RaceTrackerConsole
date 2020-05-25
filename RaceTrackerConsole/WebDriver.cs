using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.IO.Pipes;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using RaceTrackerConsole.LogicHelpers;

namespace RaceTrackerConsole
{
    public class WebDriver
    {
        private readonly ChromeOptions chromeOptions;

        private readonly ChromeDriverService chromeDriverService;

        private readonly WebDriverWait wait;

        private readonly Log log;

        public WebDriver()
        {
            this.log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
            this.chromeOptions = new ChromeOptions();
            this.chromeOptions.AddArgument("--incognito");
            this.chromeDriverService = ChromeDriverService.CreateDefaultService();
            this.chromeDriverService.HideCommandPromptWindow = true;
            this.Driver = new ChromeDriver(chromeDriverService, chromeOptions);
            this.wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(10))
            {
                PollingInterval = TimeSpan.FromSeconds(0.5)
            };
        }

        public IWebDriver Driver { get; private set; }

        public List<string> GetResultsUrls(DateTime date)
        {
            var resultsUrls = new List<string>();
            this.Driver.Navigate().GoToUrl(AppSettings.RaceDataWebsite + date.Year + "-" + date.Month + "-" + date.Day);

            var pageBody = this.wait.Until(x => x.FindElement(By.TagName("body")));
            var results = pageBody.FindElements(By.ClassName("results-title"));
            try
            {
                foreach (var result in results)
                {
                    if (!string.IsNullOrEmpty(result.GetAttribute("href")))
                    {
                        resultsUrls.Add(result.GetAttribute("href"));
                        this.log.Info(result.GetAttribute("href"));
                    }
                }
            }
            catch (Exception e)
            {
                this.log.Error("An error occured whilst extracting urls for date '" + date.Year + "-" + date.Month + "-" + date.Day + "'. Data may be incomplete", e);
            }

            return resultsUrls;
        }

        public List<string> GetRawRaceData(string url)
        {
            this.Driver.Navigate().GoToUrl(url);
            var tableRows = new List<string>();
            var raceHeaders = this.GetRaceHeaders(url);
            string tableHeaders = string.Empty;
            try
            {
                var reportBody = this.wait.Until(x => x.FindElement(By.Id("ReportBody")));
                var reportTable = reportBody.FindElement(By.TagName("table"));
                bool firstRowPassed = false;
                if (reportTable.GetAttribute("class") == "rp-table rp-results")
                {
                    var tbodyElements = reportTable.FindElements(By.TagName("tbody"));
                    for (int i = 0; i < tbodyElements.Count; i++)
                    {
                        if (tbodyElements[i].GetAttribute("class") == "rp-table-row")
                        {
                            string tableRow = string.Empty;
                            var row = tbodyElements[i].FindElement(By.TagName("tr"));
                            var rowColumns = row.FindElements(By.TagName("td"));
                            foreach (var column in rowColumns)
                            {
                                string title = column.GetAttribute("title").Trim();
                                if (string.IsNullOrEmpty(title))
                                {
                                    title = column.GetAttribute("class");
                                }

                                if (!firstRowPassed)
                                {
                                    tableHeaders += title + AppSettings.Delimiter;
                                }

                                tableRow += column.Text + AppSettings.Delimiter;
                            }

                            firstRowPassed = true;
                            tableRows.Add(tableRow);
                        }
                    }
                }
                else
                {
                    this.log.Error("Failed to find data for url '" + url + "'. Data may be incomplete");
                }
            }
            catch (Exception e)
            {
                this.log.Error("An error was encountered whilst processing data for url '" + url + "'. Data may be incomplete", e);
            }

            tableRows.Insert(0, tableHeaders);
            for (int i = raceHeaders.Count - 1; i >= 0; i--)
            {
                tableRows.Insert(0, "ReportHeader: " + raceHeaders[i].Replace("\n", " ").Replace("\r", " "));
            }

            foreach (var row in tableRows)
            {
                Console.WriteLine(url + AppSettings.Delimiter + row);
            }

            return tableRows;
        }

        private List<string> GetRaceHeaders(string url)
        {
            var headers = new List<string>();
            try
            {
                var reportHeaders = this.wait.Until(x => x.FindElement(By.Id("rp-header")));
                var headerTable = reportHeaders.FindElement(By.TagName("table"));
                if (headerTable.GetAttribute("class") == "rp-header-table")
                {
                    var tbodyElement = headerTable.FindElement(By.TagName("tbody"));
                    foreach (var row in tbodyElement.FindElements(By.TagName("tr")))
                    {
                        headers.Add(row.Text);
                    }
                }
                else
                {
                    this.log.Error("Failed to race headers for url '" + url + "'. Data may be incomplete");
                }
            }
            catch (Exception e)
            {
                this.log.Error("An error was encountered whilst processing headers for url '" + url + "'. Data may be incomplete", e);
            }

            return headers;
        }
    }
}

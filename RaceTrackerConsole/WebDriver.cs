using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Linq;
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

        public WebDriver()
        {

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

        public List<string> GetResultsUrls(DateTime date)
        {
            var resultsUrls = new List<string>();
            this.Driver.Navigate().GoToUrl(AppSettings.RaceDataWebsite + date.Year + "-" + date.Month + "-" + date.Day);

            var pageBody = this.wait.Until(x => x.FindElement(By.TagName("body")));
            var results = pageBody.FindElements(By.ClassName("results-title"));
            foreach (var result in results)
            {
                resultsUrls.Add(result.GetAttribute("href"));
                Console.WriteLine(result.GetAttribute("href"));
            }

            return resultsUrls;
        }

        public List<string> GetRawRaceData(string url)
        {
            this.Driver.Navigate().GoToUrl(url);
            var reportBody = this.wait.Until(x => x.FindElement(By.Id("ReportBody")));
            var reportTable = reportBody.FindElement(By.TagName("table"));
            var tableRows = new List<string>();
            string tableHeaders = string.Empty;
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

            tableRows.Insert(0, tableHeaders);

            foreach(var row in tableRows)
            {
                Console.WriteLine(url + AppSettings.Delimiter + row);
            }

            return tableRows;
        }

        public IWebDriver Driver { get; private set; }
    }
}

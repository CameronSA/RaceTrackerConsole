using RaceTrackerConsole.LogicHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole
{
    public class DataProcessing
    {
        private readonly Log log;

        public DataProcessing()
        {
            this.log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void FormatDailyData(List<DateTime> dates)
        {
            foreach (var date in dates)
            {
                string dateString = date.Year + "-" + date.Month + "-" + date.Day;
                foreach (var file in Directory.GetFiles(AppSettings.RaceRawDataDirectory))
                {
                    if (file.Contains(dateString))
                    {
                        this.FormatFileData(file);
                    }
                }
            }
        }

        public void FormatFileData(string file)
        {
            this.log.Info("Processing file '" + file + "'. . .");
            var processedLines = new List<string>();
            var reportHeaders = new List<string>();
            using (var fileReader = new StreamReader(file))
            {
                int counter = 0;
                var headers = new List<Tuple<string, string>>();
                do
                {
                    string line = fileReader.ReadLine();
                    if (line.StartsWith("ReportHeader:"))
                    {
                        reportHeaders.Add(line);
                    }
                    else
                    {
                        string processedLine;
                        if (counter == 0)
                        {
                            headers = this.FormatReportHeaders(reportHeaders);
                            processedLine = FormatLine(line, true);
                            for (int i = headers.Count - 1; i >= 0; i--)
                            {
                                processedLine = headers[i].Item1 + AppSettings.Delimiter + processedLine;
                            }
                        }
                        else
                        {
                            if (headers.Count > 0)
                            {
                                processedLine = FormatLine(line, false);
                                for (int i = headers.Count - 1; i >= 0; i--)
                                {
                                    processedLine = headers[i].Item2 + AppSettings.Delimiter + processedLine;
                                }
                            }
                            else
                            {
                                throw new Exception("Failed to process report header information for file '" + file + "'. Header information count: " + headers.Count);
                            }
                        }

                        processedLines.Add(processedLine);
                        Console.WriteLine(processedLine);
                        counter++;
                    }
                } while (!fileReader.EndOfStream);
            }

            if (!Directory.Exists(AppSettings.RaceProcessedDataDirectory))
            {
                Directory.CreateDirectory(AppSettings.RaceProcessedDataDirectory);
            }

            string newFilePath = file.Replace(AppSettings.RaceRawDataDirectory, AppSettings.RaceProcessedDataDirectory).Replace(AppSettings.RawDataFilePrefix, AppSettings.ProcessedDataFilePrefix);
            using (var fileWriter = new StreamWriter(newFilePath))
            {
                foreach (var line in processedLines)
                {
                    fileWriter.WriteLine(line);
                }
            }

            this.log.Info("Finished processing file '" + file + "'");
        }

        private List<Tuple<string, string>> FormatReportHeaders(List<string> headers)
        {
            var formattedHeaders = new List<Tuple<string, string>>();

            var headerStringBuilder = new StringBuilder(string.Empty);
            foreach(var header in headers)
            {
                headerStringBuilder.Append(header);
            }

            string headerString = headerStringBuilder.ToString().Replace("ReportHeader:", string.Empty);
            var titleList = new string[] { "Distance", "Prize", "Age", "Race Type", "Going" };
            foreach (var title in titleList)
            {
                int numberOccurances = this.FindNumberOfOccurances(headerString, title);
                if (numberOccurances == 0)
                {
                    throw new Exception("Could not find data field '" + title + "' in header string '" + headerString + "'");
                }
                else if(numberOccurances>1)
                {
                    throw new Exception("Found multiple occurances of data field '" + title + "' in header string '" + headerString + "'");
                }
            }

            for (int i = 0; i < titleList.Length; i++)
            {
                formattedHeaders.Add(this.ExtractInformation(headerString, titleList[i], titleList[i == titleList.Length - 1 ? i : i + 1]));
            }

            return formattedHeaders;
        }

        private Tuple<string, string> ExtractInformation(string header, string field, string nextField)
        {
            string substr;
            int fieldIndex = header.IndexOf(field);
            int nextFieldIndex = header.IndexOf(nextField);
            if (field == nextField)
            {
                substr = header.Substring(fieldIndex);
            }
            else
            {
                substr = header.Substring(fieldIndex, nextFieldIndex - fieldIndex);
            }

            var elements = substr.Split(':');
            if (elements.Length == 2)
            {
                return new Tuple<string, string>(elements[0].Trim(), elements[1].Trim());
            }
            else
            {
                throw new Exception("Failed to find distance information in header '" + header + "'. Information found: " + substr);
            }
        }

        private int FindNumberOfOccurances(string str, string searchItem)
        {
            int number = 0;
            for (int i = str.IndexOf(searchItem); i > -1; i = str.IndexOf(searchItem, i + 1))
            {
                number++;
            }

            return number;
        }

        private string FormatLine(string line, bool isHeader)
        {
            var cells = line.Split(AppSettings.Delimiter);
            StringBuilder formattedLine;
            if (isHeader)
            {
                formattedLine = ProcessHeaders(cells);
            }
            else
            {
                formattedLine = ProcessData(cells);
            }

            return formattedLine.ToString();
        }

        private StringBuilder ProcessHeaders(string[] cells)
        {
            var formattedLine = new StringBuilder(string.Empty);
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                switch (i)
                {
                    case 0:
                        formattedLine.Append("Racetrack" + AppSettings.Delimiter + "Date" + AppSettings.Delimiter + "Time" + AppSettings.Delimiter);
                        break;
                    case 1:
                        formattedLine.Append("Position" + AppSettings.Delimiter + "Draw" + AppSettings.Delimiter);
                        break;
                    case 5:
                        formattedLine.Append("Horse Name" + AppSettings.Delimiter);
                        break;
                    case 12:
                        formattedLine.Append("Horse Age" + AppSettings.Delimiter);
                        break;
                    case 13:
                        formattedLine.Append("Horse Weight" + AppSettings.Delimiter);
                        break;
                    case 15:
                        formattedLine.Append("ISP" + AppSettings.Delimiter + "Expectation");
                        break;
                }
            }

            return formattedLine;
        }

        private StringBuilder ProcessData(string[] cells)
        {
            var formattedLine = new StringBuilder(string.Empty);
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                try
                {
                    switch (i)
                    {
                        case 0:
                            cell = cell.Replace(AppSettings.RaceDataResult, string.Empty);
                            var cellElements = cell.Split('/');
                            if (cellElements.Length > 3)
                            {
                                for (int n = 0; n < 3; n++)
                                {
                                    formattedLine.Append(cellElements[n] + AppSettings.Delimiter);
                                }
                            }
                            else
                            {
                                this.log.Error("Failed to process cell '" + cell + "'. Cell has " + cellElements.Length + " elements, but a minimum of 3 is required");

                                for (int n = 0; n < 3; n++)
                                {
                                    formattedLine.Append("ERROR" + AppSettings.Delimiter);
                                }
                            }

                            break;
                        case 1:
                            if (cell.Trim().Contains(" "))
                            {
                                var elements = cell.Split(' ');
                                formattedLine.Append(elements[0] + AppSettings.Delimiter);
                                formattedLine.Append(elements[1].Replace("(", string.Empty).Replace(")", string.Empty) + AppSettings.Delimiter);
                            }
                            else
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter + "NULL" + AppSettings.Delimiter);
                            }

                            break;
                        case 5:
                            formattedLine.Append(cell + AppSettings.Delimiter);
                            break;
                        case 12:
                            if (short.TryParse(cell.ToString(), out _))
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter);
                            }
                            else
                            {
                                this.log.Error("Failed to process cell '" + cell + "'. Cell does not contain a number");
                                formattedLine.Append("ERROR" + AppSettings.Delimiter);
                            }
                            break;
                        case 13:
                            formattedLine.Append(cell + AppSettings.Delimiter);
                            break;
                        case 15:
                            try
                            {
                                string[] fractionElements = cell.Split('/');
                                string expectation = string.Empty;
                                foreach (var character in fractionElements[1])
                                {
                                    if (!int.TryParse(character.ToString(), out _))
                                    {
                                        expectation += character;
                                    }
                                }

                                if (expectation.Length > 0)
                                {
                                    fractionElements[1] = fractionElements[1].Replace(expectation, string.Empty);
                                }

                                if (int.TryParse(fractionElements[0], out int numerator) && int.TryParse(fractionElements[1], out int denominator))
                                {
                                    double fractionDecimal = (double)numerator / (double)denominator;
                                    formattedLine.Append(fractionDecimal.ToString() + AppSettings.Delimiter + expectation);
                                }
                                else
                                {
                                    this.log.Error("Could not parse numerical values for cell '" + cell + "'");
                                    formattedLine.Append("ERROR" + AppSettings.Delimiter + "ERROR");
                                }
                            }
                            catch (Exception e)
                            {
                                this.log.Error("Failed to process cell '" + cell + "'. " + ExceptionLogger.LogException(e).Message);
                                formattedLine.Append("ERROR" + AppSettings.Delimiter + "ERROR");
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    this.log.Error("Error encountered whilst processing cell '" + cell + "'", ExceptionLogger.LogException(e));
                    formattedLine.Append("ERROR");
                }
            }

            return formattedLine;
        }
    }
}

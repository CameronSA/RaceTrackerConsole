﻿using log4net;
using RaceTrackerConsole.LogicHelpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

        private string file;

        public DataProcessing()
        {
            this.log = new Log(MethodBase.GetCurrentMethod().DeclaringType);
        }

        public void CompileData(string filename)
        {
            var stopwatch = new Stopwatch();
            try
            {
                stopwatch.Start();
                if (!Directory.Exists(AppSettings.CompiledDataDirectory))
                {
                    Directory.CreateDirectory(AppSettings.CompiledDataDirectory);
                }

                string outputFilepath = AppSettings.CompiledDataDirectory + filename;
                Console.WriteLine("\nMoving processed data to file '" + outputFilepath + "'. . .");
                bool headerAdded = false;
                foreach (var file in Directory.GetFiles(AppSettings.RaceProcessedDataDirectory))
                {
                    var fileLines = new List<string>();
                    using (var streamReader = new StreamReader(file))
                    {
                        if (headerAdded)
                        {
                            streamReader.ReadLine();
                        }

                        do
                        {
                            fileLines.Add(streamReader.ReadLine());
                        } while (!streamReader.EndOfStream);
                        headerAdded = true;
                    }

                    File.Delete(file);
                    using (var streamAppender = File.AppendText(outputFilepath))
                    {
                        foreach (var line in fileLines)
                        {
                            streamAppender.WriteLine(line);
                        }
                    }

                    Output.WriteLine("Appended '" + file + "'");
                }
            }
            catch (Exception e)
            {
                this.log.Error("Error encountered whilst compiling processed data: ", e);
            }
            finally
            {
                stopwatch.Stop();
                Console.WriteLine("Data compilation complete. Time elapsed: " + stopwatch.Elapsed);
            }
        }

        public void FormatDailyData(List<DateTime> dates)
        {
            var stopwatch = new Stopwatch();
            stopwatch.Start();
            foreach (var date in dates)
            {
                string dateString = date.Year + "-" + date.Month + "-" + date.Day;
                foreach (var file in Directory.GetFiles(AppSettings.RaceRawDataDirectory))
                {
                    if (file.Contains("_" + dateString + "_"))
                    {
                        this.file = file;
                        this.FormatFileData(file);
                    }
                }
            }

            stopwatch.Stop();
            Output.WriteLine("Data processing complete. Time elapsed: " + stopwatch.Elapsed);
        }

        public void FormatFileData(string file)
        {
            this.log.Info("Processing file '" + file + "'. . .");
            var processedLines = new List<string>();
            var reportHeaders = new List<string>();
            using (var fileReader = new StreamReader(file))
            {
                int counter = 0;
                var headers = new Dictionary<string, string>();
                var headerKeys = new List<string>();
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
                            //for (int i = headers.Count - 1; i >= 0; i--)
                            //{
                            //    processedLine = headers[i].Item1 + AppSettings.Delimiter + processedLine;
                            //}
                            foreach (var item in headers)
                            {
                                processedLine = item.Key + AppSettings.Delimiter + processedLine;
                                headerKeys.Add(item.Key);
                            }
                        }
                        else
                        {
                            if (headers.Count > 0)
                            {
                                processedLine = FormatLine(line, false);
                                //for (int i = headers.Count - 1; i >= 0; i--)
                                //{
                                //    processedLine = headers[i].Item2 + AppSettings.Delimiter + processedLine;
                                //}
                                foreach (var key in headerKeys)
                                {
                                    processedLine = headers[key] + AppSettings.Delimiter + processedLine;
                                }
                            }
                            else
                            {
                                var e = new Exception("Failed to process report header information for file '" + file + "'. Header information count: " + headers.Count);
                                ExceptionLogger.LogException(e, this.file);
                                throw e;
                            }
                        }

                        processedLines.Add(processedLine);
                        Output.WriteLine(processedLine);
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

        private Dictionary<string, string> FormatReportHeaders(List<string> headers)
        {
            var formattedHeaders = new Dictionary<string, string>();

            var headerStringBuilder = new StringBuilder(string.Empty);
            foreach (var header in headers)
            {
                headerStringBuilder.Append(header);
            }

            string headerString = headerStringBuilder.ToString().Replace("ReportHeader:", string.Empty);
            foreach (var title in AppSettings.ReportHeaderFieldList)
            {
                int numberOccurances = this.FindNumberOfOccurances(headerString, title);
                if (numberOccurances > 1)
                {
                    var e = new Exception("Found multiple occurances of data field '" + title + "' in header string '" + headerString + "'");
                    ExceptionLogger.LogException(e, this.file);
                    throw e;
                }
            }

            var fieldIndices = new Dictionary<string, int>();
            foreach (var field in AppSettings.ReportHeaderFieldList)
            {
                fieldIndices.Add(field, headerString.IndexOf(field));
            }

            for (int i = 0; i < AppSettings.ReportHeaderFieldList.Length; i++)
            {
                int index = i == AppSettings.ReportHeaderFieldList.Length - 1 ? i : i + 1;
                var info = this.ExtractInformation(headerString, AppSettings.ReportHeaderFieldList[i], fieldIndices);
                formattedHeaders.Add(info.Item1, info.Item2);
            }

            return formattedHeaders;
        }

        private Tuple<string, string> ExtractInformation(string header, string field, Dictionary<string,int> fieldIndices)
        {
            try
            {
                string substr = string.Empty;
                string nextField = string.Empty;
                int nextFieldIndex = 0;
                int fieldIndex = header.IndexOf(field);
                if (fieldIndex < 0)
                {
                    return new Tuple<string, string>(field, AppSettings.Null);
                }

                var allIndices = new List<int>();
                int previousDifference = 100;
                int largestIndex = 0;
                foreach (var index in fieldIndices)
                {
                    if (index.Value < 0)
                    {
                        continue;
                    }

                    largestIndex = index.Value > largestIndex ? index.Value : largestIndex;
                    int difference = index.Value - fieldIndex;
                    if (difference > 0 && difference < previousDifference)
                    {
                        previousDifference = difference;
                        nextField = index.Key;
                        nextFieldIndex = index.Value;
                    }
                }

                if (fieldIndex == largestIndex)
                {
                    substr = header.Substring(fieldIndex);
                }
                else if (nextFieldIndex > 0)
                {
                    substr = header.Substring(fieldIndex, nextFieldIndex - fieldIndex);
                }
                else
                {
                    var e = new Exception("Failed to find " + field + " information in header '" + header + "'");
                    ExceptionLogger.LogException(e, this.file);
                    throw e;
                }

                Output.WriteLine(field + " info: '" + substr + "'");
                var elements = substr.Split(':');
                if (elements.Length == 2)
                {
                    return new Tuple<string, string>(elements[0].Trim(), elements[1].Trim());
                }
                else
                {
                    var e = new Exception("Failed to find " + field + " information in header '" + header + "'. Information found: " + substr + "\n");
                    ExceptionLogger.LogException(e, this.file);
                    throw e;
                }
            }
            catch(Exception e)
            {
                ExceptionLogger.LogException(e, this.file);
                this.log.Error(e.GetType() + ": " + e.Message);
                return new Tuple<string, string>(field, AppSettings.Null);
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
                                    formattedLine.Append(AppSettings.Error + AppSettings.Delimiter);
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
                            else if (string.IsNullOrEmpty(cell))
                            {
                                formattedLine.Append(AppSettings.Null + AppSettings.Delimiter + AppSettings.Null + AppSettings.Delimiter);
                            }
                            else
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter + AppSettings.Null + AppSettings.Delimiter);
                            }

                            break;
                        case 5:
                            if (string.IsNullOrEmpty(cell))
                            {
                                formattedLine.Append(AppSettings.Null + AppSettings.Delimiter);
                            }
                            else
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter);
                            }

                            break;
                        case 12:
                            if (short.TryParse(cell, out _))
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter);
                            }
                            else if (string.IsNullOrEmpty(cell))
                            {
                                formattedLine.Append(AppSettings.Null + AppSettings.Delimiter);
                            }
                            else
                            {
                                this.log.Error("Failed to process cell '" + cell + "'. Cell does not contain a number");
                                formattedLine.Append(AppSettings.Error + AppSettings.Delimiter);
                            }

                            break;
                        case 13:
                            if(string.IsNullOrEmpty(cell))
                            {
                                formattedLine.Append(AppSettings.Null + AppSettings.Delimiter);
                            }
                            else
                            {
                                formattedLine.Append(cell + AppSettings.Delimiter);
                            } 
                            
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
                                if (string.IsNullOrEmpty(cell))
                                {
                                    formattedLine.Append(AppSettings.Null + AppSettings.Delimiter + AppSettings.Null);
                                }
                                else
                                {
                                    this.log.Error("Failed to process cell '" + cell + "' in file '" + this.file + "'. " + ExceptionLogger.LogException(e, cell).Item2.Message);
                                    formattedLine.Append("ERROR" + AppSettings.Delimiter + "ERROR");
                                }
                            }

                            break;
                    }
                }
                catch (Exception e)
                {
                    this.log.Error("Error encountered whilst processing cell '" + cell + "' in file '" + this.file + "'", ExceptionLogger.LogException(e, cell).Item2);
                    formattedLine.Append("ERROR");
                }
            }

            return formattedLine;
        }
    }
}

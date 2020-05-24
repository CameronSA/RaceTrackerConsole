using RaceTrackerConsole.LogicHelpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole
{
    public class DataProcessing
    {
        public static void FormatDailyData(DateTime date)
        {
            string dateString = date.Year + "-" + date.Month + "-" + date.Day;
            foreach (var file in Directory.GetFiles(AppSettings.RaceRawDataDirectory))
            {
                if (file.Contains(dateString))
                {
                    var processedLines = new List<string>();
                    using (var fileReader = new StreamReader(file))
                    {
                        int counter = 0;
                        do
                        {
                            string line = FormatLine(fileReader.ReadLine(), counter == 0);
                            processedLines.Add(line);
                            counter++;
                        } while (!fileReader.EndOfStream);
                    }

                    if(!Directory.Exists(AppSettings.RaceProcessedDataDirectory))
                    {
                        Directory.CreateDirectory(AppSettings.RaceProcessedDataDirectory);
                    }

                    using (var fileWriter = new StreamWriter(AppSettings.RaceProcessedDataDirectory + "RaceProcessedData_" + dateString + ".txt"))
                    {
                        foreach(var line in processedLines)
                        {
                            fileWriter.WriteLine(line);
                        }
                    }
                }
            }
        }

        private static string FormatLine(string line, bool isHeader)
        {
            var cells = line.Split(AppSettings.Delimiter);
            StringBuilder formattedLine;
            if (isHeader)
            {
                formattedLine=ProcessHeaders(cells);
            }
            else
            {
                formattedLine=ProcessData(cells);
            }

            return formattedLine.ToString();
        }

        private static StringBuilder ProcessHeaders(string[] cells)
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

        private static StringBuilder ProcessData(string[] cells)
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
                                throw new Exception("Failed to process cell '" + cell + "'. Cell has " + cellElements.Length + " elements, but a minimum of 3 is required");
                            }

                            break;
                        case 1:
                            if (cell.Trim().Contains(" "))
                            {
                                var elements = cell.Split(' ');
                                formattedLine.Append(elements[0] + AppSettings.Delimiter);
                                formattedLine.Append(elements[1].Replace("(",string.Empty).Replace(")",string.Empty) + AppSettings.Delimiter);
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
                                throw new Exception("Failed to process cell '" + cell + "'. Cell does not contain a number");
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
                                    throw new Exception("Could not parse numerical values");
                                }
                            }
                            catch (Exception e)
                            {
                                throw new Exception("Failed to process cell '" + cell + "'. " + e.Message);
                            }
                            break;
                    }
                }
                catch(Exception e)
                {
                    formattedLine.Append("NULL");
                }
            }

            Console.WriteLine(formattedLine.ToString());
            return formattedLine;
        }
    }
}

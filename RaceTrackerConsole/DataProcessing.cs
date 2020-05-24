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
            var processedLines = new List<string>();
            foreach (var file in Directory.GetFiles(AppSettings.RaceDataDirectory))
            {
                if (file.Contains(dateString))
                {
                    using (var fileReader = new StreamReader(file))
                    {
                        do
                        {
                            string line = FormatLine(fileReader.ReadLine());
                            processedLines.Add(line);
                        } while (!fileReader.EndOfStream);
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
            return new StringBuilder();
        }

        private static StringBuilder ProcessData(string[] cells)
        {
            var formattedLine = new StringBuilder(string.Empty);
            for (int i = 0; i < cells.Length; i++)
            {
                var cell = cells[i];
                switch (i)
                {
                    case 0:
                        cell = cell.Replace(AppSettings.RaceDataWebsite, string.Empty);
                        var cellElements = cell.Split('/');
                        if (cellElements.Length > 3)
                        {
                            for (int n = 0; n < 3; n++)
                            {
                                formattedLine.Append(cellElements[i] + AppSettings.Delimiter);
                            }
                        }
                        else
                        {
                            throw new Exception("Failed to process cell '" + cell + "'. Cell has " + cellElements.Length + " elements, but a minimum of 3 is required");
                        }

                        break;
                    case 1:
                        if (short.TryParse(cell[0].ToString(), out _))
                        {
                            formattedLine.Append(cell[0]);
                        }
                        else
                        {
                            throw new Exception("Failed to process cell '" + cell + "'. Cell does not start with a number");
                        }

                        break;
                    case 2:
                        break;
                }
            }
        }
    }
}

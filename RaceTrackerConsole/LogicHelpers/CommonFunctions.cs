namespace RaceTrackerConsole.LogicHelpers
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Reflection;

    public static class CommonFunctions
    {
        private static Log log;

        public static DateTime GetDateFromFile(string filePath, DateTime defaultValue)
        {
            log = new Log(MethodBase.GetCurrentMethod().DeclaringType);

            DateTime date;
            if (File.Exists(filePath))
            {
                try
                {
                    using (var file = new StreamReader(filePath))
                    {
                        string text = file.ReadToEnd().Trim();
                        date = DateTime.ParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Failed to Read date from file '" + AppSettings.OldestDateMinedFile + "': " + e.Message);
                    throw e;
                }
            }
            else
            {
                log.Warn("Could not find file '" + filePath + "' to extract date. Setting date to '" + defaultValue.Year + "-" + defaultValue.Month + "-" + defaultValue.Day + "' instead");
                date = defaultValue;
            }

            return date;
        }

        public static T[] SubArray<T>(T[] array, int index, int length)
        {
            var list = new List<T>();
            for (int i = index; i < length; i++)
            {
                list.Add(array[i]);
            }

            return list.ToArray();
        }

        public static string RemoveInvalidFilenameChars(string filename, string replacementString)
        {
            return filename.Replace("<", replacementString).Replace(">", replacementString).Replace(":", replacementString).Replace("\"", replacementString).Replace("\\", replacementString).Replace("/", replacementString).Replace("|", replacementString).Replace("?", replacementString).Replace("*", replacementString);
        }
    }
}

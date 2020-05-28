using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole.LogicHelpers
{
    public static class ExceptionLogger
    {
        public static List<Tuple<string, Exception>> Exceptions { get; set; }

        public static Tuple<string, Exception> LogException(Exception exception, string url)
        {
            string message = "URL/Date: '" + url + "'. ";
            var exceptionDetails = new Tuple<string, Exception>(message, exception);
            try
            {
                Exceptions.Add(exceptionDetails);
            }
            catch
            {
                Exceptions = new List<Tuple<string, Exception>>
                {
                    exceptionDetails
                };
            }

            return exceptionDetails;
        }
    }
}

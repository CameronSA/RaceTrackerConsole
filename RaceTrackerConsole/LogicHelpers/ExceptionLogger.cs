using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole.LogicHelpers
{
    public static class ExceptionLogger
    {
        public static List<Exception> Exceptions { get; set; }

        public static Exception LogException(Exception exception)
        {
            try
            {
                Exceptions.Add(exception);
            }
            catch
            {
                Exceptions = new List<Exception>
                {
                    exception
                };
            }

            return exception;
        }
    }
}

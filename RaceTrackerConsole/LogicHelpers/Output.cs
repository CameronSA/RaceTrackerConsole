using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole.LogicHelpers
{
    public static class Output
    {
        public static bool PrintToConsole { get; set; } = true;

        public static void WriteLine(string message)
        {
            if (PrintToConsole)
            {
                Console.WriteLine(message);
            }
        }

        public static void Write(string message)
        {
            if(PrintToConsole)
            {
                Console.Write(message);
            }
        }
    }
}

namespace RaceTrackerConsole
{
    using System;

    class Program
    {
        static void Main(string[] args)
        {
            var date = new DateTime(2019, 5, 2);
            //DataMining.DailyData(date);
            DataProcessing.FormatDailyData(date);
        }
    }
}

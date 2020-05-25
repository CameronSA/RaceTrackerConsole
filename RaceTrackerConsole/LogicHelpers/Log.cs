using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RaceTrackerConsole.LogicHelpers
{
    public class Log
    {
        private readonly ILog log;

        public Log(Type type)
        {
            this.log = LogManager.GetLogger(type);
            GlobalContext.Properties["LogFilePath"] = AppDomain.CurrentDomain.BaseDirectory+"log4net.txt";
        }

        public void Info(string message)
        {
            this.log.Info(message);
        }

        public void Warn(string message)
        {
            this.log.Warn(message);
        }

        public void Error(string message)
        {
            this.log.Error(message);
        }

        public void Error(string message, Exception e)
        {
            this.log.Error(message, e);
        }
    }
}

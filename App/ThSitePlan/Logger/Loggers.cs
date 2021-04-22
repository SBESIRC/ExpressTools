using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace ThSitePlan.Log
{
    public interface ILogger
    {
        void LogInfo(string message);
        void LogError(string message);
    }

    public class ThSitePlanLogger :  ILogger
    {
        private Logger m_logger = null;
        public void LogInfo(string message)
        {
            m_logger.Info(message);
        }

        public void LogError(string message)
        {
            m_logger.Error(message);
        }

        public ThSitePlanLogger(Logger logger)
        {
            m_logger = logger;
        }
    }
}

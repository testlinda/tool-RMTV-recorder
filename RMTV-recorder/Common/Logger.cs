using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


//ref to https://stackoverflow.com/questions/2954900/simple-multithread-safe-log-class
namespace Log
{
    public interface ILogger
    {
        void WriteLine(string msg);
        void WriteError(string errorMsg);
        void WriteError(string errorObject, string errorAction, string errorMsg);
        void WriteWarning(string errorObject, string errorAction, string errorMsg);
    }

    public class Logger : ILogger
    {
        BlockingCollection<Param> bc = new BlockingCollection<Param>();

        // Constructor create the thread that wait for work on .GetConsumingEnumerable()
        public Logger()
        {
            Task.Factory.StartNew(() =>
            {
                foreach (Param p in bc.GetConsumingEnumerable())
                {
                    switch (p.Ltype)
                    {
                        case Log.Param.LogType.Info:
                            const string LINE_MSG = "[{0}] {1}";
                            Console.WriteLine(String.Format(LINE_MSG, LogTimeStamp(), p.Msg));
                            break;
                        case Log.Param.LogType.Warning:
                            const string WARNING_MSG = "[{3}] * Warning {0} (Action {1} on {2})";
                            Console.WriteLine(String.Format(WARNING_MSG, p.Msg, p.Action, p.Obj, LogTimeStamp()));
                            break;
                        case Log.Param.LogType.Error:
                            const string ERROR_MSG = "[{3}] *** Error {0} (Action {1} on {2})";
                            Console.WriteLine(String.Format(ERROR_MSG, p.Msg, p.Action, p.Obj, LogTimeStamp()));
                            break;
                        case Log.Param.LogType.SimpleError:
                            const string ERROR_MSG_SIMPLE = "[{0}] *** Error {1}";
                            Console.WriteLine(String.Format(ERROR_MSG_SIMPLE, LogTimeStamp(), p.Msg));
                            break;
                        default:
                            Console.WriteLine(String.Format(LINE_MSG, LogTimeStamp(), p.Msg));
                            break;
                    }
                }
            });
        }

        ~Logger()
        {
            // Free the writing thread
            bc.CompleteAdding();
        }

        // Just call this method to log something (it will return quickly because it just queue the work with bc.Add(p))
        public void WriteLine(string msg)
        {
            Param p = new Param(Log.Param.LogType.Info, msg);
            bc.Add(p);
        }

        public void WriteError(string errorMsg)
        {
            Param p = new Param(Log.Param.LogType.SimpleError, errorMsg);
            bc.Add(p);
        }

        public void WriteError(string errorObject, string errorAction, string errorMsg)
        {
            Param p = new Param(Log.Param.LogType.Error, errorMsg, errorAction, errorObject);
            bc.Add(p);
        }

        public void WriteWarning(string errorObject, string errorAction, string errorMsg)
        {
            Param p = new Param(Log.Param.LogType.Warning, errorMsg, errorAction, errorObject);
            bc.Add(p);
        }

        string LogTimeStamp()
        {
            DateTime now = DateTime.Now;
            return now.ToShortTimeString();
        }

    }

    internal class Param
    {
        internal enum LogType { Info, Warning, Error, SimpleError };

        internal LogType Ltype { get; set; }  // Type of log
        internal string Msg { get; set; }     // Message
        internal string Action { get; set; }  // Action when error or warning occurs (optional)
        internal string Obj { get; set; }     // Object that was processed when error or warning occurs (optional)

        internal Param()
        {
            Ltype = LogType.Info;
            Msg = "";
        }
        internal Param(LogType logType, string logMsg)
        {
            Ltype = logType;
            Msg = logMsg;
        }
        internal Param(LogType logType, string logMsg, string logAction, string logObj)
        {
            Ltype = logType;
            Msg = logMsg;
            Action = logAction;
            Obj = logObj;
        }
    }
}

#if HE_SYSCORE

using System;
using UnityEngine;

namespace HeathenEngineering.UX.API
{
    public static class Log
    {
        public static bool Enabled
        {
            get => log != null;
            set
            {
                if (value)
                {
                    log = new LogData();
                    Application.logMessageReceivedThreaded += Application_logMessageReceivedThreaded;
                }
                else
                {
                    if (log != null)
                    {
                        Application.logMessageReceivedThreaded -= Application_logMessageReceivedThreaded;
                        log = null;
                    }
                }
            }
        }
        public static string dateTimeFormat = "yyyy-MM-dd HH:mm:ss zzzz";
        public static bool logStackTrace = false;

        private static LogData log;

        public static string Text
        {
            get
            {
                if (log == null)
                    return string.Empty;
                else
                    return log.ToString();
            }
        }

        public static string Json
        {
            get
            {
                if (log == null)
                    return "{}";
                else
                    return log.ToJson();
            }
        }

        public static byte[] Bytes
        {
            get
            {
                if (log == null)
                    return new byte[0];
                else
                    return System.Text.Encoding.UTF8.GetBytes(log.ToJson());
            }
        }

        public static LogData Object => log;

        public static LogData Parse(string json)
        {
            return JsonUtility.FromJson<LogData>(json);
        }

        public static LogData Parse(byte[] data)
        {
            return Parse(System.Text.Encoding.UTF8.GetString(data));
        }

        public static void Reset() => log = new LogData();

        public static void LogManual(string message, string stack, LogType type) => log?.Add(message, stack, type);
        public static void LogMessage(string message) => Debug.Log(message);
        public static void LogError(string message) => Debug.LogError(message);
        public static void LogException(Exception ex) => Debug.LogException(ex);

        /// <summary>
        /// Saves the current log to a JSON formated file
        /// </summary>
        /// <param name="fileName">The name of the file to save</param>
        /// <param name="path">The path to save the file to</param>
        /// <returns>The full path of the saved file</returns>
        public static string SaveToJsonFile(string fileName, string path = null) => log.SaveJson(fileName, path);
        /// <summary>
        /// Saves the current log to a human friendly formated text file
        /// </summary>
        /// <param name="fileName">The name of the file to save</param>
        /// <param name="path">The path to save the file to</param>
        /// <returns>The full path of the saved file</returns>
        public static string SaveToTextFile(string fileName, string path = null) => log.SaveText(fileName, path);

        private static void Application_logMessageReceivedThreaded(string condition, string stackTrace, LogType type)
        {
            if (log != null)
            {
                if (logStackTrace)
                    log.Add(condition, stackTrace, type);
                else
                    log.Add(condition, string.Empty, type);
            }
        }
    }
}

#endif

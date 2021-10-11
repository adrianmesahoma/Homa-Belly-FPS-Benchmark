using System;
using System.Collections.Generic;
using System.IO;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
	/// Log class to centralizing all Log informations. Can be
	/// enabled/disabled from Homa Games Editor Settings window
	/// </summary>
    public static class HomaBellyEditorLog
    {
        private static string LOG_FILE = Application.dataPath + "/../Library/Homa Belly/Editor.log";
        private const string LOG_FORMAT = "{0}";

        public static List<KeyValuePair<Level, string>> _logTrace;
        public static List<KeyValuePair<Level, string>> LogTrace
        {
            get
            {
                if (_logTrace == null)
                {
                    _logTrace = new List<KeyValuePair<Level, string>>();
                }

                return _logTrace;
            }
        }

        public enum Level
        {
            DEBUG,
            WARNING,
            ERROR
        }

        #region Serialization
        public static void BeforeAssemblyReload()
        {
            if (_logTrace != null)
            {
                // Truncate file
                if (!File.Exists(LOG_FILE))
                {
                    FileUtilities.CreateIntermediateDirectoriesIfNecessary(LOG_FILE);
                }

                FileStream stream = File.Create(LOG_FILE);
                stream.Flush();
                stream.Close();

                // Start writting
                File.AppendAllText(LOG_FILE, "[");
                File.AppendAllText(LOG_FILE, "\n");

                for (int i = 0; i < _logTrace.Count; i++)
                {
                    string logLevel = ((int)_logTrace[i].Key).ToString();
                    string logMessage = _logTrace[i].Value;
                    File.AppendAllText(LOG_FILE, Json.Serialize($"[{logLevel}]{logMessage}"));

                    // If its not last log
                    if (i < _logTrace.Count - 1)
                    {
                        File.AppendAllText(LOG_FILE, ",\n");
                    }
                }

                File.AppendAllText(LOG_FILE, "\n]");
            }
        }

        public static void AfterAssemblyReload()
        {
            if (File.Exists(LOG_FILE))
            {
                _logTrace = new List<KeyValuePair<Level, string>>();
                List<object> logList = Json.Deserialize(FileUtilities.ReadAllText(LOG_FILE)) as List<object>;
                for (int i = 0; logList != null && i < logList.Count; i++)
                {
                    string logString = Convert.ToString(logList[i]);
                    int logLevel = int.Parse(logString.Substring(0, 3).Replace("[", "").Replace("]", ""));
                    string logMessage = logString.Substring(3);
                    _logTrace.Add(new KeyValuePair<Level, string>((Level)logLevel, logMessage));
                }
            }
        }

        public static void ResetLog()
        {
            if (File.Exists(LOG_FILE))
            {
                File.Delete(LOG_FILE);
            }

            if (_logTrace != null)
            {
                _logTrace.Clear();
            }
        }

        #endregion

        #region Basic logs

        /// <summary>
        /// Logs a message with Debug severity
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
        {
            LogTrace.Add(new KeyValuePair<Level, string>(Level.DEBUG, string.Format(LOG_FORMAT, message)));
        }

        /// <summary>
		/// Logs a message with Warning severity
		/// </summary>
		/// <param name="message"></param>
        public static void Warning(string message)
        {
            LogTrace.Add(new KeyValuePair<Level, string>(Level.WARNING, string.Format(LOG_FORMAT, message)));
        }

        /// <summary>
		/// Logs a message with Error severity
		/// </summary>
		/// <param name="message"></param>
        public static void Error(string message)
        {
            LogTrace.Add(new KeyValuePair<Level, string>(Level.ERROR, string.Format(LOG_FORMAT, message)));
        }

        #endregion

        #region Formatted string parameters

        public static void DebugFormat(string message, params object[] format)
        {
            string formattedMessage = GetFormattedMessage(message, format);
            Debug(formattedMessage);
        }

        public static void WarningFormat(string message, params object[] format)
        {
            string formattedMessage = GetFormattedMessage(message, format);
            Warning(formattedMessage);
        }

        public static void ErrorFormat(string message, params object[] format)
        {
            string formattedMessage = GetFormattedMessage(message, format);
            Error(formattedMessage);
        }

        #endregion

        #region Utils

        private static string GetFormattedMessage(string message, params object[] format)
        {
            string formattedMessage = "";

            try
            {
                formattedMessage = string.Format(message, format);
            }
            catch (System.Exception exception)
            {
                string exceptionError = string.Format("Could not format log message: {0}", exception.Message);
                UnityEngine.Debug.LogWarning(string.Format(LOG_FORMAT, exceptionError));
            }

            return formattedMessage;
        }

        #endregion
    }
}
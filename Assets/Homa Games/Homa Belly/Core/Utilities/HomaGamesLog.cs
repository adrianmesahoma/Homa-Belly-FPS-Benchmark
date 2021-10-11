using UnityEngine;

namespace HomaGames.HomaBelly
{
    /// <summary>
	/// Log class to centralizing all Log informations. Can be
	/// enabled/disabled from Homa Games Editor Settings window
	/// </summary>
    public static class HomaGamesLog
    {
        private const string LOG_FORMAT = "{0}";
        public static bool debugEnabled = false;

#region Basic logs

        /// <summary>
        /// Logs a message with Debug severity
        /// </summary>
        /// <param name="message"></param>
        public static void Debug(string message)
		{
            if (debugEnabled)
			{
                StackTraceLogType previous = GetStackTraceLogType(LogType.Log, StackTraceLogType.None);
                UnityEngine.Debug.Log(string.Format(LOG_FORMAT, message));
                SetStackTraceLogType(LogType.Log, previous);
            }
        }

        /// <summary>
		/// Logs a message with Warning severity
		/// </summary>
		/// <param name="message"></param>
        public static void Warning(string message)
		{
            StackTraceLogType previous = GetStackTraceLogType(LogType.Warning, StackTraceLogType.None);
            UnityEngine.Debug.LogWarning(string.Format(LOG_FORMAT, message));
            SetStackTraceLogType(LogType.Warning, previous);
        }

        /// <summary>
		/// Logs a message with Error severity
		/// </summary>
		/// <param name="message"></param>
        public static void Error(string message)
		{
            StackTraceLogType previous = GetStackTraceLogType(LogType.Error, StackTraceLogType.ScriptOnly);
            UnityEngine.Debug.LogError(string.Format(LOG_FORMAT, message));
            SetStackTraceLogType(LogType.Error, previous);
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

        private static StackTraceLogType GetStackTraceLogType(LogType type, StackTraceLogType stackTraceLogType)
        {
            try
            {
                StackTraceLogType previous = Application.GetStackTraceLogType(type);
                Application.SetStackTraceLogType(type, stackTraceLogType);
                return previous;
            }
            catch (UnityException)
            {
                // NO-OP
            }

            return StackTraceLogType.Full;
        }

        private static void SetStackTraceLogType(LogType type, StackTraceLogType stackTraceLogType)
        {
            try
            {
                Application.SetStackTraceLogType(type, stackTraceLogType);
            }
            catch (UnityException)
            {
                // NO-OP
            }
        }

        private static string GetFormattedMessage(string message, params object[] format)
        {
            string formattedMessage = "";

            try
            {
                formattedMessage = string.Format(message, format);
            }
            catch(System.Exception exception)
            {
                string exceptionError = string.Format("Could not format log message: {0}", exception.Message);
                UnityEngine.Debug.LogWarning(string.Format(LOG_FORMAT, exceptionError));
            }

            return formattedMessage;
        }

#endregion
    }
}


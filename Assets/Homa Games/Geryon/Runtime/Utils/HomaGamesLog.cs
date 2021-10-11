namespace HomaGames.Geryon
{
    /// <summary>
	/// Log class to centralizing all Log informations. Can be
	/// enabled/disabled from Homa Games Editor Settings window
	/// </summary>
    public static class HomaGamesLog
    {
        private const string LOG_FORMAT = "[N-Testing] {0}";
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
                UnityEngine.Debug.Log(string.Format(LOG_FORMAT, message));
			}
		}

        /// <summary>
		/// Logs a message with Warning severity
		/// </summary>
		/// <param name="message"></param>
        public static void Warning(string message)
		{
			UnityEngine.Debug.LogWarning(string.Format(LOG_FORMAT, message));
		}

        /// <summary>
		/// Logs a message with Error severity
		/// </summary>
		/// <param name="message"></param>
        public static void Error(string message)
		{
            UnityEngine.Debug.LogError(string.Format(LOG_FORMAT, message));
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


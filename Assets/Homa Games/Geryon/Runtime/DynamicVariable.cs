using System.Collections.Generic;
using System.Threading;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Class representing a dynamic variable. This can host any data type
    /// (bool, int, double, string...)
    /// </summary>
    /// <typeparam name="T">The data type of the variable</typeparam>
    public sealed class DynamicVariable<T>
    {
        private const int MAX_GET_THREAD_AWAIT_MS = 1000;
        private const int GET_THREAD_AWAIT_TICK_MS = 25;

        /// <summary>
        /// Collection of all available DynamicVariables
        /// </summary>
        private static readonly Dictionary<string, T> innerDynamicVariablesDictionary = new Dictionary<string, T>();

        public static T Get(string key, T defaultValue)
        {
            if (!Config.AdvertisingIdFetched && !Config.Initialized)
            {
                return defaultValue;
            }

            var waitedTime = 0;
            while (!Config.Initialized && waitedTime < MAX_GET_THREAD_AWAIT_MS)
            {
                Thread.Sleep(GET_THREAD_AWAIT_TICK_MS);
                waitedTime += GET_THREAD_AWAIT_TICK_MS;
            }

            if (innerDynamicVariablesDictionary.ContainsKey(key))
            {
                return innerDynamicVariablesDictionary[key];
            }

            return defaultValue;
        }

        /// <summary>
        /// Updates the dynamic variable referenced by `key`, if it exists.
        /// If not, adds it to the dictionary
        /// </summary>
        /// <param name="key">The key referencing the dynamic variable</param>
        /// <param name="value">The new value</param>
        public static void Set(string key, T value)
        {
            if (innerDynamicVariablesDictionary.ContainsKey(key))
            {
                innerDynamicVariablesDictionary[key] = value;
            }
            else
            {
                innerDynamicVariablesDictionary.Add(key, value);
            }
        }

        #region Unit Test helpers
#if UNITY_EDITOR
        /// <summary>
        /// Method to clear all stored dynamic variables. This method
        /// is only used for unit testing.
        /// </summary>
        public static void ClearDynamicVariables()
        {
            innerDynamicVariablesDictionary.Clear();
        }

        /// <summary>
        /// Getter to be used within Unit Tests
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static T GetForUnitTests(string key)
        {
            return innerDynamicVariablesDictionary[key];
        }
#endif
        #endregion
    }
}
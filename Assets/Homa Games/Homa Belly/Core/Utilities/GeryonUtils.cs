using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly.Utilities
{
    public static class GeryonUtils
    {
        private const int WAIT_ITERATION_MS = 250;      // ms to wait for 'initialized' check
        private const int N_MAX_WAIT_ITERATIONS = 8;    // 8 iterations of 250ms => 2 seconds

        /// <summary>
        /// Try to obtain Geryon NTesting ID with reflection. If not found,
        /// returns an empty string.
        ///
        /// Upon Geryon v3.0.0+, it is initialized asynchronously. Hence, this method
        /// asynchronously awaits for its initialization (2 seconds) and then tries to
        /// obtian the NTESTING_ID
        /// </summary>
        /// <returns>The Geryon NTESTING_ID if found, or an empty string if not</returns>
        public static string GetGeryonTestingId()
        {
            string geryonNtestingId = "";
            try
            {
                WaitForInitialization().ContinueWith((result) =>
                {
                    HomaGamesLog.Debug($"Looking for Geryon NTESTING_ID");
                    Type geryonConfig = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                            from type in assembly.GetTypes()
                                            where type.Namespace == "HomaGames.Geryon" && type.Name == "Config"
                                            select type).FirstOrDefault();
                    if (geryonConfig != null)
                    {
                        // After waiting for `Initialized` propery (either becoming `true` or iterations finished)
                        // try to fecth NTESTING_ID. For Geryon prior v3.0.0 this will be executed right away
                        // without waiting for `Initialized`
                        System.Reflection.PropertyInfo ntestingIdPropertyInfo = geryonConfig.GetProperty("NTESTING_ID", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (ntestingIdPropertyInfo != null)
                        {
                            var ntestingPropertyValue = ntestingIdPropertyInfo.GetValue(null, null);
                            if (ntestingPropertyValue != null)
                            {
                                geryonNtestingId = ntestingPropertyValue.ToString();
                                HomaGamesLog.Debug($"Geryon NTESTING_ID found: {geryonNtestingId}");
                            }
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"Geryon not found: {e.Message}");
            }

            return geryonNtestingId;
        }

        /// <summary>
        /// Try to obtain Geryon dynamic variable
        /// </summary>
        /// <param name="propertyName">The property name of the variable. All in caps and without type prefix: ie. IDFA_CONSENT_POPUP_DELAY_S</param>
        /// <returns></returns>
        public static string GetGeryonDynamicVariableValue(string propertyName)
        {
            string value = null;
            try
            {
                WaitForInitialization().ContinueWith((result) =>
                {
                    Type geryonDvrType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                          from type in assembly.GetTypes()
                                          where type.Name == "DVR"
                                          select type).FirstOrDefault();
                    if (geryonDvrType != null)
                    {
                        FieldInfo field = geryonDvrType.GetField(propertyName);
                        if (field != null)
                        {
                            value = field.GetValue(null).ToString();
                            UnityEngine.Debug.Log($"{propertyName} value from Geryon: {value}");
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Could not obtain {propertyName} value from Geryon: {e.Message}");
            }

            return value;
        }

        /// <summary>
        /// Obtains NTesting OverrideId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingOverrideId()
        {
            return await GetNTestingStringProperty("OverrideId");
        }

        /// <summary>
        /// Obtains NTesting VariantId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingVariantId()
        {
            return await GetNTestingStringProperty("VariantId");
        }

        /// <summary>
        /// Obtains NTesting ScopeId value through reflection
        /// </summary>
        /// <returns></returns>
        public static async Task<string> GetNTestingScopeId()
        {
            return await GetNTestingStringProperty("ScopeId");
        }

        /// <summary>
        /// Obtains by reflection the external token value for the given property name:
        /// - ExternalToken0
        /// - ExternalToken1
        /// </summary>
        /// <param name="externalTokenPropertyName"></param>
        /// <param name="externalToken"></param>
        public static async Task<string> GetNTestingExternalToken(string externalTokenPropertyName)
        {
            return await GetNTestingStringProperty(externalTokenPropertyName);
        }

        #region Private methods

        /// <summary>
        /// Obtains a string property value from the NTesting `Config` class
        /// through reflection
        /// </summary>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static async Task<string> GetNTestingStringProperty(string propertyName)
        {
            string stringPropertyValue = "";
            try
            {
                await WaitForInitialization().ContinueWith((result) =>
                {
                    Type geryonConfigType = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                             from type in assembly.GetTypes()
                                             where type.Namespace == "HomaGames.Geryon" && type.Name == "Config"
                                             select type).FirstOrDefault();
                    if (geryonConfigType != null)
                    {
                        PropertyInfo propertyInfo = geryonConfigType.GetProperty(propertyName, BindingFlags.Static | BindingFlags.Public);
                        if (propertyInfo != null)
                        {
                            var propertyValue = propertyInfo.GetValue(null, null);
                            if (propertyValue != null)
                            {
                                stringPropertyValue = propertyValue.ToString();
                            }
                        }
                    }
                }, TaskScheduler.FromCurrentSynchronizationContext());
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogWarning($"Could not obtain {propertyName} value from NTesting: {e.Message}");
            }

            return stringPropertyValue;
        }

        /// <summary>
        /// Asynchronousle awaits for Geryon to be Initialized, so we can safely
        /// access any of its properties/values with reflection
        /// </summary>
        /// <returns></returns>
        public static async Task WaitForInitialization()
        {
            try
            {
                Type geryonConfig = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                                     from type in assembly.GetTypes()
                                     where type.Namespace == "HomaGames.Geryon" && type.Name == "Config"
                                     select type).FirstOrDefault();
                if (geryonConfig != null)
                {
                    // Run the reflection asynchronously
                    await Task.Run(() =>
                    {
                        // For NTesting 3.0.0+, 'Initialized' property is available after asynchronous initialization
                        System.Reflection.PropertyInfo ntestingInitializedPropertyInfo = geryonConfig.GetProperty("Initialized", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                        if (ntestingInitializedPropertyInfo != null)
                        {
                            var propertyValue = ntestingInitializedPropertyInfo.GetValue(null, null);
                            if (propertyValue != null)
                            {
                                // Wait for `Initialized` property to be `true`
                                bool initialized = false;
                                int iterationCount = 0;
                                bool.TryParse(propertyValue.ToString(), out initialized);
                                while (!initialized && iterationCount < N_MAX_WAIT_ITERATIONS)
                                {
                                    // Not yet initialized. Debug log and wait a bit more
                                    HomaGamesLog.Debug($"NTesting not yet initialized. Iteration: {iterationCount}. Waiting...");
                                    Thread.Sleep(WAIT_ITERATION_MS);

                                    // Update value
                                    iterationCount++;
                                    propertyValue = ntestingInitializedPropertyInfo.GetValue(null, null);
                                    bool.TryParse(propertyValue.ToString(), out initialized);
                                }

                                // `Initialized` went `true` or the maximum iterations reached
                            }
                        }
                    });
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"Geryon not found: {e.Message}");
            }
        }

        #endregion
    }
}


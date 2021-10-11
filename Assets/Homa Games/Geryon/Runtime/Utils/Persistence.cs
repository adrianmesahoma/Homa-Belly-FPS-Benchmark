using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using HomaGames.Geryon.MiniJSON;
using UnityEngine;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Persistence class to store any data that must persist between game runs. ie: first time app open configuration
    /// </summary>
    public static class Persistence
    {
        private static string FIRST_TIME_REMOTE_CONFIGURATION_FILE;
        private static string FIRST_TIME_EXTERNAL_TOKENS_FILE;

        /// <summary>
        /// Setup anything required for asynchronous persistance handling. ie: path for persistent data
        /// This method needs to run in Unity Main Thread
        /// </summary>
        public static void SetUp()
        {
            FIRST_TIME_REMOTE_CONFIGURATION_FILE = Path.Combine(Application.persistentDataPath + "/homagames/ntesting/first_time_configuration.json");
            FIRST_TIME_EXTERNAL_TOKENS_FILE = Path.Combine(Application.persistentDataPath + "/homagames/ntesting/first_time_external_tokens.json");
        }
        /// <summary>
        /// Persists the First Time App Open configuration into a file
        /// </summary>
        /// <param name="firstTimeConfigurationResponse">The response fetched from NTesting API</param>
        public static void PersistFirstTimeConfiguration(ConfigurationResponse firstTimeConfigurationResponse)
        {
            if (firstTimeConfigurationResponse == null)
            {
                HomaGamesLog.Warning("Trying to persist an empty first time app open response. Skipping");
                return;
            }

            if (firstTimeConfigurationResponse.Configuration != null)
            {
                // Serialize configuration and store it in FIRST_TIME_REMOTE_CONFIGURATION_FILE
                string serializedConfiguration = Json.Serialize(firstTimeConfigurationResponse.Configuration);
                FileUtilities.CreateIntermediateDirectoriesIfNecessary(FIRST_TIME_REMOTE_CONFIGURATION_FILE);
                File.WriteAllText(FIRST_TIME_REMOTE_CONFIGURATION_FILE, serializedConfiguration);
            }

            // Serialize configuration and store it in FIRST_TIME_EXTERNAL_TOKENS_FILE
            string[] externalTokensList = new string[]
            {
                firstTimeConfigurationResponse.ExternalToken0,
                firstTimeConfigurationResponse.ExternalToken1,
                firstTimeConfigurationResponse.ExternalToken2,
                firstTimeConfigurationResponse.ExternalToken3,
                firstTimeConfigurationResponse.ExternalToken4
            };

            string serializedExternalTokens = Json.Serialize(externalTokensList);
            FileUtilities.CreateIntermediateDirectoriesIfNecessary(FIRST_TIME_EXTERNAL_TOKENS_FILE);
            File.WriteAllText(FIRST_TIME_EXTERNAL_TOKENS_FILE, serializedExternalTokens);
        }

        /// <summary>
        /// Loads the First Time App Open configuration from local file
        /// </summary>
        /// <returns>A fulfilled dictionary with the first time app open configuration loaded</returns>
        public static Dictionary<string, object> LoadFirstTimeConfigurationFromPersistence()
        {
            Dictionary<string, object> firstTimeAppOpenDictionary = new Dictionary<string, object>();
            if (File.Exists(FIRST_TIME_REMOTE_CONFIGURATION_FILE))
            {
                try
                {
                    string firstTimeAppOpenConfigurationJson = FileUtilities.ReadAllText(FIRST_TIME_REMOTE_CONFIGURATION_FILE);
                    if (!string.IsNullOrEmpty(firstTimeAppOpenConfigurationJson))
                    {
                        firstTimeAppOpenDictionary = (Dictionary<string, object>) Json.Deserialize(firstTimeAppOpenConfigurationJson);
                        return firstTimeAppOpenDictionary;
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.WarningFormat("Exception while reading first time app open from file: {0}", e.Message);
                }
            }

            return firstTimeAppOpenDictionary;
        }

        /// <summary>
        /// Loads the First Time App Open external tokens from local file
        /// </summary>
        /// <returns>A fulfilled list with first time app open external tokens</returns>
        public static string[] LoadFirstTimeExternalTokensFromPersistence()
        {
            string[] externalTokens = new string[4];
            if (File.Exists(FIRST_TIME_EXTERNAL_TOKENS_FILE))
            {
                try
                {
                    string firstTimeAppOpenExternalTokensJson = FileUtilities.ReadAllText(FIRST_TIME_EXTERNAL_TOKENS_FILE);
                    if (!string.IsNullOrEmpty(firstTimeAppOpenExternalTokensJson))
                    {
                        List<object> deserializedList = (List<object>) Json.Deserialize(firstTimeAppOpenExternalTokensJson);
                        externalTokens = deserializedList.Select(s => (string)s).ToArray();
                        return externalTokens;
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.WarningFormat("Exception while reading first time app open external tokens from file: {0}", e.Message);
                }
            }

            return externalTokens;
        }
    }
}

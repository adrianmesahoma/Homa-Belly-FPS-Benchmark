using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly.Utilities;
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Class used to fetch Damysus Remote Configuration.
    ///
    /// By sending to the server some useful information about Damysus
    /// configuration (app token, app identifier and dependencies), the
    /// server will return a configuration for the app at runtime
    /// </summary>
    public class RemoteConfiguration
    {
        /// <summary>
        /// Response model fetched from Remote Configuration endpoints
        /// </summary>
        public class RemoteConfigurationSetup
        {
            public string AppToken;
            public CrossPromotionConfigurationModel CrossPromotionConfigurationModel;
        }

        #region Public methods

        /// <summary>
        /// Fetch the Remote Configuration model as an asynchrouos Task.
        /// This method needs to be awaited and Damysus initialization
        /// needs to be defered until the Remote Configuration is fetched.
        /// </summary>
        /// <returns>A RemoteConfigurationSetup model representing the Remote Configuration to apply at runtime</returns>
        public static async Task<RemoteConfigurationSetup> FetchRemoteConfiguration()
        {
            RemoteConfigurationSetup everyTimeResult = new RemoteConfigurationSetup();

            try
            {
                Dictionary<string, object> trackingData = await ReadTrackingData();
                if (trackingData != null && trackingData.ContainsKey("ti"))
                {
                    string firstTimeUri = string.Format(RemoteConfigurationConstants.API_FIRST_TIME_URL, trackingData["ti"], GetUserAgent());
                    string everyTimeUri = string.Format(RemoteConfigurationConstants.API_EVERY_TIME_URL, trackingData["ti"], GetUserAgent(), BuildDebugParameter(trackingData));

                    // Await secuentially for both results
                    if (PlayerPrefs.GetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 0) == 0)
                    {
                        // We are not interested in FIRST TIME response, so we just ignore it
                        HomaGamesLog.Debug($"[Remote Configuration] Requesting first time config {firstTimeUri}...");

                        await Get(firstTimeUri);

                        PlayerPrefs.SetInt(RemoteConfigurationConstants.FIRST_TIME_ALREADY_REQUESTED, 1);
                        PlayerPrefs.Save();
                        HomaGamesLog.Debug($"[Remote Configuration] Done");
                    }

                    HomaGamesLog.Debug($"[Remote Configuration] Requesting every time config {everyTimeUri}");

                    everyTimeResult = await Get(everyTimeUri);

                    HomaGamesLog.Debug($"[Remote Configuration] Done");
                }
                else
                {
                    HomaGamesLog.Debug($"[Remote Configuration] Tracking data not found. Skipping...");
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"[Remote Configuration] Could not fetch Remote Configuration: {e.Message}");
            }

            return everyTimeResult;
        }

        #endregion

        #region Private helpers

        /// <summary>
        /// Obtain the User Agent to be sent within the requests
        /// </summary>
        /// <returns></returns>
        private static string GetUserAgent()
        {
            string userAgent = "ANDROID";
#if UNITY_IOS
            userAgent = "IPHONE";
            try
            {
                if (UnityEngine.iOS.Device.generation.ToString().Contains("iPad"))
                {
                    userAgent = "IPAD";
                }
            }
            catch (Exception e)
            {
                HomaGamesLog.Warning($"Could not determine iOS device generation: ${e.Message}");
            }
            
#endif
            return userAgent;
        }

        /// <summary>
        /// Creates the required Base64 string for the "dbg" request parameter
        /// </summary>
        /// <param name="trackingData">The tracking data dictionary to build the dbg parameter</param>
        /// <returns>The Base64 string to be sent through the request</returns>
        private static string BuildDebugParameter(Dictionary<string, object> trackingData)
        {
            string debugString = "";
            Dictionary<string, object> debugJson = new Dictionary<string, object>();
            if (trackingData != null && trackingData.ContainsKey("dp"))
            {
                debugJson.Add("dp", trackingData["dp"]);
            }

            if (debugJson.Count > 0)
            {
                string serializedDebugJson = Json.Serialize(debugJson);
                byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(serializedDebugJson);

                // Base 64 encode
                debugString = System.Convert.ToBase64String(plainTextBytes);

                // Escape Base 64
                debugString = UnityWebRequest.EscapeURL(debugString);
            }

            return debugString;
        }

        /// <summary>
        /// Reads the tracking data from Streaming Assets config file
        /// </summary>
        /// <returns></returns>
        private async static Task<Dictionary<string, object>> ReadTrackingData()
        {
#if UNITY_EDITOR
            if (!File.Exists(RemoteConfigurationConstants.TRACKING_FILE))
            {
                return null;
            }
#endif

            string path = RemoteConfigurationConstants.TRACKING_FILE;
            Dictionary<string, object> result= null;
            Task.Run(delegate
            {
                string trackingData = File.ReadAllText(path);
                result = Json.Deserialize(trackingData) as Dictionary<string, object>;
            });
           
            return result;
        }



        /// <summary>
        /// Asynchornous Http GET request
        /// </summary>
        /// <param name="uri">The URI to query</param>
        /// <returns></returns>
        private static async Task<RemoteConfigurationSetup> Get(string uri)
        {
            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                HttpResponseMessage response = await client.GetAsync(uri).ConfigureAwait(false);
                if (response.IsSuccessStatusCode)
                {
                    string resultString = await response.Content.ReadAsStringAsync();

                    // Return empty manifest if json string is not valid
                    if (string.IsNullOrEmpty(resultString))
                    {
                        return default;
                    }

                    // Parse result
                    RemoteConfigurationSetup remoteConfigurationSetup = new RemoteConfigurationSetup();

                    // Basic info
                    Dictionary<string, object> dictionary = Json.Deserialize(resultString) as Dictionary<string, object>;
                    if (dictionary != null)
                    {
                        remoteConfigurationSetup.AppToken = (string)dictionary["ti"];

                        if (dictionary.ContainsKey("res"))
                        {
                            Dictionary<string, object> resDictionary = (Dictionary<string, object>) dictionary["res"];
                            remoteConfigurationSetup.CrossPromotionConfigurationModel = CrossPromotionConfigurationModel.FromRemoteConfigurationDictionary(resDictionary);
                        }
                    }

                    HomaGamesLog.Debug($"[Remote Configuration] Request result to {uri}\n {resultString}");
                    return remoteConfigurationSetup;
                }
            }

            return default;
        }
#endregion
    }
}

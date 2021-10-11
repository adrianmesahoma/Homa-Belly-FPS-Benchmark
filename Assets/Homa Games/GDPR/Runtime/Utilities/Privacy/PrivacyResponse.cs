using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using HomaGames.HomaBelly;
using UnityEngine;

namespace HomaGames.GDPR
{
    public class PrivacyResponse
    {
        //[JsonProperty("s_country_code")]
        public string CountryCode;
        //[JsonProperty("s_region")]
        public string Region;
        //[JsonProperty("s_law_name")]
        public string LawName;

        /// <summary>
        /// By default, never protected
        /// </summary>
        //[JsonProperty("b_protected")]
        public bool Protected = false;

        public static async Task<PrivacyResponse> FetchPrivacyForCurrentRegion()
        {
            PrivacyResponse privacyResponse = new PrivacyResponse();

            // If network is not reachable, return default PrivacyResponse, which is
            // protected
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                return privacyResponse;
            }
            
            using (HttpClient client = HttpCaller.GetHttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(10);
                string homaBellyToken = "";
                Dictionary<string, object> trackingData = ReadTrackingData();
                if (trackingData != null && trackingData.ContainsKey("ti"))
                {
                    homaBellyToken = trackingData["ti"] as string;
                }

                string url = string.Format(Constants.API_PRIVACY_REGION_URL, homaBellyToken, GetUserAgent());
                HttpResponseMessage response = null;
                string resultString = "";
                try
                {
                    response = await client.GetAsync(url).ConfigureAwait(false);
                    if (response != null && response.IsSuccessStatusCode)
                    {
                        resultString = await response.Content.ReadAsStringAsync();
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning($"[Privacy] Could not fetch privacy region configuration. ERROR: {e.Message}");
                    return privacyResponse;
                }

                // Return empty manifest if json string is not valid
                if (string.IsNullOrEmpty(resultString))
                {
                    return default;
                }

                // Basic info
                Dictionary<string, object> dictionary = Json.Deserialize(resultString) as Dictionary<string, object>;
                if (dictionary != null)
                {
                    if (dictionary.ContainsKey("res"))
                    {
                        Dictionary<string, object> resDictionary = (Dictionary<string, object>)dictionary["res"];
                        if (resDictionary != null)
                        {
                            if (resDictionary.ContainsKey("s_country_code"))
                            {
                                privacyResponse.CountryCode = resDictionary["s_country_code"] as string;
                            }

                            if (resDictionary.ContainsKey("s_region"))
                            {
                                privacyResponse.Region = resDictionary["s_region"] as string;
                            }

                            if (resDictionary.ContainsKey("s_law_name"))
                            {
                                privacyResponse.LawName = resDictionary["s_law_name"] as string;
                            }

                            if (resDictionary.ContainsKey("b_protected"))
                            {
                                    bool.TryParse(resDictionary["b_protected"].ToString(), out privacyResponse.Protected);
                            }
                        }
                    }
                }

                return privacyResponse;
            }
        }

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
        /// Reads the tracking data from Streaming Assets config file
        /// </summary>
        /// <returns></returns>
        private static Dictionary<string, object> ReadTrackingData()
        {
#if UNITY_EDITOR
            if (!File.Exists(RemoteConfigurationConstants.TRACKING_FILE))
            {
                return null;
            }
#endif

            string trackingData = FileUtilities.ReadAllText(RemoteConfigurationConstants.TRACKING_FILE);
            return Json.Deserialize(trackingData) as Dictionary<string, object>;
        }
    }
}

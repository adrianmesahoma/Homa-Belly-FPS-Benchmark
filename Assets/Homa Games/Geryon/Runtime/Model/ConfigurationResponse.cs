using System.Collections.Generic;
using HomaGames.Geryon.MiniJSON;

namespace HomaGames.Geryon
{
    /// <summary>
    /// Model class holding server response for all Geryon requests.
    /// Some of this properties might be empty depending on the
    /// request (hgappbase, hgappfirsttime or hgappeerytime)
    ///
    /// This model represents the content of the "res" object within
    /// the server response
    /// </summary>
    public class ConfigurationResponse
    {
        private const string RESULT_FIELD = "res";
        private const string S_SCOPE_ID = "s_scope_id";
        private const string S_VARIANT_ID = "s_variant_id";
        private const string S_OVERRIDE_ID = "s_override_id";
        private const string O_CONF = "o_conf";
        private const string S_TOKEN_0 = "s_token_0";
        private const string S_TOKEN_1 = "s_token_1";
        private const string S_TOKEN_2 = "s_token_2";
        private const string S_TOKEN_3 = "s_token_3";
        private const string S_TOKEN_4 = "s_token_4";

        //[JsonProperty("s_scope_id")]
        public string ScopeId { get; private set; }
        //[JsonProperty("s_variant_id")]
        public string VariantId { get; private set; }
        //[JsonProperty("s_override_id")]
        public string OverrideId { get; private set; }
        //[JsonProperty("o_conf")]
        public Dictionary<string, object> Configuration { get; private set; }

        #region External tokens
        //[JsonProperty("s_token_0")]
        public string ExternalToken0 { get; private set; }
        //[JsonProperty("s_token_1")]
        public string ExternalToken1 { get; private set; }
        //[JsonProperty("s_token_2")]
        public string ExternalToken2 { get; private set; }
        //[JsonProperty("s_token_3")]
        public string ExternalToken3 { get; private set; }
        //[JsonProperty("s_token_4")]
        public string ExternalToken4 { get; private set; }
        #endregion

        public static ConfigurationResponse FromServerResponse(string serverResponse)
        {
            ConfigurationResponse configurationResponse = new ConfigurationResponse();
            if (!string.IsNullOrEmpty(serverResponse))
            {
                Dictionary<string, object> deserializedJson = (Dictionary<string, object>)Json.Deserialize(serverResponse);
                if (deserializedJson != null && deserializedJson.ContainsKey(RESULT_FIELD))
                {
                    Dictionary<string, object> resJson = (Dictionary<string, object>)deserializedJson[RESULT_FIELD];
                    if (resJson != null && resJson.ContainsKey(O_CONF))
                    {
                        // Obtain IDs
                        if (resJson.ContainsKey(S_SCOPE_ID))
                        {
                            configurationResponse.ScopeId = resJson[S_SCOPE_ID].ToString();
                        }

                        if (resJson.ContainsKey(S_VARIANT_ID))
                        {
                            configurationResponse.VariantId = resJson[S_VARIANT_ID].ToString();
                        }

                        if (resJson.ContainsKey(S_OVERRIDE_ID))
                        {
                            configurationResponse.OverrideId = resJson[S_OVERRIDE_ID].ToString();
                        }

                        // External tokens
                        if (resJson.ContainsKey(S_TOKEN_0) && resJson[S_TOKEN_0] != null)
                        {
                            configurationResponse.ExternalToken0 = resJson[S_TOKEN_0].ToString();
                        }

                        if (resJson.ContainsKey(S_TOKEN_1) && resJson[S_TOKEN_1] != null)
                        {
                            configurationResponse.ExternalToken1 = resJson[S_TOKEN_1].ToString();
                        }

                        if (resJson.ContainsKey(S_TOKEN_2) && resJson[S_TOKEN_2] != null)
                        {
                            configurationResponse.ExternalToken2 = resJson[S_TOKEN_2].ToString();
                        }

                        if (resJson.ContainsKey(S_TOKEN_3) && resJson[S_TOKEN_3] != null)
                        {
                            configurationResponse.ExternalToken3 = resJson[S_TOKEN_3].ToString();
                        }

                        if (resJson.ContainsKey(S_TOKEN_4) && resJson[S_TOKEN_4] != null)
                        {
                            configurationResponse.ExternalToken4 = resJson[S_TOKEN_4].ToString();
                        }

                        // Obtain configuration object
                        configurationResponse.Configuration = (Dictionary<string, object>)resJson[O_CONF];
                    }
                    else
                    {
                        HomaGamesLog.ErrorFormat("Result field \"" + O_CONF + "\" could not be found in the JSON string. JSON string given : {0}", serverResponse);
                    }
                }
                else
                {
                    HomaGamesLog.ErrorFormat("Result field \"" + RESULT_FIELD + "\" could not be found in the JSON string. JSON string given : {0}", serverResponse);
                }
            }

            return configurationResponse;
        }
    }
}
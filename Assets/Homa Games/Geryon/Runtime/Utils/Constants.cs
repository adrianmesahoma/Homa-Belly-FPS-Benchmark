using System;
using System.Collections.Generic;
using HomaGames.Geryon.MiniJSON;
using UnityEngine;
using UnityEngine.Networking;

namespace HomaGames.Geryon {

	public static class Constants {

        #region Product definition

        public const string PRODUCT_NAME = "Homa Games";
		public const string SDK_NAME = "N-Testing";
		public const string VERSION = "3.0.1";

        #endregion

        #region Resources

        public const string SETTINGS_FILENAME = "Settings";

		public const string SETTINGS_MENU_ITEM_PATH = "Window/Homa Games/" + SDK_NAME;
		public const string SETTINGS_ASSET = SETTINGS_FILENAME + ".asset";
		public const string HOMA_GAMES_RESOURCES_DIRECTORY = PRODUCT_NAME + "/Geryon/Resources";
		public const string SETTINGS_ASSET_RELATIVE_PATH = PRODUCT_NAME + "/Geryon/Resources/" + SETTINGS_FILENAME + ".asset";
		public const string ABSOLUTE_SETTINGS_PATH = "Assets/" + PRODUCT_NAME + "/Geryon/Resources/" + SETTINGS_FILENAME + ".asset";

		public const string HOMA_GAMES_SCRIPTS_DIR_PATH = PRODUCT_NAME + "/Geryon/Runtime";
		public const string DVR_FILENAME = "DVR.cs";
		public const string ABSOLUTE_DVR_PATH = HOMA_GAMES_SCRIPTS_DIR_PATH + "/" + DVR_FILENAME;
		public const string ABSOLUTE_DVR_IMPL_PATH = HOMA_GAMES_SCRIPTS_DIR_PATH + "/DVRImpl.cs";

        #endregion

        #region Type Prefixes

        public const string FLOAT_FLAG = "F_";
		public const string BOOL_FLAG = "B_";
		public const string INT_FLAG = "I_";
		public const string STRING_FLAG = "S_";

		#endregion

		#region API Constants

#if STAGE_ENV
		public const string HOST_ADDRESS = "geryon-engine.oh.stage.homagames.com";
#else
		public const string HOST_ADDRESS = "geryon-engine.homagames.com";
#endif
		private const string APP_BASE_REQUEST = "hgappbase";
		private const string FIRST_TIME_APP_OPEN_REQUEST = "hgappfirsttime";
		private const string EVERY_TIME_APP_OPEN_REQUEST = "hgappeverytime";

		public const string CONF_VERSION_VALUE = "V3";
		public const string CONF_VERSION_FIELD = "cv";
		public const string SDK_VERSION_FIELD = "sv";
		public const string APPLICATION_ID_FIELD = "ai";
		public const string APPLICATION_VERSION_FIELD = "av";
		public const string DEVICE_ID_FIELD = "di";
		public const string USER_AGENT_FIELD = "ua";

		public static string APP_BASE_URL = "https://" + HOST_ADDRESS + "/" + APP_BASE_REQUEST + "?"
			+ BuildCommonRequestParameters()
			+ "&" + USER_AGENT_FIELD + "={1}";

		public static string FIRST_TIME_APP_OPEN_URL = "https://" + HOST_ADDRESS + "/" + FIRST_TIME_APP_OPEN_REQUEST + "?"
			+ BuildCommonRequestParameters() + "&"
			+ USER_AGENT_FIELD + "=" + GetUserAgent() + "&"
			+ DEVICE_ID_FIELD + "={1}"
			+ "&dbg=" + BuildDebugParameter();

		public static string EVERYTIME_TIME_APP_OPEN_URL = "https://" + HOST_ADDRESS + "/" + EVERY_TIME_APP_OPEN_REQUEST + "?"
			+ BuildCommonRequestParameters() + "&"
			+ USER_AGENT_FIELD + "=" + GetUserAgent() + "&"
			+ DEVICE_ID_FIELD + "={1}"
			+ "&dbg=" + BuildDebugParameter();

		public static Dictionary<long, string> ErrorMessages = new Dictionary<long, string>() {
			    { 400, "App ID not provided or is wrong. You may be ok with that as long as you don't publish for this platform." },
			    { 404, "Provided App ID not found. Please double check the App ID for this platform or contact your Publish Manager" }
			};

		private static string BuildCommonRequestParameters()
		{
			return CONF_VERSION_FIELD + "=" + CONF_VERSION_VALUE
				+ "&" + APPLICATION_ID_FIELD + "={0}&"
				+ APPLICATION_VERSION_FIELD + "=" + Application.version + "&"
				+ SDK_VERSION_FIELD + "=" + VERSION;
		}

		#endregion

		/// <summary>
		/// Builds and returns the "dbg" parameter to be appended
		/// to any request
		/// </summary>
		public static string BuildDebugParameter()
		{
			Dictionary<string, object> debugJson = new Dictionary<string, object>();

			string serializedDebugJson = Json.Serialize(debugJson);
			byte[] plainTextBytes = System.Text.Encoding.UTF8.GetBytes(serializedDebugJson);

			// Base 64 encode
			string debugString = System.Convert.ToBase64String(plainTextBytes);

			// Escape Base 64
			debugString = UnityWebRequest.EscapeURL(debugString);

			return debugString;
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

	}

}

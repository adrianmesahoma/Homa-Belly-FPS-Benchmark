using System;
using UnityEngine;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace HomaGames.Geryon
{
    public static class Config
    {
        private const string FIRST_TIME_SAVE_ID = Constants.PRODUCT_NAME + "_FIRST_TIME";
        private const string SCOPE_ID_PREF_KEY = Constants.PRODUCT_NAME + "_SCOPE_ID";
        private const string VARIANT_ID_PREF_KEY = Constants.PRODUCT_NAME + "_VARIANT_ID";
        private const string DEFAULT_ID_VALUE = "000";

        private static HttpCaller httpCaller = new HttpCaller();
        private static string advertisingID;
        private static bool firstTimeRun = true;

        private static event Action _onInitialized;
        public static event Action onInitialized
        {
            add
            {
                // If Geryon is already initialized when setting 
                // the callback, invoke it directly
                if (initialized && value != null)
                {
                    value.Invoke();
                }
                else if (_onInitialized == null || !_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized += value;
                }
            }

            remove
            {
                if (_onInitialized.GetInvocationList().Contains(value))
                {
                    _onInitialized -= value;
                }
            }
        }

        internal static bool initialized = false;
        /// <summary>
        /// Determines if Geryon is initialized
        /// </summary>
        public static bool Initialized
        {
            get { return initialized; }
        }

        private static bool advertisingIdFetched = false;
        public static bool AdvertisingIdFetched
        {
            get { return advertisingIdFetched; }
        }

        /// <summary>
        /// <para>
        /// This is the Homa Games Testing ID assigned to the
        /// game run. This value needs to be informed to any
        /// attribution platform integrated within the game.
        /// </para>
        /// </summary>
        [PreserveAttribute]
        public static string NTESTING_ID
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning("Reading NTESTING_ID before fully initialized. No proper value is guaranteed. Please wait for onInitialized");
                }

                return scopeId + overrideId + variantId;
            }
        }

        private static string scopeId = DEFAULT_ID_VALUE;
        public static string ScopeId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning("Reading ScopeId before fully initialized. No proper value is guaranteed. Please wait for onInitialized");
                }

                return scopeId;
            }
        }

        private static string variantId = DEFAULT_ID_VALUE;
        public static string VariantId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning("Reading VariantId before fully initialized. No proper value is guaranteed. Please wait for onInitialized");
                }

                return variantId;
            }
        }

        private static string overrideId = DEFAULT_ID_VALUE;
        public static string OverrideId
        {
            get
            {
                if (!initialized)
                {
                    HomaGamesLog.Warning("Reading OverrideId before fully initialized. No proper value is guaranteed. Please wait for onInitialized");
                }

                return overrideId;
            }
        }

        #region External tokens

        public static string ExternalToken0 { get; private set; }
        public static string ExternalToken1 { get; private set; }
        public static string ExternalToken2 { get; private set; }
        public static string ExternalToken3 { get; private set; }
        public static string ExternalToken4 { get; private set; }

        #endregion

        private static Settings settings;

#if UNITY_2019_3_OR_NEWER
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
#else
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
        private static void OnGameLaunch()
        {
            // SetUp Persistence on Main Unity Thread
            Persistence.SetUp();

            // Load any previous saved values. This needs to be done before
            // blocking Unity Main Thread in order to access PlayerPrefs
            firstTimeRun = PlayerPrefs.GetInt(FIRST_TIME_SAVE_ID, 0) == 0;
            scopeId = PlayerPrefs.GetString(SCOPE_ID_PREF_KEY, DEFAULT_ID_VALUE);
            variantId = PlayerPrefs.GetString(VARIANT_ID_PREF_KEY, DEFAULT_ID_VALUE);

            ConfigureGeryon();
        }

        /// <summary>
        /// Start Geryon configuration
        /// </summary>
        private static void ConfigureGeryon()
        {
            /*
             * Request Advertising Identifier prior blocking the Main Thread because
             * the callback needs to happen in that thread. After fetching the advertising
             * identifier, we block the thread (if necessary) until initialization.
             * 
             * Usually fetching the identifier takes up to 100ms
             */
            if (!Application.RequestAdvertisingIdentifierAsync(OnAdvertisingIDReceived))
            {
                // For testing on the computer
#if UNITY_IOS
				advertisingID = "00000000-0000-0000-0000-000000000000";
#else
                advertisingID = "00000000-0000-0000-0000-000000000000";
#endif

                ContinueAfterAdvertisingIdFetch();
            }
        }

        /// <summary>
        /// Once advertising ID is fetched, continue
        /// </summary>
        private static void ContinueAfterAdvertisingIdFetch()
        {
            advertisingIdFetched = true;

            // Load settings and configure
            settings = Resources.Load<Settings>(Constants.SETTINGS_FILENAME);
            if (settings == null)
            {

#if UNITY_EDITOR
                HomaGamesLog.ErrorFormat("{0} settings file not found ! Please create a new settings file and follow the instructions provided with the package.", Constants.PRODUCT_NAME);
#endif
                NotifyInitializationCompleted();
                return;
            }

            // Configure Debug from Settings
            HomaGamesLog.debugEnabled = settings.IsDebugEnabled();

            // Execute the API requests even if advertising id is not available yet
            ExecuteFetchRequests();
        }

#region Private helpers

        /// <summary>
        /// Executes FIRST_TIME_APP_OPEN_URL and EVERYTIME_TIME_APP_OPEN_URL
        /// requests.
        /// </summary>
        private static void ExecuteFetchRequests()
        {
            if (string.IsNullOrEmpty(GetAppId()))
            {
                HomaGamesLog.Warning("Could not request remote configuration for empty App Id");
            }
            else
            {
                // If never triggered first time request before, trigger it
                if (firstTimeRun)
                {
                    httpCaller.Get(GetFirstTimeAppOpenUrl())
                        .ContinueWith((result) =>
                        {
                            if (!string.IsNullOrEmpty(result.Result))
                            {
                                try
                                {
                                    ConfigurationResponse firstTimeConfigurationResponse = ConfigurationResponse.FromServerResponse(result.Result);
                                    if (firstTimeConfigurationResponse != null)
                                    {
                                        // Persist First Time App Open and update variables in memory.
                                        // Persisted values will be load in subsequent game runs
                                        UpdateExternalTokens(firstTimeConfigurationResponse);
                                        Persistence.PersistFirstTimeConfiguration(firstTimeConfigurationResponse);
                                        JSONUtils.UpdateDynamicVariables(firstTimeConfigurationResponse.Configuration);
                                        scopeId = firstTimeConfigurationResponse.ScopeId;
                                        variantId = firstTimeConfigurationResponse.VariantId;
                                    }
                                    else
                                    {
                                        HomaGamesLog.Warning("First time response is null");
                                    }
                                }
                                catch (Exception e)
                                {
                                    HomaGamesLog.ErrorFormat("Exception while first time app open handling: {0}", e.Message);
                                }
                            }
                        })
                        .ContinueWith((result) =>
                        {
                            // Here a `synchronous` GET request is done because execution is already in
                            // another thread (continuating previous request)
                            string everyTimeResult = httpCaller.GetSynchronous(GetEveryTimeAppOpenUrl());
                            if (!string.IsNullOrEmpty(everyTimeResult))
                            {
                                try
                                {
                                    ConfigurationResponse everyTimeConfigurationResponse = ConfigurationResponse.FromServerResponse(everyTimeResult);
                                    if (everyTimeConfigurationResponse != null)
                                    {
                                        // Update external tokens with every time app open response
                                        UpdateExternalTokens(everyTimeConfigurationResponse);

                                        overrideId = everyTimeConfigurationResponse.OverrideId;
                                        JSONUtils.UpdateDynamicVariables(everyTimeConfigurationResponse.Configuration);
                                    }
                                    else
                                    {
                                        HomaGamesLog.Warning("Every time response is null");
                                    }
                                }
                                catch (Exception e)
                                {
                                    HomaGamesLog.ErrorFormat("Exception while every time app open handling: {0}", e.Message);
                                }
                            }
                        })
                        .ContinueWith((unused) =>
                        {
                            // Flag Geryon initialized within an asynchronous thread
                            initialized = true;
                        })
                        .ContinueWith((unused2) =>
                        {
                            // Persist and notify initialization completed in Main Thread
                            PersistFirstTimeResponseIDs();
                            NotifyInitializationCompleted();
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    httpCaller.Get(GetEveryTimeAppOpenUrl())
                        .ContinueWith((everyTimeResult) =>
                        {
                            // On the second or more run, first time app open configuration needs to be restored from persistence
                            LoadExternalTokensFromPersistence();
                            JSONUtils.UpdateDynamicVariables(Persistence.LoadFirstTimeConfigurationFromPersistence());

                            // Deserialize and configure Every Time App Open response
                            if (!string.IsNullOrEmpty(everyTimeResult.Result))
                            {
                                try
                                {
                                    ConfigurationResponse everyTimeConfigurationResponse = ConfigurationResponse.FromServerResponse(everyTimeResult.Result);
                                    if (everyTimeConfigurationResponse != null)
                                    {
                                        // Update external tokens with every time app open response
                                        UpdateExternalTokens(everyTimeConfigurationResponse);

                                        overrideId = everyTimeConfigurationResponse.OverrideId;
                                        JSONUtils.UpdateDynamicVariables(everyTimeConfigurationResponse.Configuration);
                                    }
                                    else
                                    {
                                        HomaGamesLog.Warning("Every time response is null");
                                    }
                                }
                                catch (Exception e)
                                {
                                    HomaGamesLog.ErrorFormat("Exception while every time app open handling: {0}", e.Message);
                                }
                            }
                        })
                        .ContinueWith((unused) =>
                        {
                            // Flag Geryon initialized within an asynchronous thread
                            initialized = true;
                        })
                        .ContinueWith((unused2) =>
                        {
                            // Notify initialization completed in Main Thread
                            NotifyInitializationCompleted();
                        }, TaskScheduler.FromCurrentSynchronizationContext());
                }
            }
        }

        /// <summary>
        /// Obtains the formated First Time App Open URL to be invoked
        /// </summary>
        /// <returns></returns>
        private static string GetFirstTimeAppOpenUrl()
        {
            return string.Format(Constants.FIRST_TIME_APP_OPEN_URL, GetAppId(), advertisingID);
        }

        /// <summary>
        /// Obtains the formatted Every Time App Open URL to be invoked
        /// </summary>
        /// <returns></returns>
        private static string GetEveryTimeAppOpenUrl()
        {
            return string.Format(Constants.EVERYTIME_TIME_APP_OPEN_URL, GetAppId(), advertisingID);
        }

        /// <summary>
        /// Loads persisted external tokens from disk (if any)
        /// </summary>
        private static void LoadExternalTokensFromPersistence()
        {
            string[] externalTokens = Persistence.LoadFirstTimeExternalTokensFromPersistence();

            // Security check: by design, we only expose 5 fixed external tokens
            if (externalTokens != null && externalTokens.Length == 5)
            {
                ExternalToken0 = externalTokens[0];
                ExternalToken1 = externalTokens[1];
                ExternalToken2 = externalTokens[2];
                ExternalToken3 = externalTokens[3];
                ExternalToken4 = externalTokens[4];
            }
        }

        /// <summary>
        /// Overrides external tokens with the provided ConfigurationResponse
        /// </summary>
        /// <param name="configurationResponse">The ConfigurationResponse with the new external tokens</param>
        private static void UpdateExternalTokens(ConfigurationResponse configurationResponse)
        {
            if (configurationResponse == null)
            {
                return;
            }

            /**
             *  Only update those External Tokens with some string value
             **/
            if (!string.IsNullOrEmpty(configurationResponse.ExternalToken0))
            {
                ExternalToken0 = configurationResponse.ExternalToken0;
            }

            if (!string.IsNullOrEmpty(configurationResponse.ExternalToken1))
            {
                ExternalToken1 = configurationResponse.ExternalToken1;
            }

            if (!string.IsNullOrEmpty(configurationResponse.ExternalToken2))
            {
                ExternalToken2 = configurationResponse.ExternalToken2;
            }

            if (!string.IsNullOrEmpty(configurationResponse.ExternalToken3))
            {
                ExternalToken3 = configurationResponse.ExternalToken3;
            }

            if (!string.IsNullOrEmpty(configurationResponse.ExternalToken4))
            {
                ExternalToken4 = configurationResponse.ExternalToken4;
            }
        }

        /// <summary>
        /// Persists first time app open ids for further game runs.
        /// This method needs to run in Unity Main Thread as it uses PlayerPrefs
        /// </summary>
        private static void PersistFirstTimeResponseIDs()
        {
            PlayerPrefs.SetInt(FIRST_TIME_SAVE_ID, 1);
            PlayerPrefs.SetString(SCOPE_ID_PREF_KEY, scopeId);
            PlayerPrefs.SetString(VARIANT_ID_PREF_KEY, variantId);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Mark Geryon as initialized and invoke any registered callback
        /// </summary>
        private static void NotifyInitializationCompleted()
        {
            HomaGamesLog.Debug("Initialization completed");
            if (_onInitialized != null)
            {
                _onInitialized.Invoke();
            }
        }

        /// <summary>
        /// Obtains the App ID from Settings
        /// </summary>
        /// <returns></returns>
        private static string GetAppId()
        {
            string appID = settings.GetAndroidID();
#if UNITY_IOS
			appID = settings.GetIOSID();
#endif
            return appID;
        }

#endregion

        private static void OnAdvertisingIDReceived(string adID, bool trackingEnabled, string msgError)
        {
            if (adID == null)
            {
                advertisingID = "ERROR_ID";
            }
            else
            {
                advertisingID = adID;
            }

            ContinueAfterAdvertisingIdFetch();
        }
    }
}
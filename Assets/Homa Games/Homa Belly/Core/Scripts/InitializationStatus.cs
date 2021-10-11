using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Component holding Homa Belly's initialization status at every time.
    /// This status can be accessed through the property InitializationStatus#IsInitialized
    /// or registering Events#onInitialized
    /// </summary>
    public class InitializationStatus
    {
        #region Private properties
        /// <summary>
        /// Gift INITIALIZATION_GRACE_PERIOD_MS for Homa Belly initialization.
        /// If time is elapsed, #OnInitialized is invoked to avoid any possible
        /// issue block Homa Belly usage
        /// </summary>
        private const int INITIALIZATION_GRACE_PERIOD_MS = 5000;
        private readonly object initializationLock = new object();
        private int totalComponentsToInitialize = 0;
        private int initializedComponents = 0;
        private bool initialized = false;
        private Events events = new Events();
        #endregion

        #region Public properties

        /// <summary>
        /// Determines is Homa Belly is initialized
        /// </summary>
        public bool IsInitialized
        {
            get
            {
                return initialized;
            }
        }
        #endregion

        public InitializationStatus()
        {
            List<Type> availableMediators = Reflection.GetHomaBellyAvailableClasses(typeof(IMediator));
            List<Type> availableAttributions = Reflection.GetHomaBellyAvailableClasses(typeof(IAttribution));
            List<Type> availableAnalytics = Reflection.GetHomaBellyAvailableClasses(typeof(IAnalytics));

            // Obtain total components to wait for initialization
            totalComponentsToInitialize += availableMediators != null ? availableMediators.Count : 0;
            totalComponentsToInitialize += availableAttributions != null ? availableAttributions.Count : 0;
            totalComponentsToInitialize += availableAnalytics != null ? availableAnalytics.Count : 0;
        }

        #region Public methods

        /// <summary>
        /// Starts the timer for an initialization grace period. This will allow
        /// backwards compatibility for those components not implementing
        /// yet the initialization status callback
        /// </summary>
        public void StartInitializationGracePeriod()
        {
            Task.Delay(INITIALIZATION_GRACE_PERIOD_MS).ContinueWith((result) =>
            {
                // If Homa Belly is not initialized after INITIALIZATION_GRACE_PERIOD_MS, move forward
                if (!initialized)
                {
                    HomaGamesLog.Warning($"[InitializationStatus] Forcing initialization completed after grace period");
                    initialized = true;
                    events.OnInitialized();
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Action invoked after each component is initialized
        /// </summary>
        public void OnInnerComponentInitialized()
        {
            lock (initializationLock)
            {
                initializedComponents++;
                HomaGamesLog.Debug($"[InitializationStatus] Component initialized. Total: {initializedComponents}");
            }

            if (initializedComponents >= totalComponentsToInitialize)
            {
                // Homa Belly initialization completed
                HomaGamesLog.Debug($"[InitializationStatus] Initialization completed");
                initialized = true;
                events.OnInitialized();
            }
        }

        #endregion
    }
}

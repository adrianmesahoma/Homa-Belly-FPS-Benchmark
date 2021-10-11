using Firebase.Extensions;

namespace HomaGames.HomaBelly
{
    /// <summary>
    /// Helper class to enable Firebase Push Notifications feature
    /// </summary>
    public class FirebasePushNotifications
    {
        /// <summary>
        /// Whenever you need to enable Push Notifications, just call this
        /// method in order to have Firebase setup for that feature.
        ///
        /// This method will show push notifications permissions popup
        /// asking the user to allow the feature. Until this method is invoked,
        /// no permission popup will be shown
        /// </summary>
        public static void EnablePushNotifications()
        {
#if HOMA_PUSH_NOTIFICATIONS

#if UNITY_ANDROID
            Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
                var dependencyStatus = task.Result;
                if (dependencyStatus == Firebase.DependencyStatus.Available)
                {
                    Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
                    Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
                }
            });
#else
            Firebase.Messaging.FirebaseMessaging.TokenReceived += OnTokenReceived;
            Firebase.Messaging.FirebaseMessaging.MessageReceived += OnMessageReceived;
#endif
#else
            HomaGamesLog.Warning("Firebase Push Notifications not enabled on Homa Belly manifest. Please ask your Publish Manager");
#endif
        }

#region Push notifications

        public static void OnTokenReceived(object sender, Firebase.Messaging.TokenReceivedEventArgs token)
        {
            HomaGamesLog.Debug("Received Registration Token: " + token.Token);
        }

        public static void OnMessageReceived(object sender, Firebase.Messaging.MessageReceivedEventArgs e)
        {
            HomaGamesLog.Debug("Received a new message from: " + e.Message.From);
        }

#endregion
    }
}
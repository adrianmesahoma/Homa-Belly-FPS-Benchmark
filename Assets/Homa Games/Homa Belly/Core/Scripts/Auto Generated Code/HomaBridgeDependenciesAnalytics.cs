namespace HomaGames.HomaBelly
{
	 public partial class HomaBridgeDependencies
	 {
	 	 static partial void PartialInitializeAnalytics()
	 	 {
			 analytics.Add(new FacebookImplementation());
			 analytics.Add(new GameAnalyticsImplementation());
			 analytics.Add(new FirebaseAnalyticsImpl());
	 	 }
	 }
}

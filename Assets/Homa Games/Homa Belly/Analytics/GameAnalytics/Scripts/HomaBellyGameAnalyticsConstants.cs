using UnityEngine;

namespace HomaGames.HomaBelly
{
    public static class HomaBellyGameAnalyticsConstants
    {
        public const string ID = "gameanalytics";
        public const PackageType TYPE = PackageType.ANALYTICS_SYSTEM;
        public static string CONFIG_FILE = Application.streamingAssetsPath + "/Homa Games/Homa Belly/Analytics/GameAnalytics/config.json";
        public const string CREATE_GAME_ANALYTICS_OBJECT_MENU = "Window/GameAnalytics/Create GameAnalytics Object";
    }
}

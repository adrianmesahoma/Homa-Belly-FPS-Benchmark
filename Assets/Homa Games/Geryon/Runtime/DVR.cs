using HomaGames;
using HomaGames.Geryon;

public static class DVR {
	
	#if UNITY_IOS
	
	// REGEX_IOS_MARKER
	
	#else
	
	public static long IS_MIN_LEVEL = DynamicVariable<long>.Get("I_IS_MIN_LEVEL", 2);
	public static string DISCORD_LINK = DynamicVariable<string>.Get("S_DISCORD_LINK", "https://discord.com/invite/7RXwaSDYkM");
	public static long KEY_MIN_LEVEL = DynamicVariable<long>.Get("I_KEY_MIN_LEVEL", 2);
	public static long AD_IS_INTERVAL = DynamicVariable<long>.Get("I_AD_IS_INTERVAL", 30);
	public static long AD_RV_INTERVAL = DynamicVariable<long>.Get("I_AD_RV_INTERVAL", 30);
	public static long BOSS_LEVEL_INDEX = DynamicVariable<long>.Get("I_BOSS_LEVEL_INDEX", 6);
	public static long RATING_MIN_LEVEL = DynamicVariable<long>.Get("I_RATING_MIN_LEVEL", 5);
	public static bool REMOVE_BOSS_LEVEL = DynamicVariable<bool>.Get("B_REMOVE_BOSS_LEVEL", false);
	public static double PLAYER_MAX_HEIGHT = DynamicVariable<double>.Get("F_PLAYER_MAX_HEIGHT", 50);
	public static long BONUS_LEVEL_INDEX = DynamicVariable<long>.Get("I_BONUS_LEVEL_INDEX", 3);
	public static long DISCORD_MIN_LEVEL = DynamicVariable<long>.Get("I_DISCORD_MIN_LEVEL", 5);
	public static long GIANTS_BASE_HEALTH = DynamicVariable<long>.Get("I_GIANTS_BASE_HEALTH", 4);
	public static bool END_IS_ON_NEXT_BUTTON = DynamicVariable<bool>.Get("B_END_IS_ON_NEXT_BUTTON", false);
	public static long RATING_LEVEL_INTERVAL = DynamicVariable<long>.Get("I_RATING_LEVEL_INTERVAL", 10);
	public static long DISCORD_LEVEL_INTERVAL = DynamicVariable<long>.Get("I_DISCORD_LEVEL_INTERVAL", 10);
	public static long INCREMENTAL_DAMAGE_ADD = DynamicVariable<long>.Get("I_INCREMENTAL_DAMAGE_ADD", 10);
	public static long KEY_CHESTROOM_INTERVAL = DynamicVariable<long>.Get("I_KEY_CHESTROOM_INTERVAL", 1);
	public static long AD_IS_INTERVAL_LVL_1_10 = DynamicVariable<long>.Get("I_AD_IS_INTERVAL_LVL_1_10", 30);
	public static long LOCAL_NOTIFICATION_TYPE = DynamicVariable<long>.Get("I_LOCAL_NOTIFICATION_TYPE", 1);
	public static string LOCAL_NOTIFICATION_TEXT = DynamicVariable<string>.Get("S_LOCAL_NOTIFICATION_TEXT", "Want to slice some giants? Go Hunt!");
	public static long AD_IS_INTERVAL_LVL_11_20 = DynamicVariable<long>.Get("I_AD_IS_INTERVAL_LVL_11_20", 20);
	public static long DYNAMIC_MULTIPLIER_ITEMS = DynamicVariable<long>.Get("I_DYNAMIC_MULTIPLIER_ITEMS", 50);
	public static long INCREMENTAL_DAMAGE_PRICE = DynamicVariable<long>.Get("I_INCREMENTAL_DAMAGE_PRICE", 10);
	public static long SHOP_RV_MORE_GEMS_AMOUNT = DynamicVariable<long>.Get("I_SHOP_RV_MORE_GEMS_AMOUNT", 100);
	public static string LOCAL_NOTIFICATION_TITLE = DynamicVariable<string>.Get("S_LOCAL_NOTIFICATION_TITLE", "Attack On Giants");
	public static bool ENABLE_LOCAL_NOTIFICATION = DynamicVariable<bool>.Get("B_ENABLE_LOCAL_NOTIFICATION", false);
	public static long IDLE_INCOME_RV_MULTIPLIER = DynamicVariable<long>.Get("I_IDLE_INCOME_RV_MULTIPLIER", 2);
	public static long COINS_MULTIPLIERS_INTERVAL = DynamicVariable<long>.Get("I_COINS_MULTIPLIERS_INTERVAL", 4);
	public static long INCREMENTAL_IDLE_INCOME_ADD = DynamicVariable<long>.Get("I_INCREMENTAL_IDLE_INCOME_ADD", 2);
	public static double GREEN_CIRCLES_SPEED_INCREASE = DynamicVariable<double>.Get("F_GREEN_CIRCLES_SPEED_INCREASE", 100);
	public static double INCREMENTAL_PRICE_MULTIPLIER = DynamicVariable<double>.Get("F_INCREMENTAL_PRICE_MULTIPLIER", 5);
	public static double INCREMENTAL_DAMAGE_MULTIPLIER = DynamicVariable<double>.Get("F_INCREMENTAL_DAMAGE_MULTIPLIER", 10);
	public static long INCREMENTAL_DAMAGE_BASE_VALUE = DynamicVariable<long>.Get("I_INCREMENTAL_DAMAGE_BASE_VALUE", 10);
	public static long INCREMENTAL_IDLE_INCOME_PRICE = DynamicVariable<long>.Get("I_INCREMENTAL_IDLE_INCOME_PRICE", 10);
	public static double INCREMENTAL_REACTION_SPEED_ADD = DynamicVariable<double>.Get("F_INCREMENTAL_REACTION_SPEED_ADD", 0.1);
	public static long AD_IS_INTERVAL_LVL_21_AND_MORE = DynamicVariable<long>.Get("I_AD_IS_INTERVAL_LVL_21_AND_MORE", 20);
	public static long POWERUP_SUPERSAIYAN_GIVE_PER_RV = DynamicVariable<long>.Get("I_POWERUP_SUPERSAIYAN_GIVE_PER_RV", 2);
	public static long SHOP_ROPES_COMMON_INITIAL_PRICE = DynamicVariable<long>.Get("I_SHOP_ROPES_COMMON_INITIAL_PRICE", 50);
	public static long SHOP_SKINS_COMMON_INITIAL_PRICE = DynamicVariable<long>.Get("I_SHOP_SKINS_COMMON_INITIAL_PRICE", 100);
	public static long CHESTROOM_BESTPRIZE_CHANCE_NO_RV = DynamicVariable<long>.Get("I_CHESTROOM_BESTPRIZE_CHANCE_NO_RV", 0);
	public static long INCREMENTAL_REACTION_SPEED_PRICE = DynamicVariable<long>.Get("I_INCREMENTAL_REACTION_SPEED_PRICE", 10);
	public static long POWERUP_SUPERSAIYAN_UNLOCK_LEVEL = DynamicVariable<long>.Get("I_POWERUP_SUPERSAIYAN_UNLOCK_LEVEL", 8);
	public static long SHOP_ROPES_COMMON_PRICE_INCREMENT = DynamicVariable<long>.Get("I_SHOP_ROPES_COMMON_PRICE_INCREMENT", 50);
	public static long SHOP_SKINS_COMMON_PRICE_INCREMENT = DynamicVariable<long>.Get("I_SHOP_SKINS_COMMON_PRICE_INCREMENT", 100);
	public static long SHOP_WEAPONS_COMMON_INITIAL_PRICE = DynamicVariable<long>.Get("I_SHOP_WEAPONS_COMMON_INITIAL_PRICE", 100);
	public static double INCREMENTAL_IDLE_INCOME_MULTIPLIER = DynamicVariable<double>.Get("F_INCREMENTAL_IDLE_INCOME_MULTIPLIER", 2);
	public static long CHESTROOM_BESTPRIZE_CHANCE_WITH_RV = DynamicVariable<long>.Get("I_CHESTROOM_BESTPRIZE_CHANCE_WITH_RV", 50);
	public static long INCREMENTAL_IDLE_INCOME_BASE_VALUE = DynamicVariable<long>.Get("I_INCREMENTAL_IDLE_INCOME_BASE_VALUE", 10);
	public static long SHOP_WEAPONS_COMMON_PRICE_INCREMENT = DynamicVariable<long>.Get("I_SHOP_WEAPONS_COMMON_PRICE_INCREMENT", 75);
	public static double INCREMENTAL_REACTION_SPEED_BASE_VALUE = DynamicVariable<double>.Get("F_INCREMENTAL_REACTION_SPEED_BASE_VALUE", 5);
	public static double POWERUP_SUPERSAIYAN_COUNTDOWN_SECONDS = DynamicVariable<double>.Get("F_POWERUP_SUPERSAIYAN_COUNTDOWN_SECONDS", 5);
	public static long LOCAL_NOTIFICATION_MINUTES_AFTER_PAUSE_DELAY = DynamicVariable<long>.Get("I_LOCAL_NOTIFICATION_MINUTES_AFTER_PAUSE_DELAY", 1440);
	
	#endif
	
}
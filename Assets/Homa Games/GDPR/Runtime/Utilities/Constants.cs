/*
 * Licensed to the Apache Software Foundation (ASF) under one
 * or more contributor license agreements.  See the NOTICE file
 * distributed with this work for additional information
 * regarding copyright ownership.  The ASF licenses this file
 * to you under the Apache License, Version 2.0 (the
 * "License"); you may not use this file except in compliance
 * with the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing,
 * software distributed under the License is distributed on an
 * "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
 * KIND, either express or implied.  See the License for the
 * specific language governing permissions and limitations
 * under the License.
 */

using HomaGames.HomaBelly.Utilities;
using UnityEngine;

namespace HomaGames.GDPR
{
    public class Constants
    {
        public static string PRODUCT_NAME = "GDPR";
        public static string PRODUCT_VERSION = "1.0.0";

        // Assets
        public const string GDPR_SETTINGS_ASSET_PATH = "Assets/Homa Games/GDPR/Runtime/Resources/Homa Games GDPR Settings.asset";
        public const string GDPR_PREFAB_ASSET_PATH = "Assets/Homa Games/GDPR/Runtime/Resources/Homa Games GDPR.prefab";
        public const string SETTINGS_RESOURCE_NAME = "Homa Games GDPR Settings";
        public const string PREFAB_RESOURCE_NAME = "Homa Games GDPR";

        // Colors
        public const string TOGGLE_OFF_COLOR = "#D8D8D8";
        public const string FONT_COLOR = "#4A4F58";
        public const string SECONDARY_FONT_COLOR = "#31D2A4";
        public const string BUTTON_FONT_COLOR = "#FFFFFF";
        public const string BACKGROUND_COLOR = "#FFFFFF";
        public const string TOGGLE_COLOR = "#47D7AC";

        // URL for privacy region detection
        public static string API_PRIVACY_REGION_URL = HomaBellyConstants.API_HOST + "/privacy?cv=" + HomaBellyConstants.API_VERSION
            + "&sv=" + PRODUCT_VERSION
            + "&av=" + Application.version
            + "&ti={0}"
            + "&ai=" + Application.identifier
            + "&ua={1}";

        public class PersistenceKey
        {
            public const string ABOVE_REQUIRED_AGE = "homagames.gdpr.above_required_age";
            public const string TERMS_AND_CONDITIONS = "homagames.gdpr.terms_and_conditions";
            public const string ANALYTICS_TRACKING = "homagames.gdpr.analytics_tracking";
            public const string TAILORED_ADS = "homagames.gdpr.tailored_ads";
            public const string IOS_ADS_TRACKING_ASKED = "homagames.idfa.ios_ads_tracking_asked";
            public const string IOS_GLOBAL_ADS_TRACKING_SETTING = "homagames.idfa.ios_global_ads_tracking_setting";
        }
    }   
}

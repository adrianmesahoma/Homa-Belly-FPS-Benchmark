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

using UnityEngine;

namespace HomaGames.GDPR
{
    public class Settings : ScriptableObject
    {
        public static string LEGACY_DEFAULT_APPLE_MESSAGE = "This will only be used to keep the app free by serving ads. Please support us by allowing tracking. Thank you!";
        public static string DEFAULT_APPLE_MESSAGE = "We use your information in order to enhance your game experience, by serving you personalized ads and measuring the performance of our game.";

        public enum SupportedLanguages
        {
            /// <summary>
            /// English
            /// </summary>
            EN,
            /// <summary>
            /// Spanish
            /// </summary>
            ES,
            /// <summary>
            /// German
            /// </summary>
            DE,
            /// <summary>
            /// French
            /// </summary>
            FR,
            /// <summary>
            /// Italian
            /// </summary>
            IT,
            /// <summary>
            /// Catalan (Spain)
            /// </summary>
            CA
        }

        #region Settings properties
        /// <summary>
        /// The game name to be visible in the GDPR welcome page
        /// </summary>
        [Tooltip("The game name to be visible in the GDPR welcome page")]
        public string GameName;

        /// <summary>
        /// Base background color for the GDPR windows
        /// </summary>
        public Color BackgroundColor;

        /// <summary>
        /// Base font color for the GDPR text
        /// </summary>
        public Color FontColor;
        
        /// <summary>
        /// Secondary font color for the GDPR text
        /// </summary>
        public Color SecondaryFontColor;

        /// <summary>
        /// Base toggle color for the GDPR toggle when ONs and CTA button
        /// </summary>
        public Color ToggleColor;

        /// <summary>
        /// Base font color for the GDPR buttons
        /// </summary>
        public Color ButtonFontColor;

        /// <summary>
        /// Flag informing if GDPR should be displayed in device language in runtime or not
        /// </summary>
        public bool UseDeviceLanguageWhenPossible;

        /// <summary>
        /// If `UseDeviceLanguageWhenPossible` is set to false, GDPR will be always shown
        /// in this specific language
        /// </summary>
        public SupportedLanguages Language;

        /// <summary>
        /// The customizable iOS 14 IDFA native popup request message
        /// </summary>
        public string iOSIdfaPopupMessage;

        /// <summary>
        /// Locally forces the GDPR to never appear. This might be useful
        /// for very localized builds, like Chinese builds.
        /// </summary>
        public bool ForceDisableGdpr;

        #endregion

        #region Public methods

        public void ResetToDefaultValues()
        {
            GameName = Application.productName;
            ColorUtility.TryParseHtmlString(Constants.FONT_COLOR, out FontColor);
            ColorUtility.TryParseHtmlString(Constants.BACKGROUND_COLOR, out BackgroundColor);
            ColorUtility.TryParseHtmlString(Constants.SECONDARY_FONT_COLOR, out SecondaryFontColor);
            ColorUtility.TryParseHtmlString(Constants.TOGGLE_COLOR, out ToggleColor);
            ColorUtility.TryParseHtmlString(Constants.BUTTON_FONT_COLOR, out ButtonFontColor);
            Language = SupportedLanguages.EN;
            UseDeviceLanguageWhenPossible = true;
            iOSIdfaPopupMessage = DEFAULT_APPLE_MESSAGE;
            ForceDisableGdpr = false;

            // Update localization
            Localization.LoadLanguageFromSettings();
        }

        #endregion
    }
}

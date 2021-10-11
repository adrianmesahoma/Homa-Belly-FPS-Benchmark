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
using UnityEngine.UI;

namespace HomaGames.GDPR
{
    public class ToggleExtension : MonoBehaviour
    {
        [SerializeField]
        private Image backgroundImage;
        [SerializeField]
        private GameObject checkmarkOffGameObject;

        /// <summary>
        /// Toggles Checkmark Off game object active depending
        /// on toggle status. This checkmark is shown only when
        /// toggle is OFF
        /// </summary>
        /// <param name="isToggleOn"></param>
        public void ToggleCheckmarkOff(bool isToggleOn)
        {
            if (checkmarkOffGameObject != null)
            {
                checkmarkOffGameObject.SetActive(!isToggleOn);
            }

            Settings settings = (Settings)Resources.Load(Constants.SETTINGS_RESOURCE_NAME, typeof(Settings));
            if (settings != null)
            {
                Color toggleOnColor = settings.ToggleColor;
                Color toggleOffColor = Color.gray;
                ColorUtility.TryParseHtmlString(Constants.TOGGLE_OFF_COLOR, out toggleOffColor);
                if (backgroundImage != null)
                {
                    backgroundImage.color = isToggleOn ? toggleOnColor : toggleOffColor;
                }
            }
        }
    }
}

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

using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace HomaGames.HomaBelly.IAP
{
    [Serializable]
    public class Catalog : ScriptableObject
    {
        public const string CATALOG_FILE_PATH = "Assets/Homa Games/Homa Belly/Purchases/Resources/Catalog.asset";

        [Serializable]
        public class Product
        {
            public string Id;
            public ProductType Type;
            public string DefaultSku;

            // SKUs
            [SerializeField]
            private string huaweiSku;
            public string HuaweiSku
            {
                get
                {
                    if (!string.IsNullOrEmpty(huaweiSku))
                    {
                        return huaweiSku;
                    }

                    return DefaultSku;
                }
                set
                {
                    huaweiSku = value;
                }
            }

            [SerializeField]
            private string googlePlaySku;
            public string GooglePlaySku
            {
                get
                {
                    if (!string.IsNullOrEmpty(googlePlaySku))
                    {
                        return googlePlaySku;
                    }

                    return DefaultSku;
                }
                set
                {
                    googlePlaySku = value;
                }
            }

            [SerializeField]
            private string appStoreSku;
            public string AppStoreSku
            {
                get
                {
                    if (!string.IsNullOrEmpty(appStoreSku))
                    {
                        return appStoreSku;
                    }

                    return DefaultSku;
                }
                set
                {
                    appStoreSku = value;
                }
            }

            // Unity Editor helpers
            public bool Folded;
            public bool SkusFolded;
        }

        [Tooltip("Determines the store to target. DEFAULT will target Google Play Store and Apple Store respectively")]
        public Store Store;
        [Tooltip("List of IAP product IDs available to purchase")]
        public List<Product> Products;

        #region Default Store specifics
        public string DefaultApiKey;
        #endregion

        #region Huawei Store specifics
        public string AppId;
        public string CpId;
        #endregion

        public void CreateAndAddNewEmptyProduct()
        {
            Product product = new Product();
            product.Id = $"{Products.Count + 1}";
            Products.Add(product);
        }

        public void RemoveProductById(string productId)
        {
            for (int i = 0; i < Products.Count; i++)
            {
                if (Products[i].Id == productId)
                {
                    Products.Remove(Products[i]);
                    break;
                }
            }
        }

        /// <summary>
        /// PropertyAttribute class to select from a list of strings.
        /// Specifically, it allows an attribute to be selected
        /// from the list of available Products in the Catalog.
        /// </summary>
        public class ProductIdAttribute : PropertyAttribute
        {
            public delegate string[] GetStringList();

            public string[] List
            {
                get
                {
                    return HomaStore.Catalog.Products.Select(product => product.Id).ToArray();
                }
                private set { }
            }
        }

#if UNITY_EDITOR

        /// <summary>
        /// Drawer to show a dropdown with all available strings in ProductIdAttribute
        /// </summary>
        [CustomPropertyDrawer(typeof(ProductIdAttribute))]
        public class ProductIdDrawerDrawer : PropertyDrawer
        {
            // Draw the property inside the given rect
            public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
            {
                // Using BeginProperty / EndProperty on the parent property means that
                // prefab override logic works on the entire property.
                EditorGUI.BeginProperty(position, label, property);

                var stringInList = attribute as ProductIdAttribute;
                var list = stringInList.List;
                if (property.propertyType == SerializedPropertyType.String)
                {
                    int index = Mathf.Max(0, Array.IndexOf(list, property.stringValue));
                    index = EditorGUI.Popup(position, property.displayName, index, list);

                    property.stringValue = list[index];
                }
                else if (property.propertyType == SerializedPropertyType.Integer)
                {
                    property.intValue = EditorGUI.Popup(position, property.displayName, property.intValue, list);
                }
                else
                {
                    base.OnGUI(position, property, label);
                }

                EditorGUI.EndProperty();
            }
        }

        /// <summary>
        /// Editor window to show a basic popup when Catalog scriptableobject
        /// is selected. This button opens Homa Games IAP Editor Window
        /// </summary>
        [CustomEditor(typeof(Catalog))]
        public class CatalogEditor : Editor
        {
            public override void OnInspectorGUI()
            {
                // Disable Unity inspector, forcing user to open CatalogEditorWindow
                if (GUILayout.Button("Open Catalog"))
                {
                    EditorApplication.ExecuteMenuItem("Window/Homa Games/Homa Belly/Purchases");
                }
            }
        }

#endif
    }
}

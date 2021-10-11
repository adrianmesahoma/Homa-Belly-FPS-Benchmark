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

using System.IO;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.IAP
{
    public class CatalogEditorWindow : EditorWindow
    {
        private Catalog catalog;
        private SerializedObject catalogSerializedObject;
        private bool productsFoldout;

        [MenuItem("Window/Homa Games/Homa Belly/Purchases", false, 1)]
        static void CreateWindowAndFocus()
        {
            GetWindow(typeof(CatalogEditorWindow), false, "Homa Belly Purchases", true);
        }

        private void Initialize()
        {
            catalog = CatalogManager.TryCreateAndConfigureCatalog();
            catalogSerializedObject = new SerializedObject(catalog);
        }

        private void OnGUI()
        {
            Initialize();

            EditorGUI.BeginChangeCheck();

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif
            SerializedProperty store = catalogSerializedObject.FindProperty("Store");
            EditorGUILayout.PropertyField(store);

            switch(store.intValue)
            {
                case (int) Store.DEFAULT:
                    EditorGUILayout.PropertyField(catalogSerializedObject.FindProperty("DefaultApiKey"));
                    break;
                    /*
                case (int)Store.HUAWEI:
                    EditorGUILayout.PropertyField(catalogSerializedObject.FindProperty("AppId"));
                    EditorGUILayout.PropertyField(catalogSerializedObject.FindProperty("CpId"));
                    break;
                    */
            }

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif
            EditorGUILayout.BeginHorizontal();
            productsFoldout = EditorGUILayout.Foldout(productsFoldout, "Products");
            bool add = GUILayout.Button("+", EditorStyles.miniButtonRight, GUILayout.Width(20));
            if (add)
            {
                catalog.CreateAndAddNewEmptyProduct();
            }
            EditorGUILayout.EndHorizontal();

            if (productsFoldout)
            {
                EditorGUI.indentLevel++;

                SerializedProperty productsArray = catalogSerializedObject.FindProperty("Products");
                for (int i = 0; i < productsArray.arraySize; i++)
                {
                    DrawCatalogItem(productsArray.GetArrayElementAtIndex(i));
                }

                EditorGUI.indentLevel--;
            }

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(20);
#else
            EditorGUILayout.Space();
#endif
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Restore to defaults", GUILayout.Width(200), GUILayout.Height(40)))
            {
                // Reset all Catalog with Homa Belly Manifest
                bool result = EditorUtility.DisplayDialog("Restore Catalog", "Are you sure you want to restore your Catalog to Homa Belly Manifest values?", "Yes", "No");
                if (result)
                {
                    CatalogManager.RestoreCatalogFromManifest();
                    Debug.Log("Catalog restored from Homa Belly Manifest values");
                }
            }
            GUILayout.FlexibleSpace();
            EditorGUILayout.EndHorizontal();

            if (EditorGUI.EndChangeCheck())
            {
                catalogSerializedObject.ApplyModifiedProperties();
            }
        }

        private void DrawCatalogItem(SerializedProperty item)
        {
            SerializedProperty folded = item.FindPropertyRelative("Folded");
            SerializedProperty skusFolded = item.FindPropertyRelative("SkusFolded");

            EditorGUILayout.BeginHorizontal();
            folded.boolValue = EditorGUILayout.Foldout(folded.boolValue, item.FindPropertyRelative("Id").stringValue);

#if UNITY_2019_3_OR_NEWER
            EditorGUILayout.Space(10);
#else
            EditorGUILayout.Space();
#endif
            bool remove = GUILayout.Button("-", EditorStyles.miniButtonRight, GUILayout.Width(20));
            if (remove)
            {
                catalog.RemoveProductById(item.FindPropertyRelative("Id").stringValue);
            }
            EditorGUILayout.EndHorizontal();

            if (folded.boolValue)
            {
                EditorGUI.indentLevel++;

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Product ID", "Internal product ID"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("Id"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Product Type", "Type of the product"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("Type"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField(new GUIContent("Product Sku", "Default product SKU for all stores (if not overriden)"));
                EditorGUILayout.PropertyField(item.FindPropertyRelative("DefaultSku"), GUIContent.none);
                EditorGUILayout.EndHorizontal();

                skusFolded.boolValue = EditorGUILayout.Foldout(skusFolded.boolValue, new GUIContent("Override Platform SKUs", "Override if the specific store SKU is different from default one"));
                if (skusFolded.boolValue)
                {
                    EditorGUI.indentLevel++;

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Google Play", "Google Play product SKU"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("googlePlaySku"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();

                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("App Store", "App Store product SKU"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("appStoreSku"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();

                    /**
                    EditorGUILayout.BeginHorizontal();
                    EditorGUILayout.LabelField(new GUIContent("Huawei", "Huawei product SKU"));
                    EditorGUILayout.PropertyField(item.FindPropertyRelative("huaweiSku"), GUIContent.none);
                    EditorGUILayout.EndHorizontal();
                    **/

                    EditorGUI.indentLevel--;
                }
                
                EditorGUI.indentLevel--;
            }
        }
    }
}

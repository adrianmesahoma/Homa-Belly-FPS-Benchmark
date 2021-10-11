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
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using UnityEngine;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Entry point for all Homa Games IAP operations. Product
    /// fetching happens automatically upon initialization.
    ///
    /// <para>
    /// The following callbacks might be invoked during the corresponding IAP procedures:
    /// <list type="bullet">
    ///     <item>
    ///     OnInitialized
    ///     </item>
    ///     <item>
    ///     OnInitializeFailed
    ///     </item>
    ///     <item>
    ///     OnPurchaseSuccess
    ///     </item>
    ///     <item>
    ///     OnPurchaseFailed
    ///     </item>
    ///     <item>
    ///     OnProductsRetrieved
    ///     </item>
    ///     <item>
    ///     OnPurchasesRestored
    ///     </item>
    /// </list>
    /// </para>
    /// </summary>
    public static class HomaStore
    {
        private static string FIRST_TIME_PURCHASES_RESTORED = "com.homagames.homabelly.purchases.first_time_restored";

        /// <summary>
        /// Invoked upon successfully store initialization
        /// </summary>
        public static event Action OnInitialized;

        /// <summary>
        /// Invoked upon failed store initialization
        /// </summary>
        public static event Action OnInitializeFailed;

        /// <summary>
        /// Invoked upon the purchase process has been initiated
        /// </summary>
        public static event Action<Product> OnPurchaseInitiatedForProduct;

        /// <summary>
        /// Invoked upon successfully product purchase
        /// </summary>
        public static event Action<string> OnPurchaseSuccess;

        /// <summary>
        /// Invoked upon failed product purchase
        /// </summary>
        public static event Action<string> OnPurchaseFailed;

        /// <summary>
        /// Invoked when the purchase process has finished, either
        /// successfully or with failure. See #OnPurchaseSuccess and #OnPurchaseFailed
        /// </summary>
        public static event Action<Product> OnPurchaseFinishedForProduct;

        /// <summary>
        /// Invoked after store initialization and product fetching. Product
        /// fetching happens automatically upon initialization.
        /// </summary>
        public static event Action<Product[]> OnProductsRetrieved;

        /// <summary>
        /// Invoked upon the restore purchases process has started
        /// </summary>
        public static event Action OnPurchasesRestoreInitiated;

        /// <summary>
        /// Invoked upon successfully purchases restore
        /// </summary>
        public static event Action<Product[]> OnPurchasesRestored;

        private static IStore store;
        private static Catalog catalog;
        public static Catalog Catalog
        {
            get
            {
                if (catalog == null)
                {
                    catalog = Resources.Load<Catalog>("Catalog");
                }

                return catalog;
            }
            private set { }
        }

        private static bool initializing;
        private static bool initialized;
        private static bool purchaseProcessPendingToComplete = false;

        /// <summary>
        /// Informs if the Purchaser module is initialized.
        /// </summary>
        public static bool Initialized { get { return initialized; } }

        /// <summary>
        /// Initialize Homa Games IAP with <c>Catalog</c> configuration.
        ///
        /// <para>
        /// Upon initialization, Homa Games IAP queries all configured products
        /// in <c>Catalog</c> from the target <c>Store</c>, calling <c>OnProductsRetrieved</c>
        /// when done.
        /// </para>
        ///
        /// <para>
        /// Usage example:
        /// <code>
        /// HomaStore.OnProductsRetrieved += OnProductsRetrieved;
        /// HomaStore.OnPurchaseSuccess += OnPurchaseSuccess;
        /// HomaStore.Initialize();
        /// </code>
        /// </para>
        /// </summary>
        public static void Initialize()
        {
            // Avoid double initialization
            if (initialized)
            {
                Debug.Log("[Homa Store] Homa Store already initialized");
                return;
            }

            if (initializing)
            {
                Debug.Log("[Homa Store] Homa Store still initializing. Please wait...");
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                TryInitializeWhenInternetReachable();
                OnStoreInitializeFailed();
                return;
            }

            // Obtain catalog
            if (Catalog == null)
            {
                Debug.LogWarning("[Homa Store] No products defined in Catalog. Nothing to initialize");
                return;
            }

#if UNITY_EDITOR
            store = new NoopStoreImpl();
#else
#if UNITY_IOS
            // For iOS, always use DEFAULT store
            store = new RevenueCatStoreImp(Catalog.DefaultApiKey);
#else
            // Instantiate target store on Android
            switch (Catalog.Store)
            {
                case Store.DEFAULT:
                    store = new RevenueCatStoreImp(Catalog.DefaultApiKey);
                    break;
                /*
                TODO: Disable HUAWEI store for the 1.0.0 version
                case Store.HUAWEI:
                    store = new HuaweiStoreImp();
                    break;
                */
            }
#endif
#endif

            // Set store delegates
            store.OnStoreInitialized += OnStoreInitialized;
            store.OnStoreInitializeFailed += OnStoreInitializeFailed;
            store.OnStoreProductsRetrieved += OnStoreProductsRetrieved;
            store.OnStorePurchaseSuccess += OnStorePurchaseSuccess;
            store.OnStorePurchaseFailed += OnStorePurchaseFailed;
            store.OnStoreRestorePurchasesResult += OnStorePurchasesRestored;

            // Initialize
            initializing = true;
            store.Initialize(Catalog.Products);
        }

        private static void TryInitializeWhenInternetReachable()
        {
            Task.Run(() =>
            {
                // Wait for internet reachability
                while (Application.internetReachability == NetworkReachability.NotReachable)
                {
                    Thread.Sleep(1000);
                }
            }).ContinueWith((result) =>
            {
                // Once internet is reachable, initialize again
                initializing = false;
                Initialize();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Obtains cached product once fetched from the store (if any).
        /// This method won't request an updated product nor connect to the store
        /// in any way.
        /// </summary>
        /// <param name="produtId">The identifier string of the product</param>
        /// <returns>The cached product if any, null otherwise</returns>
        [CanBeNull]
        public static Product GetProduct(string produtId)
        {
            if (!initialized)
            {
                Debug.LogWarning("[Homa Store] Store not initialized");
                return null;
            }

            return store.GetProduct(produtId);
        }

        /// <summary>
        /// Obtains cached products collection fetched from the store
        /// upon initialization. This method won't request an updated products
        /// collection from the store.
        /// </summary>
        /// <returns>The collection of cached products fetched from the store</returns>
        [CanBeNull]
        public static Dictionary<string, Product> GetProducts()
        {
            if (!initialized)
            {
                Debug.LogWarning("[Homa Store] Store not initialized");
                return null;
            }

            return store.GetProducts();
        }

        /// <summary>
        /// Request a product purchase. This method triggers the
        /// corresponding store process in order to purchase the given product.
        /// </summary>
        /// <param name="productId">The internal product ID. Can be the same than the SKU</param>
        public static void PurchaseProduct(string productId)
        {
            PurchaseProduct(store.GetProduct(productId));
        }

        /// <summary>
        /// Request a product purchase. This method triggers the
        /// corresponding store process in order to purchase the given product.
        /// </summary>
        /// <param name="product">The product to be purchased</param>
        public static void PurchaseProduct(Product product)
        {
            OnPurchaseInitiatedForProduct?.Invoke(product ?? default);

            if (purchaseProcessPendingToComplete)
            {
                Debug.LogWarning("[Homa Store] Purchase failed. Another purchase is pending to be completed");
                OnStorePurchaseFailed("");
                return;
            }

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[Homa Store] Purchase failed. Network not reachable");
                OnStorePurchaseFailed(product.Id);
                return;
            }

            if (!initialized)
            {
                Debug.LogWarning("[Homa Store] Purchase failed. Store not initialized");
                OnStorePurchaseFailed(product.Id);
                return;
            }

            if (product == null)
            {
                Debug.LogWarning($"[Homa Store] Cannot find product to purchase");
                OnStorePurchaseFailed("");
                return;
            }

            if (product != null)
            {
                if (product.Type == ProductType.NON_CONSUMABLE && product.PurchaseActive)
                {
                    Debug.LogWarning($"[Homa Store] Product {product.Id} already purchased. Invoking purchase failed event");
                    OnStorePurchaseFailed(product.Id);
                }
                else if (product.Type == ProductType.SUBSCRIPTION && product.SubscriptionInfo != null && product.SubscriptionInfo.IsSubscribed)
                {
                    Debug.LogWarning($"[Homa Store] Subscription {product.Id} already purchased. Invoking purchase failed event");
                    OnStorePurchaseFailed(product.Id);
                }
                else
                {
                    Debug.Log($"[Homa Store] Purchasing {product.Sku}");
                    purchaseProcessPendingToComplete = true;
                    store.PurchaseProduct(product);
                }
            }
        }

        /// <summary>
        /// Restores all purchases from the user
        /// </summary>
        public static void RestorePurchases()
        {
            OnPurchasesRestoreInitiated?.Invoke();

            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                Debug.LogWarning("[Homa Store] Purchases restore failed. Network not reachable");
                OnStorePurchasesRestored(null);
                return;
            }

            if (!initialized)
            {
                Debug.LogWarning("[Homa Store] Store not initialized");
                return;
            }

            Debug.Log("[Homa Store] Restoring purchases...");
            store.RestorePurchases();
        }

        /// <summary>
        /// Obtains if the default `NO ADS` has already been purchased
        /// by this user
        /// </summary>
        /// <returns></returns>
        public static bool IsDefaultNoAdsAlreadyPurchased()
        {
            if (!initialized)
            {
                Debug.LogWarning("[Homa Store] Store not initialized");
                return false;
            }

            Product product = store.GetProduct("com.homagames.purchases.default.noads");
            return product != null && product.PurchaseActive;
        }

        /// <summary>
        /// Requests to purchase Default No Ads IAP item
        /// </summary>
        public static void PurchaseDefaultNoAds()
        {
            PurchaseProduct("com.homagames.purchases.default.noads");
        }

#region Store Delegates

        private static void OnStorePurchaseFailed(string productId)
        {
            Debug.Log($"[Homa Store] Purchase failed: {productId}");
            purchaseProcessPendingToComplete = false;
            OnPurchaseFailed?.Invoke(productId);
            OnPurchaseFinishedForProduct?.Invoke(GetProduct(productId) ?? default);
        }

        private static void OnStorePurchaseSuccess(string productId)
        {
            Debug.Log($"[Homa Store] Purchase success: {productId}");
            purchaseProcessPendingToComplete = false;
            OnPurchaseSuccess?.Invoke(productId);
            OnPurchaseFinishedForProduct?.Invoke(GetProduct(productId) ?? default);
        }

        private static void OnStoreInitialized()
        {
            Debug.Log("[Homa Store] Store initialized");
            initialized = true;
            initializing = false;
            OnInitialized?.Invoke();
        }

        private static void OnStoreInitializeFailed()
        {
            Debug.Log("[Homa Store] Store failed to initialize");
            initialized = false;
            initializing = false;
            OnInitializeFailed?.Invoke();
        }

        private static void OnStorePurchasesRestored(Product[] products)
        {
            Debug.Log("[Homa Store] Purchases restored");
            OnPurchasesRestored?.Invoke(products);
        }

        private static void OnStoreProductsRetrieved(Product[] products)
        {
            int productsRetrieved = products != null ? products.Length : 0;
            Debug.Log($"[Homa Store] Products retrieved: {productsRetrieved}");
            if (products != null)
            {
                OnProductsRetrieved?.Invoke(products);
            }

            // Once products are retrieved for the very firs time, restore
            // any previous purchase on Android
#if UNITY_ANDROID
            if (!PlayerPrefs.HasKey(FIRST_TIME_PURCHASES_RESTORED))
            {
                RestorePurchases();
                PlayerPrefs.SetInt(FIRST_TIME_PURCHASES_RESTORED, 1);
                PlayerPrefs.Save();
            }
#endif
        }

#endregion
    }
}

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
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Default store implementation for iOS and Android. Homa Games
    /// has partnered with RevenueCat to leverage all the IAP operations
    /// </summary>
    public class RevenueCatStoreImp : IStore
    {
        public event Action OnStoreInitialized;
        public event Action OnStoreInitializeFailed;
        public event Action<string> OnStorePurchaseSuccess;
        public event Action<string> OnStorePurchaseFailed;
        public event Action<Product[]> OnStoreProductsRetrieved;
        public event Action<Product[]> OnStoreRestorePurchasesResult;

        private string apiKey;
        private Purchases purchases;
        private RevenueCatStoreUpdatedPurchaserInfoListener listener;
        private Purchases.PurchaserInfo latestPurchaserInfoReceived;
        private Dictionary<string, Product> cachedProducts = new Dictionary<string, Product>();
        private List<Catalog.Product> products;
        private bool productsFetchedOrBeingFetched = false;

        public RevenueCatStoreImp(string apiKey)
        {
            this.apiKey = apiKey;
        }

        public void Initialize(List<Catalog.Product> products)
        {
            if (string.IsNullOrEmpty(this.apiKey))
            {
                OnStoreInitializeFailed?.Invoke();
                Debug.LogWarning("[Homa Store] Store requires an API Key");
                return;
            }

            this.products = products;

            // Create and initialize RevenueCat game object
            GameObject revenueCatGameObject = new GameObject("RevenueCat Store");
            revenueCatGameObject.SetActive(false);
            GameObject.DontDestroyOnLoad(revenueCatGameObject);
            // Add Component
            purchases = revenueCatGameObject.AddComponent<Purchases>();
            // Listener
            listener = revenueCatGameObject.AddComponent<RevenueCatStoreUpdatedPurchaserInfoListener>();
            listener.Bind(this);
            purchases.listener = listener;
            purchases.revenueCatAPIKey = apiKey;
            revenueCatGameObject.SetActive(true);
            FetchProductsAfterPurchaserInfoReceived();
        }

        public void SetDebugEnabled(bool debug)
        {
            if (purchases != null && purchases.gameObject != null && purchases.gameObject.activeSelf)
            {
                purchases.SetDebugLogsEnabled(debug);
            }
        }

        public void PurchaseProduct(Product product)
        {
            try
            {
                purchases.PurchaseProduct(product.Sku, (productIdentifier, purchaserInfo, userCancelled, error) =>
                {
                    if (error == null)
                    {
                        // Success
                        latestPurchaserInfoReceived = purchaserInfo;
                        UpdateCachedProducts();
                        OnStorePurchaseSuccess?.Invoke(product.Id);
                    }
                    else
                    {
                        // Failure
                        Debug.LogWarning($"ERROR purchasing product {product.Sku}: {error.code} - {error.message}");
                        OnStorePurchaseFailed?.Invoke(product.Id);
                    }
                }, product.Type == ProductType.SUBSCRIPTION ? "subs" : "inapp");
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR while restoring transactions: {e.Message}");
            }
        }

        public void RestorePurchases()
        {
            try
            {
                purchases.RestoreTransactions((purchaserInfo, error) =>
                {
                    if (error == null)
                    {
                        // Success
                        latestPurchaserInfoReceived = purchaserInfo;
                        UpdateCachedProducts();

                        // Invoke the callback even if no restored products are found
                        OnStoreRestorePurchasesResult?.Invoke(cachedProducts.Values.ToArray());
                    }
                    else
                    {
                        // Failure
                        Debug.LogWarning($"ERROR restoring purchases: {error.message}");
                        OnStoreRestorePurchasesResult?.Invoke(null);
                    }
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR while restoring transactions: {e.Message}");
            }
        }

        public Dictionary<string, Product> GetProducts()
        {
            return cachedProducts;
        }

        public Product GetProduct(string productId)
        {
            Product product = null;

            // Find prodct by id or sku containing `productId`
            foreach (KeyValuePair<string, Product> entry in cachedProducts)
            {
                if ((!string.IsNullOrEmpty(entry.Value.Id) && entry.Value.Id == productId)
                    ||  (!string.IsNullOrEmpty(entry.Value.Sku) && entry.Value.Sku == productId))
                {
                    product = entry.Value;
                }
            }

            return product;
        }

#region Private Helpers

        /// <summary>
        /// After receiving the Purchaser Info (which means RevenueCat store has
        /// been fully initialized), we fetch all products (managed and subscriptions)
        /// and then notify the user the store has been initialized and
        /// the products retrieved.
        /// </summary>
        private void FetchProductsAfterPurchaserInfoReceived()
        {
            // Fetch products only first time
            if (productsFetchedOrBeingFetched)
            {
                return;
            }

            // We need to asynchronously wait for the Purchaser Info
            // to be received and then act as a continuation
            Task.Run(() =>
            {
                while (latestPurchaserInfoReceived == null)
                {
                    Thread.Sleep(100);
                }
            }).ContinueWith((result) =>
            {
                if (products != null)
                {
                    productsFetchedOrBeingFetched = true;

                    // Build the list of cached product IDs
                    List<string> managedIds = new List<string>();
                    List<string> subscriptionIds = new List<string>();
                    foreach (Catalog.Product product in products)
                    {
                        switch (product.Type)
                        {
                            case ProductType.CONSUMABLE:
                            case ProductType.NON_CONSUMABLE:
#if UNITY_IOS
                                managedIds.Add(product.AppStoreSku);
#else
                                managedIds.Add(product.GooglePlaySku);
#endif
                                break;
                            case ProductType.SUBSCRIPTION:
#if UNITY_IOS
                                subscriptionIds.Add(product.AppStoreSku);
#else
                                subscriptionIds.Add(product.GooglePlaySku);
#endif
                                break;
                        }
                    }

                    FetchProducts(managedIds.ToArray(), subscriptionIds.ToArray(), () =>
                    {
                        purchases.CollectDeviceIdentifiers();
                        purchases.SetAutomaticAppleSearchAdsAttributionCollection(true);
                        OnStoreInitialized?.Invoke();
                        OnStoreProductsRetrieved?.Invoke(cachedProducts.Values.ToArray<Product>());
                    });
                }
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void AddProductToCache(Product product)
        {
            if (!cachedProducts.ContainsKey(product.Id))
            {
                // Add
                cachedProducts.Add(product.Id, product);
            }
            else
            {
                // Update
                cachedProducts[product.Id] = product;
            }
        }

        public void GetManagedProducts(string[] productIds, Action onDone)
        {
            // Avoid fetching empty list. Invoke onDone inmediately
            if (productIds == null || productIds.Length == 0)
            {
                Debug.Log("No managed or unmanaged products configured. Please see Catalog");
                onDone?.Invoke();
                return;
            }

            try
            {
                purchases.GetProducts(productIds, (products, error) =>
                {
                    if (error == null)
                    {
                        if (products != null)
                        {
                            // Translate products
                            for (int i = 0; i < products.Count; i++)
                            {
                                Purchases.Product innerProduct = products[i];
                                Catalog.Product catalogProduct;
#if UNITY_IOS
                                catalogProduct = HomaStore.Catalog.Products.FirstOrDefault(e => e.AppStoreSku == innerProduct.identifier);
#else
                                catalogProduct = HomaStore.Catalog.Products.FirstOrDefault(e => e.GooglePlaySku == innerProduct.identifier);
#endif

                                Product product = new Product(catalogProduct.Id,
                                    innerProduct.identifier,
                                    catalogProduct.Type,
                                    innerProduct.title,
                                    innerProduct.description,
                                    innerProduct.price,
                                    innerProduct.priceString,
                                    innerProduct.currencyCode);

                                product.PurchaseActive = NonSubscriptionProductPurchaseActive(innerProduct.identifier);
                                AddProductToCache(product);
                                Debug.Log($"Managed product {product} retrieved");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Retrieved managed products list is empty");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"ERROR retrieving managed products: {error.code} - {error.message}");
                    }

                    onDone?.Invoke();
                }, "inapp");
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR retrieving managed products: {e.Message}");
            }
        }

        public void GetSubscriptionProducts(string[] productIds, Action onDone)
        {
            // Avoid fetching empty list. Invoke onDone inmediately
            if (productIds == null || productIds.Length == 0)
            {
                Debug.Log("No subscription products configured. Please see Catalog");
                onDone?.Invoke();
                return;
            }

            try
            {
                purchases.GetProducts(productIds, (products, error) =>
                {
                    if (error == null)
                    {
                        if (products != null)
                        {
                            // Translate products
                            for (int i = 0; i < products.Count; i++)
                            {
                                Purchases.Product innerProduct = products[i];
                                Product product = new Product(
#if UNITY_IOS
                                    HomaStore.Catalog.Products.FirstOrDefault(e => e.AppStoreSku == innerProduct.identifier).Id,
#else
                                    HomaStore.Catalog.Products.FirstOrDefault(e => e.GooglePlaySku == innerProduct.identifier).Id,
#endif
                                    innerProduct.identifier,
                                    ProductType.SUBSCRIPTION,
                                    innerProduct.title,
                                    innerProduct.description,
                                    innerProduct.price,
                                    innerProduct.priceString,
                                    innerProduct.currencyCode,
                                    BuildSubscriptionInfo(innerProduct.identifier));

                                AddProductToCache(product);
                                Debug.Log($"Subscription product {product} retrieved");
                            }
                        }
                        else
                        {
                            Debug.LogWarning($"Retrieved subscription products list is empty");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"ERROR retrieving subscription products: {error.code} - {error.message}");
                    }

                    onDone?.Invoke();
                });
            }
            catch (Exception e)
            {
                Debug.LogError($"ERROR retrieving subscription products: {e.Message}");
            }
        }

        private void UpdateCachedProducts()
        {
            // Do nothing if there are no cached products
            if (cachedProducts == null || cachedProducts.Count == 0)
            {
                return;
            }

            // Build the list of cached product IDs
            foreach (var item in cachedProducts)
            {
                switch (item.Value.Type)
                {
                    case ProductType.CONSUMABLE:
                    case ProductType.NON_CONSUMABLE:
                        item.Value.PurchaseActive = NonSubscriptionProductPurchaseActive(item.Value.Sku);
                        break;
                    case ProductType.SUBSCRIPTION:
                        item.Value.SubscriptionInfo = BuildSubscriptionInfo(item.Value.Sku);
                        break;
                }
            }
        }

        /// <summary>
        /// This method queries RevenueCat for all the informed product IDs (aka SKUs) and
        /// notifies onComplete once done.
        ///
        /// Because RevenueCat clears the product fetch callback (sets it to null) after
        /// each query, we need to perform all actions sequentially. We do use Task
        /// and wait asynchronously to each query completion
        /// </summary>
        /// <param name="managedIds">List of managed skus to be retrieved</param>
        /// <param name="subscriptionIds">List of subscription skus to be retrieved</param>
        /// <param name="onComplete">Action to be executed after fetching completes</param>
        private async void FetchProducts(string[] managedIds, string[] subscriptionIds, Action onComplete)
        {
            bool managedProductsCompleted = false;
            bool subscriptionProductsCompleted = false;
              
            // Update managed products
            GetManagedProducts(managedIds.ToArray(), () =>
            {
                managedProductsCompleted = true;
            });

            // Wait until managed products have been fetched
            await Task.Run(() =>
            {
                // Wait for completion
                while (!managedProductsCompleted)
                {
                    Thread.Sleep(100);
                }
            });

            // Update subscription products
            GetSubscriptionProducts(subscriptionIds.ToArray(), () =>
            {
                subscriptionProductsCompleted = true;
            });

            // Wait until subscription products have been fetched
            await Task.Run(() =>
            {
                // Wait for completion
                while (!subscriptionProductsCompleted)
                {
                    Thread.Sleep(100);
                }
            });

            onComplete?.Invoke();
        }

        private SubscriptionInfo BuildSubscriptionInfo(string productId)
        {
            if (latestPurchaserInfoReceived != null)
            {
                // Obtain EntitlementInfo for the given product
                Purchases.EntitlementInfo entitlementInfo = null;
                if (latestPurchaserInfoReceived.Entitlements != null && latestPurchaserInfoReceived.Entitlements.All.ContainsKey(productId))
                {
                    Debug.Log($"Subscription entitlement found for {productId}");
                    entitlementInfo = latestPurchaserInfoReceived.Entitlements.All[productId];

                    return new SubscriptionInfo(
                        entitlementInfo.LatestPurchaseDate,
                        entitlementInfo.IsActive,
                        !entitlementInfo.IsActive,
                        !entitlementInfo.IsActive,
                        entitlementInfo.PeriodType.ToLower() == "trial",
                        entitlementInfo.WillRenew,
                        entitlementInfo.ExpirationDate?.Subtract(DateTime.UtcNow),
                        entitlementInfo.ExpirationDate,
                        // TODO
                        false,
                        // TODO
                        null,
                        0,
                        ""
                    );
                }
                // If entitlement info not found, search for ActiveSubscriptions at least
                else if (latestPurchaserInfoReceived.ActiveSubscriptions != null && latestPurchaserInfoReceived.ActiveSubscriptions.Contains(productId))
                {
                    Debug.Log($"Subscription Active found for {productId}");
                    return new SubscriptionInfo(
                        null,
                        true,
                        false,
                        false,
                        false,
                        false,
                        null,
                        null,
                        // TODO
                        false,
                        // TODO
                        null,
                        0,
                        ""
                    );
                }
            }

            // If not found, return default object (which represents a non subscribed status)
            return SubscriptionInfo.EmptySubscriptionInfo();
        }

        /// <summary>
        /// Determines if a non subscription product has been purchased at least one
        /// </summary>
        /// <returns></returns>
        private bool NonSubscriptionProductPurchaseActive(string productId)
        {
            // Obtain transactions for the given product
            if (latestPurchaserInfoReceived != null && latestPurchaserInfoReceived.AllPurchasedProductIdentifiers != null)
            {
                return latestPurchaserInfoReceived.AllPurchasedProductIdentifiers.Contains(productId);
            }

            return false;
        }

#endregion

#region Purchases.UpdatedPurchaserInfoListener

        public class RevenueCatStoreUpdatedPurchaserInfoListener : Purchases.UpdatedPurchaserInfoListener
        {
            private WeakReference<RevenueCatStoreImp> _storeImp;
            public void Bind(RevenueCatStoreImp storeImp)
            {
                _storeImp = new WeakReference<RevenueCatStoreImp>(storeImp);
            }

            public override void PurchaserInfoReceived(Purchases.PurchaserInfo purchaserInfo)
            {
                Debug.Log($"Purchaser info received: {JsonUtility.ToJson(purchaserInfo)}");
                RevenueCatStoreImp target;
                _storeImp.TryGetTarget(out target);
                target.latestPurchaserInfoReceived = purchaserInfo;

                target.UpdateCachedProducts();
            }
        }

#endregion
    }
}

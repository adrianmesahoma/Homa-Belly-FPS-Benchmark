using System;
using UnityEngine;
using UnityEngine.Events;

namespace HomaGames.HomaBelly.IAP
{
    [Serializable]
    public class ProductEvent : UnityEvent<Product> { }

    /// <summary>
    /// Helper script to be attached to an In App Purchase UI button.
    /// This script will automatically query Homa Store and obtain
    /// the configured product, which you will always be able to access
    /// and obtain any required information.
    ///
    /// Remind calling `InitiatePurchase` when the user clicks on the
    /// corresponding UI button, which will start the purchase process.
    /// </summary>
    public class ProductButtonHelper : MonoBehaviour
    {
        [Header("Configuration")]
        [Catalog.ProductId, Tooltip("Select the Android product id you want to be fetched by this helper")]
        public string AndroidProductId;
        [Catalog.ProductId, Tooltip("Select the Apple product id you want to be fetched by this helper")]
        public string AppleProductId;

        [Header("Events"), Space(10)]
        public ProductEvent OnProductFetched;
        public UnityEvent OnPurchaseSucceded;
        public UnityEvent OnPurchaseFailed;
        public UnityEvent OnPurchaseRestored;

        /// <summary>
        /// The configured Product object once fetched
        /// and all its data updated. Use this to obtain
        /// metadata (price, name, description...) or status
        /// (subscription status, purchase active, etc...)
        /// </summary>
        [HideInInspector]
        public Product Product;

        /// <summary>
        /// Status flag to inform if the product is currently under
        /// purchasing process
        /// </summary>
        [HideInInspector]
        public bool IsPurchasing = false;

        private void OnEnable()
        {
            // Start listeining Homa Store events
            HomaStore.OnPurchaseSuccess += OnPurchaseSuccess;
            HomaStore.OnPurchaseFailed += OnPurchaseFailure;
            HomaStore.OnPurchasesRestored += OnInnerPurchaseRestored;
            HomaStore.OnProductsRetrieved += OnProductsRetrieved;

            // If the store is already initialized, bind the product.
            // Wait for initialization event otherwise.
            if (HomaStore.Initialized && HomaStore.GetProducts().Count > 0)
            {
                BindProduct();
            }
        }

        private void OnDisable()
        {
            // Stop listening Homa Store events
            HomaStore.OnProductsRetrieved -= OnProductsRetrieved;
            HomaStore.OnPurchaseSuccess -= OnPurchaseSuccess;
            HomaStore.OnPurchaseFailed -= OnPurchaseFailure;
            HomaStore.OnPurchasesRestored -= OnInnerPurchaseRestored;
        }

        #region Events

        private void OnInnerPurchaseRestored(Product[] products)
        {
            if (Product == null || products == null)
            {
                return;
            }

            foreach (var item in products)
            {
                bool isThisProduct = Product.Id == item.Id || Product.Sku == item.Sku;
                bool isActive = item.Type != ProductType.SUBSCRIPTION && item.PurchaseActive;
                isActive |= item.Type == ProductType.SUBSCRIPTION && item.SubscriptionInfo != null && item.SubscriptionInfo.IsSubscribed;

                if (isThisProduct && isActive)
                {
                    OnPurchaseRestored?.Invoke();
                }
            }
        }

        private void OnProductsRetrieved(Product[] products)
        {
            HomaStore.OnProductsRetrieved -= OnProductsRetrieved;
            BindProduct();
        }

        private void OnPurchaseSuccess(string productId)
        {
            if (Product != null && (Product.Id == productId || Product.Sku == productId))
            {
                OnPurchaseSucceded?.Invoke();
                IsPurchasing = false;
            }
        }

        private void OnPurchaseFailure(string productId)
        {
            if (Product == null || Product.Id != productId)
            {
                return;
            }

            IsPurchasing = false;
            OnPurchaseFailed?.Invoke();
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Obtains the product from Homa Store and notifies OnProductFetched action.
        /// This Product is ready to be displayed to the user to be purchased.
        /// </summary>
        private void BindProduct()
        {
#if UNITY_ANDROID
            Product = HomaStore.GetProduct(AndroidProductId);
#elif UNITY_IPHONE
            Product = HomaStore.GetProduct(AppleProductId);
#endif
            if (Product != null)
            {
                OnProductFetched?.Invoke(Product);
            }
        }

        /// <summary>
        /// Invoke to initiate the purchase of the configured product. OnPurchaseSucceded
        /// or OnPurchaseFailed will be called as a result of this process.
        /// </summary>
        public void InitiatePurchase()
        {
            if (Product != null)
            {
                IsPurchasing = true;
                HomaStore.PurchaseProduct(Product.Sku);
            }
            else
            {
                UnityEngine.Debug.LogWarning($"Trying to initiate purchase but no Product is configured on this button");
            }
        }

#endregion
    }
}

using System;
using System.Collections.Generic;
using System.Linq;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Store implementation for Unity Editor.
    ///
    /// It acts as a NO-OP allowing flow debug
    /// </summary>
    public class NoopStoreImpl : IStore
    {
        public event Action OnStoreInitialized;
        public event Action OnStoreInitializeFailed;
        public event Action<string> OnStorePurchaseSuccess;
        public event Action<string> OnStorePurchaseFailed;
        public event Action<Product[]> OnStoreProductsRetrieved;
        public event Action<Product[]> OnStoreRestorePurchasesResult;

        private List<Product> products = new List<Product>();

        public Product GetProduct(string productId)
        {
            return products.FirstOrDefault(p => p.Sku == productId || p.Id == productId);
        }

        public Dictionary<string, Product> GetProducts()
        {
            Dictionary<string, Product> productsDict = new Dictionary<string, Product>();
            foreach (var p in products)
            {
                productsDict.Add(p.Sku, p);
            }

            return productsDict;
        }

        public void Initialize(List<Catalog.Product> products)
        {
            // Translate products
            for (int i = 0; i < products.Count; i++)
            {
                Catalog.Product innerProduct = products[i];
                Product product = new Product(
                    innerProduct.Id,
#if UNITY_IOS
                    innerProduct.AppStoreSku,
#else
                    innerProduct.GooglePlaySku,
#endif
                    innerProduct.Type,
                    innerProduct.Id,
                    "Testing IAP for Unity Editor",
                    1.23f,
                    "1.23$",
                    "USD",
                    innerProduct.Type == ProductType.SUBSCRIPTION ? SubscriptionInfo.EmptySubscriptionInfo() : null);

                this.products.Add(product);
            }

            OnStoreInitialized?.Invoke();
            OnStoreProductsRetrieved?.Invoke(this.products.ToArray());
        }

        public void PurchaseProduct(Product product)
        {
            if (product.Type == ProductType.SUBSCRIPTION)
            {
                product.SubscriptionInfo = new SubscriptionInfo(DateTime.Now, true, false, false, false, true, null, null, false, null, 0, "");
            }
            else
            {
                product.PurchaseActive = true;
            }
            
            OnStorePurchaseSuccess?.Invoke(product.Sku);
        }

        public void RestorePurchases()
        {
            OnStoreRestorePurchasesResult?.Invoke(products.ToArray());
        }

        public void SetDebugEnabled(bool debug)
        {
            // NO-OP
        }
    }
}
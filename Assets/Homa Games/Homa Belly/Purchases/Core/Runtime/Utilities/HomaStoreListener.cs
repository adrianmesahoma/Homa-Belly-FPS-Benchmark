using UnityEngine;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Internal listener to automatically track IAP events
    /// </summary>
    public class HomaStoreListener
    {
        [RuntimeInitializeOnLoadMethod]
        public static void StartListening()
        {
            HomaStore.OnPurchaseSuccess += OnPurchaseSuccess;
        }

        private static void OnPurchaseSuccess(string productId)
        {
            Product product = HomaStore.GetProduct(productId);
            if (product != null)
            {
                HomaBelly.Instance.TrackInAppPurchaseEvent(product.Id, product.CurrencyCode, product.Price);
            }
        }
    }
}
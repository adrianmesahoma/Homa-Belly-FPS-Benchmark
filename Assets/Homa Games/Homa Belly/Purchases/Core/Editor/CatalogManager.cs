using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Manager to configure Catalog on Unity Editor
    /// </summary>
    public class CatalogManager
    {
        /// <summary>
        /// Determines if Catalog.asset exists. If not creates it and configures
        /// it from values in Homa Belly Manifest
        /// </summary>
        public static Catalog TryCreateAndConfigureCatalog()
        {
            Catalog catalog = (Catalog)Resources.Load("Catalog", typeof(Catalog));
            if (catalog == null)
            {
                // Only if Catalog Resource does not exist, create it from Manifest
                RestoreCatalogFromManifest();
                catalog = (Catalog)Resources.Load("Catalog", typeof(Catalog));
            }

            return catalog;
        }

        /// <summary>
        /// Despite Catalog asset exiting or not (if not creates it), restores
        /// all configuration from Homa Belly Manifest removing any previous
        /// modifications
        /// </summary>
        public static void RestoreCatalogFromManifest()
        {
            PluginManifest pluginManifest = PluginManifest.LoadFromLocalFile();
            if (pluginManifest != null)
            {
                PackageComponent packageComponent = pluginManifest.Packages
                    .GetPackageComponent(HomaBellyPurchasesConstants.ID, HomaBellyPurchasesConstants.TYPE);
                if (packageComponent != null)
                {
                    Dictionary<string, string> configurationData = packageComponent.Data;

                    if (configurationData != null)
                    {
                        Catalog catalog = TryLoadCatalog();
                        if (catalog != null)
                        {
                            // Configure default api key
                            if (configurationData.ContainsKey("s_revenuecat_api_key"))
                            {
                                catalog.DefaultApiKey = configurationData["s_revenuecat_api_key"] as string;
                            }
                            else
                            {
                                HomaGamesLog.Warning("Could not configure Purchases. Catalog not found or `s_revenuecat_api_key` not present in configuration");
                            }

                            // Remove any previous configured product. This will enforce to use Homa Belly manifest to configure products
                            if (catalog.Products != null && catalog.Products.Count > 0)
                            {
                                catalog.Products = new List<Catalog.Product>();
                            }

                            // Configure remote IAP items
                            ConfigureSkusOnCatalog(configurationData, "s_android_consumable_skus_csv", catalog, ProductType.CONSUMABLE);
                            ConfigureSkusOnCatalog(configurationData, "s_ios_consumable_skus_csv", catalog, ProductType.CONSUMABLE);

                            ConfigureSkusOnCatalog(configurationData, "s_android_nonconsumable_skus_csv", catalog, ProductType.NON_CONSUMABLE);
                            ConfigureSkusOnCatalog(configurationData, "s_ios_nonconsumable_skus_csv", catalog, ProductType.NON_CONSUMABLE);

                            ConfigureSkusOnCatalog(configurationData, "s_android_subscription_skus_csv", catalog, ProductType.SUBSCRIPTION);
                            ConfigureSkusOnCatalog(configurationData, "s_ios_subscription_skus_csv", catalog, ProductType.SUBSCRIPTION);

                            ConfigureNoAdsProduct(configurationData, catalog);

                            if (!EditorApplication.isPlaying)
                            {
                                // Mark asset to dirty to force save'
                                EditorUtility.SetDirty(catalog);
                                AssetDatabase.ForceReserializeAssets(new string[] { Catalog.CATALOG_FILE_PATH });
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Determine if the given sku is the no ads sku for iOS or Android
        /// </summary>
        /// <param name="sku">The sku to check</param>
        /// <param name="configurationData">The whole catalog configuration</param>
        /// <returns>true if the sku is the same as the no ads sku</returns>
        private static bool IsNoAdsSku(string sku, Dictionary<string, string> configurationData)
        {
            bool isNoAdsSku = false;

            if (string.IsNullOrEmpty(sku))
            {
                return isNoAdsSku;
            }

            if (configurationData != null)
            {
                // If there is an android no ads sku and is the same, return true
                if (configurationData.ContainsKey("s_android_no_ads_sku")
                    && configurationData["s_android_no_ads_sku"] == sku)
                {
                    isNoAdsSku = true;
                }
                // Otherwise, check ios no ads sku
                else if (configurationData.ContainsKey("s_ios_no_ads_sku")
                    && configurationData["s_ios_no_ads_sku"] == sku)
                {
                    isNoAdsSku = true;
                }
            }

            return isNoAdsSku;
        }

        /// <summary>
        /// Configures the incoming "s_android_no_ads_sku" and "s_ios_no_ads_sku"
        /// as the internal com.homagames.purchases.default.noads Product for
        /// IDFA Paywall
        /// </summary>
        /// <param name="configurationData">The configuration data fetched from manifest</param>
        /// <param name="catalog">The Catalog object to place the skus</param>
        private static void ConfigureNoAdsProduct(Dictionary<string, string> configurationData, Catalog catalog)
        {
            Catalog.Product product = new Catalog.Product();
            product.Id = "com.homagames.purchases.default.noads";
            if (configurationData.ContainsKey("s_android_no_ads_sku"))
            {
                product.GooglePlaySku = configurationData["s_android_no_ads_sku"];
                product.DefaultSku = product.GooglePlaySku;
            }

            if (configurationData.ContainsKey("s_ios_no_ads_sku"))
            {
                product.AppStoreSku = configurationData["s_ios_no_ads_sku"];
                product.DefaultSku = product.AppStoreSku;
            }

            product.Type = ProductType.NON_CONSUMABLE;

            // Avoid configuring a Product in Catalog with no SKUs
            if (!string.IsNullOrEmpty(product.DefaultSku))
            {
                if (catalog.Products == null)
                {
                    catalog.Products = new List<Catalog.Product>();
                }

                try
                {
                    Catalog.Product oldProduct = catalog.Products.FirstOrDefault(p => p.Id == product.Id);
                    if (oldProduct != null)
                    {
                        catalog.Products.Remove(oldProduct);
                    }
                }
                catch (Exception e)
                {
                    HomaGamesLog.Warning(string.Format("Exception trying to remove old product from Catalog: {0}", e.Message));
                }

                catalog.Products.Add(product);
                HomaGamesLog.Debug(string.Format("No Ads IAP product configured: {0} - {1}", product.Id, product.DefaultSku));
            }
            else
            {
                HomaGamesLog.Warning(string.Format("No Ads IAP could not be configured. SKU is missing"));
            }
        }


        /// <summary>
        /// Try to configure remote IAP items into the Catalog
        /// </summary>
        /// <param name="configurationData">The configuration data fetched from manifest</param>
        /// <param name="configuraitonDataKey">The key identifying the SKUs list<param>
        /// <param name="catalog">The Catalog object to place the skus</param>
        /// <param name="type">The ProductType of the desired skus: CONSUMABLE, NON_CONSUMABLE or SUBSCRIPTION</param>
        private static void ConfigureSkusOnCatalog(Dictionary<string, string> configurationData, string configuraitonDataKey, Catalog catalog, ProductType type)
        {
            if (configurationData.ContainsKey(configuraitonDataKey))
            {
                string skusCsv = configurationData[configuraitonDataKey].ToString().Trim().Replace(" ", string.Empty).Replace("\t", string.Empty);
                string[] skus = skusCsv.Split(',');

                if (skus != null)
                {
                    foreach (string sku in skus)
                    {
                        Catalog.Product product = new Catalog.Product();
                        product.Id = sku;
                        product.DefaultSku = sku;
                        product.Type = type;

                        if (catalog.Products == null)
                        {
                            catalog.Products = new List<Catalog.Product>();
                        }

                        bool contains = catalog.Products.Any(p => p.Id == product.Id || p.DefaultSku == product.DefaultSku);
                        bool isNoAdsSku = IsNoAdsSku(product.AppStoreSku, configurationData) || IsNoAdsSku(product.GooglePlaySku, configurationData);
                        if (!contains && !isNoAdsSku)
                        {
                            HomaGamesLog.Debug(string.Format("{0} IAP product configured: {1}", type, sku));
                            catalog.Products.Add(product);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Try to load the Catalog asset, creating it if necessary
        /// </summary>
        /// <returns>The Catalog object loaded from Resources</returns>
        private static Catalog TryLoadCatalog()
        {
            // Try to load Catalog asset
            Catalog catalog = (Catalog)Resources.Load("Catalog", typeof(Catalog));
            if (catalog == null)
            {
                // Creating intermediate directories
                FileUtilities.CreateIntermediateDirectoriesIfNecessary(Catalog.CATALOG_FILE_PATH);

                // Create asset
                Catalog asset = ScriptableObject.CreateInstance<Catalog>();
                AssetDatabase.CreateAsset(asset, Catalog.CATALOG_FILE_PATH);
                AssetDatabase.SaveAssets();

                // Load again
                catalog = (Catalog)Resources.Load("Catalog", typeof(Catalog));
            }

            return catalog;
        }
    }
}
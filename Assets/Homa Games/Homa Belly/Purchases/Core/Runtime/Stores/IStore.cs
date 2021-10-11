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

namespace HomaGames.HomaBelly.IAP
{
    public interface IStore
    {
        // Events
        event Action OnStoreInitialized;
        event Action OnStoreInitializeFailed;
        event Action<string> OnStorePurchaseSuccess;
        event Action<string> OnStorePurchaseFailed;
        event Action<Product[]> OnStoreProductsRetrieved;
        event Action<Product[]> OnStoreRestorePurchasesResult;

        void Initialize(List<Catalog.Product> products);
        void SetDebugEnabled(bool debug);
        void PurchaseProduct(Product product);
        void RestorePurchases();

        /// <summary>
        /// Obtains the cached product once fetched from the store (if any).
        /// This method won't request an updated method nor connect to the store
        /// in any way. The fetched product must contain 'productId' string
        /// within its id or sku values
        /// </summary>
        /// <param name="produtId">The identifier of the product</param>
        /// <returns></returns>
        Product GetProduct(string produtId);

        /// <summary>
        /// Obtains the cached collection of products fetched from the store
        /// upon initialization. This method won't request an updated collection
        /// of products from the store.
        /// </summary>
        /// <returns>The collection of cached products fetched from the store</returns>
        Dictionary<string, Product> GetProducts();
    }   
}

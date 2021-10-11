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
using JetBrains.Annotations;

namespace HomaGames.HomaBelly.IAP
{
    /// <summary>
    /// Model representing a Product on the store with all its values and
    /// statuses
    /// </summary>
    [Serializable]
    public class Product
    {
        private readonly string _id;
        /// <summary>
        /// The item ID. Usually the same as its Sku
        /// </summary>
        public string Id { get { return _id; } }

        private readonly string _sku;
        /// <summary>
        /// Item unique identifier on the corresponding store
        /// </summary>
        public string Sku { get { return _sku; } }

        private readonly string _title;
        /// <summary>
        /// Item title
        /// </summary>
        public string Title { get { return _title; } }

        private readonly string _description;
        /// <summary>
        /// Item description
        /// </summary>
        public string Description { get { return _description; } }

        private readonly float _price;

        /// <summary>
        /// Raw product price
        /// </summary>
        public float Price { get { return _price; } }

        private readonly string _priceString;

        /// <summary>
        /// Price string already converted to user's currency
        /// </summary>
        public string PriceString { get { return _priceString; } }

        private readonly string _currencyCode;
        public string CurrencyCode { get { return _currencyCode; } }

        private readonly ProductType _type;
        public ProductType Type { get { return _type; } }

        private SubscriptionInfo _subscriptionInfo;

        /// <summary>
        /// Holds all necessary information for a subscription product: if it is active,
        /// cancelled, expired, etc...
        ///
        /// Will be null for non-subscription products
        /// </summary>
        [CanBeNull] public SubscriptionInfo SubscriptionInfo {
            get
            {
                if (Type != ProductType.SUBSCRIPTION)
                {
                    UnityEngine.Debug.LogWarning("Trying to read SubscriptionInfo object from a non-subscription Product. It will always be null");
                }

                return _subscriptionInfo;
            }
            internal set { _subscriptionInfo = value;} }

        /// <summary>
        /// Determines if this product has already been purchased and it
        /// is active (for example, user did not request a refund)
        /// </summary>
        public bool PurchaseActive { get; internal set; }

        public Product(
            string id,
            string sku,
            ProductType type)
        {
            _id = id;
            _sku = sku;
            _type = type;
        }

        public Product(
            string id,
            string sku,
            ProductType type,
            string title,
            string description,
            float price,
            string priceString,
            string currencyCode,
            SubscriptionInfo subscriptionInfo = null)
        {
            _id = id;
            _sku = sku;
            _type = type;
            _title = title;
            _description = description;
            _price = price;
            _priceString = priceString;
            _currencyCode = currencyCode;
            _subscriptionInfo = subscriptionInfo;
        }

        public override string ToString()
        {
            return $"{_sku} ({_type}) - {_title} - {_priceString}. Already purchased and/or active? {(Type != ProductType.SUBSCRIPTION ? PurchaseActive : SubscriptionInfo.IsSubscribed)}";
        }

        public override bool Equals(object obj)
        {
            if (obj == null) return false;
            Product objAsProduct = obj as Product;
            if (objAsProduct == null) return false;
            else return objAsProduct.Sku == _sku;
        }

        public override int GetHashCode()
        {
            return Sku.GetHashCode();
        }
    }
}

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
    [Serializable]
    public class SubscriptionInfo
    {
        private readonly DateTime? _purchaseDate;
        [CanBeNull] public DateTime? PurchaseDate { get { return _purchaseDate; } }

        private readonly bool _isSubscribed;
        /// <summary>
        /// If the subscription is currently active
        /// </summary>
        public bool IsSubscribed { get { return _isSubscribed; } }

        private readonly bool _isExpired;
        /// <summary>
        /// If the subscription has expired
        /// </summary>
        public bool IsExpired { get { return _isExpired; } }

        private readonly bool _isCancelled;
        /// <summary>
        /// If the subscription is cancelled
        /// </summary>
        public bool IsCancelled { get { return _isCancelled; } }

        private readonly bool _isFreeTrial;
        /// <summary>
        /// If the subscription is currently under free trial period
        /// </summary>
        public bool IsFreeTrial { get { return _isFreeTrial; } }

        private readonly bool _isAutoRenewing;
        /// <summary>
        /// If the subscription will be renewed automatically
        /// </summary>
        public bool IsAutoRenewing { get { return _isAutoRenewing; } }

        private readonly TimeSpan? _remainingTime;
        /// <summary>
        /// Time until the subscription becomes inactive or it is auto-renewed
        /// </summary>
        [CanBeNull] public TimeSpan? RemainingTime { get { return _remainingTime; } }

        private readonly DateTime? _expireDate;
        /// <summary>
        /// The expire date
        /// </summary>
        [CanBeNull] public DateTime? ExpireDate { get { return _expireDate; } }

        private readonly bool _isIntroductoryPricePeriod;
        public bool IsIntroductoryPricePeriod { get { return _isIntroductoryPricePeriod; } }

        private readonly TimeSpan? _introductoryPricePeriod;
        [CanBeNull] public TimeSpan? IntroductoryPricePeriod { get { return _introductoryPricePeriod; } }

        private readonly long _introductoryPricePeriodCycles;
        public long IntroductoryPricePeriodCycles { get { return _introductoryPricePeriodCycles; } }

        private readonly string _introductoryPrice;
        public string IntroductoryPrice { get { return _introductoryPrice; } }

        public SubscriptionInfo(DateTime? purchaseDate,
            bool isSubscribed,
            bool isExpired,
            bool isCancelled,
            bool isFreeTrial,
            bool isAutoRenewing,
            TimeSpan? remainingTime,
            DateTime? expireDate,
            bool isIntroductoryPricePeriod,
            TimeSpan? introductoryPricePeriod,
            long introductoryPricePeriodCycles,
            string introductoryPrice)
        {
            _purchaseDate = purchaseDate;
            _isSubscribed = isSubscribed;
            _isExpired = isExpired;
            _isCancelled = isCancelled;
            _isFreeTrial = isFreeTrial;
            _isAutoRenewing = isAutoRenewing;
            _remainingTime = remainingTime;
            _expireDate = expireDate;
            _isIntroductoryPricePeriod = isIntroductoryPricePeriod;
            _introductoryPricePeriod = introductoryPricePeriod;
            _introductoryPricePeriodCycles = introductoryPricePeriodCycles;
            _introductoryPrice = introductoryPrice;
        }

        public static SubscriptionInfo EmptySubscriptionInfo()
        {
            return new SubscriptionInfo(
                null,
                false,
                false,
                false,
                false,
                false,
                null,
                null,
                false,
                null,
                0,
                ""
            );
        }
    }
}

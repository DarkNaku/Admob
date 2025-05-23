using System;
using System.Collections;
using System.Collections.Generic;
using DarkNaku.Admob;
using GoogleMobileAds.Api;
using UnityEngine;

namespace DarkNaku.Admob {
    public class AdmobReward : IDisposable {
        public bool IsLoaded => _rewardedAd != null && _rewardedAd.CanShowAd();

        private string _adUnitId;
        private RewardedAd _rewardedAd;
        private Action<bool> _onClose;
        private bool _isRewardCompleted;
        private IDispatcher _dispatcher;

        public AdmobReward(IDispatcher dispatcher, string adUnitId) {
            _dispatcher = dispatcher;
            _adUnitId = adUnitId;

            Load();
        }

        public void Dispose() {
        }

        public void Show(Action<bool> onClose) {
            if (_rewardedAd == null) {
                Debug.LogError("[Admob-Reward] Show : Rewarded ad is not loaded.");
                return;
            }

            if (_rewardedAd.CanShowAd()) {
                _isRewardCompleted = false;
                _rewardedAd.Show(_ => _isRewardCompleted = true);
            } else {
                Debug.LogError("[Admob-Reward] Show : Rewarded ad is not loaded.");
            }

            _onClose = onClose;
        }

        private void Load() {
            if (_rewardedAd != null) {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            RewardedAd.Load(_adUnitId, new AdRequest(), OnLoaded);
        }

        private void OnLoaded(RewardedAd ad, LoadAdError error) {
            if (error != null || ad == null) {
                Debug.LogError($"[Admob-Reward] Load : Initialize failed - {error}");
                return;
            }

            ad.OnAdPaid += OnAdPaid;
            ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
            ad.OnAdClicked += OnAdClicked;
            ad.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
            ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;

            _rewardedAd = ad;

            Debug.Log($"[Admob-Reward] OnLoaded - {ad.GetResponseInfo()}");
        }

        private void OnAdPaid(AdValue adValue) {
            Debug.Log($"[Admob-Reward] OnAdPaid : {adValue.Value} - {adValue.CurrencyCode}.");
        }

        private void OnAdImpressionRecorded() {
            Debug.Log("[Admob-Reward] OnAdImpressionRecorded.");
        }

        private void OnAdClicked() {
            Debug.Log("[Admob-Reward] OnAdClicked.");
        }

        private void OnAdFullScreenContentOpened() {
            Debug.Log("[Admob-Reward] OnAdFullScreenContentOpened.");
        }

        private void OnAdFullScreenContentClosed() {
            _dispatcher?.Enqueue(() => _onClose?.Invoke(_isRewardCompleted));

            Load();

            Debug.Log("[Admob-Reward] OnAdFullScreenContentClosed.");
        }


        private void OnAdFullScreenContentFailed(AdError error) {
            _dispatcher?.Enqueue(() => _onClose?.Invoke(false));

            Load();

            Debug.LogError($"[Admob-Reward] OnAdFullScreenContentFailed - {error}.");
        }
    }
}
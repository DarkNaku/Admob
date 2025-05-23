using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace DarkNaku.Admob {
    public class AdmobInterstitial : IDisposable {
        public bool IsLoaded => _interstitialAd != null && _interstitialAd.CanShowAd();

        private InterstitialAd _interstitialAd;
        private string _adUnitId;
        private Action _onClose;
        private IDispatcher _dispatcher;

        public AdmobInterstitial(IDispatcher dispatcher, string adUnitId) {
            _dispatcher = dispatcher;
            _adUnitId = adUnitId;

            Load();

            Debug.Log("[Admob-Interstitial] Created.");
        }

        public void Dispose() {
            if (_interstitialAd != null) {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }
        }

        public void Show(Action onClose) {
            if (_interstitialAd == null) {
                Debug.LogError("[Admob-Interstitial] Show : Interstitial ad is not loaded.");
                return;
            }

            if (_interstitialAd.CanShowAd()) {
                _interstitialAd.Show();
            } else {
                Debug.LogError("[Admob-Interstitial] Show : Interstitial ad is not loaded.");
            }

            _onClose = onClose;
        }

        private void Load() {
            if (_interstitialAd != null) {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            InterstitialAd.Load(_adUnitId, new AdRequest(), OnLoaded);
        }

        private void OnLoaded(InterstitialAd ad, LoadAdError error) {
            if (error != null || ad == null) {
                Debug.LogError($"[Admob-Interstitial] Load : Initialize failed - {error}");
                return;
            }

            ad.OnAdPaid += OnAdPaid;
            ad.OnAdImpressionRecorded += OnAdImpressionRecorded;
            ad.OnAdClicked += OnAdClicked;
            ad.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            ad.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
            ad.OnAdFullScreenContentFailed += OnAdFullScreenContentFailed;

            _interstitialAd = ad;

            Debug.Log($"[Admob-Interstitial] OnLoaded : {ad.GetResponseInfo()}");
        }

        private void OnAdPaid(AdValue adValue) {
            Debug.Log($"[Admob-Interstitial] OnAdPaid : {adValue.Value} - {adValue.CurrencyCode}.");
        }

        private void OnAdImpressionRecorded() {
            Debug.Log("[Admob-Interstitial] Impression.");
        }

        private void OnAdClicked() {
            Debug.Log("[Admob-Interstitial] OnAdClicked.");
        }

        private void OnAdFullScreenContentOpened() {
            Debug.Log("[Admob-Interstitial] OnAdFullScreenContentOpened.");
        }

        private void OnAdFullScreenContentClosed() {
            _dispatcher?.Enqueue(() => _onClose?.Invoke());

            Load();

            Debug.Log("[Admob-Interstitial] OnAdFullScreenContentClosed.");
        }

        private void OnAdFullScreenContentFailed(AdError error) {
            _dispatcher?.Enqueue(() => _onClose?.Invoke());

            Load();

            Debug.LogError($"[Admob-Interstitial] OnAdFullScreenContentFailed : {error}");
        }
    }
}
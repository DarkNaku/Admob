using System;
using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace DarkNaku.Admob {
    public class AdmobBanner : IDisposable {
        private BannerView _bannerView;
        private IDispatcher _dispatcher;

        public AdmobBanner(IDispatcher dispatcher, string adUnitId, AdSize adSize, AdPosition position) {
            _dispatcher = dispatcher;
            _bannerView = new BannerView(adUnitId, adSize, position);
            _bannerView.OnBannerAdLoaded += OnBannerAdLoaded;
            _bannerView.OnBannerAdLoadFailed += OnBannerAdLoadFailed;
            _bannerView.OnAdPaid += OnAdPaid;
            _bannerView.OnAdImpressionRecorded += OnAdImpressionRecorded;
            _bannerView.OnAdClicked += OnAdClicked;
            _bannerView.OnAdFullScreenContentOpened += OnAdFullScreenContentOpened;
            _bannerView.OnAdFullScreenContentClosed += OnAdFullScreenContentClosed;
            _bannerView.LoadAd(new AdRequest());

            Debug.Log("[Admob-Banner] Created.");
        }

        public void Dispose() {
            _bannerView.Destroy();
            _bannerView = null;

            Debug.Log("[Admob-Banner] Destroyed.");
        }

        private void OnBannerAdLoaded() {
            Debug.Log($"[Admob-Banner] OnBannerAdLoaded : {_bannerView.GetResponseInfo()}");
        }

        private void OnBannerAdLoadFailed(LoadAdError error) {
            Debug.LogError($"[Admob-Banner] OnBannerAdLoadFailed - {error}");
        }

        private void OnAdPaid(AdValue adValue) {
            Debug.Log($"[Admob-Banner] OnAdPaid : {adValue.Value} - {adValue.CurrencyCode}.");
        }

        private void OnAdImpressionRecorded() {
            Debug.Log("[Admob-Banner] OnAdImpressionRecorded.");
        }

        private void OnAdClicked() {
            Debug.Log("[Admob-Banner] OnAdClicked.");
        }

        private void OnAdFullScreenContentOpened() {
            Debug.Log("[Admob-Banner] OnAdFullScreenContentOpened.");
        }

        private void OnAdFullScreenContentClosed() {
            Debug.Log("[Admob-Banner] OnAdFullScreenContentClosed.");
        }
    }
}
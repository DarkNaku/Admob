using System.Collections;
using System.Collections.Generic;
using GoogleMobileAds.Api;
using UnityEngine;

namespace DarkNaku.Admob {
    public class Admob : MonoBehaviour {
        public static Admob Instance {
            get {
                if (_isDestroyed) return null;

                lock (_lock) {
                    if (_instance == null) {
                        var instances = FindObjectsByType<Admob>(FindObjectsInactive.Include, FindObjectsSortMode.None);

                        if (instances.Length > 0) {
                            _instance = instances[0];

                            for (int i = 1; i < instances.Length; i++) {
                                Debug.LogWarningFormat("[Admob] Instance Duplicated - {0}", instances[i].name);
                                Destroy(instances[i]);
                            }
                        } else {
                            _instance = new GameObject($"[Singleton] Admob").AddComponent<Admob>();
                        }
                    }

                    return _instance;
                }
            }
        }

        public static bool Initialized => Instance._isInitialized;
        public static bool IsInterstitialLoaded => Instance._interstitialAd != null && Instance._interstitialAd.CanShowAd();
        public static bool IsRewardLoaded => Instance._rewardedAd != null && Instance._rewardedAd.CanShowAd();

        private static readonly object _lock = new();
        private static Admob _instance;
        private static bool _isDestroyed;

        private bool _isInitialized;
        private BannerView _bannerView;
        private InterstitialAd _interstitialAd;
        private RewardedAd _rewardedAd;
        private System.Action _onCloseInterstitialAd;
        private System.Action<bool> _onCloseRewardAd;
        private bool _isInterstitialAdClosed;
        private bool _isRewardedAdClosed;
        private bool _isRewardedCompleted;

        public static void Initialize() => Instance._Initialize();
        public static void ShowInterstitialAd(System.Action onClose) => Instance._ShowInterstitialAd(onClose);
        public static void ShowRewardedAd(System.Action<bool> onClose) => Instance._ShowRewardedAd(onClose);
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration() {
            _instance = null;
            _isDestroyed = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad() {
            if (AdmobConfig.InitializeOnStart) Initialize();
        }

        private void Update() {
            if (_onCloseInterstitialAd != null) {
                _onCloseInterstitialAd?.Invoke();
                _onCloseInterstitialAd = null;
            }

            if (_onCloseRewardAd != null) {
                _onCloseRewardAd?.Invoke(_isRewardedCompleted);
                _onCloseRewardAd = null;
            }
        }

        private void _Initialize() {
            if (_isInitialized) return;

            MobileAds.Initialize((initStatus) => {
                Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();

                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map) {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;

                    switch (status.InitializationState) {
                        case AdapterState.NotReady:
                            // The adapter initialization did not complete.
                            MonoBehaviour.print($"[Admob] Adapter : {className} not ready.");
                            break;
                        case AdapterState.Ready:
                            // The adapter was successfully initialized.
                            MonoBehaviour.print($"[Admob] Adapter : {className} is initialized.");
                            break;
                    }
                }

                if (AdmobConfig.BannerEnabled) LoadBanner();
                if (AdmobConfig.InterstitialEnabled) LoadInterstitialAd();
                if (AdmobConfig.RewardedEnabled) LoadRewardedAd();

                _isInitialized = true;
            });
        }

        private void LoadBanner() {
            if (_bannerView != null) {
                DestroyBanner();
            }

            _bannerView = new BannerView(AdmobConfig.AdmobBannerId, AdSize.Banner, AdPosition.Bottom);

            _bannerView.OnBannerAdLoaded += () => Debug.Log($"[Admob] Banner : Loaded - {_bannerView.GetResponseInfo()}");
            _bannerView.OnBannerAdLoadFailed += (LoadAdError error) => Debug.LogError($"[Admob] Banner : Failed to load - {error}");
            _bannerView.OnAdPaid += (AdValue adValue) => Debug.Log($"[Admob] Banner : Paid {adValue.Value} {adValue.CurrencyCode}.");
            _bannerView.OnAdImpressionRecorded += () => Debug.Log("[Admob] Banner : Impression.");
            _bannerView.OnAdClicked += () => Debug.Log("[Admob] Banner : Clicked.");
            _bannerView.OnAdFullScreenContentOpened += () => Debug.Log("[Admob] Banner : Full screen content opened.");
            _bannerView.OnAdFullScreenContentClosed += () => Debug.Log("[Admob] Banner : Full screen content closed.");

            _bannerView.LoadAd(new AdRequest());

            Debug.Log("[Admob] Banner : Created.");
        }

        private void DestroyBanner() {
            if (_bannerView != null) {
                _bannerView.Destroy();
                _bannerView = null;
                Debug.Log("[Admob] Banner : Destroyed.");
            }
        }

        private void LoadInterstitialAd() {
            if (_interstitialAd != null) {
                _interstitialAd.Destroy();
                _interstitialAd = null;
            }

            InterstitialAd.Load(AdmobConfig.AdmobInterstialId, new AdRequest(), (ad, error) => {
                if (error != null || ad == null) {
                    Debug.LogError($"[Admob] Interstitial : Failed to load - {error}");
                    return;
                }

                _interstitialAd = ad;

                RegisterEventHandlers(_interstitialAd);

                Debug.Log($"[Admob] Interstitial : Loaded - {ad.GetResponseInfo()}");
            });
        }

        private void RegisterEventHandlers(InterstitialAd interstitialAd) {
            interstitialAd.OnAdPaid += (AdValue adValue) => Debug.Log($"[Admob] Interstitial : AD paid {adValue.Value} {adValue.CurrencyCode}.");
            interstitialAd.OnAdImpressionRecorded += () => Debug.Log("[Admob] Interstitial : Impression.");
            interstitialAd.OnAdClicked += () => Debug.Log("[Admob] Interstitial : Clicked.");
            interstitialAd.OnAdFullScreenContentOpened += () => Debug.Log("[Admob] Interstitial : Full screen content opened.");
            interstitialAd.OnAdFullScreenContentClosed += () => {
                _isInterstitialAdClosed = true;

                LoadInterstitialAd();

                Debug.Log("[Admob] Interstitial : Full screen content closed.");
            };

            interstitialAd.OnAdFullScreenContentFailed += (AdError error) => {
                _isInterstitialAdClosed = true;

                LoadInterstitialAd();

                Debug.LogError($"[Admob] Interstitial : Failed to open full screen content - {error}");
            };
        }

        private void _ShowInterstitialAd(System.Action onClose) {
            if (_interstitialAd != null && _interstitialAd.CanShowAd()) {
                _isInterstitialAdClosed = false;
                _onCloseInterstitialAd = onClose;
                _interstitialAd.Show();
            }
        }

        private void LoadRewardedAd() {
            if (_rewardedAd != null) {
                _rewardedAd.Destroy();
                _rewardedAd = null;
            }

            RewardedAd.Load(AdmobConfig.AdmobRewardId, new AdRequest(), (ad, error) => {
                if (error != null || ad == null) {
                    Debug.LogError($"[Admob] Rewarded : Failed to load - {error}");
                    return;
                }

                _rewardedAd = ad;

                RegisterEventHandlers(_rewardedAd);

                Debug.Log($"[Admob] Rewarded : Loaded - {ad.GetResponseInfo()}");
            });
        }

        private void RegisterEventHandlers(RewardedAd ad) {
            ad.OnAdPaid += (adValue) => Debug.Log($"[Admob] Rewarded : Paid {adValue.Value} {adValue.CurrencyCode}.");
            ad.OnAdImpressionRecorded += () => Debug.Log("[Admob] Rewarded : Impression.");
            ad.OnAdClicked += () => Debug.Log("[Admob] Rewarded : Clicked.");
            ad.OnAdFullScreenContentOpened += () => Debug.Log("[Admob] Rewarded : Full screen content opened.");

            ad.OnAdFullScreenContentClosed += () => {
                _isRewardedAdClosed = true;

                LoadRewardedAd();

                Debug.Log("[Admob] Rewarded : Full screen content closed.");
            };

            ad.OnAdFullScreenContentFailed += (error) => {
                _isRewardedAdClosed = true;

                LoadRewardedAd();

                Debug.LogError($"[Admob] Rewarded : Failed to open full screen content - {error}.");
            };
        }

        private void _ShowRewardedAd(System.Action<bool> onClose) {
            if (_rewardedAd != null && _rewardedAd.CanShowAd()) {
                _isRewardedAdClosed = false;
                _isRewardedCompleted = false;
                _onCloseRewardAd = onClose;
                
                _rewardedAd.Show((Reward reward) => {
                    _isRewardedCompleted = true;

                    Debug.Log($"[Admob] Rewarded : Rewarded the user. Type: {reward.Type}, amount: {reward.Amount}.");
                });
            } else {
                onClose?.Invoke(false);
            }
        }
    }
}
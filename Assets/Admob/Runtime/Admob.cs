using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public static bool IsInterstitialLoaded => Instance._interstitial != null && Instance._interstitial.IsLoaded;
        public static bool IsRewardLoaded => Instance._reward != null && Instance._reward.IsLoaded;

        private static readonly object _lock = new();
        private static Admob _instance;
        private static bool _isDestroyed;

        private bool _isInitialized;
        private AdmobBanner _banner;
        private AdmobInterstitial _interstitial;
        private AdmobReward _reward;

        public static void Initialize() => Instance._Initialize();
        public static void LoadBanner() => Instance._LoadBanner();
        public static void LoadInterstitial() => Instance._LoadInterstitial();
        public static void ShowInterstitial(System.Action onClose) => Instance._ShowInterstitial(onClose);
        public static void LoadReward() => Instance._LoadReward();
        public static void ShowReward(System.Action<bool> onClose) => Instance._ShowReward(onClose);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void OnSubsystemRegistration() {
            _instance = null;
            _isDestroyed = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void OnBeforeSceneLoad() {
            if (AdmobConfig.InitializeOnStart) Initialize();
        }

        private void Awake() {
            if (_instance == null) {
                _instance = this;
                DontDestroyOnLoad(gameObject);
            } else if (_instance != this) {
                Debug.LogWarningFormat("[Admob] Instance Duplicated - {0}", name);
                Destroy(gameObject);
                return;
            }
        }

        private void OnApplicationQuit() {
            if (_instance != this) return;

            _banner?.Dispose();
            _interstitial?.Dispose();
            _reward?.Dispose();
            _instance = null;
            _isDestroyed = true;

            Debug.Log($"[Admob] Destroyed.");
        }

        private async void _Initialize() {
            if (_isInitialized) return;

            var isCompleted = new TaskCompletionSource<bool>();

            MobileAds.Initialize((initStatus) => {
                Dictionary<string, AdapterStatus> map = initStatus.getAdapterStatusMap();

                foreach (KeyValuePair<string, AdapterStatus> keyValuePair in map) {
                    string className = keyValuePair.Key;
                    AdapterStatus status = keyValuePair.Value;

                    switch (status.InitializationState) {
                        case AdapterState.NotReady:
                            MonoBehaviour.print($"[Admob] Adapter : {className} not ready.");
                            break;
                        case AdapterState.Ready:
                            MonoBehaviour.print($"[Admob] Adapter : {className} is initialized.");
                            break;
                    }
                }

                isCompleted.SetResult(true);
            });

            await isCompleted.Task;

            _isInitialized = true;

            Debug.Log("[Admob] Initialized.");

            if (AdmobConfig.BannerEnabled) LoadBanner();
            if (AdmobConfig.InterstitialEnabled) LoadInterstitial();
            if (AdmobConfig.RewardedEnabled) LoadReward();
        }

        private void _LoadBanner() {
            _banner ??= new AdmobBanner(AdmobConfig.AdmobBannerId, AdSize.Banner, AdPosition.Bottom);
        }

        private void _LoadInterstitial() {
            _interstitial ??= new AdmobInterstitial(AdmobConfig.AdmobInterstialId);
        }

        private void _ShowInterstitial(System.Action onClose) {
            if (_interstitial != null && _interstitial.IsLoaded) {
                _interstitial.Show(onClose);
            } else {
                onClose?.Invoke();
            }
        }

        private void _LoadReward() {
            _reward ??= new AdmobReward(AdmobConfig.AdmobRewardId);
        }

        private void _ShowReward(System.Action<bool> onClose) {
            if (_reward != null && _reward.IsLoaded) {
                _reward.Show(onClose);
            } else {
                onClose?.Invoke(false);
            }
        }
    }
}
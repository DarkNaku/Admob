using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GoogleMobileAds.Api;
using UnityEngine;

namespace DarkNaku.Admob {
    public interface IDispatcher {
        void Enqueue(System.Action action);
    }

    public class Admob : MonoBehaviour, IDispatcher {
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
        private Queue<Action> _dispatchQueue = new();

        public static void Initialize() => Instance._Initialize();
        public static void LoadBanner() => Instance._LoadBanner();
        public static bool IsElapsedInterstitial(TimeSpan timeSpan) => Instance._IsElapsedInterstitial(timeSpan);
        public static void LoadInterstitial() => Instance._LoadInterstitial();
        public static void ShowInterstitial(System.Action onClose) => Instance._ShowInterstitial(onClose);
        public static bool IsElapsedReward(TimeSpan timeSpan) => Instance._IsElapsedReward(timeSpan);
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

        public void Enqueue(Action action) {
            if (action == null) return;

            lock (_dispatchQueue) {
                _dispatchQueue.Enqueue(action);
            }
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

        private void Update() {
            while (_dispatchQueue.Count > 0) {
                Action action;

                lock (_dispatchQueue) {
                    action = _dispatchQueue.Dequeue();
                }

                action?.Invoke();
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

            var requestConfiguration = new RequestConfiguration();
            requestConfiguration.TestDeviceIds.AddRange(AdmobConfig.TestDeviceIDs);
            MobileAds.SetRequestConfiguration(requestConfiguration);

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
            _banner ??= new AdmobBanner(this, AdmobConfig.AdmobBannerId, AdSize.Banner, AdPosition.Bottom);
        }

        private bool _IsElapsedInterstitial(TimeSpan timeSpan) {
            if (_interstitial != null && _interstitial.IsLoaded) {
                return _interstitial.IsElapsed(timeSpan);
            } else {
                return false;
            }
        }

        private void _LoadInterstitial() {
            _interstitial ??= new AdmobInterstitial(this, AdmobConfig.AdmobInterstialId);
        }

        private void _ShowInterstitial(System.Action onClose) {
            if (_interstitial != null && _interstitial.IsLoaded) {
                _interstitial.Show(onClose);
            } else {
                onClose?.Invoke();
            }
        }
        
        private bool _IsElapsedReward(TimeSpan timeSpan) {
            if (_reward != null && _reward.IsLoaded) {
                return _reward.IsElapsed(timeSpan);
            } else {
                return false;
            }
        }

        private void _LoadReward() {
            _reward ??= new AdmobReward(this, AdmobConfig.AdmobRewardId);
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
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkNaku.Admob {
    public class AdmobConfig : ScriptableObject {
        [SerializeField] private bool _initializeOnStart = true;
        [SerializeField] private bool _bannerEnabled = true;
        [SerializeField] private bool _interstitialEnabled = true;
        [SerializeField] private bool _rewardedEnabled = true;
        [SerializeField] private string _admobGoBannerId = "ca-app-pub-3940256099942544/6300978111";
        [SerializeField] private string _admobGoInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _admobGoRewardId = "ca-app-pub-3940256099942544/5224354917";
        [SerializeField] private string _admobApBannerId = "ca-app-pub-3940256099942544/6300978111";
        [SerializeField] private string _admobApInterstitialId = "ca-app-pub-3940256099942544/1033173712";
        [SerializeField] private string _admobApRewardId = "ca-app-pub-3940256099942544/5224354917";

        public static AdmobConfig Instance {
            get {
                if (_isDestroyed) return null;

                lock (_lock) {
                    if (_instance == null) {
                        var assetName = typeof(AdmobConfig).Name;

                        _instance = Resources.Load<AdmobConfig>(assetName);

                        if (_instance == null) {
                            _instance = CreateInstance<AdmobConfig>();

#if UNITY_EDITOR
                            var assetPath = "Resources";
                            var resourcePath = System.IO.Path.Combine(Application.dataPath, assetPath);

                            if (System.IO.Directory.Exists(resourcePath) == false) {
                                UnityEditor.AssetDatabase.CreateFolder("Assets", assetPath);
                            }

                            UnityEditor.AssetDatabase.CreateAsset(_instance, $"Assets/{assetPath}/{assetName}.asset");
#endif
                        }
                    }
                }

                return _instance;
            }
        }

        public static bool InitializeOnStart => Instance._initializeOnStart;
        public static bool BannerEnabled => Instance._bannerEnabled;
        public static bool InterstitialEnabled => Instance._interstitialEnabled;
        public static bool RewardedEnabled => Instance._rewardedEnabled;

#if UNITY_ANDROID
        public static string AdmobBannerId => Instance._admobGoBannerId;
        public static string AdmobInterstialId => Instance._admobGoInterstitialId;
        public static string AdmobRewardId => Instance._admobGoRewardId;
#elif UNITY_IOS
        public static string AdmobBannerId => Instance._admobApBannerId;
        public static string AdmobInterstialId => Instance._admobApInterstitialId;
        public static string AdmobRewardId => Instance._admobApRewardId;
#else
        public static string AdmobBannerId => Instance._admobGoBannerId;
        public static string AdmobInterstialId => Instance._admobGoInterstitialId;
        public static string AdmobRewardId => Instance._admobGoRewardId;
#endif

        private static readonly object _lock = new();
        private static AdmobConfig _instance;
        private static bool _isDestroyed;

#if UNITY_EDITOR
        [UnityEditor.MenuItem("Tools/Admob Config")]
        private static void SelectConfig() {
            UnityEditor.Selection.activeObject = Instance;
        }
#endif
    }
}
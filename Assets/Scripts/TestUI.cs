using System.Collections;
using System.Collections.Generic;
using DarkNaku.Admob;
using UnityEngine;
using UnityEngine.UIElements;

public class TestUI : MonoBehaviour {
    [SerializeField] private UIDocument _uiDocument;

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            Admob.ShowInterstitial(() => Debug.Log("Interstitial Shown."));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) {
            Admob.ShowReward(result => Debug.Log($"Reward Shown - {result}"));
        }
    }

    private void OnValidate() {
        _uiDocument ??= GetComponent<UIDocument>();

        _uiDocument.rootVisualElement.Q<Button>("InterstitialShowButton")
            .RegisterCallback<ClickEvent>(e => Admob.ShowInterstitial(() => Debug.Log("Interstitial Shown.")));

        _uiDocument.rootVisualElement.Q<Button>("RewardShowButton")
            .RegisterCallback<ClickEvent>(e => Admob.ShowReward(result => Debug.Log($"Reward Shown - {result}")));
    }
}
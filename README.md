# Admob

### 소개
구글 애드몹을 간단히 사용하기 위한 패키지

### 설치방법
1. 패키지 관리자의 툴바에서 좌측 상단에 플러스 메뉴를 클릭합니다.
2. 추가 메뉴에서 Add package from git URL을 선택하면 텍스트 상자와 Add 버튼이 나타납니다.
3. https://github.com/DarkNaku/Admob.git?path=/Assets/Admob 입력하고 Add를 클릭합니다.

### 사용방법
* https://github.com/googleads/googleads-mobile-unity 에서 애드몹 SDK 패키지를 설치해야 합니다.
* 'Tools > Admob Config' 메뉴에서 애드몹 키와 광고 타입별 사용 여부를 설정할 수 있습니다.
* 배너 광고의 경우 사용으로 체크하는 경우 자동으로 로드 되어 노출 됩니다.

```csharp
// 전면광고
if (Admob.IsInterstitialLoaded) {
    Admob.ShowInterstitialAd();
}

// 보상형 광고
if (Admob.IsRewardLoaded) {
    Admob.ShowRewardedAd((success) => {
        // success 시청 완료 여부
    });
}
```
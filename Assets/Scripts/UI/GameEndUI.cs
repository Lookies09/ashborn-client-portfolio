using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class GameEndUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI resultText;

    [SerializeField] private TextMeshProUGUI goldInfoText;
    [SerializeField] private TextMeshProUGUI goldText;
    [SerializeField] private TextMeshProUGUI bounceGoldText;
    [SerializeField] private TextMeshProUGUI rewardAdsBtnText;
    [SerializeField] private Button rewardAdsButton;
    [SerializeField] private Button goToMainButton;
    [SerializeField] private Button reviveButton;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform panelRect;

    [SerializeField] private Color32 deadColor;
    [SerializeField] private Color32 escapedColor;

    private bool _isEscaped;

    private void Start()
    {
        rewardAdsButton.onClick.RemoveAllListeners();
        rewardAdsButton.onClick.AddListener(OnClickRewardAds);

        reviveButton.onClick.RemoveAllListeners();
        reviveButton.onClick.AddListener(OnClickReviveBtn);

        goToMainButton.onClick.RemoveAllListeners();
        goToMainButton.onClick.AddListener(() => { GameManager.Instance.GoToLobby(); });
    }

    private void OnEnable()
    {
        RewardButtonSwap(true);
        // 광고 핸들러의 이벤트 구독 (AdSDK를 통해 접근한다고 가정)
        AdSDK.Instance.RewardedAdHandler.OnRewardGranted += HandleRewardEffect;
    }

    private void OnDisable()
    {
        if (AdSDK.Instance != null)
            AdSDK.Instance.RewardedAdHandler.OnRewardGranted -= HandleRewardEffect;
    }

    public void SetUIByResult(bool isEscaped)
    {
        rewardAdsButton.interactable = true;

        _isEscaped = isEscaped;

        if (isEscaped)
        {
            if (InGameManager.Instance.CanGetGoldBonus)
            {
                rewardAdsBtnText.text = "광고 시청하고 50% 추가 골드 획득하기";
            }
            else
            {
                rewardAdsBtnText.text = "시청해주셔서 감사합니다.";
                rewardAdsButton.interactable = false;
            }


            resultText.text = "ESCAPED";
            resultText.color = escapedColor;
            goldInfoText.text = "획득한 골드 :";
            goldText.text = $"+{PlayerWallet.Instance.TempGold.ToString("N0")}G";
            bounceGoldText.text = "+0G";
            
        }
        else
        {
            if (InGameManager.Instance.CanRevive)
            {
                rewardAdsBtnText.text = "광고 시청하고 부활하기";
            }
            else
            {
                rewardAdsBtnText.text = "리워드를 이미 받으셨습니다.";
                rewardAdsButton.interactable = false;
            }

                resultText.text = "YOU DIED";
            resultText.color = deadColor;
            goldInfoText.text = "잃어버릴 골드 :";
            goldText.text = $"{PlayerWallet.Instance.TempGold.ToString("N0")}G";
            bounceGoldText.text = "+0G";
        }

        Show(isEscaped);
    }

    public void Show(bool isEscaped)
    {
        panelRect.DOKill();
        canvasGroup.DOKill();

        canvasGroup.alpha = 0; // 처음에 투명하게
        panelRect.localScale = Vector3.zero; // 크기 0에서 시작

        canvasGroup.DOFade(1f, 2f).SetDelay(2f).SetUpdate(true);

        if (isEscaped)
        {
            panelRect.DOScale(1f, 1f).SetEase(Ease.OutBack).SetUpdate(true);
        }
        else
        {
            panelRect.DOScale(1f, 0.3f).SetUpdate(true).OnComplete(() => {
                panelRect.DOShakePosition(1f, 10f).SetUpdate(true);
            });
        }
    }

    public void OnClickRewardAds()
    {
        bool isSuccess = AdSDK.Instance.TryShowReward();
        if (!isSuccess)
        {
            rewardAdsBtnText.text = "광고를 불러올 수 없습니다. \n나중에 다시 시도해주세요.";
            rewardAdsButton.transform.DOShakePosition(0.5f, 10f).SetUpdate(true);
        }
    }

    public void OnClickReviveBtn()
    {
        InGameManager.Instance.OnPlayerRevived();
    }

    private void HandleRewardEffect()
    {
        if (_isEscaped)
        {
            rewardAdsButton.interactable = false;

            int bonusGold = Mathf.RoundToInt(PlayerWallet.Instance.TempGold * 0.5f);

            bounceGoldText.text = $"+{bonusGold:N0}G";
            bounceGoldText.color = escapedColor;

            // 텍스트가 톡 튀어나오는 연출
            bounceGoldText.transform.DOPunchScale(Vector3.one * 1.5f, 0.5f).SetUpdate(true);
            rewardAdsBtnText.text = "추가 보상 획득 완료!";

            InGameManager.Instance.UseGoldBonusAd();
        }
        else
        {
            // 2. 사망 시: 부활 연출
            resultText.text = "REVIVED";
            resultText.color = Color.green;
            rewardAdsBtnText.text = "부활";

            // 여기서 버튼 스왑
            RewardButtonSwap(false);

            InGameManager.Instance.UseReviveAd();
        }
    }

    private void RewardButtonSwap(bool isRewardActive)
    {
        if (rewardAdsButton.gameObject.activeSelf == isRewardActive &&
            reviveButton.gameObject.activeSelf == !isRewardActive) return;

        rewardAdsButton.gameObject.SetActive(isRewardActive);
        reviveButton.gameObject.SetActive(!isRewardActive);
        if (!isRewardActive)
        {
            reviveButton.transform.localScale = Vector3.zero;
            reviveButton.transform.DOScale(1f, 0.5f).SetEase(Ease.OutBack).SetUpdate(true);
        }
    }


}

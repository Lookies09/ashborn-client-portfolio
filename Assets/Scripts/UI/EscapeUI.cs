using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class EscapeUI : MonoBehaviour
{
    [SerializeField] private Image progressGauge;
    [SerializeField] private Image glowEffectImg;
    [SerializeField] private TextMeshProUGUI stateText;
    [SerializeField] private TextMeshProUGUI percentText;

    [Header("Glow Settings")]
    [SerializeField] private float minAlpha = 0.2f;    // 최소 투명도
    [SerializeField] private float maxAlpha = 0.8f;    // 최대 투명도
    [SerializeField] private float glowSpeed = 3.0f;   // 파동 속도

        private void Start()
    {
        // 초기화: 게이지 0부터 시작
        if (progressGauge != null) progressGauge.fillAmount = 0;
        SetPercentText(0);
    }

    private void Update()
    {
        ApplyGlowAnimation();
    }

    /// <summary>
    /// 글로우 이미지의 알파값을 Sin 파동으로 조절합니다.
    /// </summary>
    private void ApplyGlowAnimation()
    {
        if (glowEffectImg == null) return;

        // 시간에 따른 Sin 값 계산 (-1 ~ 1 -> 0 ~ 1로 치환)
        float sinValue = (Mathf.Sin(Time.time * glowSpeed) + 1.0f) * 0.5f;

        // 설정한 Alpha 범위 내에서 유동적으로 변하게 설정
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, sinValue);

        Color color = glowEffectImg.color;
        color.a = targetAlpha;
        glowEffectImg.color = color;
    }

    /// <summary>
    /// 외부(EscapePortal)에서 게이지 수치를 업데이트할 때 호출합니다.
    /// </summary>
    public void UpdateEscapeProgress(float progress01)
    {
        // 1. 게이지 이미지 업데이트
        if (progressGauge != null) progressGauge.fillAmount = progress01;

        // 2. 퍼센트 텍스트 업데이트
        SetPercentText(progress01 * 100f);

        // 3. (추천) 탈출 완료 시 연출
        if (progress01 >= 1f && stateText.text != "COMPLETE!")
        {
            OnEscapeComplete();
        }
    }

    private void SetPercentText(float value)
    {
        if (percentText != null)
            percentText.text = $"{Mathf.FloorToInt(value)}%";
    }

    private void OnEscapeComplete()
    {
        stateText.text = "COMPLETE!";
        stateText.color = Color.green;

        // DOTween을 활용한 완료 연출: 텍스트가 커졌다가 작아짐
        stateText.transform.DOPunchScale(Vector3.one * 0.2f, 0.5f);

        // 글로우를 아주 밝게 고정
        glowEffectImg.DOFade(1f, 0.2f);
    }
}

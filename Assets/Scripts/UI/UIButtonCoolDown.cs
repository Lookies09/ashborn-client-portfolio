using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;


[RequireComponent(typeof(Button))]
public class UIButtonCoolDown : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float cooldownTime = 1.0f;
    [SerializeField] private Image cooldownOverlay;

    [SerializeField] private Button button;
    private bool _isCoolingDown = false;

    public Button Button => button;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }       

        if (cooldownOverlay != null)
            cooldownOverlay.gameObject.SetActive(false);
    }

    public void OnButtonClicked(UnityAction onClick)
    {
        if (_isCoolingDown) return;

        // 1. 실제 동작 실행
        onClick?.Invoke();

        // 2. 쿨타임 시작
        StartCoroutine(CoStartCooldown());
    }

    private IEnumerator CoStartCooldown()
    {
        _isCoolingDown = true;
        button.interactable = false;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.gameObject.SetActive(true);
            cooldownOverlay.fillAmount = 1f;
        }

        float elapsed = 0f;
        while (elapsed < cooldownTime)
        {
            elapsed += Time.unscaledDeltaTime;
            if (cooldownOverlay != null)
                cooldownOverlay.fillAmount = 1f - (elapsed / cooldownTime);

            yield return null;
        }

        if (cooldownOverlay != null)
            cooldownOverlay.gameObject.SetActive(false);

        button.interactable = true;
        _isCoolingDown = false;
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class InGameHUD : MonoBehaviour
{
    [SerializeField] private Button settingButton;
    [SerializeField] private TextMeshProUGUI timerText;

    [SerializeField] private GameObject timerTextObject;
    [SerializeField] private GameObject bossChasetextObject;


    [SerializeField] private GameObject escapeUIRoot;
    [SerializeField] private EscapeUI escapeUIController;



    private void Start()
    {
        settingButton.onClick.RemoveAllListeners();
        if (UIManager.Instance != null)
        {
            settingButton.onClick.AddListener(() => { SettingManager.Instance.SetSettinPanel(true); });
        }
        
    }

    public void UpdateTimer(float time)
    {
        if (timerText != null)
        {
            time = Mathf.Max(time, 0f);
            int displayMinutes = Mathf.FloorToInt(time / 60f);
            int displaySeconds = Mathf.CeilToInt(time % 60f);
            timerText.text = $"{displayMinutes:00}:{displaySeconds:00}";
        }

    }

    public void SetBossChaseActive(bool isActive)
    {
        timerTextObject.SetActive(!isActive);
        bossChasetextObject.SetActive(isActive);
    }

    public void UpdateEscapeProgress(float progress01)
    {
        if (escapeUIController != null)
        {
            escapeUIController.UpdateEscapeProgress(progress01);
        }
    }

    public void SetEscapeUIActivate(bool isActive)
    {
        if (escapeUIRoot != null)
            escapeUIRoot.SetActive(isActive);
    }
}

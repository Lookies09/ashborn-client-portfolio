using UnityEngine;

public class InGameHUDController : MonoBehaviour
{
    [SerializeField] private InGameHUD inGameHUD;

    private void Start()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnSecondElapsed += UpdateTimerUI;

            InGameManager.Instance.OnTimerEnded += inGameHUD.SetBossChaseActive;
        }
    }

    private void OnEnable()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnSecondElapsed += UpdateTimerUI;
            InGameManager.Instance.OnTimerEnded += inGameHUD.SetBossChaseActive;
        }
    }

    private void OnDisable()
    {
        if (InGameManager.Instance != null)
        {
            InGameManager.Instance.OnSecondElapsed -= UpdateTimerUI;
            InGameManager.Instance.OnTimerEnded -= inGameHUD.SetBossChaseActive;
        }
    }

    private void UpdateTimerUI(float remainingSeconds)
    {
        inGameHUD.UpdateTimer(remainingSeconds);
    }

    public void SetEscapeUI(bool isActive)
    {
        inGameHUD.SetEscapeUIActivate(isActive);
    }
    public void UpdateEscapeGauge(float progress01)
    {
        inGameHUD.UpdateEscapeProgress(progress01);
    }
}

using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using static GameEnums;

public class AdFlowController : MonoBehaviour
{
    public static AdFlowController Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void TryShowInterstitial(System.Action onComplete = null)
    {
        if (!AdSDK.Instance.IsInitialized)
        {
            onComplete?.Invoke();
            return;
        }

        if (AdSDK.Instance.TryShowInterstitial())
        {
            AdSDK.Instance.SetInterstitialClosedCallback(() =>
            {
                onComplete?.Invoke();
            });
            return;
        }

        // 광고 못 띄우면 즉시 진행
        onComplete?.Invoke();
    }


}


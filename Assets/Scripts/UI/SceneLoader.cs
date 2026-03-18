using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using DG.Tweening;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;

    [Header("UI References")]
    public GameObject loadingPanel;
    public CanvasGroup loadingCanvasGroup;
    public CanvasGroup blackCanvasGroup;
    public Image loadingBarImage;

    private Material _burnMaterial;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); 
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        //  초기화
        if (loadingBarImage != null)
            _burnMaterial = loadingBarImage.material;

        InitUI();
    }

    private void InitUI()
    {
        loadingPanel.SetActive(false);
        loadingCanvasGroup.alpha = 0;
        blackCanvasGroup.alpha = 0;
        blackCanvasGroup.gameObject.SetActive(false);
    }

    public void LoadGameScene(string sceneName)
    {
        // 코루틴 중복 실행 방지
        StopAllCoroutines();
        StartCoroutine(LoadAsync(sceneName));
    }

    IEnumerator LoadAsync(string sceneName)
    {
        // 로딩 UI 켜기
        loadingPanel.SetActive(true);
        loadingCanvasGroup.DOFade(1f, 0.5f);

        float progress = 0;
        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);
        operation.allowSceneActivation = false;

        // 게이지 채우기
        while (progress < 1.0f)
        {
            float targetProgress = Mathf.Clamp01(operation.progress / 0.9f);
            progress = Mathf.MoveTowards(progress, targetProgress, Time.deltaTime * 0.5f);

            if (loadingBarImage != null) loadingBarImage.fillAmount = progress;
            if (_burnMaterial != null) _burnMaterial.SetFloat("_FillAmount", progress);

            // 게이지가 다 찼을 때 검은 화면 덮기
            if (progress >= 0.99f && blackCanvasGroup.alpha < 1 && !DOTween.IsTweening(blackCanvasGroup))
            {
                blackCanvasGroup.gameObject.SetActive(true);
                blackCanvasGroup.DOFade(1f, 0.5f);
            }

            if (progress >= 1f && operation.progress >= 0.9f)
            {
                // 씬 전환 허용
                operation.allowSceneActivation = true;
            }
            yield return null;
        }

        // 씬 전환이 완전히 완료될 때까지 대기
        while (!operation.isDone)
        {
            yield return null;
        }

        // 인게임 진입 후 연출 (밝아지는 효과)
        yield return new WaitForSeconds(1.5f);

        loadingCanvasGroup.alpha = 0; // 로딩바는 즉시 숨김

        if (blackCanvasGroup != null)
        {
            // 검은 화면을 서서히 걷어냄
            yield return blackCanvasGroup.DOFade(0f, 1.5f).SetEase(Ease.OutQuart).WaitForCompletion();

            blackCanvasGroup.gameObject.SetActive(false);
            loadingPanel.SetActive(false);
        }
    }

    // 검은판넬로만 씬 전환
    public void LoadSceneWithFade(string sceneName)
    {
        StopAllCoroutines();
        StartCoroutine(FadeSimpleAsync(sceneName));
    }

    IEnumerator FadeSimpleAsync(string sceneName)
    {
        blackCanvasGroup.gameObject.SetActive(true);
        yield return blackCanvasGroup.DOFade(1f, 0.5f).SetEase(Ease.OutQuad).WaitForCompletion();

        AsyncOperation operation = SceneManager.LoadSceneAsync(sceneName);

        while (!operation.isDone)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.3f);

        yield return blackCanvasGroup.DOFade(0f, 0.8f).SetEase(Ease.InSine).WaitForCompletion();

        blackCanvasGroup.gameObject.SetActive(false);
        loadingPanel.SetActive(false);
    }
}
using UnityEngine;
using TMPro;
using System.Collections;

public class LoadingTextEffect : MonoBehaviour
{
    public TextMeshProUGUI loadingText;
    public string baseText = "Loading";
    public float speed = 0.5f;

    void OnEnable() // 로딩창이 켜질 때 시작
    {
        StartCoroutine(AnimateDots());
    }

    IEnumerator AnimateDots()
    {
        while (true)
        {
            loadingText.text = baseText + ".";
            yield return new WaitForSeconds(speed);
            loadingText.text = baseText + "..";
            yield return new WaitForSeconds(speed);
            loadingText.text = baseText + "...";
            yield return new WaitForSeconds(speed);
        }
    }
}
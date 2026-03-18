using UnityEngine;
using UnityEngine.Audio;

public class SceneSoundPlayer : MonoBehaviour
{
    [Header("BGM Settings")]
    [SerializeField] private AudioClip sceneBGM;
    [SerializeField] bool playOnStart = true;

    private void Start()
    {
        if (playOnStart && sceneBGM != null)
        {
            // 이전에 만든 SoundManager 인스턴스를 통해 재생
            SoundManager.Instance.PlayBGM(sceneBGM);
        }
    }
}
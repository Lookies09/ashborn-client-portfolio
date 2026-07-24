using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class EffectSound : MonoBehaviour
{
    [Header("사운드 설정")]
    [SerializeField] private bool isLooping = false;

    private AudioSource _audioSource;

    void Awake()
    {
        _audioSource = GetComponent<AudioSource>();

        // 인스펙터 설정을 스크립트가 한 번 더 확인
        _audioSource.loop = isLooping;
        _audioSource.playOnAwake = false; 
    }

    void OnEnable()
    {
        if (_audioSource == null) _audioSource = GetComponent<AudioSource>();
        if (_audioSource != null)
        {
            _audioSource.time = 0f;
            _audioSource.Play();
        }
    }

    void OnDisable()
    {
        // 이펙트 오브젝트가 SetActive(false) 되거나 파괴될 때 소리 즉시 정지
        if (_audioSource != null && _audioSource.isPlaying)
        {
            _audioSource.Stop();
        }
    }
}
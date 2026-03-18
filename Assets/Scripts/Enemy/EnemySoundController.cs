using UnityEngine;
using UnityEngine.Audio;

public class EnemySoundController : MonoBehaviour
{
    [Header("Audio Sources")]
    [SerializeField] private AudioSource audioSource;

    [Header("Audio Clips")]
    [SerializeField] private AudioClip[] playerNotFoundStateAudioClips;
    [SerializeField] private AudioClip[] investigateAudioClip;
    [SerializeField] private AudioClip[] chaseAudioClips;
    [SerializeField] private AudioClip[] attackAudioClips;
    [SerializeField] private AudioClip[] deadAudioClips;

    private bool isDead = false;

    public void OnStateChanged(EnemyState newState)
    {
        switch (newState)
        {
            case EnemyState.IDLE:
                if (playerNotFoundStateAudioClips == null) break;
                PlayRandomClip(playerNotFoundStateAudioClips);
                break;
            case EnemyState.PATROL:
                PlayRandomClip(playerNotFoundStateAudioClips);
                break;
            case EnemyState.WANDER:
                PlayRandomClip(playerNotFoundStateAudioClips);
                break;
            case EnemyState.INVESTIGATE:
                PlayRandomClip(investigateAudioClip);
                break;
            case EnemyState.CHASE:
                PlayRandomClip(chaseAudioClips);
                break;
            case EnemyState.COMBAT:
                PlayRandomClip(attackAudioClips);
                break;
            case EnemyState.DEAD:
                if (deadAudioClips == null) return;
                if (isDead) return;
                isDead = true;
                PlayRandomClip(deadAudioClips);
                break;
        }
    }

    private void PlayRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return;

        int randomIndex = Random.Range(0, clips.Length);
        AudioClip clip = clips[randomIndex];

        if (clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void PlayDeathSound()
    {
        if (deadAudioClips == null || deadAudioClips.Length == 0) return;

        int randomIndex = Random.Range(0, deadAudioClips.Length);
        AudioClip clip = deadAudioClips[randomIndex];

        if (clip != null)
        {
            // 사망 소리는 객체가 파괴되어도 들려야 하므로 SoundManager 이용
            // 만약 SoundManager에 PlaySFX가 있다면 아래와 같이 사용
            SoundManager.Instance?.PlaySFX(clip);
        }
    }
}

using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource sfxSource;


    [SerializeField] private AudioClip goldChangeClip;
    [SerializeField] private AudioClip notEnoughGoldClip; 
    [SerializeField] private AudioClip levelUpClip;
    [SerializeField] private AudioClip equipClip;
    [SerializeField] private AudioClip unEquipClip;
    [SerializeField] private AudioClip inventoryOpenClip;
    [SerializeField] private AudioClip inventoryCloseClip;
    [SerializeField] private AudioClip quickItemAddClip;
    [SerializeField] private AudioClip chestOpenClip;
    [SerializeField] private AudioClip chestCloseClip;

    [SerializeField] private AudioClip upgradeUIOpenClip;
    [SerializeField] private AudioClip upgradeUICloseClip;
    [SerializeField] private AudioClip settingOpenClip;

    [SerializeField] private AudioClip enterDungeonaudioClip;


    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
    }

    // 배경음 재생
    public void PlayBGM(AudioClip clip)
    {
        bgmSource.clip = clip;
        bgmSource.loop = true;
        bgmSource.Play();
    }

    // 효과음 재생
    public void PlaySFX(AudioClip clip)
    {
        sfxSource.PlayOneShot(clip);
    }

    public void PlayQuickItemAddSound()
    {
        PlaySFX(quickItemAddClip);
    }

    public void PlayInventoryOpenSound()
    {
        PlaySFX(inventoryOpenClip);
    }

    public void PlayInventoryCloseSound()
    {
        PlaySFX(inventoryCloseClip);
    }

    public void NotEnoughGoldSound()
    {
        PlaySFX(notEnoughGoldClip);
    }

    // 설정창에서 호출할 볼륨 조절 함수
    public void SetGroupVolume(string parameterName, float volume)
    {
        // volume은 0~1 사이 값, Mixer는 -80dB~0dB를 사용하므로 계산 필요
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20;
        audioMixer.SetFloat(parameterName, dB);
    }

    public void PlayButtonSound(AudioClip clip)
    {
        if (clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void PlayGoldChangeSound()
    {
        PlaySFX(goldChangeClip);
    }

    public void PlayLevelUpSound()
    {
        PlaySFX(levelUpClip);
    }

    public void PlayEquipSound()
    {
        PlaySFX(equipClip);
    }

    public void PlayUnEquipSound()
    {
        PlaySFX(unEquipClip);
    }

    public void PlayChestOpen()
    {
        PlaySFX(chestOpenClip);
    }

    public void PlayChestClose()
    {
        PlaySFX(chestCloseClip);
    }

    public void PlayUpgradeUIOpenSound()
    {
        PlaySFX(upgradeUIOpenClip);
    }

    public void PlayUpgradeUICloseSound()
    {
        PlaySFX(upgradeUICloseClip);
    }

    public void PlaySettingOpenSound()
    { 
        PlaySFX(settingOpenClip);
    }

    public void EnterDungeonSound()
    {
        PlaySFX(enterDungeonaudioClip);
    }
}
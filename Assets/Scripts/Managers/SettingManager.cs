using UnityEngine;
using UnityEngine.Audio;
using TMPro;
using UnityEngine.UI;

public class SettingManager : MonoBehaviour
{
    public static SettingManager Instance { get; private set; }

    [Header("Audio Settings")]
    [SerializeField] private AudioMixer mainMixer;   

    [SerializeField] private GameObject settingPanel;

    [SerializeField] private string creditUrl = "https://lookies09.github.io/ASH-BORN-Credit/";

    public const string BGM_PARAM = "BGM";
    public const string SFX_PARAM = "SFX";
    public const string MONSTER_PARAM = "Monster";
    public const string FPS_PARAM = "SavedFPSIndex";

    private VariableJoystick _joystick;

    public GameObject SettingPanel => settingPanel;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        //LoadSettings();


        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceaneChanged += LoadSettings;
        }
    }

    private void OnDisable()
    {

        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnSceaneChanged -= LoadSettings;
        }
    }

    public void SetBGMVolume(float volume) => SetMixerVolume(BGM_PARAM, volume);
    public void SetSFXVolume(float volume) => SetMixerVolume(SFX_PARAM, volume);
    public void SetMonsterVolume(float volume) => SetMixerVolume(MONSTER_PARAM, volume);

    private void SetMixerVolume(string parameter, float volume)
    {
        float dB = Mathf.Log10(Mathf.Max(0.0001f, volume)) * 20f;
        mainMixer.SetFloat(parameter, dB);
        PlayerPrefs.SetFloat(parameter, volume);
    }

    // --- [ 프레임 설정 (드롭다운 연결용) ] ---
    public void OnFrameRateChanged(int index)
    {
        int targetFPS = 60;
        switch (index)
        {
            case 0: targetFPS = 30; break;
            case 1: targetFPS = 60; break;
            case 2: targetFPS = 120; break;
        }

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = targetFPS;
        PlayerPrefs.SetInt(FPS_PARAM, index);
    }

    // --- [ 조이스틱 설정 (토글 연결용) ] ---
    public void OnJoystickModeChanged(bool isFloating)
    {
        PlayerPrefs.SetInt("JoystickDynamic", isFloating ? 1 : 0);
        PlayerPrefs.Save(); 

        if (_joystick == null)
        {
            return;
        }

        if (isFloating)
            _joystick.SetMode(JoystickType.Floating);
        else
            _joystick.SetMode(JoystickType.Fixed);
    }

    // --- [ 데이터 로드 ] ---
    private void LoadSettings()
    {
        // 사운드 복구
        SetBGMVolume(PlayerPrefs.GetFloat(BGM_PARAM, 0.75f));
        SetSFXVolume(PlayerPrefs.GetFloat(SFX_PARAM, 0.75f));
        SetMonsterVolume(PlayerPrefs.GetFloat(MONSTER_PARAM, 0.75f));

        // 프레임 복구
        int fpsIndex = PlayerPrefs.GetInt(FPS_PARAM, 1);
        OnFrameRateChanged(fpsIndex);

        // 조이스틱 복구
        bool isDynamic = PlayerPrefs.GetInt("JoystickDynamic", 0) == 1;
        OnJoystickModeChanged(isDynamic);
    }


    public void RegisterJoystick(VariableJoystick newJoystick)
    {
        _joystick = newJoystick;

        bool isFloating = PlayerPrefs.GetInt("JoystickDynamic", 0) == 1;
        OnJoystickModeChanged(isFloating);
    }

    public void SetSettinPanel(bool isActive)
    {
        if (settingPanel == null)
        {
            settingPanel = SettingManager.Instance.SettingPanel;
        }
        settingPanel.SetActive(isActive);
        UIManager.Instance.ApplyPause(isActive);
    }

    public void OnCreditButtonClick()
    {
        if (!string.IsNullOrEmpty(creditUrl))
        {
            Application.OpenURL(creditUrl);
        }
    }
}
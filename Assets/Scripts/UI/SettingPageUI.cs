using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SettingPageUI : MonoBehaviour
{
    [Header("Manager Reference")]
    [SerializeField] private SettingManager manager;

    [Header("UI Elements")]
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private Slider monsterSlider;
    [SerializeField] private TMP_Dropdown fpsDropdown;
    [SerializeField] private Button floatingBtn;
    [SerializeField] private Button fixedBtn;

    [Header("Button Colors")]
    [SerializeField] private Color activeColor = Color.white;
    [SerializeField] private Color inactiveColor = Color.gray;


    private void Start()
    {
        floatingBtn.onClick.AddListener(() => OnJoystickButtonClicked(true));
        fixedBtn.onClick.AddListener(() => OnJoystickButtonClicked(false));


        bgmSlider.onValueChanged.AddListener(manager.SetBGMVolume);
        sfxSlider.onValueChanged.AddListener(manager.SetSFXVolume);
        monsterSlider.onValueChanged.AddListener(manager.SetMonsterVolume);
        fpsDropdown.onValueChanged.AddListener(manager.OnFrameRateChanged);


        RefreshUI();
    }

    private void OnEnable()
    {
        RefreshUI();
    }

    public void RefreshUI()
    {
        // 매니저에 저장된 PlayerPrefs 값을 슬라이더 등에 다시 뿌려줌
        bgmSlider.value = PlayerPrefs.GetFloat(SettingManager.BGM_PARAM, 0.75f);
        sfxSlider.value = PlayerPrefs.GetFloat(SettingManager.SFX_PARAM, 0.75f);
        monsterSlider.value = PlayerPrefs.GetFloat(SettingManager.MONSTER_PARAM, 0.75f);

        int savedFPSIndex = PlayerPrefs.GetInt(SettingManager.FPS_PARAM, 1);
        Debug.Log(savedFPSIndex);
        fpsDropdown.value = savedFPSIndex;
        fpsDropdown.RefreshShownValue();

        bool isDynamic = PlayerPrefs.GetInt("JoystickDynamic", 0) == 1;
        UpdateButtonVisuals(isDynamic);
    }

    private void UpdateButtonVisuals(bool isDynamic)
    {
        // 선택된 버튼은 밝게, 선택되지 않은 버튼은 어둡게 표시
        floatingBtn.GetComponent<Image>().color = isDynamic ? activeColor : inactiveColor;
        fixedBtn.GetComponent<Image>().color = isDynamic ? inactiveColor : activeColor;
    }

    private void OnJoystickButtonClicked(bool isDynamic)
    {
        // 매니저에게 로직 실행 요청
        manager.OnJoystickModeChanged(isDynamic);

        // 버튼 불빛(색상) 업데이트
        UpdateButtonVisuals(isDynamic);
    }

   
}
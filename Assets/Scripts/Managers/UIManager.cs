using UnityEngine;
using System.Collections;

/// <summary>
/// 게임 전반의 UI 루트. 하위 HUD/패널 컨트롤러를 참조만 노출하고
/// 글로벌 UI 상태 전환(예: 일시정지, HUD 숨김)을 담당합니다.
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [Header("HUD & Panel Roots")]
    [SerializeField] private GameObject playerHudRoot;
    [SerializeField] private GameObject inventoryPanel;
    [SerializeField] private GameObject statUpgradePanel;
    [SerializeField] private GameObject gameEndInfoPanel;


    [Header("UI 컨트롤러")]
    [Tooltip("인벤토리 윈도우 컨트롤러 (상자 인벤토리 UI 관리)")]
    [SerializeField] private InventoryWindowController inventoryWindowController;

    [SerializeField] private GameEndUI gameEndUI;
    [SerializeField] private ShopController shopController;
    [SerializeField] private ItemInfoModal itemInfoModal;

    private Coroutine _pauseCoroutine;
    public GameObject PlayerHudRoot => playerHudRoot;
    public GameObject InventoryPanel => inventoryPanel;
    public GameObject StatUpgradePanel => statUpgradePanel;
    public GameObject GameEndInfoPanel => gameEndInfoPanel;
    public InventoryWindowController InventoryWindowController => inventoryWindowController;

    public ShopController ShopController => shopController;

    private bool _isPaused;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void TogglePause()
    {
        SetPause(!_isPaused);
    }

    public void SetPause(bool pause, float delay = 0f)
    {
        // 진행 중인 일시정지 예약이 있다면 취소
        if (_pauseCoroutine != null)
        {
            StopCoroutine(_pauseCoroutine);
            _pauseCoroutine = null;
        }

        if (pause && delay > 0f)
        {
            // 지연 후 일시정지 실행
            _pauseCoroutine = StartCoroutine(Co_SetPauseWithDelay(delay));
        }
        else
        {
            // 즉시 실행
            ApplyPause(pause);
        }
    }

    public void ApplyPause(bool pause)
    {
        _isPaused = pause;
        Time.timeScale = pause ? 0f : 1f;
    }

    private IEnumerator Co_SetPauseWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ApplyPause(true);
        _pauseCoroutine = null;
    }

    public void SetPlayerHudVisible(bool visible)
    {
        if (playerHudRoot != null)
            playerHudRoot.SetActive(visible);
    }


    public void SetInventoryVisible(bool visible)
    {
        if (visible)
        {
            SoundManager.Instance.PlayInventoryOpenSound();
        }
        else
        {  SoundManager.Instance.PlayInventoryCloseSound();

        }

        if (inventoryPanel != null)
            inventoryPanel.SetActive(visible);
    }
    
    public void SetShopVisible(bool visible)
    {
        if (shopController != null)
            shopController.gameObject.SetActive(visible);
            SetInventoryVisible(visible);
    }

    public void SetStatUpgradeVisible(bool visible)
    {
        if (statUpgradePanel != null)
            statUpgradePanel.SetActive(visible);
    }


    public void SetGameEndInfoVisible(bool visible, bool isEscaped, float delayTime)
    {
        if (gameEndUI != null && gameEndInfoPanel != null)
        {
            gameEndUI.SetUIByResult(isEscaped);
            gameEndInfoPanel.SetActive(visible);
            SetPause(visible, delayTime);
        }            
    }

    public void ShowItemDetailModal(ItemDataSO itemData)
    {
        itemInfoModal.Open(itemData);
    }
}



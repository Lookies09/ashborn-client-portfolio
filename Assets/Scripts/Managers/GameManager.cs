using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameEnums.GameState CurrentState { get; private set; }

    public event Action OnSceaneChanged;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        ChangeState(GameEnums.GameState.Lobby);
    }

    public void ChangeState(GameEnums.GameState next)
    {
        CurrentState = next;
        OnSceaneChanged?.Invoke();
        Debug.Log($"[GameState] -> {next}");
    }

    public void GoToGame()
    {

        ChangeState(GameEnums.GameState.InGame);
        SceneLoader.Instance.LoadGameScene(SceneNames.Game);

        UIManager.Instance.SetPause(false);
    }

    public void OnGameOver(bool isEscaped, float delayTime = 2f)
    {
        ChangeState(GameEnums.GameState.Result);
        UIManager.Instance.SetGameEndInfoVisible(true ,isEscaped, delayTime);
    }

    public void GoToLobby()
    {
        bool isCleared = InGameManager.Instance.IsEscapeSuccessful;
        bool watchedAd = !InGameManager.Instance.CanGetGoldBonus;

        if (!isCleared)
        {
            ResetPlayerData();
        }

        PlayerWallet.Instance.EndStage(isCleared, watchedAd);

        ObjectPoolManager.Instance.DespawnAll();

        AdFlowController.Instance.TryShowInterstitial(() =>
        {           
            ChangeState(GameEnums.GameState.Lobby);
            SceneLoader.Instance.LoadSceneWithFade(SceneNames.Lobby);
            UIManager.Instance.SetPause(false);
        });        
    }

    public void ResetPlayerData()
    {
        InventoryManager.PlayerInventory.ClearInventory();
        EquipmentManager.Instance.ClearAllEquipments();
        QuickSlotManager.Instance.ClearAllQuickSlots();
    }

}

using UnityEngine.SceneManagement;
using static GameEnums;

public class SceneFlowController
{
    public void LoadLobby()
    {
        GameManager.Instance.ChangeState(GameState.Lobby);
        SceneManager.LoadScene("LobbyScene");
    }

    public void LoadInGame()
    {
        GameManager.Instance.ChangeState(GameState.InGame);
        SceneManager.LoadScene("InGameScene");
    }

    public void LoadResult()
    {
        GameManager.Instance.ChangeState(GameState.Result);
        SceneManager.LoadScene("ResultScene");
    }

}

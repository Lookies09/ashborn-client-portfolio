/// <summary>
/// 상호작용 가능한 오브젝트가 구현해야 하는 인터페이스
/// (상자, 문, NPC, 상점 등)
/// </summary>
public interface IInteractable
{
    /// <summary>
    /// 플레이어가 상호작용 범위에 들어왔을 때 호출
    /// </summary>
    void OnFocus(PlayerInteractor interactor);

    /// <summary>
    /// 플레이어가 상호작용 범위에서 벗어났을 때 호출
    /// </summary>
    void OnLoseFocus(PlayerInteractor interactor);

    /// <summary>
    /// 실제로 상호작용 버튼을 눌렀을 때 호출
    /// </summary>
    void Interact(PlayerInteractor interactor);
}

using UnityEngine;

/// <summary>
/// 플레이어의 상호작용을 감지하고 처리하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Collider))]
public class PlayerInteractor : MonoBehaviour
{
    [Header("참조")]
    [Tooltip("플레이어 HUD 컴포넌트")]
    [SerializeField] private PlayerHUD playerHUD;
    
    [Tooltip("플레이어 인벤토리 매니저")]
    [SerializeField] private InventoryManager playerInventory;

    [Header("상호작용 설정")]
    [Tooltip("상호작용 가능한 레이어 마스크")]
    [SerializeField] private LayerMask interactableLayer;
    
    [Tooltip("최대 상호작용 거리")]
    [SerializeField] private float maxInteractDistance = 3f;

    private IInteractable _currentInteractable;

    public InventoryManager PlayerInventory => playerInventory;

    private void Awake()
    {
        // Collider가 Trigger로 설정되어 있는지 확인
        Collider col = GetComponent<Collider>();
        if (col != null && !col.isTrigger)
        {
            Debug.LogWarning($"PlayerInteractor: Collider가 Trigger로 설정되어 있지 않습니다. {gameObject.name}");
        }

        // 초기 상태 설정
        if (playerHUD != null)
        {
            playerHUD.SetInteractButtonVisible(false);
            playerHUD.SetInteractButtonHandler(null);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        TrySetInteractable(other);
    }

    private void OnTriggerExit(Collider other)
    {
        var interactable = other.GetComponent<IInteractable>();
        if (interactable != null && interactable == _currentInteractable)
        {
            ClearCurrentInteractable();
        }
    }

    /// <summary>
    /// 충돌한 오브젝트가 상호작용 가능한지 확인하고 설정
    /// </summary>
    private void TrySetInteractable(Collider other)
    {
        // 레이어 확인
        if (((1 << other.gameObject.layer) & interactableLayer.value) == 0)
            return;

        // IInteractable 컴포넌트 확인
        var interactable = other.GetComponent<IInteractable>();
        if (interactable == null)
            return;

        // 거리 확인
        float distance = Vector3.Distance(transform.position, other.transform.position);
        if (distance > maxInteractDistance)
            return;

        SetCurrentInteractable(interactable);
    }

    /// <summary>
    /// 현재 상호작용 대상을 설정
    /// </summary>
    private void SetCurrentInteractable(IInteractable interactable)
    {
        if (_currentInteractable == interactable)
            return;

        // 기존 상호작용 대상에게 포커스 해제 알림
        _currentInteractable?.OnLoseFocus(this);

        // 새로운 상호작용 대상 설정
        _currentInteractable = interactable;
        _currentInteractable.OnFocus(this);

        // HUD 버튼 활성화 및 핸들러 설정
        if (playerHUD != null)
        {
            playerHUD.SetInteractButtonVisible(true);
            playerHUD.SetInteractButtonHandler(OnInteractButtonClicked);
        }
    }

    /// <summary>
    /// 현재 상호작용 대상 제거
    /// </summary>
    private void ClearCurrentInteractable()
    {
        _currentInteractable?.OnLoseFocus(this);
        _currentInteractable = null;

        if (playerHUD != null)
        {
            playerHUD.SetInteractButtonVisible(false);
            playerHUD.SetInteractButtonHandler(null);
        }
    }

    /// <summary>
    /// 상호작용 버튼이 클릭되었을 때 호출
    /// </summary>
    public void OnInteractButtonClicked()
    {
        if (_currentInteractable == null)
            return;

        _currentInteractable.Interact(this);
    }
}

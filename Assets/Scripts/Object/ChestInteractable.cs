using UnityEngine;

/// <summary>
/// 상자의 상호작용을 구현하는 컴포넌트
/// </summary>
[RequireComponent(typeof(Collider))]
public class ChestInteractable : MonoBehaviour, IInteractable
{
    [Header("인벤토리 설정")]
    [Tooltip("상자 인벤토리 매니저")]
    [SerializeField] private InventoryManager chestInventory;

    [Header("UI 참조")]
    [Tooltip("인벤토리 윈도우 컨트롤러 (UIManager에서 가져옴)")]
    private InventoryWindowController _inventoryWindowController;

    [Header("애니메이션 (선택사항)")]
    [Tooltip("상자 애니메이터 (열기/닫기 애니메이션용)")]
    [SerializeField] private Animator chestAnimator;
    
    [Tooltip("애니메이터 파라미터 이름")]
    [SerializeField] private string animatorBoolName = "IsOpen";
    private bool _isOpen = false;

    private void OnEnable()
    {
        if (_inventoryWindowController == null && UIManager.Instance != null)
        {
            _inventoryWindowController = UIManager.Instance.InventoryWindowController;
        }

        if (_inventoryWindowController == null)
        {
            _inventoryWindowController = FindFirstObjectByType<InventoryWindowController>();
        }
    }

    public void OnFocus(PlayerInteractor interactor)
    {
        // 필요시 하이라이트 효과 등 추가 가능
        // 예: GetComponent<Outline>()?.enabled = true;
    }

    public void OnLoseFocus(PlayerInteractor interactor)
    {
        // 하이라이트 해제 등 추가 가능
        // 예: GetComponent<Outline>()?.enabled = false;
    }

    public void Interact(PlayerInteractor interactor)
    {
        if (_inventoryWindowController == null || chestInventory == null)
            return;

        if (_inventoryWindowController.IsChestOpen &&
            _inventoryWindowController.CurrentChestInventory == chestInventory)
        {
            _inventoryWindowController.CloseChest();
            PlayAnim(false);
        }
        else
        {
            _inventoryWindowController.OpenChest(chestInventory, this);
            PlayAnim(true);
        }
    }

    private void PlayAnim(bool isOpen)
    {
        if (chestAnimator != null)
            chestAnimator.SetBool(animatorBoolName, isOpen);
        _isOpen = isOpen;
    }

    public void CloseFromUI()
    {
        _isOpen = false;

        if (chestAnimator != null)
            chestAnimator.SetBool(animatorBoolName, false);
    }

}

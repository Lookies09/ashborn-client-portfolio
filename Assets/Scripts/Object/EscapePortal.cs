using UnityEngine;
using UnityEngine.UI;

public class EscapePortal3D : MonoBehaviour
{
    [Header("설정")]
    [SerializeField] private float escapeDelay = 3.0f;
    [SerializeField] private LayerMask playerLayer;

    private InGameHUDController _hudController;
    private float _currentTimer = 0f;
    private bool _isPlayerInside = false;
    private bool _hasEscaped = false;

    private void Awake()
    {
        _hudController = FindFirstObjectByType<InGameHUDController>();
    }

    private void Update()
    {
        if (_hasEscaped) return;

        // 타이머 계산
        if (_isPlayerInside) _currentTimer += Time.deltaTime;
        else _currentTimer = Mathf.Max(0, _currentTimer - Time.deltaTime);

        // UI 업데이트 (컨트롤러를 통해 전달)
        if (_hudController != null)
        {
            _hudController.UpdateEscapeGauge(_currentTimer / escapeDelay);
        }

        if (_currentTimer >= escapeDelay) ExecuteEscape();
    }

    private void ExecuteEscape()
    {
        if(_hasEscaped) return;
        _hasEscaped = true;
        InGameManager.Instance.OnEscaped();
    }

    // 3D 전용 트리거 함수
    private void OnTriggerEnter(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _isPlayerInside = true;
            if (_hudController != null) _hudController.SetEscapeUI(true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (((1 << other.gameObject.layer) & playerLayer) != 0)
        {
            _isPlayerInside = false;
            if (_hudController != null) _hudController.SetEscapeUI(false);
        }
    }
}
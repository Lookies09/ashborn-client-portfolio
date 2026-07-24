using Unity.Behavior;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

public class EnemyDetection : MonoBehaviour
{
    [SerializeField] private Transform eyePoint;

    private EnemyController _enemyController;
    private Transform _playerTransform;
    private PlayerMovement _playerMovement;
    private Vector3 _lastPlayerPosition;
    private bool _isSuspicious = false;
    private bool _isDetected = false;

    private IHealth _playerHealth;

    [SerializeField] private LayerMask sightMask;
    [SerializeField] private float suspicion = 0f;
    [SerializeField] private float suspicionGainRate = 30f;  // 초당 증가량
    [SerializeField] private float suspicionLossRate = 20f;  // 초당 감소량
    [SerializeField] private float maxSuspicion = 100f;   

    public float Suspicion => suspicion;
    public Vector3 LastPlayerPosition => _lastPlayerPosition;

    public void Init(EnemyController enemyController)
    {
        _enemyController = enemyController;
    }

    private void Start()
    {
        RefreshReferences();
    }

    private void Update()
    {
        if (_playerTransform == null) return;
        if(_enemyController.Health.IsDead) return;
        if (_playerHealth.IsDead) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        bool isMoving = IsPlayerWalking();
        bool inSight = IsInLineOfSight();


        _enemyController.BTAgent.GetVariable("IsInSight", out BlackboardVariable<bool> sight);
        if (sight != inSight)
        {
            _enemyController.BTAgent.SetVariableValue("IsInSight", inSight);
        }        

        // 플레이어가 의심 범위 내에서 움직이는 경우
        if (distance <= _enemyController.Data.SuspiciousRange && isMoving)
        {
            // 마지막 위치 갱신
            if (!_isSuspicious)
            {
                suspicion += suspicionGainRate * Time.deltaTime;
                _lastPlayerPosition = _playerTransform.position;
            }
        }
        else
        {
            // 감소
            suspicion -= suspicionLossRate * Time.deltaTime;
        }

        suspicion = Mathf.Clamp(suspicion, 0, maxSuspicion);

        // 시야 안이면 즉시 발견 처리
        if (distance <= _enemyController.Data.DetectionRange && inSight)
        {
            if (!_isDetected)
            {
                SetSuspicious(true);
                SetTargetDetected(true);
            }
        }

        // suspicion 최대 → Investigate
        if (suspicion >= maxSuspicion)
        {
            if (_isSuspicious) return;
            SetSuspicious(true);
        }

    }


    public void RefreshReferences()
    {
        // 상태 초기화
        suspicion = 0f;
        _isSuspicious = false;
        _isDetected = false;
        _lastPlayerPosition = Vector3.zero;

        // 새로운 플레이어 참조 갱신
        if (_enemyController != null && _enemyController.Player != null)
        {
            _playerTransform = _enemyController.Player.transform;
            _playerMovement = _enemyController.Player.GetComponent<PlayerMovement>();
            _playerHealth = _enemyController.Player.GetComponent<IHealth>();
            _lastPlayerPosition = _playerTransform.position;
        }
        else
        {
            // 플레이어를 못 찾았을 경우 참조 비우기
            _playerTransform = null;
            _playerMovement = null;
            _playerHealth = null;
        }
    }

    private bool IsPlayerWalking()
    {
         if (_playerMovement != null)
        {
            return _playerMovement.IsMoving;
        }

        return false;

        
    }

    private bool IsInLineOfSight()
    {        
        if (_playerTransform == null) return false;


        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);

        // 시야각 체크
        if (angle > _enemyController.Data.SightAngl * 0.5f) return false;

        // 장애물 체크 (레이캐스트)
        RaycastHit hit;
        if (Physics.Raycast(
            eyePoint.position,
            direction,
            out hit,
            _enemyController.Data.DetectionRange,
            sightMask
            ))
        {
            return hit.collider.transform == _playerTransform;
        }

            return false;
    }

    public void SetSuspicious(bool value)
    {
        if (_playerHealth.IsDead)
        {
            _enemyController.BTAgent.SetVariableValue("IsSuspicious", false);
            _isSuspicious = false;
            return;
        }

        if (_enemyController != null && _enemyController.BTAgent != null)
        {            
            _enemyController.BTAgent.SetVariableValue("IsSuspicious", value);
        }

        _isSuspicious = value;
    }

    public void SetTargetDetected(bool value)
    {
        if (_playerHealth.IsDead)
        {
            _enemyController.BTAgent.SetVariableValue("IsTargetDetected", false);
            _isDetected = false;
            return;
        }

        if (_enemyController != null && _enemyController.BTAgent != null)
        {            
            _enemyController.BTAgent.SetVariableValue("IsTargetDetected", value);
        }
        _isDetected = value;
    }


    public void SetChaseing()
    {

        if (_playerTransform == null || _playerTransform.Equals(null)) return;

        float directDistance = Vector3.Distance(_playerTransform.position, transform.position);

        bool inSight = IsInLineOfSight();

        // 추적 거리 밖이면 즉시 추적 종료
        if (directDistance > _enemyController.Data.ChaseRange)
        {
            SetSuspicious(false);
            SetTargetDetected(false);
            suspicion = 0f;
            return;
        }

        // 시아 안이라면 즉시 최대치로 설정
        if (inSight)
        {
            suspicion = maxSuspicion;
            return;
        }

        // 추적 범위 안이지만 시야에 안보이면 10초 후 종료로직
        suspicion -= (maxSuspicion/10) * Time.deltaTime;

        if (suspicion > 0f)
        {
            _isDetected = true; // 아직 추적 유지
            return;
        }


        // 3) 일정 시간 지나면 추적 종료
        if (suspicion <= 0f)
        {
            SetSuspicious(false);
            SetTargetDetected(false);
            suspicion = 0f;
        }
    }


    private void OnDrawGizmos()
    {
        if (eyePoint == null) return;
        if (_enemyController == null) return;
        if (_playerTransform == null) return;

        Vector3 origin = eyePoint.position;
        Vector3 direction = (_playerTransform.position - transform.position).normalized;
        float range = _enemyController.Data.DetectionRange;

        Gizmos.color = Color.cyan; // 레이 방향
        Gizmos.DrawLine(origin, origin + direction * range);

        // 충돌 지점 표시
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, range))
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(hit.point, 0.1f);
        }
    }

}

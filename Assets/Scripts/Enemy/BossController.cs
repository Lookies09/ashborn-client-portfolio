using UnityEngine;

/// <summary>
/// 보스 컨트롤러: 일반 몬스터보다 복잡한 패턴 (여전히 단순 거리 계산 기반)
/// </summary>
public class BossController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float attackRange = 3f;
    [SerializeField] private float chargeRange = 10f;
    [SerializeField] private float chargeCooldown = 5f;
    [SerializeField] private int attackDamage = 30;
    
    private Transform _playerTransform;
    private IHealth _playerHealth;
    private EnemyHealth _bossHealth;
    private CharacterController _cc;
    
    private float _lastChargeTime = 0f;
    private bool _isCharging = false;
    private Vector3 _chargeDirection;

    private void Awake()
    {
        _cc = GetComponent<CharacterController>();
        _bossHealth = GetComponent<EnemyHealth>();
    }

    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _playerTransform = player.transform;
            _playerHealth = player.GetComponent<IHealth>();
        }
    }

    private void Update()
    {
        if (_bossHealth.IsDead || _playerTransform == null) return;
        if (_playerHealth != null && _playerHealth.IsDead) return;

        float distance = Vector3.Distance(transform.position, _playerTransform.position);

        // 돌진 패턴: 일정 거리 이상이고 쿨다운이 지났으면
        if (!_isCharging && distance > chargeRange && Time.time >= _lastChargeTime + chargeCooldown)
        {
            StartCharge();
        }

        if (_isCharging)
        {
            ChargeAttack();
        }
        else
        {
            // 일반 이동 및 공격
            MoveTowardsPlayer();
            TryAttack(distance);
        }
    }

    private void MoveTowardsPlayer()
    {
        Vector3 direction = (_playerTransform.position - transform.position);
        direction.y = 0f;
        direction.Normalize();
        
        _cc.Move(direction * moveSpeed * Time.deltaTime);
        
        transform.rotation = Quaternion.Slerp(
            transform.rotation,
            Quaternion.LookRotation(direction),
            Time.deltaTime * 5f
        );
    }

    private void StartCharge()
    {
        _isCharging = true;
        _lastChargeTime = Time.time;
        _chargeDirection = (_playerTransform.position - transform.position);
        _chargeDirection.y = 0f;
        _chargeDirection.Normalize();
    }

    private void ChargeAttack()
    {
        // 돌진 이동 (더 빠른 속도)
        _cc.Move(_chargeDirection * moveSpeed * 3f * Time.deltaTime);
        
        // 플레이어와 충돌 체크
        float distance = Vector3.Distance(transform.position, _playerTransform.position);
        if (distance <= attackRange)
        {
            // 돌진 공격
            if (_playerHealth is IDamageable damageable)
            {
                damageable.TakeDamage(attackDamage);
            }
            _isCharging = false;
        }
        
        // 일정 시간 후 자동 종료 (플레이어를 놓쳤을 경우)
        if (Time.time >= _lastChargeTime + 2f)
        {
            _isCharging = false;
        }
    }

    private void TryAttack(float distance)
    {
        if (distance <= attackRange && Time.time >= _lastChargeTime + 1f)
        {
            if (_playerHealth is IDamageable damageable)
            {
                damageable.TakeDamage(attackDamage);
                _lastChargeTime = Time.time;
            }
        }
    }
}


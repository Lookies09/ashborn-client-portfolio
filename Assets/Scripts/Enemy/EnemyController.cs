using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적군 컨트롤러: 모든 적군 컴포넌트를 통합 관리
/// </summary>
public class EnemyController : MonoBehaviour
{
    [SerializeField] private EnemyMovement movement;
    [SerializeField] private EnemyHealth health;
    [SerializeField] private EnemyAttack attack;
    [SerializeField] private EnemyAnimationController animationController;
    [SerializeField] private EnemyDataSO enemyData;
    [SerializeField] private NavMeshAgent nevMeshAgent;
    [SerializeField] private BehaviorGraphAgent btAgent;
    [SerializeField] private EnemyDetection detection;
    [SerializeField] private Collider enemyColider;

    public GameObject WanderPoint { get; private set; }
    public GameObject[] PatrolPoints { get; private set; }

    private GameObject _player;

    public EnemyMovement Movement => movement;
    public EnemyHealth Health => health;
    public EnemyAttack Attack => attack;
    public NavMeshAgent NavAgent => nevMeshAgent;
    public BehaviorGraphAgent BTAgent => btAgent;
    public EnemyDataSO Data => enemyData;
    public EnemyAnimationController AnimationController => animationController;
    public EnemyDetection Detection => detection;
    public GameObject Player => _player;

    private bool _deathProcessed;
    private bool _isInitializing;
    private void Awake()
    {
        if (movement == null) movement = GetComponent<EnemyMovement>();
        if (health == null) health = GetComponent<EnemyHealth>();
        if (attack == null) attack = GetComponent<EnemyAttack>();
        if (nevMeshAgent == null) nevMeshAgent = GetComponent<NavMeshAgent>();
        if (btAgent == null) btAgent = GetComponent<BehaviorGraphAgent>();
        if(animationController == null) animationController = GetComponent<EnemyAnimationController>();
        if (detection == null) detection = GetComponent<EnemyDetection>();

        InitAll();
    }

    private void Start()
    {
        if (health != null)
        {
            health.OnKilled += HandleDeath;
            health.OnDead += HandleDeath;
        }
    }

    private void OnEnable()
    {
        UpdatePlayerReference();
    }

    private void OnDestroy()
    {
        if (health != null)
        {
            health.OnKilled -= HandleDeath;
            health.OnDead -= HandleDeath;
        }
    }

    private void UpdatePlayerReference()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag("Player");
        if (foundPlayer != null)
        {
            _player = foundPlayer;
            if (btAgent != null)
            {
                btAgent.SetVariableValue("Target", _player);
            }
        }
    }

    private void HandleDeath()
    {
        if (_isInitializing || _deathProcessed)
            return;
        enemyColider.isTrigger = true;
        btAgent.SetVariableValue("IsDead", true);

        _deathProcessed = true;

        // 사망 애니메이션 재생
        if (animationController != null)
        {
            animationController.PlayRandomDeath();
        }
        
        GrantRewards();
    }

    private void GrantRewards()
    {
        if (enemyData == null)
        {
            Debug.LogWarning($"{name}: EnemyDataSO가 설정되지 않아 경험치 정보를 찾을 수 없습니다.");
            return;
        }

        if (RewardManager.Instance == null)
        {
            Debug.Log("리워드 메니저가 없음");
            return;
        }

        // 보상 넘기기
        RewardManager.Instance.Grant(enemyData);
    }


    private void InitAll()
    {
        movement.Init(this);
        animationController.Init(this);
        detection.Init(this);
    }

    public void SetWanderData(GameObject spawnPoint, GameObject[] patrolPoints)
    {
        WanderPoint = spawnPoint;
        this.PatrolPoints = patrolPoints;
    }

    public void OnSpawnInitialize()
    {
        _isInitializing = true;

        WanderPoint = null;
        PatrolPoints = null;

        if (health != null) health.ResetHealth();
        if (enemyColider != null) enemyColider.isTrigger = false;

        if (btAgent != null)
        {
            btAgent.enabled = false;

            btAgent.SetVariableValue<bool>("IsDead", false);
            btAgent.SetVariableValue<bool>("IsSuspicious", false);
            btAgent.SetVariableValue<bool>("IsTargetDetected", false);

            btAgent.SetVariableValue<EnemyState>("EnemyState", EnemyState.IDLE);

            UpdatePlayerReference();
            btAgent.SetVariableValue<GameObject>("Target", Player);

            btAgent.enabled = true;
        }

        if (detection != null) detection.RefreshReferences();

        _deathProcessed = false;
        _isInitializing = false;
    }
}


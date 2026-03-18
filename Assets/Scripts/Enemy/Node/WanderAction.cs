using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Wander", story: "Wander Around [WanderPoint] Within [Radius]", category: "Action", id: "4c24ef1d5101d53c03c8c765f7122396")]
public partial class WanderAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> WanderPoint;
    [SerializeReference] public BlackboardVariable<float> Radius;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.5f);
    [SerializeReference] public BlackboardVariable<int> MaxSampleAttempts = new(10);

    private NavMeshAgent _navAgent;
    private Vector3 _currentTarget;
    private bool _hasDestination;
    private Vector3 _wanderCenter;

    protected override Status OnStart()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            Debug.LogError("WanderAction: EnemyController is null");
            return Status.Failure;
        }

        WanderPoint.Value = EnemyController.Value.WanderPoint;

        if (WanderPoint == null || WanderPoint.Value == null)
        {
            Debug.LogError("WanderAction: WanderPoint is null");
            return Status.Failure;
        }

        _navAgent = EnemyController.Value.NavAgent;
        if (_navAgent == null)
        {
            Debug.LogError("WanderAction: NavMeshAgent is null");
            return Status.Failure;
        }

        if (!_navAgent.enabled)
        {
            Debug.LogError("WanderAction: NavMeshAgent is not enabled");
            return Status.Failure;
        }

        _wanderCenter = WanderPoint.Value.transform.position;

        // 랜덤 위치로 이동
        if (MoveToRandomPosition())
        {
            return Status.Running;
        }
        else
        {
            Debug.LogWarning("WanderAction: Failed to find valid wander position");
            return Status.Failure;
        }
    }

    protected override Status OnUpdate()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            return Status.Failure;
        }

        if (WanderPoint == null || WanderPoint.Value == null)
        {
            return Status.Failure;
        }

        if (_navAgent == null || !_navAgent.enabled)
        {
            return Status.Failure;
        }

        // 도착 확인
        if (_hasDestination)
        {
            // 경로 계산이 완료되었는지 확인
            if (!_navAgent.pathPending)
            {
                // 도착했는지 확인
                if (_navAgent.remainingDistance <= DistanceThreshold.Value &&
                    (!_navAgent.hasPath || _navAgent.velocity.sqrMagnitude < 0.1f))
                {
                    // 도착 완료
                    return Status.Success;
                }
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.ResetPath();
        }
        _hasDestination = false;
    }

    private bool MoveToRandomPosition()
    {
        if (_navAgent == null || !_navAgent.isOnNavMesh)
        {
            return false;
        }

        // WanderCenter 업데이트 (WanderPoint가 이동할 수 있으므로)
        if (WanderPoint.Value != null)
        {
            _wanderCenter = WanderPoint.Value.transform.position;
        }

        // NavMesh 위의 유효한 랜덤 위치 찾기
        Vector3 randomPosition = FindRandomNavMeshPosition();

        if (randomPosition == Vector3.zero)
        {
            Debug.LogWarning("WanderAction: Could not find valid NavMesh position");
            return false;
        }

        _currentTarget = randomPosition;

        // NavMeshAgent로 이동 명령
        _navAgent.isStopped = false;
        bool success = _navAgent.SetDestination(_currentTarget);

        if (success)
        {
            _hasDestination = true;
            return true;
        }
        else
        {
            Debug.LogError($"WanderAction: Failed to set destination to {_currentTarget}");
            _hasDestination = false;
            return false;
        }
    }

    private Vector3 FindRandomNavMeshPosition()
    {
        if (_navAgent == null)
        {
            return Vector3.zero;
        }

        // NavMesh 위의 유효한 위치를 찾기 위해 여러 번 시도
        for (int i = 0; i < MaxSampleAttempts.Value; i++)
        {
            // 반경 내 랜덤 위치 생성
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * Radius.Value;
            Vector3 randomPosition = _wanderCenter + new Vector3(randomCircle.x, 0f, randomCircle.y);

            // NavMesh 위에 있는지 확인
            NavMeshHit hit;
            if (NavMesh.SamplePosition(
                randomPosition,
                out hit,
                Radius.Value, // 최대 검색 거리
                NavMesh.AllAreas))
            {
                // NavMesh 위의 유효한 위치를 찾았음
                return hit.position;
            }
        }

        // 모든 시도 실패 시, 현재 위치 근처에서 다시 시도
        Vector3 currentPos = _navAgent.transform.position;
        for (int i = 0; i < MaxSampleAttempts.Value; i++)
        {
            Vector2 randomCircle = UnityEngine.Random.insideUnitCircle * (Radius.Value * 0.5f);
            Vector3 randomPosition = currentPos + new Vector3(randomCircle.x, 0f, randomCircle.y);

            NavMeshHit hit;
            if (NavMesh.SamplePosition(
                randomPosition,
                out hit,
                Radius.Value * 0.5f,
                NavMesh.AllAreas))
            {
                return hit.position;
            }
        }

        // 여전히 실패하면 현재 위치 반환 (최후의 수단)
        return currentPos;
    }
}


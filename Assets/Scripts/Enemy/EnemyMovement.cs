using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 적군 이동: 플레이어를 향한 단순 거리 계산 기반 이동
/// </summary>
public class EnemyMovement : MonoBehaviour
{        
    private EnemyController _enemyController;

    private bool _hasArrived = true;
    public bool IsMoving => _enemyController.NavAgent.angularSpeed > 0;

    private void Update()
    {
        if (_enemyController.Health.IsDead) return;
        SetArrived();
    }

    public void Init(EnemyController enemyController)
    {
        _enemyController = enemyController;
    }

    private bool HasArrived()
    {
        if (_enemyController.NavAgent.pathPending) return false;

        if (_enemyController.NavAgent.remainingDistance > _enemyController.NavAgent.stoppingDistance)
            return false;

        if (_enemyController.NavAgent.hasPath && _enemyController.NavAgent.velocity.sqrMagnitude > 0f)
            return false;

        return true;
    }

    private void SetArrived()
    {
        bool arrivedNow = HasArrived();

        // 값이 변경되었을 때만 BT에 전달
        if (_hasArrived != arrivedNow)
        {
            _hasArrived = arrivedNow;
            _enemyController.BTAgent.SetVariableValue("HasArrived", arrivedNow);
        }
    }

    public void SetVelocity(EnemyState currentState)
    {        
        switch (currentState)
        {
            case EnemyState.PATROL:
                _enemyController.NavAgent.isStopped = false;
                _enemyController.NavAgent.speed = _enemyController.Data.WalkSpeed;
                break;
            case EnemyState.WANDER:
                _enemyController.NavAgent.isStopped = false;
                _enemyController.NavAgent.speed = _enemyController.Data.WalkSpeed;
                break;
            case EnemyState.INVESTIGATE:
                _enemyController.NavAgent.isStopped = false;
                _enemyController.NavAgent.speed = _enemyController.Data.WalkSpeed;
                break;
            case EnemyState.CHASE:
                _enemyController.NavAgent.isStopped = false;
                _enemyController.NavAgent.speed = _enemyController.Data.RunSpeed;
                break;
            case EnemyState.IDLE:
                _enemyController.NavAgent.isStopped = true;
                _enemyController.NavAgent.speed = 0;
                break;
            case EnemyState.COMBAT:
                _enemyController.NavAgent.isStopped = true;
                _enemyController.NavAgent.speed = _enemyController.Data.RunSpeed;
                break;
            case EnemyState.TAKE_COVER:
            case EnemyState.DODGE:
            case EnemyState.FLEE:
            case EnemyState.DEAD:
                if (_enemyController.Health.IsDead) break;
                _enemyController.NavAgent.isStopped = true;
                _enemyController.NavAgent.speed = 0;
                break;           

        }
    }
}


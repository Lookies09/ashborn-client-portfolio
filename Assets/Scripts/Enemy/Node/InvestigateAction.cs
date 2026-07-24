using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Investigate", story: "Do Investigate By [EnemyController]", category: "Action", id: "ec64d7c0335790e3cdb1870d328ee906")]
public partial class InvestigateAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.5f);

    private NavMeshAgent _navAgent;
    private Vector3 _currentTarget;
    private bool _hasDestination;

    protected override Status OnStart()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            Debug.LogError("WanderAction: EnemyController is null");
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

        MoveToPosition(EnemyController.Value.Detection.LastPlayerPosition);
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            return Status.Failure;
        }

        if (_navAgent == null || !_navAgent.enabled)
        {
            return Status.Failure;
        }

        if (_hasDestination)
        {
            if (!_navAgent.pathPending)
            {
                if (_navAgent.remainingDistance <= DistanceThreshold.Value &&
                    (!_navAgent.hasPath || _navAgent.velocity.sqrMagnitude < 0.1f))
                {

                    EnemyController.Value.AnimationController.PlayLookAround();
                    EnemyController.Value.Detection.SetSuspicious(false);
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

    private void MoveToPosition(Vector3 position)
    {
        if (_navAgent == null) return;
        NavMeshPath path = new NavMeshPath();
        if (_navAgent.CalculatePath(position, path))
        {
            _navAgent.SetDestination(position);
            _currentTarget = position;
            _hasDestination = true;
        }
        else
        {
            Debug.Log("해당 위치로 이동할수 없음");
            _hasDestination = false;
        }
    }
}


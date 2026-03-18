using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Custom Patrol", story: "Patrol using [EnemyController] and [Waypoints]", category: "Action", id: "6ca4bc42bc54086bbf8fdb444ec610f9")]
public partial class CustomPatrolAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<List<GameObject>> Waypoints;
    [SerializeReference] public BlackboardVariable<float> DistanceThreshold = new(0.5f);

    private NavMeshAgent _navAgent;
    private Vector3 _currentTarget;
    private bool _hasDestination;
    protected override Status OnStart()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            Debug.LogError("PatrolAction: EnemyController is null");
            return Status.Failure;
        }

        Waypoints.Value = EnemyController.Value.PatrolPoints.ToList();

        if (Waypoints == null || Waypoints.Value == null || Waypoints.Value.Count == 0)
        {
            Debug.LogError("PatrolAction: Waypoints is null or empty");
            return Status.Failure;
        }

        _navAgent = EnemyController.Value.NavAgent;
        if (_navAgent == null)
        {
            Debug.LogError("PatrolAction: NavMeshAgent is null");
            return Status.Failure;
        }

        if (!_navAgent.enabled)
        {
            Debug.LogError("PatrolAction: NavMeshAgent is not enabled");
            return Status.Failure;
        }

        MoveToRandomWaypoint();
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            return Status.Failure;
        }

        if (Waypoints == null || Waypoints.Value == null || Waypoints.Value.Count == 0)
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

    private void MoveToRandomWaypoint()
    {
        if (Waypoints.Value == null || Waypoints.Value.Count == 0)
        {
            Debug.LogError("PatrolAction: No waypoints available");
            return;
        }
        int randomIndex = UnityEngine.Random.Range(0, Waypoints.Value.Count);
        GameObject waypoint = Waypoints.Value[randomIndex];

        if (waypoint == null)
        {
            Debug.LogError("PatrolAction: Selected waypoint is null");
            return;
        }

        _currentTarget = waypoint.transform.position;

        if (_navAgent != null && _navAgent.isOnNavMesh)
        {
            _navAgent.isStopped = false;
            bool success = _navAgent.SetDestination(_currentTarget);

            if (success)
            {
                _hasDestination = true;
            }
            else
            {
                _hasDestination = false;
            }
        }
    }
}

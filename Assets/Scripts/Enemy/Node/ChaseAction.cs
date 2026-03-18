using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Chase", story: "Chase [Target]", category: "Action", id: "eb121cedc15226ff4902d271ae75cb29")]
public partial class ChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;

    private NavMeshAgent _navAgent;
    private Vector3 _currentTarget;
    private Transform targetTransform;

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
        if (Target.Value != null)
        {
            targetTransform = Target.Value.transform;
        }
        else
        {
            Target.Value = GameObject.FindFirstObjectByType<Player>().gameObject;
            targetTransform = Target.Value.transform;            
        }



            _navAgent.isStopped = false;
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

        // 계속 갱신 → 실시간 추격
        _navAgent.SetDestination(Target.Value.transform.position);

        // 디버깅/거리 업데이트
        EnemyController.Value.Detection.SetChaseing();

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_navAgent != null && _navAgent.isOnNavMesh)
            _navAgent.ResetPath();

    }

    
}


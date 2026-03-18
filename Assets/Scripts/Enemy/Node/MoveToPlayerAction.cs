using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "MoveToPlayer", story: "Move To [Target] Until [IsInsight] By [EnemyController]", category: "Action", id: "60f5b008dd81a6ed91d852b4770cf3ec")]
public partial class MoveToPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsInsight;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    private NavMeshAgent _nav;
    private Transform _player;


    protected override Status OnStart()
    {
        if (EnemyController == null || EnemyController.Value == null)
            return Status.Failure;

        _nav = EnemyController.Value.NavAgent;
        _player = EnemyController.Value.Player?.transform;
        if (_nav == null || !_nav.enabled)
            return Status.Failure;

        _player = EnemyController.Value.Player?.transform;
        if (_player == null)
            return Status.Failure;

        _nav.isStopped = false;
        
        return Status.Running;

    }

    protected override Status OnUpdate()
    {
        if (_player == null || _nav == null || !_nav.enabled)
            return Status.Failure;

        if (IsInsight)
        {
            _nav.ResetPath();
            return Status.Success;
        }

        _nav.SetDestination(_player.position);

        return Status.Running;
    }

    protected override void OnEnd()
    {
        if (_nav != null && _nav.isOnNavMesh)
            _nav.ResetPath();
    }

}


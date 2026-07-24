using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SetMoveSpeed", story: "Set [CurrentState] Speed by [EnemyController]", category: "Action", id: "271d408e609242860437bdb0d42eb28a")]
public partial class SetMoveSpeedAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyState> CurrentState;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    protected override Status OnStart()
    {
        if (EnemyController == null || EnemyController.Value == null)
        {
            Debug.LogError("SetMoveSpeedAction: EnemyController is null");
            return Status.Failure;
        }

        if (CurrentState == null)
        {
            Debug.LogError("SetMoveSpeedAction: CurrentState is null");
            return Status.Failure;
        }

        if (EnemyController.Value.Movement == null)
        {
            Debug.LogError("SetMoveSpeedAction: Movement is null");
            return Status.Failure;
        }

        EnemyController.Value.Movement.SetVelocity(CurrentState.Value);
        return Status.Success;
    }

    protected override Status OnUpdate()
    {
        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


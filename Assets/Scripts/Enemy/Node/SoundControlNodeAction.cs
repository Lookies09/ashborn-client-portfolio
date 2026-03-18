using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SoundControlNode", story: "[SoundController] Play Sound By [EnemyState]", category: "Action", id: "5dcb4de784c05520209d8cbb72c93de3")]
public partial class SoundControlNodeAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemySoundController> SoundController;
    [SerializeReference] public BlackboardVariable<EnemyState> EnemyState;

    protected override Status OnStart()
    {
        if (SoundController.Value == null)
        {
            Debug.LogWarning($"[{nameof(SoundControlNodeAction)}] SoundController가 블랙보드에 할당되지 않았습니다.");
            return Status.Failure;
        }

        SoundController.Value.OnStateChanged(EnemyState.Value);

        return Status.Success;
    }

}


using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "Despawn After Delay", story: "Despawn [Self] to pool after [Delay] seconds", category: "Action", id: "despawn-after-delay-node")]
public partial class DespawnAfterDelayAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<float> Delay;
    private float _start;

    protected override Status OnStart()
    {
        _start = Time.time;
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        float wait = Delay != null ? Delay.Value : 0f;
        if (Time.time - _start >= wait)
        {
            if (Self?.Value != null)
            {
                // 풀에 등록된 오브젝트면 Despawn, 아니면 Destroy
                if (ObjectPoolManager.Instance != null)
                    ObjectPoolManager.Instance.Despawn(Self.Value);
                else
                    UnityEngine.Object.Destroy(Self.Value);
            }
            return Status.Success;
        }

        return Status.Running;
    }
}

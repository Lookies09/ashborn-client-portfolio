using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "SelectAttack", story: "Select Attack Type", category: "Action", id: "10d4b0d8108a86529d4d72d7fc5f35bf")]
public partial class SelectAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<int> CurrentAttackIndex;

    protected override Status OnUpdate()
    {
        var ctrl = EnemyController?.Value;
        var target = Target?.Value;

        if (ctrl == null)
        {
            Debug.LogWarning("[SelectAttack] EnemyController null");
            return Status.Failure;
        }

        if (target == null)
        {
            Debug.LogWarning("[SelectAttack] Target null");
            return Status.Failure;
        }

        if (ctrl.Health != null && ctrl.Health.IsDead)
        {
            Debug.LogWarning("[SelectAttack] Enemy is dead");
            return Status.Failure;
        }

        if (target.TryGetComponent<IHealth>(out var th) && th.IsDead)
        {
            Debug.LogWarning("[SelectAttack] Target is dead");
            return Status.Failure;
        }

        var data = ctrl.Data;
        if (data == null)
        {
            Debug.LogWarning("[SelectAttack] Enemy Data null");
            return Status.Failure;
        }

        if (data.Attacks == null || data.Attacks.Count == 0)
        {
            Debug.LogWarning("[SelectAttack] No attacks in Data");
            return Status.Failure;
        }

        // 거리 체크
        float dist = Vector3.Distance(ctrl.transform.position, target.transform.position);
        if (dist > data.BaseAttackRange)
        {
            Debug.LogWarning($"[SelectAttack] Dist {dist:F2} > baseAttackRange {data.BaseAttackRange}");
            SetIndex(-1);
            return Status.Failure;
        }

        // 전역 쿨다운 체크
        if (ctrl.Attack != null && !ctrl.Attack.IsGlobalCooldownReady(ctrl.Data.GlobalAttackCooldown))
        {
            SetIndex(-1);
            return Status.Running;
        }

        var candidates = new System.Collections.Generic.List<(int idx, float weight)>();
        for (int i = 0; i < data.Attacks.Count; i++)
        {
            var p = data.Attacks[i];

            if (dist < p.minDistance)
            {
                Debug.Log($"[SelectAttack] Attack {i} rejected: dist {dist:F2} < min {p.minDistance}");
                continue;
            }

            if (dist > p.maxDistance)
            {
                Debug.Log($"[SelectAttack] Attack {i} rejected: dist {dist:F2} > max {p.maxDistance}");
                continue;
            }

            // 개별 쿨다운 체크
            if (ctrl.Attack != null && !ctrl.Attack.IsCooldownReady(i, p.cooldown))
            {
                Debug.Log($"[SelectAttack] Attack {i} rejected: cooldown");
                continue;
            }

            candidates.Add((i, Mathf.Max(0.0001f, p.weight)));
        }

        if (candidates.Count == 0)
        {
            SetIndex(-1);
            return Status.Failure;
        }

        float total = 0f;
        foreach (var c in candidates) total += c.weight;

        float pick = UnityEngine.Random.value * total;
        int chosen = candidates[0].idx;

        foreach (var c in candidates)
        {
            pick -= c.weight;
            if (pick <= 0f)
            {
                chosen = c.idx;
                break;
            }
        }

        SetIndex(chosen);
        EnemyController.Value.Attack.SetCurrentAttackProfile(EnemyController.Value.Data.Attacks[chosen], chosen);
        return Status.Success;
    }


    private void SetIndex(int value)
    {
        if (CurrentAttackIndex != null) CurrentAttackIndex.Value = value;
        else
        {
            EnemyController?.Value?.BTAgent?.SetVariableValue("CurrentAttackIndex", value);
        }
    }
}


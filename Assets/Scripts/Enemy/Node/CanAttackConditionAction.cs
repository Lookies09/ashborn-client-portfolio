using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "CanAttackCondition ", story: "Check [Self] Can Attack [Target] By [EnemyController]", category: "Action", id: "da1faeb7268549cfcb4dfcb8b1e7b9de")]
public partial class CanAttackConditionAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    protected override Status OnStart()
    {
        return Status.Running;
    }

    protected override Status OnUpdate()
    {

        var ctrl = EnemyController?.Value;
        var self = Self?.Value;
        var target = Target?.Value;
        if (ctrl == null || self == null || target == null) return Status.Failure;

        // 타깃 생존 여부
        if (ctrl.Health != null && ctrl.Health.IsDead) return Status.Failure;
        if (target.TryGetComponent<IHealth>(out var th) && th.IsDead)
        {
            SetTargetAttack(false);
            return Status.Failure;
        }

        float dist = Vector3.Distance(self.transform.position, target.transform.position);
        if (dist > ctrl.Data.BaseAttackRange)  // 1차 근접 체크
        {
            SetTargetAttack(false);
            return Status.Failure;
        }

        SetTargetAttack(true);  // BT 블랙보드 변수 "CanAttack" 등 세팅
        return Status.Success;
    }



    public void SetTargetAttack(bool value)
    {
        var ctrl = EnemyController?.Value;
        if (ctrl?.BTAgent == null) return;

        if (ctrl.BTAgent.GetVariable("CanAttack", out BlackboardVariable<bool> var))
        {
            if (var.Value == value) return; // 값이 같으면 그대로
            ctrl.BTAgent.SetVariableValue("CanAttack", value);
        }
    }
}


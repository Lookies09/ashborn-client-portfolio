using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ExplosionDead", story: "Explosion in [ExplosionPoint] By [EnemyController]", category: "Action", id: "c7d4e84903eba261bb0258ac21ee3897")]
public partial class ExplosionDeadAction : Action
{
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<GameObject> ExplosionPoint;
    private EnemyAttackProfile _attackProfile;

    protected override Status OnStart()
    {
        _attackProfile = EnemyController.Value.Data.Attacks.Find(a => a.type == AttackType.Explosive);

        if (ObjectPoolManager.Instance != null) 
        {
            ObjectPoolManager.Instance.Spawn(_attackProfile.castEffect.name, ExplosionPoint.Value.transform.position, Quaternion.identity);
            
        }
        else
        {
            GameObject.Instantiate(_attackProfile.castEffect, ExplosionPoint.Value.transform.position, Quaternion.identity);
        }

        Collider[] hits = Physics.OverlapSphere(ExplosionPoint.Value.transform.position, _attackProfile.range);
        HashSet<IDamageable> processed = new HashSet<IDamageable>();

        foreach (var hit in hits)
        {
            IDamageable dmg = hit.GetComponentInParent<IDamageable>();
            if (dmg == null)
                continue;

            // 이미 처리한 대상이면 skip
            if (!processed.Add(dmg))
                continue;

            dmg.TakeDamage(_attackProfile.damage);
        }


        EnemyController.Value.Health.Kill();

        return Status.Success;
    }

}


using System;
using UnityEngine;

public class ProjectileAttack : BaseStrategy
{
    [SerializeField] private Transform[] shootOrigins;

    public override void Execute(EnemyAttackProfile profile)
    {

        if (profile.projectilePrefab == null)
        {
            Debug.LogWarning("ProjectileAttack: projectilePrefab Null");
            return;
        }

        if (shootOrigins == null || shootOrigins.Length == 0)
        {
            Debug.LogWarning("ProjectileAttack: shootOrigins Null");
            return;
        }

        foreach (var origin in shootOrigins)
        {
            if (origin == null) continue;

            // 투사체 생성

            GameObject proj = null;

            if (ObjectPoolManager.Instance != null)
            {

                proj = ObjectPoolManager.Instance.Spawn(
                    profile.projectilePrefab.name,
                    origin.position,
                    origin.rotation
                );

                ObjectPoolManager.Instance.Spawn(
                    profile.castEffect.name,
                    origin.position,
                    origin.rotation
                );
            }
            else
            {
                Debug.LogWarning("ProjectileAttack: ObjectPoolManager Instance Null");
                proj = Instantiate(
                profile.projectilePrefab,
                origin.position,
                origin.rotation               
            );
                Instantiate(
                    profile.castEffect,
                    origin.position,
                    origin.rotation
                );
            }

            proj.GetComponent<IProjectile>()?.Init(
                profile.damage,
                profile.projectileSpeed,
                profile.projectileLifeTime,
                origin.forward,
                profile.hitEffect,
                profile.hitSound
            );
        }
    }

}

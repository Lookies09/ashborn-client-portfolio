
using UnityEngine;

public interface IProjectile
{

    void Init(int damage, float speed, float lifeTime, Vector3 direction, GameObject hitEffect, AudioClip hitSound);
}

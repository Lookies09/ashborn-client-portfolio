using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : BaseStrategy
{
    private EnemyAttackProfile _currentAttackProfile;
    // 재사용 버퍼로 GC 방지 + 중복 피격 방지
    private readonly Collider[] _overlapBuffer = new Collider[8];
    private readonly HashSet<IDamageable> _uniqueDamageables = new();

    [SerializeField] private Color gizmoColor = new Color(1f, 0f, 0f, 0.25f);
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private GameObject meleeTrail;

    public override void Execute(EnemyAttackProfile _currentAttackProfile)
    {
        if (_currentAttackProfile == null || attackOrigin == null) return;

        this._currentAttackProfile = _currentAttackProfile;

        // 중복 대상 제거
        _uniqueDamageables.Clear();

        int hitCount = Physics.OverlapSphereNonAlloc(
            attackOrigin.position,
            _currentAttackProfile.meleeRadius,
            _overlapBuffer,
            _currentAttackProfile.targetMask);

        for (int i = 0; i < hitCount; i++)
        {
            var col = _overlapBuffer[i];
            if (col == null) continue;

            if (col.TryGetComponent<IDamageable>(out var damageable))
            {
                // 같은 대상이 여러 콜라이더를 가질 때 중복 방지
                if (_uniqueDamageables.Add(damageable))
                {
                    damageable.TakeDamage(_currentAttackProfile.damage);
                }
            }
        }
    }

    public void OnMeleeTrail()
    {
        meleeTrail.SetActive(true);
    }
    public void OffMeleeTrail()
    {
        meleeTrail.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        if (attackOrigin == null || _currentAttackProfile == null) return;
        Gizmos.color = gizmoColor;
        Gizmos.DrawSphere(attackOrigin.position, _currentAttackProfile.meleeRadius);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackOrigin.position, _currentAttackProfile.meleeRadius);
    }

}

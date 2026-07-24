using UnityEngine;

public abstract class BaseStrategy : MonoBehaviour
{
    [SerializeField] protected AttackType type;
    public AttackType Type => type; 

    public abstract void Execute(EnemyAttackProfile profile);

}

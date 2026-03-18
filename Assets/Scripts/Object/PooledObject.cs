using UnityEngine;

/// <summary>
/// 풀에 등록된 오브젝트가 자신의 풀 정보를 보관하도록 돕는 컴포넌트.
/// </summary>
public class PooledObject : MonoBehaviour
{
    public string PoolId { get; private set; }
    private ObjectPoolManager _owner;

    public void Initialize(string poolId, ObjectPoolManager owner)
    {
        PoolId = poolId;
        _owner = owner;
    }

    public void ReturnToPool()
    {
        if (_owner == null)
        {
            Destroy(gameObject);
            return;
        }

        _owner.Despawn(gameObject);
    }
}



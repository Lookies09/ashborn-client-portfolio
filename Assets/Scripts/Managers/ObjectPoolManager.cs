using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 이펙트/투사체 등 다양한 프리팹을 풀링하는 전역 매니저.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    [System.Serializable]
    public class PoolConfig
    {
        public string poolId;
        public GameObject prefab;
        [Min(0)] public int preloadCount = 5;
        public bool expandable = true;
    }

    [SerializeField] private PoolConfig[] initialPools;

    private readonly Dictionary<string, Queue<GameObject>> _pools = new Dictionary<string, Queue<GameObject>>();
    private readonly Dictionary<string, GameObject> _prefabLookup = new Dictionary<string, GameObject>();
    private readonly List<GameObject> _runtimeContainers = new List<GameObject>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeConfiguredPools();
    }

    private void InitializeConfiguredPools()
    {
        if (initialPools == null) return;

        foreach (var config in initialPools)
        {
            if (config == null || string.IsNullOrWhiteSpace(config.poolId) || config.prefab == null)
                continue;

            RegisterPool(config.poolId, config.prefab, config.preloadCount, config.expandable);
        }
    }

    public void RegisterPool(string poolId, GameObject prefab, int preloadCount = 0, bool expandable = true)
    {
        if (string.IsNullOrWhiteSpace(poolId) || prefab == null)
        {
            Debug.LogWarning("[ObjectPoolManager] 잘못된 풀 등록 요청입니다.");
            return;
        }

        if (_pools.ContainsKey(poolId))
        {
            Debug.LogWarning($"[ObjectPoolManager] 이미 존재하는 풀입니다: {poolId}");
            return;
        }

        _pools[poolId] = new Queue<GameObject>();
        _prefabLookup[poolId] = prefab;

        for (int i = 0; i < preloadCount; i++)
        {
            var instance = CreateInstance(poolId);
            Recycle(poolId, instance);
        }
    }

    public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
    {
        if (!_pools.TryGetValue(poolId, out Queue<GameObject> queue))
        {
            Debug.LogWarning($"[ObjectPoolManager] 등록되지 않은 풀입니다: {poolId}");
            return null;
        }

        GameObject obj = queue.Count > 0 ? queue.Dequeue() : CreateInstance(poolId);
        if (obj == null)
            return null;

        obj.transform.SetPositionAndRotation(position, rotation);
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(GameObject target)
    {
        if (target == null) return;

        var pooled = target.GetComponent<PooledObject>();
        if (pooled == null || string.IsNullOrEmpty(pooled.PoolId))
        {            
            Destroy(target);
            return;
        }
        Recycle(pooled.PoolId, target);
    }

    public void DespawnAll()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            GameObject child = transform.GetChild(i).gameObject;

            // 현재 켜져 있는 오브젝트만 처리
            if (child.activeSelf)
            {
                Despawn(child);
            }
        }
    }

    private GameObject CreateInstance(string poolId)
    {
        if (!_prefabLookup.TryGetValue(poolId, out GameObject prefab) || prefab == null)
        {
            Debug.LogWarning($"[ObjectPoolManager] 프리팹을 찾을 수 없습니다: {poolId}");
            return null;
        }

        GameObject instance = Instantiate(prefab, this.transform);
        instance.name = $"{prefab.name}_Pooled";

        var pooled = instance.GetComponent<PooledObject>();
        if (pooled == null)
            pooled = instance.AddComponent<PooledObject>();

        pooled.Initialize(poolId, this);

        instance.SetActive(false);
        return instance;
    }

    private void Recycle(string poolId, GameObject target)
    {
        target.SetActive(false);

        if (!_pools.TryGetValue(poolId, out Queue<GameObject> queue))
        {
            Destroy(target);
            return;
        }

        queue.Enqueue(target);
    }
}



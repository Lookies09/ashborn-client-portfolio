using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public enum AreaTier { Easy, Middle, Hard, Boss }

public class Tile : MonoBehaviour
{
    public int x;
    public int y;
    public bool IsActive;
    public Bounds bounds;
    public List<GameObject> contents = new List<GameObject>();
    public TileType type;
    public AreaTier tier;

    [SerializeField] private Transform[] eventPoints;
    [SerializeField] private Transform[] monsterSpawnPoints;

    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private GameObject escapeEffectPrefab;

    public Transform[] GetMonsterPoints() => monsterSpawnPoints;

    void Awake()
    {
        var renderer = GetComponentInChildren<MeshRenderer>();
        if (renderer != null)
            bounds = renderer.bounds;
        else
            bounds = new Bounds(transform.position, new Vector3(10, 1, 10));

    }
    
    public void Init(int x, int y, bool IsActive, TileType tileType = TileType.Field, AreaTier areaTier = AreaTier.Easy)
    {
        this.x = x;
        this.y = y;
        this.IsActive = IsActive;
        this.type = tileType;
        this.tier = areaTier; // 구역 등급 설정

        // 1. 이벤트 처리 (아이템, 탈출구)
        if (tileType == TileType.Item) SpawnEventObject(itemPrefab);
        if (tileType == TileType.Escape) SpawnEventObject(escapeEffectPrefab);
    }

    private void SpawnEventObject(GameObject prefab)
    {
        if (eventPoints == null || eventPoints.Length == 0) return;

        Quaternion rotation = Quaternion.Euler(0, 180f, 0);

        Transform t = eventPoints[Random.Range(0, eventPoints.Length)];
        GameObject obj = Instantiate(prefab, t.position, rotation);
        obj.transform.SetParent(transform);
        contents.Add(obj);
    }
}

public enum TileType
{
    Escape,
    Field,
    BossRoom,
    Item,
}

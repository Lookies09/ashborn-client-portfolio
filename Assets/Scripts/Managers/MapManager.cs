using System.Collections.Generic;
using System.Linq;
using Unity.AI.Navigation;
using Unity.AppUI.UI;
using UnityEngine;
using UnityEngine.UIElements;

public class MapManager : MonoBehaviour
{
    [SerializeField] private float activeRadius = 30f;
    [SerializeField] private float tileSize = 20f;
    [SerializeField] private float updateThreshold = 1f;

    [SerializeField] private int mapSizeX = 5;
    [SerializeField] private int mapSizeY = 5;

    [Header("아이템 타일 생성 관련 변수")]
    [SerializeField] private int minItemTiles = 3;
    [SerializeField] private int maxItemTiles = 5;

    [Header("탈출 타일 생성 관련 변수")]
    //[SerializeField] private int maxEscapeTiles = 3;

    [SerializeField] private GameObject[] fieldTilePrefabs;
    [SerializeField] private GameObject bossTilePrefab;

    [SerializeField] private EnemySpawner enemySpawner;

    private Tile[,] _tiles = new Tile[7, 7];
    private List<Tile> _tileList = new List<Tile>();
    private Dictionary<AreaTier, List<Tile>> _tierTileDictionary = new();
    private Transform _player;
    private Vector3 _lastPlayerPos;
    private Vector3 _mapOrigin;
    private NavMeshSurface _surface;
    //private bool _built = false;


    void Awake()
    {
        _tiles = new Tile[mapSizeX, mapSizeY];

        _surface = GetComponent<NavMeshSurface>();
        _player = GameObject.FindGameObjectWithTag("Player").transform;
        _lastPlayerPos = _player.position;
        
        CreateTiles();
        CreateInvisibleWalls();

        AreaTier[] tiers = { AreaTier.Easy, AreaTier.Middle, AreaTier.Hard };
        foreach (var tier in tiers)
        {
            List<Tile> candidates = _tierTileDictionary[tier];
            if (candidates.Count > 0)
            {
                int index = Random.Range(0, candidates.Count);
                Tile escapeTile = candidates[index];

                escapeTile.Init(escapeTile.x, escapeTile.y, false, TileType.Escape, tier);

                // 다른 용도(아이템 등)로 사용되지 않게 제거
                candidates.RemoveAt(index);
            }
        }

        List<Tile> remainTiles = new List<Tile>();
        foreach (var list in _tierTileDictionary.Values)
        {
            remainTiles.AddRange(list);
        }

        int itemCount = Random.Range(minItemTiles, maxItemTiles + 1);

        // 아이템 타일 무작위 배치
        for (int i = 0; i < itemCount; i++)
        {
            if (remainTiles.Count == 0) break;

            int index = Random.Range(0, remainTiles.Count);
            Tile itemTile = remainTiles[index];

            itemTile.Init(itemTile.x, itemTile.y, false, TileType.Item, itemTile.tier);
            remainTiles.RemoveAt(index);
        }

        _tileList = null;
        _mapOrigin = _tiles[0, 0].transform.position;

        _surface.BuildNavMesh();

        _tiles.Cast<Tile>()
      .Where(t => t != null)
      .ToList()
      .ForEach(t => t.gameObject.SetActive(false));

        StartCoroutine(enemySpawner.CoSpawnAllEnemies(_tiles));
        // 메모리 해제
        _tileList = null;
        _tierTileDictionary.Clear();
        _mapOrigin = _tiles[0, 0].transform.position;
    }

    private void Start()
    {
        Transform[] spawnPoes = _tiles[0, 0].GetMonsterPoints();
        InGameManager.Instance.SetPlayerOnStartPos(spawnPoes[0]);
    }

    void Update()
    {
        Vector3 p = _player.position;
        if ((p - _lastPlayerPos).sqrMagnitude <= updateThreshold * updateThreshold)
            return;

        _lastPlayerPos = p;

        int px = Mathf.FloorToInt((p.x - _mapOrigin.x) / tileSize);
        int py = Mathf.FloorToInt((p.z - _mapOrigin.z) / tileSize);

        int radiusTiles = Mathf.CeilToInt(activeRadius / tileSize);

        for (int x = Mathf.Max(0, px - radiusTiles); x < Mathf.Min(mapSizeX, px + radiusTiles + 1); x++)
        {
            for (int y = Mathf.Max(0, py - radiusTiles); y < Mathf.Min(mapSizeY, py + radiusTiles + 1); y++)
            {
                Tile tile = _tiles[x, y];
                if (tile == null) continue;

                float distSqr = (tile.transform.position - p).sqrMagnitude;
                bool active = distSqr < activeRadius * activeRadius;

                if (tile.gameObject.activeSelf != active)
                    tile.gameObject.SetActive(active);
            }
        }

    }

    private void CreateTiles()
    {
        int centerX = mapSizeX / 2;
        int centerY = mapSizeY / 2;

        _tierTileDictionary[AreaTier.Easy] = new List<Tile>();
        _tierTileDictionary[AreaTier.Middle] = new List<Tile>();
        _tierTileDictionary[AreaTier.Hard] = new List<Tile>();

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Vector3 pos = new Vector3(x * tileSize, 0, y * tileSize);
                int dist = Mathf.Abs(x - centerX) + Mathf.Abs(y - centerY);

                AreaTier tier;
                if (x == centerX && y == centerY) tier = AreaTier.Boss;
                else if (dist <= 2) tier = AreaTier.Hard;
                else if (dist <= 4) tier = AreaTier.Middle;
                else tier = AreaTier.Easy;

                float randomRotY = new float[] { 0f, 90f, 180f, 270f }[Random.Range(0, 4)];
                Quaternion rotation = Quaternion.Euler(0, randomRotY, 0);

                GameObject prefab = (tier == AreaTier.Boss) ? bossTilePrefab : fieldTilePrefabs[Random.Range(0, fieldTilePrefabs.Length)];
                GameObject tileObj = Instantiate(prefab, pos, rotation, transform);

                Tile t = tileObj.GetComponent<Tile>();
                TileType type = (tier == AreaTier.Boss) ? TileType.BossRoom : TileType.Field;

                // 수정된 Init 호출 (AreaTier 전달)
                t.Init(x, y, false, type, tier);
                _tiles[x, y] = t;

                if (tier != AreaTier.Boss)
                {
                    if (!(x == 0 && y == 0))
                    {
                        _tierTileDictionary[tier].Add(t);
                    }
                }
            }
        }
    }

    private void CreateInvisibleWalls()
    {
        float mapWidth = mapSizeX * tileSize;
        float mapHeight = mapSizeY * tileSize;
        float wallThickness = 1f;
        float wallHeight = 10f;
        float offset = 1f;

        float centerX = (mapWidth - tileSize) / 2f;
        float centerZ = (mapHeight - tileSize) / 2f;

        // North (위쪽): Z축 방향으로 +offset
        SpawnWall("Wall_North",
            new Vector3(centerX, wallHeight / 2, (mapHeight - (tileSize / 2)) + offset),
            new Vector3(mapWidth + (offset * 2), wallHeight, wallThickness));

        // South (아래쪽): Z축 방향으로 -offset
        SpawnWall("Wall_South",
            new Vector3(centerX, wallHeight / 2, (-tileSize / 2) - offset),
            new Vector3(mapWidth + (offset * 2), wallHeight, wallThickness));

        // East (오른쪽): X축 방향으로 +offset
        SpawnWall("Wall_East",
            new Vector3((mapWidth - (tileSize / 2)) + offset, wallHeight / 2, centerZ),
            new Vector3(wallThickness, wallHeight, mapHeight + (offset * 2)));

        // West (왼쪽): X축 방향으로 -offset
        SpawnWall("Wall_West",
            new Vector3((-tileSize / 2) - offset, wallHeight / 2, centerZ),
            new Vector3(wallThickness, wallHeight, mapHeight + (offset * 2)));
    }

    private void SpawnWall(string name, Vector3 pos, Vector3 scale)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.parent = this.transform;
        wall.transform.position = pos;
        wall.transform.localScale = scale;

        // 렌더러 제거해서 투명하게 만들기
        Destroy(wall.GetComponent<MeshRenderer>());
    }

}

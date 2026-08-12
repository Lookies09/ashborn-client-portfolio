using System.Collections.Generic;
using Unity.AI.Navigation;
using Unity.Profiling;
using UnityEngine;

public class MapManager : MonoBehaviour
{
    private static readonly ProfilerMarker UpdateActiveTilesMarker = new("MapManager.UpdateActiveTiles");
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
    private HashSet<Tile> _activeTiles = new();
    private HashSet<Tile> _desiredActiveTiles = new();
    private Transform _player;
    private Vector3 _lastPlayerPos;
    private Vector3 _mapOrigin;
    private NavMeshSurface _surface;
    //private bool _built = false;


#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Runtime Diagnostics (Read Only)")]
    [SerializeField] private Vector2Int diagnosticPlayerGrid;
    [SerializeField] private int diagnosticTotalTiles;
    [SerializeField] private int diagnosticActiveTiles;
    [SerializeField] private float diagnosticActiveRadius;
#endif

    private void Awake()
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

        DeactivateAllTiles();

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

    private void Update()
    {
        Vector3 playerPosition = _player.position;
        if ((playerPosition - _lastPlayerPos).sqrMagnitude <= updateThreshold * updateThreshold)
        {
            return;
        }

        _lastPlayerPos = playerPosition;

        int playerGridX = Mathf.FloorToInt((playerPosition.x - _mapOrigin.x) / tileSize);
        int playerGridY = Mathf.FloorToInt((playerPosition.z - _mapOrigin.z) / tileSize);
        UpdateActiveTiles(playerPosition, playerGridX, playerGridY);
    }

    private void UpdateActiveTiles(Vector3 playerPosition, int playerGridX, int playerGridY)
    {
        using (UpdateActiveTilesMarker.Auto())
        {
            _desiredActiveTiles.Clear();

            int radiusTiles = Mathf.CeilToInt(activeRadius / tileSize);
            float activeRadiusSqr = activeRadius * activeRadius;

            for (int x = Mathf.Max(0, playerGridX - radiusTiles); x < Mathf.Min(mapSizeX, playerGridX + radiusTiles + 1); x++)
            {
                for (int y = Mathf.Max(0, playerGridY - radiusTiles); y < Mathf.Min(mapSizeY, playerGridY + radiusTiles + 1); y++)
                {
                    Tile tile = _tiles[x, y];
                    if (tile == null)
                    {
                        continue;
                    }

                    float distSqr = (tile.transform.position - playerPosition).sqrMagnitude;
                    if (distSqr < activeRadiusSqr)
                    {
                        _desiredActiveTiles.Add(tile);
                    }
                }
            }

            foreach (Tile tile in _activeTiles)
            {
                if (tile != null && !_desiredActiveTiles.Contains(tile))
                {
                    SetTileActive(tile, false);
                }
            }

            foreach (Tile tile in _desiredActiveTiles)
            {
                if (!_activeTiles.Contains(tile))
                {
                    SetTileActive(tile, true);
                }
            }

            HashSet<Tile> previousActiveTiles = _activeTiles;
            _activeTiles = _desiredActiveTiles;
            _desiredActiveTiles = previousActiveTiles;
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        RefreshDiagnostics(playerGridX, playerGridY);
#endif
    }

    private void DeactivateAllTiles()
    {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int totalTiles = 0;
#endif

        for (int x = 0; x < mapSizeX; x++)
        {
            for (int y = 0; y < mapSizeY; y++)
            {
                Tile tile = _tiles[x, y];
                if (tile == null)
                {
                    continue;
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                totalTiles++;
#endif
                SetTileActive(tile, false);
            }
        }

        _activeTiles.Clear();
        _desiredActiveTiles.Clear();

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        diagnosticTotalTiles = totalTiles;
        diagnosticActiveTiles = 0;
        diagnosticActiveRadius = activeRadius;
#endif
    }

    private static void SetTileActive(Tile tile, bool active)
    {
        if (tile.gameObject.activeSelf != active)
        {
            tile.gameObject.SetActive(active);
        }

        tile.IsActive = active;
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RefreshDiagnostics(int playerGridX, int playerGridY)
    {
        diagnosticPlayerGrid = new Vector2Int(playerGridX, playerGridY);
        diagnosticActiveTiles = _activeTiles.Count;
        diagnosticActiveRadius = activeRadius;
    }
#endif

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

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        if (_tiles == null)
        {
            return;
        }

        for (int x = 0; x < _tiles.GetLength(0); x++)
        {
            for (int y = 0; y < _tiles.GetLength(1); y++)
            {
                Tile tile = _tiles[x, y];
                if (tile == null)
                {
                    continue;
                }

                Gizmos.color = tile.gameObject.activeSelf ? Color.green : Color.gray;
                Gizmos.DrawWireCube(tile.transform.position, new Vector3(tileSize, 0.1f, tileSize));
            }
        }

        if (_player == null)
        {
            return;
        }

        Vector3 playerPosition = _player.position;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(playerPosition, activeRadius);

        int playerGridX = Mathf.FloorToInt((playerPosition.x - _mapOrigin.x) / tileSize);
        int playerGridY = Mathf.FloorToInt((playerPosition.z - _mapOrigin.z) / tileSize);
        if (playerGridX < 0 || playerGridX >= _tiles.GetLength(0) || playerGridY < 0 || playerGridY >= _tiles.GetLength(1))
        {
            return;
        }

        Tile currentTile = _tiles[playerGridX, playerGridY];
        if (currentTile != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(currentTile.transform.position + Vector3.up * 0.1f, new Vector3(tileSize, 0.2f, tileSize));
        }
    }
#endif
}

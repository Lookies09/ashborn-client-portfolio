using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;
public class EnemySpawner : MonoBehaviour
{
    [Header("Monster Pool IDs")]
    // ObjectPoolManager의 poolId와 일치해야 합니다.
    [SerializeField] private string[] easyMonsterIDs = { "Skeleton_Basic" };
    [SerializeField] private string[] middleMonsterIDs = { "Skeleton_Basic", "Skeleton_Explosion", "Skeleton_Ranger" };
    [SerializeField] private string[] hardMonsterIDs = { "Skeleton_Ranger", "Skeleton_Armored" };
    [SerializeField] private string[] bossMonsterIDs = { "Skeleton_Boss" };

    [SerializeField] private EnemyCullingManager enemyCullingManager;
    private Tile bossTile;


    private void Awake()
    {
        if (enemyCullingManager == null)
        {
            enemyCullingManager = FindFirstObjectByType<EnemyCullingManager>();
        }
    }

    private void Start()
    {
        InGameManager.Instance.OnTimerEnded += ResponeBoss;
    }

    private void OnDisable()
    {
        InGameManager.Instance.OnTimerEnded -= ResponeBoss;
    }

    public IEnumerator CoSpawnAllEnemies(Tile[,] tiles)
    {
        yield return new WaitForEndOfFrame(); // NavMesh 베이크 대기

        foreach (Tile tile in tiles)
        {
            if (tile == null) continue;

            // 보스 타일인 경우 따로 저장만 하고 일반 몹 스폰은 건너뜁니다.
            if (tile.tier == AreaTier.Boss)
            {
                bossTile = tile;
                continue;
            }

            if (tile.x == 0 && tile.y == 0) continue; // 시작 타일 건너뛰기

            string[] selectedPoolIDs = GetPoolIDsByTier(tile.tier);
            Transform[] spawnPoints = tile.GetMonsterPoints();

            if (spawnPoints == null) continue;

            foreach (Transform sp in spawnPoints)
            {
                if (Random.value > 0.6f) continue; // 60% 확률로 스폰

                string targetID = selectedPoolIDs[Random.Range(0, selectedPoolIDs.Length)];

                float randomY = Random.Range(0f, 360f);
                Quaternion randomRotation = Quaternion.Euler(0, randomY, 0);

                GameObject enemy = ObjectPoolManager.Instance.Spawn(targetID, sp.position, randomRotation);

                if (enemy != null)
                {
                    EnemyController enemyController = enemy.GetComponent<EnemyController>();
                    InitNavMeshAgent(enemy, sp.position);
                    PlaceEnemyOnNavMesh(enemy, sp.position, sp.gameObject);
                    enemyCullingManager.RegisterEnemy(enemyController);
                }
            }
            yield return null; // 프레임 분산
        }
        enemyCullingManager.InitializeCulling();
    }

    

    private void InitNavMeshAgent(GameObject enemy, Vector3 pos)
    {
        var agent = enemy.GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            agent.enabled = true;
            agent.Warp(pos);
        }
    }

    private string[] GetPoolIDsByTier(AreaTier tier) => tier switch
    {
        AreaTier.Easy => easyMonsterIDs,
        AreaTier.Middle => middleMonsterIDs,
        AreaTier.Hard => hardMonsterIDs,
        AreaTier.Boss => bossMonsterIDs,
        _ => easyMonsterIDs
    };

    private void PlaceEnemyOnNavMesh(GameObject enemy, Vector3 position, GameObject pos)
    {
        enemy.SetActive(true);

        var controller = enemy.GetComponent<EnemyController>();
        if (controller != null)
        {
            // 생성 시 초기화 (사망 플래그 등)
            controller.OnSpawnInitialize();


            GameObject[] patrolpoint = new GameObject[1];
            patrolpoint[0] = pos;
            controller.SetWanderData(pos, patrolpoint);
        }

        var agent = enemy.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.enabled = false;
            agent.enabled = true;
            agent.Warp(position);
        }
    }

    public void ResponeBoss(bool _)
    {
        // 저장해둔 보스 타일이 있는지 확인
        if (bossTile == null)
        {
            Debug.LogError("저장된 보스 타일이 없습니다! 맵 생성 시 AreaTier.Boss 설정 확인 필요.");
            return;
        }

        Transform[] spawnPoints = bossTile.GetMonsterPoints();
        if (spawnPoints == null || spawnPoints.Length == 0) return;

        // 보스 소환 (보통 보스 타일의 중앙이나 첫 번째 포인트 사용)
        Transform sp = spawnPoints[0];
        string targetID = bossMonsterIDs[Random.Range(0, bossMonsterIDs.Length)];

        GameObject boss = ObjectPoolManager.Instance.Spawn(targetID, sp.position, Quaternion.identity);

        if (boss != null)
        {
            EnemyController bossController = boss.GetComponent<EnemyController>();

            // 보스 초기화 (일반 몹과 동일한 방식 적용)
            InitNavMeshAgent(boss, sp.position);
            PlaceEnemyOnNavMesh(boss, sp.position, sp.gameObject);

            Debug.Log($"<color=red>Boss {targetID} Spawned!</color>");
        }
    }
}
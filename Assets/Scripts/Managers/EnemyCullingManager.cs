using System.Collections.Generic;
using UnityEngine;

public class EnemyCullingManager : MonoBehaviour
{
    [SerializeField] private float cullDistance = 35f;    
    [SerializeField] private int checksPerFrame = 10; // 한 프레임에 검사할 마릿수
    [SerializeField] private float updateThreshold = 1.0f; // 플레이어가 최소 1m는 움직여야 체크

    private List<EnemyController> _allEnemies = new List<EnemyController>();
    private int _currentIndex = 0;
    private Vector3 _lastCheckedPlayerPos;
    private Transform _player;

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }



    void Update()
    {
        if (_allEnemies.Count == 0) return;

        // 플레이어 위치 변화 체크
        float moveDistSqr = (_player.position - _lastCheckedPlayerPos).sqrMagnitude;

        // 플레이어가 거의 움직이지 않았다면 이번 프레임은 연산을 건너뜁니다.
        if (moveDistSqr < updateThreshold * updateThreshold) return;

        // 위치가 변했다면 업데이트 기록
        _lastCheckedPlayerPos = _player.position;

        // 분할 업데이트 실행
        for (int i = 0; i < checksPerFrame; i++)
        {
            _currentIndex = (_currentIndex + 1) % _allEnemies.Count;
            var enemy = _allEnemies[_currentIndex];
            if (enemy == null) continue;

            bool isDead = enemy.Health.IsDead;
            if (isDead) continue;

            float distSqr = (enemy.transform.position - _player.position).sqrMagnitude;
            bool shouldBeActive = distSqr < cullDistance * cullDistance;            

            if (enemy.gameObject.activeSelf != shouldBeActive)
            {
                enemy.gameObject.SetActive(shouldBeActive);
            }
        }
    }
    public void RegisterEnemy(EnemyController enemy) => _allEnemies.Add(enemy);

    public void InitializeCulling()
    {
        if (_allEnemies.Count == 0) return;

        _lastCheckedPlayerPos = _player.position;
        float distThresholdSqr = cullDistance * cullDistance;

        foreach (var enemy in _allEnemies)
        {
            if (enemy == null) continue;

            float distSqr = (enemy.transform.position - _player.position).sqrMagnitude;
            bool shouldBeActive = distSqr < distThresholdSqr;

            // 처음에 상태를 강제로 맞춰줌
            enemy.gameObject.SetActive(shouldBeActive);
        }
    }
}
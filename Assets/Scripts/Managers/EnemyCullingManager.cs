using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

public class EnemyCullingManager : MonoBehaviour
{
    private static readonly ProfilerMarker ScanChunkMarker = new("EnemyCulling.ScanChunk");
    [SerializeField] private float cullDistance = 35f;    
    [SerializeField] private int checksPerFrame = 10; // 한 프레임에 검사할 마릿수
    [SerializeField] private float updateThreshold = 1.0f; // 플레이어가 최소 1m는 움직여야 체크

    private readonly List<EnemyController> _allEnemies = new();
    private int _currentIndex;
    private int _scanTargetCount;
    private bool _scanInProgress;
    private Vector3 _scanPlayerPosition;
    private Vector3 _lastCheckedPlayerPos;
    private Transform _player;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    [Header("Runtime Diagnostics (Read Only)")]
    [SerializeField] private int diagnosticTotalEnemies;
    [SerializeField] private int diagnosticEnemiesPerFrame;
    [SerializeField] private int diagnosticScanIndex;
    [SerializeField] private bool diagnosticScanInProgress;
    [SerializeField] private int diagnosticCompletedScanCycles;
    [SerializeField] private int diagnosticActiveEnemies;

    private int _activeEnemiesInCurrentScan;
#endif

    private void Awake()
    {
        _player = GameObject.FindGameObjectWithTag("Player").transform;
    }



    private void Update()
    {
        if (_allEnemies.Count == 0)
        {
            return;
        }

        if (!_scanInProgress)
        {
            float moveDistSqr = (_player.position - _lastCheckedPlayerPos).sqrMagnitude;
            if (moveDistSqr < updateThreshold * updateThreshold)
            {
                return;
            }

            BeginScan();
        }

        ProcessScanChunk();
    }

    private void BeginScan()
    {
        _scanPlayerPosition = _player.position;
        _lastCheckedPlayerPos = _scanPlayerPosition;
        _currentIndex = 0;
        _scanTargetCount = _allEnemies.Count;
        _scanInProgress = _scanTargetCount > 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        _activeEnemiesInCurrentScan = 0;
        RefreshDiagnostics();
#endif
    }

    private void ProcessScanChunk()
    {
        using (ScanChunkMarker.Auto())
        {
            int processedCount = 0;
            int frameBudget = Mathf.Max(1, checksPerFrame);
            float distThresholdSqr = cullDistance * cullDistance;

            while (processedCount < frameBudget && _currentIndex < _scanTargetCount)
            {
                EnemyController enemy = _allEnemies[_currentIndex];
                _currentIndex++;
                processedCount++;

                if (enemy == null)
                {
                    continue;
                }

                if (!enemy.Health.IsDead)
                {
                    float distSqr = (enemy.transform.position - _scanPlayerPosition).sqrMagnitude;
                    bool shouldBeActive = distSqr < distThresholdSqr;

                    if (enemy.gameObject.activeSelf != shouldBeActive)
                    {
                        enemy.gameObject.SetActive(shouldBeActive);
                    }
                }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
                if (enemy.gameObject.activeSelf)
                {
                    _activeEnemiesInCurrentScan++;
                }
#endif
            }
        }

        if (_currentIndex < _scanTargetCount)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            RefreshDiagnostics();
#endif
            return;
        }

        _scanInProgress = false;
        _scanTargetCount = 0;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        diagnosticCompletedScanCycles++;
        diagnosticActiveEnemies = _activeEnemiesInCurrentScan;
        RefreshDiagnostics();
#endif
    }

    public void RegisterEnemy(EnemyController enemy)
    {
        if (enemy == null)
        {
            return;
        }

        _allEnemies.Add(enemy);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        RefreshDiagnostics();
#endif
    }

    public void InitializeCulling()
    {
        _scanInProgress = false;
        _scanTargetCount = 0;
        _currentIndex = 0;

        if (_allEnemies.Count == 0)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            diagnosticActiveEnemies = 0;
            RefreshDiagnostics();
#endif
            return;
        }

        _lastCheckedPlayerPos = _player.position;
        float distThresholdSqr = cullDistance * cullDistance;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        int activeEnemyCount = 0;
#endif

        foreach (EnemyController enemy in _allEnemies)
        {
            if (enemy == null)
            {
                continue;
            }

            float distSqr = (enemy.transform.position - _player.position).sqrMagnitude;
            bool shouldBeActive = distSqr < distThresholdSqr;
            enemy.gameObject.SetActive(shouldBeActive);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            if (enemy.gameObject.activeSelf)
            {
                activeEnemyCount++;
            }
#endif
        }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        diagnosticActiveEnemies = activeEnemyCount;
        RefreshDiagnostics();
#endif
    }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
    private void RefreshDiagnostics()
    {
        diagnosticTotalEnemies = _allEnemies.Count;
        diagnosticEnemiesPerFrame = Mathf.Max(1, checksPerFrame);
        diagnosticScanIndex = _currentIndex;
        diagnosticScanInProgress = _scanInProgress;
    }
#endif
}

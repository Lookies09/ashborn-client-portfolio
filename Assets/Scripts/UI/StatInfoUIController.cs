using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static PlayerEnums;
using static StatNameResolver;
using static StatValueFormatter;


public class StatInfoUIController : MonoBehaviour
{
    [SerializeField] private RectTransform slotsParent;
    [SerializeField] private GameObject statInfoSlotPrefab;
    private List<StatInfoUI> _uiList = new List<StatInfoUI>();

    private PlayerRuntimeStats _playerRuntimeStats;

    private void Awake()
    {
        _playerRuntimeStats = FindFirstObjectByType<PlayerRuntimeStats>();
    }

    private void Start()
    {
        CreateStatUI();

        // PlayerRuntimeStats의 이벤트를 직접 구독
        if (_playerRuntimeStats != null)
        {
            _playerRuntimeStats.OnStatsChanged += UpdateStatUI;
        }

        UpdateStatUI();
    }

    private void OnDisable()
    {
        if (_playerRuntimeStats != null)
        {
            _playerRuntimeStats.OnStatsChanged -= UpdateStatUI;
        }
    }

    private void UpdateStatUI()
    {
        foreach (var v in _uiList)
        {
            float typeValue = _playerRuntimeStats.GetStat(v.statType);
            v.RefreshUI(FormatStatValue(v.statType, typeValue));
        }
    }

    private void UpdateTargetStatUI(PlayerStatType type, string value)
    {
        foreach (var v in _uiList)
        {
            if (!v.statType.Equals(type)) continue;
            v.RefreshUI(value);
        }
    }

    private void CreateStatUI()
    {
        // 초기화
        foreach (Transform child in slotsParent) Destroy(child.gameObject);
        _uiList.Clear();

        var statTypes = Enum.GetValues(typeof(PlayerStatType));

        foreach (PlayerStatType type in statTypes)
        {
            // 슬롯 생성
            GameObject go = Instantiate(statInfoSlotPrefab, slotsParent);
            StatInfoUI slot = go.GetComponent<StatInfoUI>();

            float typeValue = _playerRuntimeStats.GetStat(type);

            if (slot != null)
            {
                slot.Init(StatNameResolver.Get(type), FormatStatValue(type, typeValue), type);
                _uiList.Add(slot);
            }
        }
    }

}

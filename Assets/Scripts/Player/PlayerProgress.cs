using System;
using UnityEngine;

public class PlayerProgress : MonoBehaviour
{
    public event Action<int, int> OnProgressChanged;
    public event Action<int> OnLevelUp;
    public int CurrentExp { get; private set; }
    public int CurrentLevel { get; private set; } = 1;

    public void AddExp(int amount)
    {
        CurrentExp += amount;

        // 레벨업 가능 여부 체크
        PlayerLevelManager.Instance.TryLevelUp(this);

        // 경험치 획득 알림 (현재 경험치와 현재 레벨 전달)
        OnProgressChanged?.Invoke(CurrentExp, CurrentLevel);
    }

    public void LevelUp(int overflowExp)
    {
        CurrentLevel++;
        CurrentExp = overflowExp;

        // 레벨업 시 스킬 선택창 띄우기
        PlayerLevelManager.Instance.ShowLevelUpSKillSelection();
        SoundManager.Instance.PlayLevelUpSound();

        // 이벤트 호출
        OnLevelUp?.Invoke(CurrentLevel);
        OnProgressChanged?.Invoke(CurrentExp, CurrentLevel);
    }
}

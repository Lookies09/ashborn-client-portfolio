
public interface IHealth
{
    int CurrentHP { get; }
    bool IsDead { get; }

    void Heal(int amount);
    void Kill();
    void IncreaseMaxHP(int amount);
    void ResetHealth();
    void Revive(float healthPercentage = 1f);
}


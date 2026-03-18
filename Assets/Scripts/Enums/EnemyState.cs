using Unity.Behavior;

[BlackboardEnum]
public enum EnemyState
{
    IDLE,
    PATROL,
    WANDER,
    INVESTIGATE,
    CHASE,
    COMBAT,
    TAKE_COVER,
    DODGE,
    FLEE,
    DEAD
}

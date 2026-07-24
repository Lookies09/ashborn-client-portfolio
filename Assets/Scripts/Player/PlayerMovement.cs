using UnityEngine;
using static PlayerEnums;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private PlayerStatSystem stat;
    [SerializeField] private PlayerRuntimeStats runtimeStat;
    private PlayerStateMachine _stateMachine;

    private CharacterController _cc;

    private float _cacheMoveSpeed = 0f;

    public bool IsMoving { get; private set; } = false;
    public Vector2 MoveDirection { get; private set; } = Vector2.zero;
    public float Velocity { get; private set; } = 0f;
    public float NormalizedVelocity { get; private set; } = 0f;    


    [Tooltip("플레이어 회전 속도")]
    [SerializeField] private float rotateSpeed = 10f;

    private void Awake()
    {
        _stateMachine = GetComponent<PlayerStateMachine>();
        _cc = GetComponent<CharacterController>();
    }

    private void Start()
    {
        runtimeStat.OnStatsChanged += GetSpeedStat;

        _cacheMoveSpeed = runtimeStat.GetStat(PlayerStatType.MoveSpeed);
    }

    void Update()
    {
        if (GameManager.Instance.CurrentState.Equals(GameEnums.GameState.Lobby)) return;

        if (_stateMachine.CurrentState == PlayerState.Dead)  return;

        if (Joystick.Instance == null) return;

        float x = Joystick.Instance.Horizontal;
        float y = Joystick.Instance.Vertical;

        MoveDirection = new Vector2(x, y);
        IsMoving = MoveDirection.sqrMagnitude > 0;

        Velocity = MoveDirection.magnitude * _cacheMoveSpeed;
        NormalizedVelocity = Mathf.Clamp(Velocity / stat.MAX_MOVESPEED_LIMIT, 0f, 1f);

        if (IsMoving)
        {
            Vector3 move = new Vector3(MoveDirection.x, 0, MoveDirection.y);

            _cc.Move(move * _cacheMoveSpeed * Time.deltaTime);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(move),
                Time.deltaTime * rotateSpeed
            );
        }
    }

    public void GetSpeedStat()
    {
        _cacheMoveSpeed = runtimeStat.GetStat(PlayerStatType.MoveSpeed);
    }

}

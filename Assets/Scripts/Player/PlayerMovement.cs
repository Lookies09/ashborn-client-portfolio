using UnityEngine;
#if UNITY_WEBGL || UNITY_EDITOR
using UnityEngine.InputSystem;
#endif
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

#if !UNITY_WEBGL && !UNITY_EDITOR
        if (Joystick.Instance == null) return;
#endif

        MoveDirection = Vector2.ClampMagnitude(ReadMoveInput(), 1f);
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

    private static Vector2 ReadMoveInput()
    {
#if UNITY_WEBGL || UNITY_EDITOR
        Vector2 keyboardInput = ReadKeyboardInput();
        if (keyboardInput.sqrMagnitude > 0f)
        {
            return keyboardInput;
        }
#endif

        return Joystick.Instance != null
            ? new Vector2(Joystick.Instance.Horizontal, Joystick.Instance.Vertical)
            : Vector2.zero;
    }

#if UNITY_WEBGL || UNITY_EDITOR
    private static Vector2 ReadKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
        {
            return Vector2.zero;
        }

        float horizontal = 0f;
        float vertical = 0f;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            horizontal -= 1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            horizontal += 1f;
        }

        if (keyboard.sKey.isPressed || keyboard.downArrowKey.isPressed)
        {
            vertical -= 1f;
        }

        if (keyboard.wKey.isPressed || keyboard.upArrowKey.isPressed)
        {
            vertical += 1f;
        }

        return new Vector2(horizontal, vertical);
    }
#endif

    public void GetSpeedStat()
    {
        _cacheMoveSpeed = runtimeStat.GetStat(PlayerStatType.MoveSpeed);
    }

}

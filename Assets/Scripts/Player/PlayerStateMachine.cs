using System;
using UnityEngine;

public enum PlayerState
{
    Idle,
    Move,
    Dead
}

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState CurrentState { get; private set; } = PlayerState.Idle;

    public event Action<PlayerState, PlayerState> OnStateChanged; // (previousState, newState)

    private PlayerMovement _playerMovement;
    private IHealth _health;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _health = GetComponent<IHealth>();
    }

    private void Start()
    {
        if (_health is PlayerHealth playerHealth)
        {
            playerHealth.OnKilled += () => ChangeState(PlayerState.Dead);
            playerHealth.OnRevived += () => ChangeState(PlayerState.Idle);
        }
    }

    private void OnDestroy()
    {
        if (_health is PlayerHealth playerHealth)
        {
            playerHealth.OnKilled -= () => ChangeState(PlayerState.Dead);
            playerHealth.OnRevived -= () => ChangeState(PlayerState.Idle);
        }
    }

    private void Update()
    {
        if (CurrentState == PlayerState.Dead) return;

        if (_playerMovement.IsMoving)
        {
            ChangeState(PlayerState.Move);
        }
        else
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void ChangeState(PlayerState newState)
    {
        if (CurrentState == newState) return;
        
        PlayerState previousState = CurrentState;
        CurrentState = newState;

        //Debug.Log($"현재 상태: {newState}");
        
        OnStateChanged?.Invoke(previousState, newState);
    }
}

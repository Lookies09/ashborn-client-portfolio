using UnityEngine;

public class PlayerAnimationController : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerStateMachine stateMachine;
    
    [SerializeField, Range(0f, 1f)] private float smoothTime = 0.1f;
    private Animator _animator;

    public readonly int velocityHash = Animator.StringToHash("Velocity");
    public readonly int dieHash = Animator.StringToHash("Die");

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        
        if (stateMachine == null)
        {
            stateMachine = GetComponent<PlayerStateMachine>();
        }
    }

    private void Start()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged += HandleStateChanged;
        }
    }

    private void OnDestroy()
    {
        if (stateMachine != null)
        {
            stateMachine.OnStateChanged -= HandleStateChanged;
        }
    }

    private void Update()
    {
        if (stateMachine != null && stateMachine.CurrentState != PlayerState.Dead)
        {
            _animator.SetFloat(velocityHash, playerMovement.NormalizedVelocity, smoothTime, Time.deltaTime);
        }
    }

    private void HandleStateChanged(PlayerState previousState, PlayerState newState)
    {
        if (newState == PlayerState.Dead)
        {
            _animator.CrossFade(dieHash, 0.1f);
        }
    }

    public void ResetAnimation()
    {
        _animator.Rebind();
    }
}

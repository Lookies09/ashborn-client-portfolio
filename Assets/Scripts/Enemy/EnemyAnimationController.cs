using UnityEngine;
using UnityEngine.AI;
using Unity.Behavior;

public class EnemyAnimationController : MonoBehaviour
{
    [SerializeField] private string[] deadAnimationNames;
    [SerializeField] private string lookAroundAnimationName;
    private int[] _attackHashes;
    private int[] _deadHashes;
    private int _lookAroundHash;
    private float _speedSmooth;

    public readonly int velocityHash = Animator.StringToHash("Velocity"); 
    public readonly int detectedHash = Animator.StringToHash("IsTargetDetected");

    private Animator _animator;
    private EnemyController _enemyController;

    public Animator Animator => _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();


        _deadHashes = new int[deadAnimationNames.Length];
        for (int i = 0; i < deadAnimationNames.Length; i++)
            _deadHashes[i] = Animator.StringToHash(deadAnimationNames[i]);

        _lookAroundHash = Animator.StringToHash(lookAroundAnimationName);
    }

    private void Update()
    {
        if (_enemyController.Health.IsDead) return;

        float currentSpeed = _enemyController.NavAgent.velocity.magnitude;

        // 0~1 정규화
        float normalizedSpeed = currentSpeed / _enemyController.Data.MaxMoveSpeed;
        normalizedSpeed = Mathf.Clamp01(normalizedSpeed);

        _enemyController.BTAgent.GetVariable("IsTargetDetected", out BlackboardVariable<bool> detected);

        // 애니메이션 보간
        _speedSmooth = Mathf.Lerp(_speedSmooth, normalizedSpeed, Time.deltaTime * 10f);

        _animator.SetFloat(velocityHash, _speedSmooth);
        _animator.SetBool(detectedHash, detected);

    }

    public void Init(EnemyController enemyController)
    {
        _enemyController = enemyController;
    }

    public void PlayAttackAnimation(string animationNames)
    {
        if (animationNames == null || _animator == null) return;
        _animator.CrossFade(animationNames, 0.1f);
    }

    public void PlayRandomDeath()
    {
        if (_deadHashes.Length == 0) return;
        int idx = Random.Range(0, _deadHashes.Length);
        _animator.CrossFade(_deadHashes[idx], 0.1f);
    }

    public void PlayLookAround()
    {
        _animator.CrossFade(_lookAroundHash, 0.1f);
    }

    public void AnimatorReset()
    {
        if (_animator == null) return;
        _animator.Rebind();
        _animator.Update(0f);
    }
}

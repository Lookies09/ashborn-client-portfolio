using System;
using Unity.Behavior;
using Unity.Properties;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Localization.SmartFormat;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "ExecuteAttack", story: "Attack [Target] by [EnemyController] and AttackIndex", category: "Action", id: "c6414afa1222ff4e6156537353936c6c")]
public partial class ExecuteAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<EnemyController> EnemyController;
    [SerializeReference] public BlackboardVariable<int> CurrentAttackIndex;

    private bool _attackStarted;
    private bool _useDurationTimeout;
    private int _animHash;
    private Animator _anim;
    private EnemyAttackProfile _profile;
    private float _startTime;
    private float _expectedDuration;
    private int _currentIndex = -1;

    protected override Status OnStart()
    {
        EnemyController.Value.BTAgent.SetVariableValue("IsAttacking", true);
        var ctrl = EnemyController?.Value;
        int idx = CurrentAttackIndex?.Value ?? -1;
        if (ctrl == null || idx < 0 || ctrl.Data == null || idx >= ctrl.Data.Attacks.Count)
            return Status.Failure;

        _currentIndex = idx;
        _profile = ctrl.Data.Attacks[idx];
        _anim = ctrl.AnimationController?.Animator;
        if (_anim == null) return Status.Failure;

        string animName = _profile.animationName;
        if (string.IsNullOrEmpty(animName)) return Status.Failure;

        _animHash = Animator.StringToHash(animName);

        if (_profile.animationDuration > 0f)
        {
            _expectedDuration = _profile.animationDuration;
            _useDurationTimeout = true;
        }
        else
        {
            float clipLen = GetClipLength(_anim, animName);
            if (clipLen > 0f)
            {
                _expectedDuration = clipLen;
                _useDurationTimeout = true;
            }
            else
            {
                // 애니메이션 클립 길이를 찾을 수 없으면 기본값 사용
                Debug.Log("기본 3초 사용");
                _expectedDuration = 3f;
                _useDurationTimeout = false;
            }
        }


        _startTime = Time.time;

        _anim.SetBool("IsAttacking", true);
        ctrl.AnimationController.PlayAttackAnimation(animName);
        _attackStarted = true;

        return Status.Running;
    }


    protected override Status OnUpdate()
    {
        var ctrl = EnemyController?.Value;
        var target = Target?.Value;
        if (ctrl == null || target == null)
        {
            return Status.Failure;
        }
        if (ctrl.Health?.IsDead == true)
        {
            return Status.Failure;
        }
        if (target.TryGetComponent<IHealth>(out var th) && th.IsDead)
        {
            return Status.Failure;
        }
        if (!_attackStarted || _anim == null)
        {
            return Status.Failure;
        }

        var state = _anim.GetCurrentAnimatorStateInfo(0);
        SmoothLookAt(target.transform);

        float elapsed = Time.time - _startTime;

        // 사용자 입력 기반 종료
        if (_useDurationTimeout && elapsed >= _expectedDuration)
        {
            ctrl.Attack?.MarkAttackUsed(_currentIndex);
            ctrl.Attack?.MarkGlobalUsed();
            return Status.Success;
        }

        // normalizedTime 기반 종료
        if (!_anim.IsInTransition(0) &&
            (state.shortNameHash == _animHash || state.fullPathHash == _animHash))
        {
            if (state.normalizedTime >= 1f)
            {
                ctrl.Attack?.MarkAttackUsed(_currentIndex);
                ctrl.Attack?.MarkGlobalUsed();
                return Status.Success;
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        _anim.SetBool("IsAttacking", false);
        EnemyController.Value.BTAgent.SetVariableValue("IsAttacking", false);
    }

    private float GetClipLength(Animator animator, string clipName)
    {
        if (animator?.runtimeAnimatorController == null) return 0f;
        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip != null && clip.name == clipName)
                return clip.length;
        }
        return 0f;
    }

    private void SmoothLookAt(Transform target)
    {
        Vector3 dir = (target.position - Self.Value.transform.position).normalized;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.0001f) return;

        Quaternion targetRot = Quaternion.LookRotation(dir);
        Self.Value.transform.rotation = Quaternion.Slerp(
            Self.Value.transform.rotation,
            targetRot,
            10f * Time.deltaTime
        );
    }

}


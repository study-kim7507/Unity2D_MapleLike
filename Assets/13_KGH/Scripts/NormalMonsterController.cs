using Google.Protobuf.Protocol;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

enum NormalMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}


// TODO: 현재 일반 몬스터의 스킬 구현 계획 없음.
enum NormalMonsterSkill
{
    Skill0 = 0,
    Skill1 = 1,
    // ...
}

public class NormalMonsterController : MonsterController
{
    private Coroutine inactiveHPBarCoroutine = null;

    // 패킷 수신 시 세팅되는 값
    NormalMonsterState currentState;
    NormalMonsterSkill currentSkill = NormalMonsterSkill.Skill0;    // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
    bool isRight;

    protected override void Update()
    {
        if (isAlreadyDie) return;

        base.Update();

        // 변수 초기화
        if (currentState != NormalMonsterState.Skill) hasUsedSkill = false;

        switch (currentState)
        {
            case NormalMonsterState.Idle:
                Idle();
                break;
            case NormalMonsterState.Move:
                Move();
                break;
            case NormalMonsterState.Stun:
                // Stun();
                break;
            case NormalMonsterState.Skill:
                Skill();
                break;
            case NormalMonsterState.Dead:
                Dead();
                break;
        }
    }

    protected override void FixedUpdate()
    {
        // 위치 동기화 (SyncPos)
        base.FixedUpdate();
    }

    protected override void Idle()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("idle")) monsterAnimator.SetTrigger("idle");
    }

    protected override void Move()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("move")) monsterAnimator.SetTrigger("move");

        monsterSpriteRenderer.flipX = isRight ? true : false;
    }

    protected override void Stun(int hitCount = 1)
    {
        StartCoroutine(PlayStunAudio(hitCount));
        monsterAnimator.SetTrigger("hit");
    }

    protected override void Skill()
    {
        // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
    }

    protected override void Dead()
    {
        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("die"))
        {
            isAlreadyDie = true;

            // 모든 트리거 초기화
            monsterAnimator.ResetTrigger("idle");
            monsterAnimator.ResetTrigger("hit");
            monsterAnimator.ResetTrigger("move");

            monsterAudioSource.PlayOneShot(monsterAudioClips.dieAudioClip);
            monsterAnimator.SetTrigger("die");
            monsterCollider.enabled = false;

            if (inactiveHPBarCoroutine != null)
                StopCoroutine(inactiveHPBarCoroutine);
            hpBar.SetActive(false);
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 State 설정
    public override void SetState(MonsterState newState, int hitCount = 1)
    {
        switch (newState)
        {
            case MonsterState.MIdle:
                currentState = NormalMonsterState.Idle;
                break;
            case MonsterState.MMoving:
                currentState = NormalMonsterState.Move;
                break;
            case MonsterState.MStun:
                currentState = NormalMonsterState.Stun;
                Stun(hitCount);
                break;
            case MonsterState.MSkill:
                currentState = NormalMonsterState.Skill;
                break;
            case MonsterState.MDead:
                currentState = NormalMonsterState.Dead;
                break;
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 스프라이트 방향 설정 
    public override void SetDirection(bool isRight)
    {
        this.isRight = isRight;
    }


    // TODO: 아직 일반 몬스터 스킬 구현 계획 없음.
    public void SetNormalMonsterSkillType(BossMonsterSkillType newSkill)
    {
        currentSkill = (NormalMonsterSkill)newSkill;
    }

    public void UpdateHPBarGauge()
    {
        hpBar.SetActive(true);

        if (inactiveHPBarCoroutine != null)
            StopCoroutine(inactiveHPBarCoroutine);

        hpBarGauge.fillAmount = (float)info.StatInfo.Hp / (float)info.StatInfo.MaxHp;

        inactiveHPBarCoroutine = StartCoroutine(InActiveHPBar());
    }

    private IEnumerator InActiveHPBar()
    {
        yield return new WaitForSeconds(5.0f);
        hpBar.SetActive(false);
        inactiveHPBarCoroutine = null;
    }

    private IEnumerator PlayStunAudio(int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            monsterAudioSource.PlayOneShot(monsterAudioClips.stunAudioClip);
            yield return new WaitForSeconds(0.1f); // 0.1초 대기
        }
    }
}
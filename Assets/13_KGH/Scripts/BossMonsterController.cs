/*
 * 현재 구현은 시그너스 기준. 추후 보스가 추가되면 상속을 통해 확장 가능   
*/

using Google.Protobuf.Protocol;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

enum BossMonsterState
{
    Idle = 0,
    Move = 1,
    Stun = 2,
    Skill = 3,
    Dead = 4,
}

enum BossMonsterSkill
{
    DarkGenesis = 0,
    DarknessBall = 1,
    AbyssTornado = 2,
    SpawnNormalMonster = 3,
}

public class BossMonsterController : MonsterController
{
    // 패킷 수신 시 세팅되는 값
    BossMonsterState currentState = BossMonsterState.Idle;
    BossMonsterSkill currentSkill = BossMonsterSkill.DarkGenesis;
    bool isRight;

    [Header("보스몬스터 스킬")]
    // 스킬 발사체 스폰 포지션
    [SerializeField] GameObject darknessBallSpawnPositionLeft;
    [SerializeField] GameObject darknessBallSpawnPositionRight;

    // 스킬 프리팹
    [SerializeField] GameObject darkGenesisSkillProjectilePrefab;
    [SerializeField] GameObject darknessBallSkillProjectilePrefab;
    [SerializeField] GameObject abyssTornadoSkillProjectilePrefab;

    protected override void Update()
    {
        if (isAlreadyDie) return;

        base.Update();

        // 변수 초기화
        if (currentState != BossMonsterState.Skill) hasUsedSkill = false;
        
        switch (currentState)
        {
            case BossMonsterState.Idle:
                Idle();
                break;
            case BossMonsterState.Move:
                Move();
                break;
            case BossMonsterState.Stun:
                // Stun();
                break;
            case BossMonsterState.Skill:
                Skill();
                break;
            case BossMonsterState.Dead:
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
        // 보스몬스터는 히트 애니메이션 존재하지 않음.
        StartCoroutine(PlayStunAudio(hitCount));
    }

    protected override void Skill()
    {
        if (hasUsedSkill) return;

        AnimatorStateInfo animatorStateInfo = monsterAnimator.GetCurrentAnimatorStateInfo(0);
        if (!animatorStateInfo.IsName("darkGenesis") && !animatorStateInfo.IsName("darknessBall") && !animatorStateInfo.IsName("abyssTornado") && !animatorStateInfo.IsName("SpawnNormalMonster"))
        {
            switch(currentSkill)
            {
                case BossMonsterSkill.DarkGenesis:
                    monsterAnimator.SetTrigger("darkGenesis");
                    break;
                case BossMonsterSkill.DarknessBall:
                    monsterAnimator.SetTrigger("darknessBall");
                    break;
                case BossMonsterSkill.AbyssTornado:
                    monsterAnimator.SetTrigger("abyssTornado");
                    break;
                case BossMonsterSkill.SpawnNormalMonster:
                    monsterAnimator.SetTrigger("spawnNormalMonster");
                    break;

            }
            monsterAudioSource.PlayOneShot(monsterAudioClips.skillAudioClips[(int)currentSkill]);
            hasUsedSkill = true;
        }
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

            monsterAnimator.ResetTrigger("darkGenesis");
            monsterAnimator.ResetTrigger("darknessBall");
            monsterAnimator.ResetTrigger("abyssTornado");
            monsterAnimator.ResetTrigger("spawnNormalMonster");


            monsterAudioSource.PlayOneShot(monsterAudioClips.dieAudioClip);
            monsterAnimator.SetTrigger("die");
            monsterCollider.enabled = false;

            hpBar.SetActive(false);
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 State 설정.
    public override void SetState(MonsterState newState, int hitCount = 1)
    {
        switch (newState)
        {
            case MonsterState.MIdle:
                currentState = BossMonsterState.Idle;
                break;
            case MonsterState.MMoving:
                currentState = BossMonsterState.Move;
                break;
            case MonsterState.MStun:
                currentState = BossMonsterState.Stun;
                Stun(hitCount);
                break;
            case MonsterState.MSkill:
                currentState = BossMonsterState.Skill;
                break;
            case MonsterState.MDead:
                currentState = BossMonsterState.Dead;
                break;
        }
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 스프라이트 방향 설정.
    public override void SetDirection(bool isRight)
    {
        this.isRight = isRight;
    }

    // 패킷 수신 시 핸들러에서 호출. 서버에서 보낸 패킷에 따라 어떤 스킬을 사용할 지 결정.

    public void SetBossSkillType(BossMonsterSkillType newSkill)
    {
        currentSkill = (BossMonsterSkill)newSkill;
    }

    public void UpdateHPBarGauge()
    {
        hpBarGauge.fillAmount = (float)info.StatInfo.Hp / (float)info.StatInfo.MaxHp;
    }

    private IEnumerator PlayStunAudio(int hitCount)
    {
        for (int i = 0; i < hitCount; i++)
        {
            monsterAudioSource.PlayOneShot(monsterAudioClips.stunAudioClip);
            yield return new WaitForSeconds(0.1f); // 0.1초 대기
        }
    }


    #region 스킬

    // darkGenesis 애니메이션에서 이벤트로 호출됨.
    public void DarkGenesis()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> players = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player_"))
            {
                players.Add(obj);
            }
        }

        foreach (GameObject player in players)
        {
            GameObject projectile = Instantiate(darkGenesisSkillProjectilePrefab);
            projectile.GetComponent<MonsterSkill>().SetDamage(info.StatInfo.AttackPower);
            projectile.transform.position = new Vector2(player.transform.position.x, -2.558871f);
        }
    }

    // darknessBall 애니메이션에서 이벤트로 호출됨.
    public void DarknessBall()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> players = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player_"))
            {
                players.Add(obj);
            }
        }

        foreach (GameObject player in players)
        {
            GameObject projectile = Instantiate(darknessBallSkillProjectilePrefab);
            projectile.GetComponent<MonsterSkill>().SetDamage(info.StatInfo.AttackPower);

            // flipX에 따라 x 좌표를 반전
            Vector3 spawnPosition = monsterSpriteRenderer.flipX ? darknessBallSpawnPositionRight.transform.position : darknessBallSpawnPositionLeft.transform.position;

            projectile.transform.position = spawnPosition;
            projectile.GetComponent<DarknessBall>().target = player;
        }
    }

    // abyssTornado 애니메이션에서 이벤트로 호출됨.
    public void AbyssTornado()
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();
        List<GameObject> players = new List<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player"))
            {
                players.Add(obj);
            }
        }

        foreach (GameObject player in players)
        {
            GameObject projectile = Instantiate(abyssTornadoSkillProjectilePrefab);
            projectile.GetComponent<MonsterSkill>().SetDamage(info.StatInfo.AttackPower);

            projectile.transform.position = new Vector2(player.transform.position.x, -2.558871f);
        }
    }

    // spawnNormalMonster 애니메이션에서 이벤트로 호출됨.
    public void SpawnNormalMonster()
    {
        // 몬스터 소환은 서버에서 클라이언트에서는 아무런 구현 필요 없음.
    }
    #endregion
}
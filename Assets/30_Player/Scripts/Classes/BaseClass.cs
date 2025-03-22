using Google.Protobuf.Protocol;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// 직업 요소를 담는 추상 클래스
/// 캐릭터의 스킬 타격범위 트리거를 활성화하는 역할도 지니고 있다.
/// 트리거를 활성화, 비활성화하는 함수와 공격이 끝난 후 컨트롤러에서 공격상태가 종료됨을 알리는 함수를 가짐
/// 이 함수들은 애니메이션 내에 이벤트로 지정되어 있다.
/// </summary>
public abstract class BaseClass : MonoBehaviour
{
    protected PlayerInformation info;
    protected PlayerInfo playerInfo;
    protected PlayerStatInfo playerStatInfo;
    //protected Controller controller;
    protected YHSMyPlayerController controller;

    [Header("스킬 요소")]
    public Dictionary<string, SkillData> skillList = new Dictionary<string, SkillData>();    // 해당 직업의 스킬과 현재 스킬 레벨
    public Dictionary<string, AudioClip> skillSound = new Dictionary<string, AudioClip>();
    public Skill[] classSkill;    // 해당 직업의 스킬
    public AudioClip[] SkillSound;
    public GameObject Hitbox;
    public GameObject Effect;
    public GameObject BuffEffect;
    public GameObject HitObject;
    protected GameObject currentHit;
    protected AudioSource SkillSource;

    protected MonsterController target;

    protected virtual void Start()
    {
        SkillSource = gameObject.AddComponent<AudioSource>();
        info = GetComponentInParent<PlayerInformation>();
        playerInfo = PlayerInformation.playerInfo;          // static이어서 없어도 됩니다.
        playerStatInfo = PlayerInformation.playerStatInfo;  // static이어서 없어도 됩니다.
        controller = GetComponentInParent<YHSMyPlayerController>();
    }

    /// <summary>
    /// 직업의 스펙을 조정한다.
    /// </summary>
    public abstract void ClassStat();

    /// <summary>
    /// 스킬을 사용할 때 사용한다. Bool타입으로 스킬 사용에 성공했는지 여부를 리턴한다.
    /// </summary>
    public virtual bool UseSkill()
    {
        int cost = skillList["string"].GetManaCost();
        if (!CheckMana(cost))
        {
            Debug.Log("MP 부족");
            return false;
        }
        else
        {
            info.SetPlayerMp(-cost);
            return true;
        }
    }

    public virtual bool UseSkill(string skillName)
    {
        int cost = skillList[skillName].GetManaCost();
        if (!CheckMana(cost))
        {
            Debug.Log("MP 부족");
            return false;
        }
        else
        {
            info.SetPlayerMp(-cost);
            return true;
        }
    }

    /// <summary>
    /// 해당 직업의 스킬을 초기화한다.
    /// </summary>
    protected void InitializeSkill()
    {
        if (classSkill == null) { return; }
        for (int i = 0; i < classSkill.Length; i++)
        {
            skillList[classSkill[i].skillName] = new SkillData(classSkill[i], 1);
        }
        for(int i = 0; i < SkillSound.Length; i++)
        {
            skillSound[classSkill[i].skillName] = SkillSound[i];
        }
    }

    /// <summary>
    /// 어떤 스킬을 사용할 때 마나가 충분한지를 확인하는 함수
    /// </summary>
    protected bool CheckMana(int cost)
    {
        if (info.GetPlayerMp() >= cost)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// 스킬포인트를 이용해 스킬 레벨을 올릴 때 사용한다.
    /// </summary>
    protected bool PointUpSkill(string skillName)
    {
        if (!skillList.ContainsKey(skillName)) { return false; }
        if (skillList[skillName].level >= skillList[skillName].skill.maxLevel) { return false; }

        skillList[skillName].level++;
        return true;
    }

    protected abstract void ActiveHitbox();

    /// <summary>
    /// 실제 타격 이펙트를 생성하는 함수. 데미지 처리 밑 패킷을 생성하여 보내는 처리도 여기서 할 예정
    /// 히트 오브젝트를 생성하는 함수
    /// 트리거 스크립트로부터 대상을 받아 해당 대상의 위치에 히트 오브젝트 생성
    /// </summary>
    public abstract void CreateHitEffect();

    protected void SendHitMonsterPacket(List<int> damageList)
    {
        if (controller == null)
        {
            return;
        }

        C_HitMonster HitMonster = new C_HitMonster();
        HitMonster.MonsterId = target.Id;

        foreach (var damage in damageList)
        {
            HitMonster.PlayerAttackPowers.Add(damage);
        }
        
        NetworkManager.Instance.Send(HitMonster);
        Debug.Log("패킷 송신함, totalDamage : " + damageList);
    }

    protected virtual void DeactiveHitbox()
    {
        if (currentHit != null)
        {
            Destroy(currentHit);
        }
        if (controller != null)
        {
            controller.isDamaged = true;
            controller.isAttacking = false;
        }
    }

    /// <summary>
    /// 플레이어가 몬스터를 공격했을 때의 데미지를 계산하는 메서드
    /// </summary>
    /// <param name="target">공격한 몬스터</param>
    /// <param name="power">플레이어 공격력 또는 마력</param>
    /// <param name="skillKey">플레이어가 발동한 스킬</param>
    /// <param name="totalDamage">플레이어가 가한 총 데미지</param>
    /// <returns></returns>
    protected virtual List<int> CalculatePlayerToMonsterDamage(MonsterController target, int power, string skillKey, out int totalDamage)
    {
        List<int> damageList = new List<int>();
        totalDamage = 0;

        // 1레벨 스킬 데이터 가져오기
        float skillDamage = skillList[skillKey].skill.damage / 100f;
        int skillHitCount = skillList[skillKey].skill.hitCount;

        // 몬스터 방어력 가져오기
        float monsterDefense = 1 - target.info.StatInfo.Defense / 100f;

        for (int i = 0; i < skillHitCount; i++)
        {
            // 데미지 무작위 편차 부여
            float randomOffset = Random.Range(-0.5f, 0.5f);   // 0.5f 값을 추후에 "숙련도" 스탯으로 대체

            // 공식에 따라 데미지 계산
            int finalDamage = (int)((power * (1f + randomOffset)) * skillDamage * monsterDefense);
            finalDamage = Mathf.Clamp(finalDamage, 0, finalDamage);
            damageList.Add(finalDamage);
            totalDamage += finalDamage;
        }

        return damageList;
    }

    public abstract bool UseBuffSkill();
    protected abstract void RemoveBuff();
}

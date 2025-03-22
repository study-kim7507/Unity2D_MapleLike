using Google.Protobuf.Protocol;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(AudioSource))]
public class MonsterController : BaseController
{
    [Serializable]
    public class MonsterAudioClips
    {
        public AudioClip stunAudioClip;
        public AudioClip dieAudioClip;
        public List<AudioClip> skillAudioClips; 
    }

    public MonsterInfo info = new MonsterInfo();

    public void UpdateInfo(MonsterInfo newInfo)
    {
        info = newInfo;
        info.Name = newInfo.Name;  
        info.DestinationX = newInfo.DestinationX;
        info.DestinationY = newInfo.DestinationY;
        info.StatInfo = newInfo.StatInfo;
        info.CreatureState = newInfo.CreatureState;
    }

    protected Rigidbody2D monsterRigidbody;
    protected Collider2D monsterCollider;
    protected Animator monsterAnimator;
    protected SpriteRenderer monsterSpriteRenderer;
    protected AudioSource monsterAudioSource;

    [Header("몬스터가 재생할 수 있는 오디오 클립")]
    [SerializeField] protected MonsterAudioClips monsterAudioClips;

    [Header("몬스터 체력")]
    [SerializeField] protected GameObject hpBar;
    [SerializeField] protected Image hpBarGauge;

    // 지속적으로 서버로부터 넘어오는 State 변경 관련 패킷에 대해 애니메이션 동기화를 위한 변수
    protected bool hasUsedSkill = false;                          
    protected bool isAlreadyDie = false;

    public int lastHitPlayerId;

    protected void OnEnable()
    {
        monsterRigidbody = GetComponent<Rigidbody2D>();
        monsterCollider = GetComponent<Collider2D>();
        monsterAnimator = GetComponent<Animator>();
        monsterSpriteRenderer = GetComponent<SpriteRenderer>();
        monsterAudioSource = GetComponent<AudioSource>();

        StartCoroutine(SpawnCoroutine());
    }


    //======================================================================================================
    // 자식클래스의 구조에 따라 재정의가 필요한 함수.
    //====================================================================================================== 
    
    protected virtual void Idle() { }
    protected virtual void Move() { }
    protected virtual void Stun(int hitCount = 1) { }
    protected virtual void Skill() { }
    protected virtual void Dead() { }
    public virtual void SetState(MonsterState newState, int hitCount = 1) { }
    public virtual void SetDirection(bool isRight) { }

    //======================================================================================================
    // 자식클래스에서 구조 변경이 필요없음. 공통적으로 사용되는 함수. 재정의 필요없음
    //====================================================================================================== 

    public void SetCurrentHp(int newHp) => info.StatInfo.Hp = newHp;

    private IEnumerator SpawnCoroutine()
    {
        monsterCollider.enabled = false;
        monsterRigidbody.bodyType = RigidbodyType2D.Kinematic;

        float duration = 2.0f;
        float elapsedTime = 0f;

        Color monsterSpriteColor = monsterSpriteRenderer.color;
        
        Color startColor = new Color(monsterSpriteColor.r, monsterSpriteColor.g, monsterSpriteColor.b, 0);
        Color endColor = new Color(monsterSpriteColor.r, monsterSpriteColor.g, monsterSpriteColor.b, 1);

        monsterSpriteRenderer.color = startColor;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            monsterSpriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration);
            yield return null;
        }

        monsterSpriteRenderer.color = endColor;
        monsterCollider.enabled = true;
        monsterRigidbody.bodyType = RigidbodyType2D.Dynamic;
    }

    public void Despawn()
    {
        StartCoroutine(DespawnCoroutine());
    }

    private IEnumerator DespawnCoroutine()
    {
        monsterCollider.enabled = false;
        monsterRigidbody.bodyType = RigidbodyType2D.Kinematic;

        yield return new WaitForSeconds(1.0f);

        float duration = 2.0f; // 지속 시간
        float elapsedTime = 0f; // 경과 시간

        Color startColor = monsterSpriteRenderer.color; // 시작 색상
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); // 끝 색상 (Alpha 0)

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 업데이트
            monsterSpriteRenderer.color = Color.Lerp(startColor, endColor, elapsedTime / duration); // 색상 보간
            yield return null; // 다음 프레임까지 대기
        }

        monsterSpriteRenderer.color = endColor; // 최종 색상 설정
        Destroy(gameObject);
    }
}




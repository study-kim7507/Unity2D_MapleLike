using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Unity.Cinemachine;
using UnityEngine;

public class YHSMyPlayerController : PlayerController
{
    // 플레이어 데이터 컴포넌트
    public PlayerInformation playerInformation;
    public PlayerInventory playerInventory;
    public PlayerEquip playerEquip;
    public PlayerCanvas playerCanvas;

    // 위치 계산에 필요한 컴포넌트
    private Rigidbody2D rb;
    private BoxCollider2D playerDamagedCollider;    // 피격 판별용 (trigger)
    private EdgeCollider2D playerTerrainCollider;   // 지형 판별용

    // 기타 컴포넌트
    private CinemachineCamera cinemachine;
    private bool isPressedJump = false;    // 직접 점프를 눌렀는지 여부

    private bool isMyOnPortal = false;          // 플레이어가 포탈 위에 있는지
    private MapName nextMapName;                // 이동할 맵 이름

    private bool isMyOnBossPortal = false;      // 플레이어가 보스 입장 포탈 위에 있는지
    private GameObject bossRoomPortal;     

    private bool isKnockback = false;      // 넉백 상태
    private bool _invincible;
    private bool invincible                // 무적 상태
    {
        get => _invincible;
        set
        {
            _invincible = value;
            playerDamagedCollider.enabled = !value;
            if (value)
            {
                StartCoroutine(InvincibleTime());
            }
        }
    }

    protected override void Awake()
    {
        // FSM 생성
        base.Awake();

        // My Player로써 키 입력에 따라 동작하기 위한 컴포넌트들 추가
        gameObject.AddComponent<InputManager>();

        playerInformation = gameObject.AddComponent<PlayerInformation>();
        playerInventory = gameObject.AddComponent<PlayerInventory>();
        playerEquip = gameObject.AddComponent<PlayerEquip>();
        playerCanvas = gameObject.GetComponentInChildren<PlayerCanvas>();

        rb = gameObject.AddComponent<Rigidbody2D>();

        gameObject.GetComponentInChildren<SpriteRenderer>().sortingOrder = 50;

        // collider 컴포넌트들 추가
        playerDamagedCollider = gameObject.AddComponent<BoxCollider2D>();
        playerDamagedCollider.isTrigger = true;
        playerTerrainCollider = gameObject.AddComponent<EdgeCollider2D>();
        playerTerrainCollider.excludeLayers = 1 << LayerMask.NameToLayer("Monster") 
                                            | 1 << LayerMask.NameToLayer("BossMonster")
                                            | 1 << LayerMask.NameToLayer("Props2")
                                            | 1 << LayerMask.NameToLayer("Player")
                                            | 1 << LayerMask.NameToLayer("MyPlayer");

        // 플레이어 스탯 UI 초기화
        StatusBarManager.Instance.InitStatusBar(playerInformation);
        StatWindowManager.Instance.InitStatWindow(playerInformation);

        // 사망 처리를 위한 UI 초기화
        DeathManager.Instance.player = this;

        // 카메라 초기화
        cinemachine = GameObject.Find("CinemachineCamera").GetComponent<CinemachineCamera>();

        if (cinemachine == null)
        {
            Debug.LogError("Can't find Cinemachine Camera");
        }
    }

    protected override void Start()
    {
        base.Start();

        // Rigidbody2D 컴포넌트 세팅
        rb.gravityScale = 3;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.freezeRotation = true;

        // collider 컴포넌트 세팅
        playerDamagedCollider.offset = new Vector2(0f, 0.7f);
        playerDamagedCollider.size = new Vector2(0.4f, 1.4f);
        playerDamagedCollider.isTrigger = true;
        playerTerrainCollider.points = new Vector2[] { new Vector2(0f, 0.01f), new Vector2(0f, 0f) };

        // Cinemachine Camera 컴포넌트 세팅
        cinemachine.PreviousStateIsValid = false;
        cinemachine.Follow = transform;
        cinemachine.LookAt = transform;
        cinemachine.transform.position = transform.position;
        cinemachine.transform.LookAt(transform.position);

        //장비창 인벤토리창 연결 
        UIManager.Instance.ConnectPlayer();
        UIManager.Instance.InitItem();
        UIManager.Instance.InitMpPoitions();
        UIManager.Instance.InitHpPoitions();
        UIManager.Instance.hasInitialized = true;

        // 이 코루틴은 Start 최하단에 고정시켜주세요!!!
        StartCoroutine(FadeInOutManager.Instance.FadeIn());
    }

    protected override void Update()
    {
        //base.Update();
        playerSM.Execute();

        if (isAttacking == false && !isDead)
        {
            OperatePlayer();
        }
        if (isDead && !DeathManager.Instance.IsPopupActive())
        {
            OnDead();
            return;
        }
    }

    protected override void FixedUpdate()
    {
        if (isAttacking == false && !isDead)
        {
            MovePlayer();
        }
    }

    private void MovePlayer()
    {
        if (isDead == true)
        {
            return;
        }

        // 플레이어 좌우 이동
        float horizontalDirection = Input.GetAxis("Horizontal");
        SetMyPlayerDirection(horizontalDirection);

        if (horizontalDirection != 0)
        {
            //Debug.Log("Key Pressed: " + horizontalDirection);

            if (IsGrounded() == true)
            {
                // jumpState일 때는 jumpState를 유지한다.
                if (isKnockback)
                {
                    return;
                }
                rb.linearVelocityX = PlayerInformation.playerStatInfo.Speed * horizontalDirection;
                OnMove();
            }
            else
            {
                OnJump();
            }
        }
        else
        {
            if (IsGrounded() == true)
            {
                // jumpState일 때는 jumpState를 유지한다.
                if (isDamaged == true)
                {
                    OnHit();
                }
                else if (isAttacking)
                {
                    OnAttack();
                }
                else
                {
                    OnIdle();
                }
            }
            else
            {
                OnJump();
            }
        }

        // 플레이어 점프
        if (IsGrounded() == true && Input.GetKey(KeyCode.LeftAlt) && !isKnockback)
        {
            SoundManager.Instance.PlaySoundOneShot(ConstList.Jump);
            rb.linearVelocityY = PlayerInformation.playerStatInfo.Jump;
            isPressedJump = true;
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            if (playerClass.UseBuffSkill())
            {
                OnHit();
            }
        }
    }

    private void OperatePlayer()
    {
        if (isDead == true)
        {
            return;
        }

        // 플레이어 상하 이동
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            if (isMyOnPortal)
            {
                // 포탈로 다음 맵 이동
                isAttacking = false;
                isDamaged = false;
                OnIdle();
                SendChangeMapPacket();

                SoundManager.Instance.PlaySoundOneShot(ConstList.Portal);
            }

            if (isMyOnBossPortal)
            {
                // 포탈로 보스 맵 이동
                bossRoomPortal.GetComponent<BossRoomPortal>().BossRoomEnterUIActive(gameObject);
            }
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {

        }

        // 플레이어 공격
        if (Input.GetKey(KeyCode.LeftControl) && !isAttacking)
        {
            //Debug.Log("Key Pressed: Left Ctrl");
            OnAttack();
        }
    }


    #region 패킷 송신 기능
    /// <summary>
    /// 현재 위치, 방향, 상태를 서버에 송신한다.
    /// </summary>
    public void SendPlayerMovePacket()
    {
        C_PlayerMove movePacket = new C_PlayerMove();
        movePacket.State = (PlayerState)playerSM.NowState;
        movePacket.PositionX = transform.position.x;
        movePacket.PositionY = transform.position.y;
        movePacket.IsRight = isRight;

        NetworkManager.Instance.Send(movePacket);
    }

    /// <summary>
    /// 피격 사실을 서버에 송신한다.
    /// </summary>
    public void SendPlayerDamaged(int damage)
    {
        C_PlayerDamaged damagedPacket = new C_PlayerDamaged();
        damagedPacket.Damage = damage;
        NetworkManager.Instance.Send(damagedPacket);
    }

    /// <summary>
    /// 사망 상태를 서버에 송신한다.
    /// </summary>
    public void SendPlayerDiePacket()
    {
        //Debug.Log("Player Id [ " + Id + " ] has dead.");
        C_PlayerDie diePacket = new C_PlayerDie();
        NetworkManager.Instance.Send(diePacket);
    }

    /// <summary>
    /// 이동할 맵의 데이터를 서버에 송신한다.
    /// </summary>
    private void SendChangeMapPacket()
    {
        StartCoroutine(FadeOutAndSendChangeMapPacket());
    }

    private IEnumerator FadeOutAndSendChangeMapPacket()
    {
        yield return StartCoroutine(FadeInOutManager.Instance.FadeOut());

        StatWindowManager.Instance.SetWindowActive(false);  // 스탯창 팝업 닫기

        C_ChangeMap changeMapPacket = new C_ChangeMap();
        changeMapPacket.MapId = (int)nextMapName;
        NetworkManager.Instance.Send(changeMapPacket);
    }
    #endregion

    #region 패킷 수신 후 기능

    #endregion

    #region 동작 관련 메서드
    /// <summary>
    /// 플레이어가 지형에 발을 붙이고 있는지 판별하는 메서드
    /// </summary>
    public bool IsGrounded()
    {
        Vector2 boxCenter = new Vector2(playerTerrainCollider.bounds.center.x, playerTerrainCollider.bounds.min.y - 0.05f);
        Vector2 boxSize = new Vector2(playerTerrainCollider.bounds.size.x, 0.1f);

        bool isOnGround = Physics2D.OverlapBox(boxCenter, boxSize, 0f, LayerMask.GetMask("Terrain"));

        if (isPressedJump == true)
        {
            if (rb.linearVelocityY <= 0)
            {
                isPressedJump = false;
            }
            return false;
        }
        if (isOnGround)
        {
            isKnockback = false;
        }
        return isOnGround;
    }

    /// <summary>
    /// 내 플레이어가 바라보는 방향을 전환하는 메서드
    /// </summary>
    private void SetMyPlayerDirection(float moveDirection)
    {
        Vector3 scale = transform.localScale;

        if (moveDirection * scale.x > 0)
        {
            scale.x *= -1;
            transform.localScale = scale;
            isRight = !isRight;
        }

        // 이름표는 뒤집히지 않게
        playerCanvas.FlipCanvas(isRight);
    }

    /// <summary>
    /// 플레이어를 마을로 보내는 메서드
    /// </summary>
    public void SendPlayerToVillage()
    {
        nextMapName = MapName.Village;
        SendChangeMapPacket();
    }
    #endregion

    #region 충돌 처리 관련 메서드
    /// <summary>
    /// 몬스터가 플레이어를 공격했을 때의 데미지를 계산하는 메서드
    /// </summary>
    /// <param name="target">공격당한 플레이어</param>
    /// <param name="power">몬스터 공격력</param>
    /// <param name="totalDamage">몬스터가 가한 총 데미지</param>
    private List<int> CalculateMonsterToPlayerDamage(MonsterController monster, out int totalDamage)
    {
        List<int> damageList = new List<int>();
        totalDamage = 0;

        // 몬스터 공격력 가져오기
        int monsterPower = monster.info.StatInfo.AttackPower;

        // 플레이어 방어력 가져오기
        int defense = PlayerInformation.playerStatInfo.Defense;

        int skillHitCount = 1;  // 우선은 1타만 맞도록 구현
        for (int i = 0; i < skillHitCount; i++)
        {
            // 데미지 무작위 편차 부여
            float randomOffset = Random.Range(-0.25f, 0.25f);

            // 공식에 따라 데미지 계산
            int finalDamage = (int)((monsterPower - defense) * (1 + randomOffset));
            finalDamage = Mathf.Clamp(finalDamage, 0, finalDamage);

            damageList.Add(finalDamage);
            totalDamage += finalDamage;
        }

        return damageList;
    }

    /// <summary>
    /// 몬스터가 플레이어를 스킬로 공격했을 때의 데미지를 계산하는 메서드
    /// </summary>
    /// <param name="target">공격당한 플레이어</param>
    /// <param name="power">몬스터 공격력</param>
    /// <param name="totalDamage">몬스터가 가한 총 데미지</param>
    private List<int> CalculateMonsterSkillToPlayerDamage(MonsterSkill monsterSkill, out int totalDamage)
    {
        List<int> damageList = new List<int>();
        totalDamage = 0;

        // 몬스터 공격력 가져오기
        int monsterPower = monsterSkill.GetDamage();

        // 플레이어 방어력 가져오기
        int defense = PlayerInformation.playerStatInfo.Defense;

        int skillHitCount = 1;  // 우선은 1타만 맞도록 구현
        for (int i = 0; i < skillHitCount; i++)
        {
            // 데미지 무작위 편차 부여
            float randomOffset = Random.Range(-0.25f, 0.25f);

            // 공식에 따라 데미지 계산
            int finalDamage = (int)((monsterPower - defense) * (1 + randomOffset));
            finalDamage = Mathf.Clamp(finalDamage, 0, finalDamage);

            damageList.Add(finalDamage);
            totalDamage += finalDamage;
        }

        return damageList;
    }

    /// <summary>
    /// 플레이어에게 넉백효과를 준다. 현재는 몬스터 반대방향으로 튀어나간다.
    /// </summary>
    /// <param name="target"></param>
    private void KnockBack(Transform target)
    {
        Vector3 targetPosition = target.GetComponent<BoxCollider2D>().bounds.center;

        float backVector = Mathf.Sign(transform.GetComponent<BoxCollider2D>().bounds.center.x - targetPosition.x);
        rb.linearVelocityY = 4f;
        rb.linearVelocityX = backVector * 2f;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            if (invincible)
            {
                return;
            }
            int totalDamage = 0;
            List<int> damageList;

            MonsterController monster = collision.GetComponent<MonsterController>();
            damageList = CalculateMonsterToPlayerDamage(
                monster,
                out totalDamage
                );

            // 플레이어가 공격받았을 때 데미지를 화면에 띄운다. -> 패킷 수신 시점으로 이동

            playerInformation.SetPlayerHp(-totalDamage);
            SendPlayerDamaged(totalDamage);

            if (isDead)
            {
                OnDead();
                playerDamagedCollider.enabled = false;
                return;
            }
            if (totalDamage > 0)
            {
                isKnockback = true;
                OnHit();
                invincible = true;
                KnockBack(collision.transform);
            }
        }

        if (collision.CompareTag("MonsterSkill"))
        {
            if (invincible)
            {
                return;
            }
            int totalDamage = 0;
            List<int> damageList;

            MonsterSkill monsterSkill = collision.GetComponent<MonsterSkill>();
            damageList = CalculateMonsterSkillToPlayerDamage(
                monsterSkill,
                out totalDamage
                );

            // 플레이어가 공격받았을 때 데미지를 화면에 띄운다. -> 패킷 수신 시점으로 이동

            playerInformation.SetPlayerHp(-totalDamage);
            SendPlayerDamaged(totalDamage);

            if (isDead)
            {
                OnDead();
                playerDamagedCollider.enabled = false;
                return;
            }
            if (totalDamage > 0)
            {
                isKnockback = true;
                OnHit();
                invincible = true;
                KnockBack(collision.transform);
            }
        }

        if (collision.CompareTag("Portal"))
        {
            isMyOnPortal = true;
            nextMapName = collision.gameObject.GetComponent<Portal>().nextMapName;
        }

        if (collision.CompareTag("BossRoomEnterPortal"))
        {
            isMyOnBossPortal = true;
            bossRoomPortal = collision.gameObject;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Portal"))
        {
            isMyOnPortal = false;
        }

        if (collision.CompareTag("BossRoomEnterPortal"))
        {
            isMyOnBossPortal = false;
        }
    }

    public void SetInvincible()
    {
        invincible = true;
    }

    /// <summary>
    /// 플레이어에게 1.5초간 무적시간을 부여한다.
    /// </summary>
    IEnumerator InvincibleTime()
    {
        float duration = 1.5f;
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        invincible = false;
        yield break;
    }
    #endregion
}
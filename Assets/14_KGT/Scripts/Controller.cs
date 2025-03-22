using UnityEngine;

public class Controller : MonoBehaviour
{
    public PlayerStateMachine sm;
    private Rigidbody2D rb;
    public Animator animator;

    public BaseClass playerClass;

    private Vector2 movement;
    [Header("공격")]
    public bool isAttacking = false;
    [Header("착지")]
    public bool isGround = false;
    [Header("스피드")]
    [SerializeField] private float playerSpeed = 2f;
    [Header("점프")]
    [SerializeField] private float jumpForce = 3f;
    [Header("타격")]
    public bool isDamaged = false;
    [Header("캐릭터 사망")]
    public bool isDead = false;

    private void Awake()
    {
        //sm = new PlayerStateMachine(this);
        rb = GetComponent<Rigidbody2D>();
        animator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void Start()
    {
        sm.Initialize(sm.idleState);
        sm.Initialize(sm.jumpState);
        sm.Initialize(sm.attackState);
        sm.Initialize(sm.moveState);
        sm.Initialize(sm.hitState);
        sm.Initialize(sm.deadState);
    }

    private void Update()
    {
        sm.Execute();

        if (!isDead)    // 사망상태가 아닐 때
        {
            float horizontal;
            if (!isGround)    // 공중이면
            {
                horizontal = rb.linearVelocityX;
                movement.x = horizontal;    // 운동에너지 유지
            }
            else              // 땅에 붙어있으면
            {
                horizontal = Input.GetAxisRaw("Horizontal");
                movement.x = horizontal * playerSpeed;
            }

            movement.y = rb.linearVelocityY;    // y축 움직임 값

            if (isAttacking && isGround) { movement.x = 0; }    // 땅에서 공격했을 때 움직임 X

            rb.linearVelocity = movement;    // 움직임 적용
            if (Mathf.Abs(movement.x) > 0 && !isAttacking && isGround)    // 좌우로 움직임이 있고, 땅에서 비공격 상태일 때
            {
                OnMove();
                UpdatePlayerScale(movement);
            }
            else if (!isGround && !isAttacking)    // 공중에 있을 때
            {
                OnJump();
                UpdatePlayerScale(movement);
            }
            else if (isDamaged)    // 데미지를 받았을 때
            {
                OnHit();
            }
            else
            {
                OnIdle();
            }

            if (Input.GetKeyDown(KeyCode.LeftAlt) && isGround)    // 땅에서 점프키를 눌렀을 때
            {
                rb.AddForce(jumpForce * Vector2.up, ForceMode2D.Impulse);
            }

            if (Input.GetKeyDown(KeyCode.LeftControl))    // 공격키를 눌렀을 때
            {
                OnAttack();
            }
        } else
        {
            OnDead();
        }
    }

    #region 스프라이트 방향전환
    /// <summary>
    /// 움직이는 방향에 따라 캐릭터의 스프라이트의 바라보는 방향을 변경하는 함수
    /// </summary>
    /// <param name="moveDirection"></param>
    private void UpdatePlayerScale(Vector2 moveDirection)
    {
        Vector3 scale = transform.localScale;

        if ((moveDirection.x < 0f && scale.x < 0f) || (moveDirection.x > 0f && scale.x > 0f))
        {
            scale.x *= -1f;
            transform.localScale = scale;
        }
    }
#endregion

    #region 상태변환함수
    public void OnIdle()
    {
        if (sm.CurrentState != sm.idleState && !isAttacking && isGround)
        {
            sm.TransitionTo(sm.idleState);
        }
    }

    public void OnHit()
    {
        if (sm.CurrentState != sm.hitState)
        {
            sm.TransitionTo(sm.hitState);
        }
    }

    public void OnMove()
    {
        if (sm.CurrentState != sm.moveState)
        {
            sm.TransitionTo(sm.moveState);
        }
    }

    public void OnAttack()
    {
        if (sm.CurrentState != sm.attackState)
        {
            isAttacking = true;
            if (isGround)
            {
                rb.linearVelocity = Vector2.zero;
            }
            /// Todo - 해당 직업의 스킬을 사용한다.
            /// 직업의 스킬은 BaseClass의 abstract로 선언된 UseSkill을 각 직업에서 상속하여 override한 UseSkill이다.
            
            sm.TransitionTo(sm.attackState);
            //playerClass.ActiveHitbox();
        }
    }

    public void OnJump()
    {
        if (sm.CurrentState != sm.jumpState)
        {
            sm.TransitionTo(sm.jumpState);
        }
    }

    public void OnDead()
    {
        if (sm.CurrentState != sm.deadState)
        {
            sm.TransitionTo(sm.deadState);
        }
    }
#endregion

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = true;
        }
        // 이 곳에 캐릭터와 충돌이 있는 오브젝트에 대한 작업을 나열한다.
        //if (collision.gameObject.CompareTag("Monster"))
        //{
        //    isDamaged = true;
        //}
    }

    public void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGround = false;
        }
    }
}

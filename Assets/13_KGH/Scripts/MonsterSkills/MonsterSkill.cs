using Google.Protobuf.Protocol;
using UnityEngine;

public class MonsterSkill : MonoBehaviour
{
    protected SpriteRenderer spriteRenderer;
    protected Collider2D collider2D;
    
    // 플레이어가 몬스터 스킬과 충돌을 입었을 시, 해당 값을 바탕으로 체력 감소로직에 적용시켜야 함.
    private int damage;
    
    protected virtual void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider2D = GetComponent<Collider2D>();
    }

    // 애니메이션에서 이벤트로 호출, 애니메이션 종료 시 오브젝트가 Destroy 되도록
    public void DestroyEvent()
    {
        Destroy(gameObject);
    }

    public void SetDamage(int damage)
    {
        this.damage = damage;
    }

    public int GetDamage()
    {
        return damage;
    }
}

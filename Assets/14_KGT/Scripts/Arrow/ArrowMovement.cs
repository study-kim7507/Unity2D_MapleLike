using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class ArrowMovement : MonoBehaviour
{
    private Vector3 direction;
    private Vector3 destination;
    private float speed;
    private float duration;
    private GameObject hitObject;
    private MonsterController monster;
    private float currentDuration;

    private Action onHit;
    private int hitDamage;
    private Vector3 toTarget;

    public int num;

    public void Initialize(Vector3 dir, Vector3 end, float moveSpeed, float dur, GameObject hitObj, MonsterController mon, Action createDamageSkin)
    {
        currentDuration = 0f;
        direction = dir;
        destination = end;
        speed = moveSpeed;
        duration = dur;
        hitObject = hitObj;
        onHit += createDamageSkin;
        

        if (mon != null)
        {
            monster = mon;
            duration = 10f;
        }
        else
        {
            onHit -= createDamageSkin;
        }

        if (num != 0)
        {
            onHit -= createDamageSkin;
        }
    }

    void Update()
    {
        transform.position += direction * speed * Time.deltaTime;
        currentDuration += Time.deltaTime;
        if (currentDuration > duration)
        {
            Destroy(gameObject);
        }
        toTarget = destination - transform.position;
        if (Vector3.Dot(direction, toTarget) <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster") && collision.GetComponent<MonsterController>() == monster)
        {
            if (PlayerInformation.playerInfo.PlayerId == FindAnyObjectByType<YHSMyPlayerController>().Id)
            {
                Vector3 targetPosition = collision.GetComponent<BoxCollider2D>().bounds.center;

                GameObject hitGo = Instantiate(hitObject, targetPosition, Quaternion.identity);

                hitGo.GetComponent<SpriteRenderer>().flipX =
                    (transform.GetComponent<BoxCollider2D>().bounds.center.x > targetPosition.x);
                // Archer로부터 함수를 받아 데미지 처리 패킷을 보내고, 데미지스킨이 출력되도록 한다.
                hitGo.GetComponent<Animator>().SetTrigger("Hit");
                Debug.Log("히트 이펙트 생성");
                onHit?.Invoke();
                Destroy(hitGo, 0.45f);
            }

            Destroy(gameObject);
        }
    }
}

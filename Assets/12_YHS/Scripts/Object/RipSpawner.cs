using System.Collections;
using UnityEngine;

public class RipSpawner : MonoBehaviour
{
    Animator ripAnimator;

    void Start()
    {
        ripAnimator = GetComponent<Animator>();
        ripAnimator.SetTrigger("Falling");

        StartCoroutine(FallRIP());
    }

    /// <summary>
    /// 비석이 떨어지고, 땅에 착지할 때까지의 작업
    /// </summary>
    IEnumerator FallRIP()
    {
        // 비석이 땅에 떨어질 때까지 기다린다.
        // 비석 오브젝트에 Rigidbody2D가 있으며, 본체 캐릭터 오브젝트와 레이어 제외를 각각해주었음.
        while (transform.localPosition.y > 0f)
        {
            yield return null;
        }

        // 비석이 땅에 떨어진 후 착지 애니메이션 및 Idle로 이어지게 한다.
        ripAnimator.SetTrigger("Stand");

        // 좌표 동기화
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Static;
        transform.localPosition = Vector3.zero;

        yield break;
    }
}

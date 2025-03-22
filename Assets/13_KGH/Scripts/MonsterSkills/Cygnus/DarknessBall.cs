using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DarknessBall : MonsterSkill
{
    [HideInInspector] public GameObject target;

    private Vector3 initalProjectilePos = new Vector3();
    private Vector3 initialTargetPos = new Vector3();
    private Vector3 direction;

    protected override void Start()
    { 
        base.Start();

        initalProjectilePos = transform.position;
        initialTargetPos = target.transform.position;
        direction = (initialTargetPos - initalProjectilePos).normalized;
        
        spriteRenderer.flipX = transform.position.x > initialTargetPos.x ? false : true;
        
        StartCoroutine(DestroyCoroutine());
    }

    private void FixedUpdate()
    {
        transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, Time.fixedDeltaTime * 1.5f);
    }

    IEnumerator DestroyCoroutine()
    {
        // 5초 대기
        yield return new WaitForSeconds(5f);

        // 1에서 0으로 alpha 값을 서서히 변화
        float duration = 2.5f; // 7.5초까지의 시간
        float startAlpha = 1f;
        float endAlpha = 0f;
        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            Color color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;

            yield return null; // 다음 프레임까지 대기
        }

        Color finalColor = spriteRenderer.color;
        finalColor.a = endAlpha;
        spriteRenderer.color = finalColor;

        Destroy(gameObject); 
    }
}

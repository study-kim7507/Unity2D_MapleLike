using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageSpawner : MonoBehaviour
{
    [SerializeField] List<Sprite> playerDamageSkin;
    [SerializeField] List<Sprite> monsterDamageSkin;
    [SerializeField] List<Sprite> autoHealDamageSkin;
    [SerializeField] GameObject damageNumberPrefab;     // 데미지스킨 스프라이트를 할당할 흰색 Square
    private List<Sprite> currentDamageSkin;             // 공격/피격에 따라 데미지스킨 선택

    //public List<int> damageList = new List<int>{ 12, 345, 6789, 20250228 }; // temp

    private float horizontalOffset = 0.4f;  // 좌우 방향 숫자 위치(간격)

    /// <summary>
    /// DamageSpawner 생성 후 이 메서드를 실행하면 출력하게 된다.
    /// </summary>
    /// <param name="damageList"></param>
    public void InitAndSpawnDamage(List<int> damageList, int damageSkinIndex)
    {
        switch (damageSkinIndex)
        {
            case 0:
                currentDamageSkin = playerDamageSkin;
                break;
            case 1:
                currentDamageSkin = monsterDamageSkin;
                break;
            case 2:
                currentDamageSkin = autoHealDamageSkin;
                break;
            default:
                currentDamageSkin = playerDamageSkin;
                break;
        }

        for (int i = 0; i < damageList.Count; i++)
        {
            float oneLinePositionY = i * 0.5f;      // 데미지가 여러 줄일 때 각 줄의 상하 간격
            float oneLinePositionZ = -i * 0.1f;     // 데미지가 여러 줄일 때 각 줄이 겹치지 않게 하도록 하는 깊이
            SpawnDamage(damageList[i], oneLinePositionY, oneLinePositionZ);     // 인수로 받는 리스트를 사용할 경우 this 지우기
            // 띄우는 시간 간격을 코드로?
        }

        // 1초 후 사라지도록
        StartCoroutine(DestoryDamage(1f));
    }

    private void SpawnDamage(int damageAmount, float oneLinePositionY, float oneLinePositionZ)
    {
        GameObject oneLineDamage = new GameObject("OneLineDamage");     // 데미지 한 줄을 위한 빈 오브젝트
        oneLineDamage.transform.SetParent(gameObject.transform);

        int numCount = 0;       // 한 줄에 띄울 숫자 개수

        if (damageAmount == 0)
        {
            // 데미지 0이면 MISS 출력
            SpawnMiss(oneLineDamage);
        }
        else
        {
            int oneLineNumber = damageAmount;
            SpawnNumber(oneLineDamage, oneLineNumber, out numCount);
        }

        // 한 줄의 데미지를 알맞은 자리로 이동
        oneLineDamage.transform.localPosition = new Vector3(
            -((float)(numCount - 1) / 2 + 0.5f) * horizontalOffset,     // 가운데로 정렬
            oneLinePositionY,
            oneLinePositionZ
            );
    }

    private int ReverseNumber(int oneLineNum, out int numCount)
    {
        string str = Mathf.Abs(oneLineNum).ToString();
        numCount = str.Length;

        string reverseStr = "";

        for (int i = str.Length - 1; i >= 0; i--)
            reverseStr += str[i];

        if (reverseStr.Length == 0)
            return 0;

        return Int32.Parse(reverseStr);
    }

    private void SpawnNumber(GameObject oneLineDamage, int oneLineNumber, out int numCount)
    {
        numCount = 0;       // 한 줄에 띄울 숫자 개수
        oneLineNumber = ReverseNumber(oneLineNumber, out numCount);     // 출력 순서 효과를 위해 수 뒤집기

        int depth = 0;      // position z에 곱해질 값으로, 숫자가 좌우로 겹칠 때 보여질 순서

        // 한 줄의 데미지 출력
        for (int i = 0; i < numCount; i++)
        {
            int number = oneLineNumber % 10;    // 높은 자리수부터 출력
            oneLineNumber /= 10;

            float positionY = UnityEngine.Random.Range(-0.07f, 0.07f);  // 한 줄 내에서 상하 방향 숫자 위치
            float positionZ = depth * 0.01f;                            // 한 줄 내에서 각 숫자들이 겹치지 않게 하도록 하는 깊이

            GameObject numberObject = Instantiate(damageNumberPrefab, oneLineDamage.transform);
            numberObject.transform.localPosition = new Vector3(
                i * horizontalOffset,
                positionY,
                positionZ
                );
            numberObject.GetComponent<SpriteRenderer>().sprite = currentDamageSkin[number];

            depth--;
        }
    }

    private void SpawnMiss(GameObject oneLineDamage)
    {
        GameObject numberObject = Instantiate(damageNumberPrefab, oneLineDamage.transform);
        numberObject.GetComponent<SpriteRenderer>().sprite = currentDamageSkin[10];
    }

    IEnumerator DestoryDamage(float duration)
    {
        yield return new WaitForSeconds(duration);

        Destroy(transform.parent.gameObject);

        yield break;
    }
}

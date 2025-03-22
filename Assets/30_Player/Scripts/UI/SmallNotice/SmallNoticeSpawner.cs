using System.Collections;
using TMPro;
using UnityEngine;

public class SmallNoticeSpawner : MonoBehaviour
{
    public void InitAndSpawnSmallNotice(string notice)
    {
        GetComponent<TMP_Text>().text = notice;

        // 2초 후 사라지도록
        StartCoroutine(DestorySmallNotice(2f));
    }

    IEnumerator DestorySmallNotice(float duration)
    {
        yield return new WaitForSeconds(duration);

        Destroy(gameObject);

        yield break;
    }
}

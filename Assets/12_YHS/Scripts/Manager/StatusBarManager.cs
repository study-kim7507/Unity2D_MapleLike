using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusBarManager : MonoBehaviour
{
    [SerializeField] List<Sprite> levelNumberSprite;
    [SerializeField] List<Sprite> gaugeNumberSprite;

    [SerializeField] GameObject levelNumber;        // 숫자가 출력될 위치
    [SerializeField] GameObject hpNumber;           // 숫자가 출력될 위치
    [SerializeField] GameObject mpNumber;           // 숫자가 출력될 위치
    [SerializeField] GameObject expNumber;          // 숫자가 출력될 위치
    
    [SerializeField] GameObject rect3x3Prefab;      // 사각형 프리팹
    [SerializeField] GameObject rect4x9Prefab;
    [SerializeField] GameObject rect7x9Prefab;
    [SerializeField] GameObject rect9x9Prefab;
    [SerializeField] GameObject rect5x10Prefab;
    [SerializeField] GameObject rect7x10Prefab;

    [SerializeField] GameObject slashTextPrefab;

    [SerializeField] GameObject hpGauge;
    [SerializeField] GameObject mpGauge;
    [SerializeField] GameObject expGauge;
    [SerializeField] GameObject expEffect;

    private static StatusBarManager instance;
    public static StatusBarManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitStatusBar(PlayerInformation playerInformation)
    {
        playerInformation.UpdateHpAction += UpdateHpGauge;
        playerInformation.UpdateMpAction += UpdateMpGauge;
        playerInformation.UpdateExpAction += UpdateExpGauge;
        playerInformation.UpdateLevelUpAction += UpdateLevelUp;
    }

    #region 수치 계산 관련 메서드
    public void UpdateHpGauge(int hp, int maxHp)
    {
        UpdateGaugeNumber(hp, maxHp, hpNumber.transform, false);

        float scale = 0f;

        if (hp != 0)
        {
            scale = (float)hp / maxHp;
        }
        
        StartCoroutine(ScaleOverTime(hpGauge, scale, 0.5f, false));
    }

    public void UpdateMpGauge(int mp, int maxMp)
    {
        UpdateGaugeNumber(mp, maxMp, mpNumber.transform, false);

        float scale = 0f;

        if (mp != 0)
        {
            scale = (float)mp / maxMp;
        }

        StartCoroutine(ScaleOverTime(mpGauge, scale, 0.5f, false));
    }

    public void UpdateExpGauge(int exp, int totalExp)
    {
        UpdateGaugeNumber(exp, totalExp, expNumber.transform, true);

        float scale = 0f;

        if (exp != 0)
        {
            scale = (float)exp / totalExp;
        }

        StartCoroutine(ScaleOverTime(expGauge, scale, 0.5f, true));

        // TODO: 경험치바 끄트머리 반짝이 효과는 보류
        //Bounds bounds = expGauge.GetComponent<Renderer>().bounds;
        //float rightBound = bounds.max.x;
        //Vector3 effectPosition = expEffect.transform.position;
        //effectPosition.x = rightBound;
        //expEffect.transform.position = effectPosition;
    }

    public void UpdateLevelUp(int level)
    {
        UpdateLevelNumber(level, levelNumber.transform);
    }
    #endregion

    #region 출력 관련 메서드
    /// <summary>
    /// 게이지 증감이 시간에 따라 부드럽게 이루어지도록 하는 메서드
    /// </summary>
    /// <param name="go">게이지</param>
    /// <param name="targetScaleX">목표 스케일</param>
    /// <param name="time">목표 소요시간</param>
    /// <returns></returns>
    IEnumerator ScaleOverTime(GameObject go, float targetScaleX, float time, bool isExp)
    {
        Vector3 startScale = go.transform.localScale;
        Vector3 targetScale = startScale;
        targetScale.x = targetScaleX;

        if (isExp == true && startScale.x > targetScale.x)
        {
            // 최대 경험치 초과 시 스케일 0부터 다시 시작
            startScale.x = 0f;
        }

        float elapsedTime = 0f;

        while (elapsedTime < time)
        {
            go.transform.localScale = Vector3.Lerp(startScale, targetScale, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        go.transform.localScale = targetScale;
    }

    /// <summary>
    /// UI에 레벨을 출력하는 메서드
    /// </summary>
    /// <param name="currentLevel"></param>
    /// <param name="parentTransform"></param>
    public void UpdateLevelNumber(int currentLevel, Transform parentTransform)
    {
        // 기존 데이터 삭제
        int childCount = parentTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.Destroy(parentTransform.GetChild(i).gameObject);
        }

        string level = currentLevel.ToString();
        foreach (char number in level)
        {
            GameObject go;
            if (number == '1')
            {
                go = Instantiate(rect5x10Prefab, parentTransform);
            }
            else
            {
                go = Instantiate(rect7x10Prefab, parentTransform);
            }
            go.GetComponent<Image>().sprite = levelNumberSprite[number - '0'];
        }
    }

    /// <summary>
    /// 게이지를 덮는 숫자를 출력하는 메서드
    /// </summary>
    /// <param name="currentAmount">현재 수치</param>
    /// <param name="maxAmount">최대 수치</param>
    /// <param name="parentTransform">출력 위치</param>
    public void UpdateGaugeNumber(int currentAmount, int maxAmount, Transform parentTransform, bool isExp)
    {
        // 기존 데이터 삭제
        int childCount = parentTransform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            GameObject.Destroy(parentTransform.GetChild(i).gameObject);
        }

        // 게이지 현재 용량 출력
        string amount = currentAmount.ToString();
        foreach (char number in amount)
        {
            GameObject go;
            if (number == '1')
            {
                go = Instantiate(rect4x9Prefab, parentTransform);
            }
            else
            {
                go = Instantiate(rect7x9Prefab, parentTransform);
            }
            go.GetComponent<Image>().sprite = gaugeNumberSprite[number - '0'];
        }

        // 경험치 게이지를 위한 분기
        if (isExp == true)
        {
            UpdateExpRate(currentAmount, maxAmount, parentTransform);
            return;
        }

        // 슬래시(/) 출력
        Instantiate(slashTextPrefab, parentTransform);

        // 게이지 최대 용량 출력
        amount = maxAmount.ToString();
        foreach (char number in amount)
        {
            GameObject go;
            if (number == '1')
            {
                go = Instantiate(rect4x9Prefab, parentTransform);
            }
            else
            {
                go = Instantiate(rect7x9Prefab, parentTransform);
            }
            go.GetComponent<Image>().sprite = gaugeNumberSprite[number - '0'];
        }
    }

    /// <summary>
    /// 현재 경험치 / 전체 경험치 비율을 출력하는 메서드
    /// </summary>
    /// <param name="currentExp">현재 경험치</param>
    /// <param name="maxExp">전체 경험치</param>
    /// <param name="parentTransform">출력 위치</param>
    public void UpdateExpRate(int currentExp, int maxExp, Transform parentTransform)
    {
        GameObject go;

        // "[" 출력
        go = Instantiate(rect7x9Prefab, parentTransform);
        go.GetComponent<Image>().sprite = gaugeNumberSprite[10];

        string rate = ((float)currentExp / maxExp * 100).ToString("F2");

        foreach (char number in rate)
        {
            // 소수점 "." 출력
            if (number == '.')
            {
                go = Instantiate(rect3x3Prefab, parentTransform);
                go.GetComponent<Image>().sprite = gaugeNumberSprite[11];
                continue;
            }

            if (number == '1')
            {
                go = Instantiate(rect4x9Prefab, parentTransform);
            }
            else
            {
                go = Instantiate(rect7x9Prefab, parentTransform);
            }
            go.GetComponent<Image>().sprite = gaugeNumberSprite[number - '0'];
        }

        // "%" 출력
        go = Instantiate(rect9x9Prefab, parentTransform);
        go.GetComponent<Image>().sprite = gaugeNumberSprite[12];

        // "[" 출력
        go = Instantiate(rect4x9Prefab, parentTransform);
        go.GetComponent<Image>().sprite = gaugeNumberSprite[13];
    }
#endregion
}

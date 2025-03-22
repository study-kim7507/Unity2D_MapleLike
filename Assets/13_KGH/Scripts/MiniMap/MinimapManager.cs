using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance { get; private set; }

    [SerializeField] private GameObject canvas;
    [SerializeField] private List<GameObject> miniMaps;
    [SerializeField] private GameObject playerIconPrefab;

    [SerializeField] private GameObject miniMapImage;

    Transform player = null;
    GameObject playerIcon = null;

    int sceneIndex = -1;

    private List<Tuple<Vector2, Vector2>> mapRange = new List<Tuple<Vector2, Vector2>>
    {
        // 달팽이동산 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-16.17f, 17.596f), // X 범위 (Min, Max)
            new Vector2(-7.252f, 6.501f)   // Y 범위 (Min, Max)
        ),

        // 암허스트 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-14.6f, 62.089f), // X 범위 (Min, Max)
            new Vector2(-4.544f, 13.86f)   // Y 범위 (Min, Max)
        ),

        // 헤네시스 사냥터 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-20.312f, 18.846f), // X 범위 (Min, Max)
            new Vector2(-6.967f, 15.301f)   // Y 범위 (Min, Max)
        ),

        // 남의 집 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-12.416f, 12.408f), // X 범위 (Min, Max)
            new Vector2(-6.624f, 9.872f)   // Y 범위 (Min, Max)
        ),

        // 비밀정원 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-19.175f, 16.937f), // X 범위 (Min, Max)
            new Vector2(-5.288f, 9.727f)   // Y 범위 (Min, Max)
        ),

        // 시그너스의 정원 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-23.919f, 15.908f), // X 범위 (Min, Max)
            new Vector2(-5.514f, 11.889f)   // Y 범위 (Min, Max)
        ),

        // 시그너스의 전당 맵의 범위 (X 범위, Y 범위)
        new Tuple<Vector2, Vector2>(
            new Vector2(-22.54f, 19.88f), // X 범위 (Min, Max)
            new Vector2(-5.295f, 10.76f)   // Y 범위 (Min, Max)
        ),
    };

    // 미니맵 화면의 X, Y 범위
    private float miniMapMaxX = 375.5f;
    private float miniMapMinX = -375.4f;
    private float miniMapMaxY = 164.6f;
    private float miniMapMinY = -211.1f;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, LoadSceneMode mode)
    {
        UpdateMinimap(scene.name);
    }

    private void Update()
    {
        if (ObjectManager.Instance.MyPlayer != null && playerIcon != null)
        {
            player = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).transform;

            // 플레이어 씬 좌표 (월드 좌표)
            Vector2 scenePos = new Vector2(player.position.x, player.position.y);

            Vector2 minimapPos = WorldToMinimap(scenePos, sceneIndex);

            // 플레이어 아이콘 위치 업데이트
            playerIcon.GetComponent<RectTransform>().anchoredPosition = minimapPos;
        }
    }

    private void UpdateMinimap(string sceneName)
    {
        // 모든 미니맵 비활성화
        foreach (var miniMap in miniMaps)
        {
            miniMap.SetActive(false);
        }

        if (playerIcon != null)
        {
            Destroy(playerIcon);
            playerIcon = null;
        }

        sceneIndex = -1;
        switch (sceneName)
        {
            case "Tutorial": sceneIndex = 0; break;
            case "Village": sceneIndex = 1; break;
            case "Field1": sceneIndex = 2; break;
            case "Field2": sceneIndex = 3; break;
            case "Field3": sceneIndex = 4; break;
            case "BossWaitRoom": sceneIndex = 5; break;
            case "BossRoom": sceneIndex = 6; break;
        }

        if (sceneIndex != -1)
        {
            miniMaps[sceneIndex].SetActive(true);

            // 플레이어 아이콘 생성
            playerIcon = Instantiate(playerIconPrefab, miniMaps[sceneIndex].GetComponent<RectTransform>());
        }
    }

    // 월드 좌표를 미니맵 좌표로 변환
    private Vector2 WorldToMinimap(Vector2 worldPos, int sceneIndex)
    {
        // 현재 맵의 범위 (현재 맵에 맞는 범위 가져오기)
        var mapRangeX = mapRange[sceneIndex].Item1; // 예를 들어, 첫 번째 맵의 X 범위
        var mapRangeY = mapRange[sceneIndex].Item2; // 예를 들어, 첫 번째 맵의 Y 범위

        // 월드 좌표를 정규화 (0~1로 비율로 변환)
        float normalizedX = (worldPos.x - mapRangeX.x) / (mapRangeX.y - mapRangeX.x);
        float normalizedY = (worldPos.y - mapRangeY.x) / (mapRangeY.y - mapRangeY.x);

        // 정규화된 값을 미니맵의 화면 좌표로 변환
        float minimapX = miniMapMinX + normalizedX * (miniMapMaxX - miniMapMinX);
        float minimapY = miniMapMinY + normalizedY * (miniMapMaxY - miniMapMinY);

        return new Vector2(minimapX, minimapY);
    }

    public void OnOffMinimap()
    {
        if (canvas.activeSelf == true)
            canvas.SetActive(false);
        else
            canvas.SetActive(true);
    }
}

using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using Google.Protobuf.Collections;
using TMPro;

public class SpawnManager : MonoBehaviour
{
    private static SpawnManager instance;
    public static SpawnManager Instance { get { return instance; } }

    [SerializeField] Texture2D cursorTexture;
    [SerializeField] GameObject damagePrefab;
    [SerializeField] GameObject smallNoticePrefab;
    
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

        // 커서 텍스처 변경
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.ForceSoftware);
    }

    #region 출력
    /// <summary>
    /// Addressable Group에서 key로 asset을 탐색해 Instantiate하는 메서드
    /// </summary>
    /// <param name="assetKey"></param>
    /// <returns></returns>
    public void SpawnAsset(string assetKey, Transform transform)
    {
        AsyncOperationHandle<GameObject> loadHandler;
        loadHandler = Addressables.LoadAssetAsync<GameObject>(assetKey);

        loadHandler.Completed += handle =>
        {
            if (loadHandler.Status == AsyncOperationStatus.Succeeded)
            {
                //Addressables.InstantiateAsync(loadHandler.Result);
                Instantiate(loadHandler.Result, transform);
            }
            else
            {
                Debug.LogError("Addressable Load Failed: " + assetKey);
            }
        };
    }
    #endregion

    #region 데미지 출력
    /// <summary>
    /// 데미지를 화면에 띄우는 메서드
    /// </summary>
    /// <param name="damageList">연산이 끝난 데미지 리스트</param>
    /// <param name="target">공격 대상의 Transform</param>
    public void SpawnDamage(List<int> damageList, Transform target, int damageSkinIndex)
    {
        // 이거 SpawnManager의 SpawnAsset이랑 겹치는데;
        Vector3 spawnPosition = target.transform.position;

        Collider2D collider = target.GetComponent<Collider2D>();
        float y = 0f;

        if (collider != null)
        {
            y = collider.bounds.max.y + 0.5f;
        }
        else
        {
            y = target.transform.position.y + 2.0f;
        }
        
        spawnPosition.y = y;

        GameObject damage = Instantiate(damagePrefab, spawnPosition, Quaternion.identity);
        damage.GetComponentInChildren<DamageSpawner>().InitAndSpawnDamage(damageList, damageSkinIndex);
    }

    /// <summary>
    /// 데미지를 화면에 띄우는 메서드 overload
    /// </summary>
    /// <param name="damages"></param>
    /// <param name="target"></param>
    /// <param name="isPlayerDamaged"></param>
    public void SpawnDamage(RepeatedField<int> damages, Transform target, int damageSkinIndex)
    {
        List<int> damageList = new List<int>();

        foreach (var damage in damages)
        {
            damageList.Add(damage);
        }

        SpawnDamage(damageList, target, damageSkinIndex);
    }
    #endregion

    #region 화면 우하단 안내 출력
    public void SpawnSmallNotice(string notice, Transform transform)
    {
        GameObject smallNotice = Instantiate(smallNoticePrefab, transform);
        smallNotice.GetComponent<SmallNoticeSpawner>().InitAndSpawnSmallNotice(notice);
    }
    #endregion
}

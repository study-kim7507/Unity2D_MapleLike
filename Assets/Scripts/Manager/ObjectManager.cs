using System;
using System.Collections;
using Google.Protobuf.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceLocations;
using Object = UnityEngine.Object;

public class ObjectManager : MonoBehaviour
{
    private static ObjectManager _instance;
    public static ObjectManager Instance { get { return _instance; } }
    public PlayerController MyPlayer { get; set; }
    // 동기화가 필요한 모든 오브젝트를 딕셔너리로 관리한다고 보면 됩니다.
    public Dictionary<int, GameObject> _objects = new Dictionary<int, GameObject>();

    // Prefabs
    [SerializeField] List<GameObject> classPrefabList;
    [SerializeField] GameObject playerCanvasPrefab;

    private void Awake()
    {
        if (_instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public static GameObjectType GetObjectTypeById(int id)
    {
        int type = (id >> 24) & 0x7F;
        return (GameObjectType)type;
    }


    #region 아이템 생성
    public void SpawnItem(S_ItemSpawn itemSpawn, System.Action<GameObject> callback)
        {
            Addressables.LoadResourceLocationsAsync("ItemObj").Completed += (handle) =>
            {
                var locations = handle.Result;
                if (locations == null || locations.Count == 0)
                {
                    Debug.LogError("ItemObj에 대한 리소스를 찾을 수 없습니다.");
                    callback?.Invoke(null);
                    return;
                }

                // ItemType에 맞는 location 찾기
                IResourceLocation selectedLocation = locations.FirstOrDefault(loc => loc.PrimaryKey.Contains(itemSpawn.ItemType.ToString()));

                Debug.Log(itemSpawn.ItemType.ToString());
                if (selectedLocation == null)
                {
                    Debug.LogError($"❌ 해당 타입({itemSpawn.ItemType})의 아이템을 찾을 수 없습니다.");
                    callback?.Invoke(null);
                    return;
                }

                Debug.Log($"✅ 매칭 성공: {selectedLocation.PrimaryKey}");

                // 아이템 생성
                Addressables.InstantiateAsync(selectedLocation,
                    new Vector3(itemSpawn.ItemInfo.PositionX, itemSpawn.ItemInfo.PositionY, 0),
                    Quaternion.identity).Completed += (instanceHandle) =>
                {
                    if (instanceHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        var spawnedObj = instanceHandle.Result;
                        _objects.Add(itemSpawn.ItemInfo.ItemId, spawnedObj);
                        spawnedObj.transform.GetComponentInChildren<InitItem>().Serverid = itemSpawn.ItemInfo.ItemId;
                        spawnedObj.transform.GetComponentInChildren<InitItem>().CanOnlyOwnerLootTime = itemSpawn.CanOnlyOwnerLootTime;

                        Debug.Log($"🟢 아이템 생성 완료: {spawnedObj.gameObject.name} | 위치: ({itemSpawn.ItemInfo.PositionX}, {itemSpawn.ItemInfo.PositionY})");

                        // 콜백을 통해 생성된 GameObject 전달
                        callback?.Invoke(spawnedObj);
                    }
                    else
                    {
                        Debug.LogError("❌ 아이템 생성 실패!");
                        callback?.Invoke(null);
                    }
                };
            };
        }
    #endregion


    #region Z키 누르면 실행되는 함수 
    public void PickupNearbyItems2()
    {
        var player = FindById(MyPlayer.Id);
        var pickupRange = player.GetComponent<InputManager>().pickupRange;
        var itemLayer = player.GetComponent<InputManager>().itemLayer;

        Collider2D[] items = Physics2D.OverlapCircleAll(player.transform.position, pickupRange, itemLayer);
        Debug.Log("Found " + items.Length + " nearby items");

        if (items.Length == 0) return; // 근처에 아이템이 없으면 종료

        // 가장 가까운 아이템 찾기
        Collider2D nearestItem = null;
        float minDistance = float.MaxValue;

        foreach (Collider2D item in items)
        {
            float distance = Vector2.Distance(player.transform.position, item.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearestItem = item;
            }
        }
        if (nearestItem == null) return; // 근처에 유효한 아이템이 없으면 종료
        
        InitItem itemInfo = nearestItem.GetComponentInChildren<InitItem>();
        if (itemInfo == null) return;
        
        C_LootItem lootItem = new C_LootItem();
        
        lootItem.ItemId = itemInfo.Serverid;
        NetworkManager.Instance.Send(lootItem);
        
        if (itemInfo.Ownerid != MyPlayer.Id)
        {
            Debug.Log("못먹으면되 ");
            return;
        }
        
        //아이템을 먼저 먹을 권리가 있는 캐릭터 아이디가  == 현재 플레이어 아이디와 같으면
        // if (itemInfo.Ownerid == MyPlayer.Id|| itemInfo.Ownerid == -1)
        // {
        if (itemInfo.Property.ItemType != ItemType.Gold)
        {
            // UIManager.Instance.InventoryItems.Add(itemInfo.Serverid,itemInfo);
        }
        // }
        // else if (itemInfo.Ownerid != MyPlayer.Id)
        // {
        //     Debug.Log("못먹으면되 ");
        //     return;
        // }
        
        Debug.Log(FindById(itemInfo.Serverid));
    }
    #endregion


    #region 시간지나면 나오는 DespwanItem
    public void DespwanItem(S_ItemDespawn itemDespawnPkt)
    {
        var Item = FindById(itemDespawnPkt.ItemId);
        SpriteRenderer spriteRenderer = Item.GetComponentInChildren<SpriteRenderer>();
        
        if (spriteRenderer != null)
        {
            StartCoroutine(FadeOutAndDestroy(Item, spriteRenderer, itemDespawnPkt.ItemId));
        }
        else
        {
            // 스프라이트가 없으면 즉시 삭제
            RemoveItem(Item, itemDespawnPkt.ItemId);
        }
    }
    private IEnumerator FadeOutAndDestroy(GameObject item, SpriteRenderer spriteRenderer, int itemId)
    {
        float fadeDuration = 1.0f; // 페이드아웃 지속 시간
        float elapsedTime = 0f;  // 기본값 초기화  이걸로 시간 누적 구할거임 
        Color originalColor = spriteRenderer.color;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(originalColor.a, 0, elapsedTime / fadeDuration); //시간 부드럽게 만들기 페이드아웃을 부드럽게 하기위함
            spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            yield return null;
        }

        // 완전히 투명해지면 삭제
        RemoveItem(item, itemId);
    }
    private void RemoveItem(GameObject item, int itemId)
    {
        Debug.Log($"🗑️ {itemId} 아이템 제거 완료");
        Addressables.ReleaseInstance(item);
        _objects.Remove(itemId);
    }
    #endregion
 
    
    #region 아이템 z키로 먹을 때 발생하는 DespwanItem2
    public void DespwanItem2(S_LootItem itemDespawnPkt)
    {
        
        Debug.Log(FindById(itemDespawnPkt.ItemId)); 
        Addressables.ReleaseInstance(FindById(itemDespawnPkt.ItemId));
        _objects.Remove(itemDespawnPkt.ItemId);
    }
    #endregion

    public void AddPlayer(PlayerInfo info, bool myPlayer = false)
    {
        GameObjectType objectType = GetObjectTypeById(info.PlayerId);
        if (objectType == GameObjectType.Player)
        {
            if (myPlayer)
            {
                // TODO: 코드 정리 필요
                GameObject go = new GameObject();   // 오브젝트 모음
                go.name = info.Name;
                go.layer = LayerMask.NameToLayer("MyPlayer");
                go.transform.position = new Vector3(info.PositionX, info.PositionY, 0f);

                InitPlayer(go, (int)info.StatInfo.ClassType, myPlayer);

                MyPlayer = go.AddComponent<YHSMyPlayerController>();
                MyPlayer.GetComponent<YHSMyPlayerController>().playerInformation.InitPlayerInfo(info);
                MyPlayer.Id = info.PlayerId;

                MyPlayer.SetDestination(info.PositionX, info.PositionY);
                MyPlayer.SetPlayerState(info.CreatureState);
                

                _objects.Add(info.PlayerId, go);
            }
            else
            {
                GameObject go = new GameObject();
                go.name = info.Name;
                go.layer = LayerMask.NameToLayer("Player");

                InitPlayer(go, (int)info.StatInfo.ClassType, myPlayer);

                PlayerController pc = go.AddComponent<PlayerController>();
                pc.Id = info.PlayerId;

                // 다른 플레이어의 위치, 상태 동기화를 위해 필요한 부분
                pc.transform.position = new Vector3(info.PositionX, info.PositionY, 0f);
                pc.SetDestination(info.PositionX, info.PositionY);
                pc.SetPlayerState(info.CreatureState);

                _objects.Add(info.PlayerId, go);
            }
        }
    }

    public void AddMonster(MonsterInfo info)
    {
        GameObjectType objectType = GetObjectTypeById(info.MonsterId);

        if (objectType == GameObjectType.Normalmonster)
        {
            string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");     // "_"와 뒤에 숫자 제거

            GameObject go = Instantiate(MonsterManager.Instance.GetMonsterPrefab(monsterName));
            go.name = info.Name;
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0f);

            go.GetComponent<MonsterController>().UpdateInfo(info);
            go.GetComponent<BaseController>().SetDestination(info.DestinationX, info.DestinationY);

            _objects.Add(info.MonsterId, go);

            NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
            nmc.Id = info.MonsterId;
        }
        else if (objectType == GameObjectType.Bossmonster)
        {
            string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");     // "_"와 뒤에 숫자 제거

            GameObject go = Instantiate(MonsterManager.Instance.GetMonsterPrefab(monsterName));
            go.name = info.Name;
            go.transform.position = new Vector3(info.DestinationX, info.DestinationY, 0f);

            go.GetComponent<MonsterController>().UpdateInfo(info);
            go.GetComponent<BaseController>().SetDestination(info.DestinationX, info.DestinationY);

            _objects.Add(info.MonsterId, go);

            BossMonsterController bmc = go.GetComponent<BossMonsterController>();
            bmc.Id = info.MonsterId;
        }
    }

    public void Remove(int id)
    {
        GameObject go = FindById(id);
        if (go == null)
            return;

        // 몬스터의 경우, 각 스크립트에서 Destroy 처리.
        if (go.TryGetComponent<MonsterController>(out MonsterController mc))
        {
            if (go.TryGetComponent<NormalMonsterController>(out NormalMonsterController nmc))
            {
                nmc.SetState(MonsterState.MDead);
                nmc.Despawn();
            }
            else if (go.TryGetComponent<BossMonsterController>(out BossMonsterController bmc))
            {
                bmc.SetState(MonsterState.MDead);
                bmc.Despawn();
            }
                
            mc.SetCurrentHp(0);

            // 퀘스트 업데이트
            if (DeathManager.Instance.player.Id == mc.lastHitPlayerId && QuestManager.Instance.currentQuest != null)
                QuestManager.Instance.currentQuest.UpdateQuest(mc.name);
        }
        else
            Object.Destroy(go); // Unity 메인 스레드에서 오브젝트 삭제하고
        _objects.Remove(id); // 딕셔너리에서 제거한다.
    }

    // 딕셔너리에서 대상 오브젝트를 가져온다.
    public GameObject FindById(int id)
    {
        GameObject go = null;
        _objects.TryGetValue(id, out go);
        return go;
    }

    public void Clear()
    {
        foreach (GameObject obj in _objects.Values)
            Object.Destroy(obj); // Unity 메인 스레드에서 오브젝트 삭제하고

        _objects.Clear(); // 딕셔너리 클리어
        MyPlayer = null;
    }

    private void InitPlayer(GameObject go, int classIndex, bool myPlayer)
    {
        // 체력바나 이름표 등도 넣을 수 있지 않을까?

        {
            // 직업에 따른 프리팹 인스턴싱
            GameObject character = Instantiate(classPrefabList[classIndex], go.transform);
            character.name = "Character";
        }

        if (myPlayer == true)
        {
            // 자신을 식별할 수 있는 이름표 인스턴싱
            // Player_OOOOOOO 하위 PlayerCanvas로 생성됩니다.
            GameObject playerCanvas = Instantiate(playerCanvasPrefab, go.transform);
            playerCanvas.name = "PlayerCanvas";
        }
    }
}
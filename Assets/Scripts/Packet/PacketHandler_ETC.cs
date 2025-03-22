using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using NUnit.Framework;
using UnityEngine;


// 기타 패킷 핸들러
public partial class PacketHandler
{
    public static event Action OnActivateStartScenePanel;
    public static void S_ConnectedHandler(PacketSession session, IMessage packet)
    {
        OnActivateStartScenePanel?.Invoke();
    }

    //드랍 
    public static void S_ItemSpawnHandler(PacketSession session, IMessage packet)
    {
        //실제 로직 기능 구현 
        S_ItemSpawn itemSpawnPkt = packet as S_ItemSpawn;
        ObjectManager.Instance.SpawnItem(itemSpawnPkt, (spawnedObj) => {
            if (spawnedObj != null)
            {
                if (itemSpawnPkt == null)
                {
                    Debug.Log($"PlayerId: {itemSpawnPkt.ItemInfo.OwnerId} 없음");
                    return;
                }
                spawnedObj.GetComponentInChildren<InitItem>().Ownerid = itemSpawnPkt.ItemInfo.OwnerId;
            }
            else
            {
                Debug.LogError("❌ 아이템 생성 실패!");
            }
        });
    }

    //사라짐 
    public static void S_ItemDespawnHandler(PacketSession session, IMessage packet)
    {
        // 몇초 뒤 사라지는 기능  + 누가 먹으면  사라지게 보여야해 
        S_ItemDespawn itemDespawnPkt = packet as S_ItemDespawn;

        var itemDespawnobj =ObjectManager.Instance.FindById(itemDespawnPkt.ItemId);
        if (itemDespawnobj == null)
        {
            return;
        }
        
        ObjectManager.Instance.DespwanItem(itemDespawnPkt);
        
        // 어떻게 아이템먹을때 호출 시킬까?
    }
    
    //먹기
    public static void S_LootItemHandler(PacketSession session, IMessage packet)
    {
        // 실제 로직 기능 구현
        S_LootItem lootItemPkt = packet as S_LootItem;
        if (lootItemPkt == null)
        {
            Debug.Log("아무것도 안들어옴");
            return;
        }

        var item = ObjectManager.Instance.FindById(lootItemPkt.ItemId);
        if (item == null)
        {
            Debug.Log("들어옵니다!!!");
            return;
        }

        var itemPickup = item.GetComponent<ItemPickup>();
        itemPickup.StartAttracting2(lootItemPkt);
        
        //
        // Debug.Log($"먹을 아이템 : {lootItemPkt.ItemId}");
        // Debug.Log($"아이템을 먹을 권리가 있는 캐릭 : {initItem.Ownerid}");
        // Debug.Log($"z키 눌러서 먹을려고 시도하는  캐릭 : {lootItemPkt.PlayerId}");
        // Debug.Log($"클라이언트의 마이플레이어  캐릭 : {ObjectManager.Instance.MyPlayer.Id}");
        
        //z키 눌러서 먹을려고 시도하는  캐릭 과  아이템을 먹을 권리가 있는 캐릭이 같을 경우 실행  또는 먹을 수있는 조건이 전부인 경우 
        // if (lootItemPkt.PlayerId == initItem.Ownerid || initItem.Ownerid == -1 )
        // {
      
      
        // }
        // //z키 눌러서 먹을려고 시도하는  캐릭 과  아이템을 먹을 권리가 있는 캐릭이 다를 경우 실행 
        // else if (lootItemPkt.PlayerId != initItem.Ownerid)
        // {
        //     Debug.Log("다르기 때문에 먹을 수없음 ");
        // }
    }
}

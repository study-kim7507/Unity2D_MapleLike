using System;
using System.Collections;
using Google.Protobuf.Protocol;
using UnityEngine;



public class InitItem : MonoBehaviour
{
    [SerializeField] ItemType itemType;
    public Item Property;
    public int Serverid;
    public int Ownerid; // 누가 죽였는지 
    public int CanOnlyOwnerLootTime;
    
    
    void Start()
    {
        
        foreach (var item in ItemManager.Instance.ItemList)
        {
            if (itemType == item.ItemType)
            {
                Property = item;
                break;
                
            }
        }
        if (Property == null)
        {
            Debug.LogWarning($"'{itemType}'에 해당하는 아이템을 찾지 못했습니다.");
        }
        // 🔥 코루틴 시작
        // 민혁님과 상의 해야됨 
        // StartCoroutine(LootTimeCountdown());
    }

    private IEnumerator LootTimeCountdown()
    {
        while (CanOnlyOwnerLootTime > 0)
        {
            yield return new WaitForSeconds(0.01f); // 1초당 
            CanOnlyOwnerLootTime -= 10; // 1씩감소 
        }

        if (CanOnlyOwnerLootTime == 0)
        {
            Ownerid = -1;
        }

        Debug.Log("그만");
    }
}

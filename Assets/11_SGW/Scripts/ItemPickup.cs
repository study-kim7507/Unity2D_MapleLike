using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections;

public class ItemPickup : MonoBehaviour
{
    public float attackSpeed = 5f;  // 아이템이 빨려가는 속도
    public bool isAttracting = false;
    private Coroutine attractCoroutine;

    
    public void StartAttracting2(S_LootItem lootItem)
    {
        
       isAttracting = true;
       
       // 기존 코루틴이 실행 중이라면 중지
       if (attractCoroutine != null)
       {
           StopCoroutine(attractCoroutine);
       }
       
       // 새로운 코루틴 시작
       attractCoroutine = StartCoroutine(AttractCoroutine(lootItem));
        
    }

    
    
    private IEnumerator AttractCoroutine(S_LootItem lootItem)
    {
        while (isAttracting)
        {
            
            Transform playerTransform = ObjectManager.Instance.FindById(lootItem.PlayerId).transform;

            // 플레이어에게 점진적으로 이동
            transform.position = Vector2.MoveTowards(transform.position, playerTransform.position, attackSpeed * Time.deltaTime);

            // 일정 거리 이내로 들어오면 줍기 처리
            if (Vector2.Distance(transform.position, playerTransform.position) < 0.5f)
            {
                PickUp(lootItem);
                isAttracting = false;
                yield break; // 코루틴 종료
            }

            yield return null; // 다음 프레임까지 대기
        }
    }

    void PickUp(S_LootItem lootItem)
    {
        var ItemInfo = transform.GetComponentInChildren<InitItem>();
        if (ItemInfo != null)
        {
            Debug.Log(ItemInfo);
        }

        // 죽인사람과 먹는 사람이 같거나 소유주가 전체인 경우
        if (lootItem.PlayerId == ObjectManager.Instance.MyPlayer.Id)
        {
            UIManager.Instance.AddItem(ItemInfo.Property);
            UIManager.Instance.PlaySoundPickupItem();
            Debug.Log("죽인사람과 먹는 사람이 같음 or 소유주가 전체 ");
        }
        // else
        // {
        //     Debug.Log("xxxxxxx 죽인사람과 먹는 사람이 다름");
        // }

        // 아이템 흡수 종료
        isAttracting = false;
        Debug.Log($"[ItemPickup] isAttracting이 false로 변경됨! {isAttracting}");
        Debug.Log("아이템 줍기 완료!");

        // 아이템 메모리에서 해제
        // Addressables.ReleaseInstance(gameObject);
        
        ObjectManager.Instance.DespwanItem2(lootItem);
    }
}

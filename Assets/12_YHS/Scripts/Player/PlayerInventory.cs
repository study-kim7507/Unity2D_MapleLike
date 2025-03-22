using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;


public class PlayerInventory : MonoBehaviour
{    
    //자식오브젝트에 들어있는 Slot 스크립트 전부 가져오기 
    public List<Slot> Slots = new List<Slot>();
    
    public TextMeshProUGUI TxtGold;
    
   

    
    private void Start()
    {
       
        
        UIManager.Instance.TxtGold.text = UIManager.Instance.Income.ToString();
        UIManager.Instance.ConnectPlayer();
        
    }
    
    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return;
        }
        // 기존에 있는 아이템인지 확인
        Slot existingSlot = Slots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        
        //골드 일때를 인벤토리에 넣지 말고 income 추가후 갱신 
        if (newItem.ItemType == ItemType.Gold)
        {
            UIManager.Instance.Income += 1000;
            TxtGold.text = UIManager.Instance.Income.ToString();
            Debug.Log($"🟡 골드 획득! 현재 보유 골드: {UIManager.Instance.Income}");
            return;  // 인벤토리에 추가되지 않도록 여기서 함수 종료
        }
     
        if (existingSlot != null)
        {   
         
         
            // 🔥 이미 존재하는 아이템이면 개수 증가
            existingSlot.Count++;
            existingSlot.UpdateUI();
            Debug.Log($"🟢 {newItem.itemName} 개수 증가: {existingSlot.Count}");
        }
        else
        {
            // 🔥 새로운 아이템이면 빈 슬롯에 추가
            Slot emptySlot = Slots.FirstOrDefault(slot => slot.CurrentItem == null);
            if (emptySlot != null)
            {
                emptySlot.Count++;
                emptySlot.SetItem(newItem);
                emptySlot.UpdateUI();
                Debug.Log($"🟢 {newItem.itemName} 개수 증가: {emptySlot.Count}");
            }
            else
            {
                Debug.LogWarning("❌ 인벤토리에 빈 슬롯이 없습니다!");
            }
        }
    }

    /// <summary>
    /// 소비 아이템 사용
    /// </summary>
    /// <param name="newItem"></param>
    public void UseItem(Item newItem)
    {
        if (newItem == null) { return; }

        if (newItem.category == Item.ItemCategory.Equipment)
        {
            Debug.Log("소비 아님");
            return;
        }
        Slot existingSlot = Slots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot !=null)
        {
            /*
             * 사용 했을 시 사용 효과 적용
             */
            PlayerInformation temp = GetComponent<PlayerInformation>();

            if (existingSlot.CurrentItem is Consumable consume)
            {
                switch (existingSlot.CurrentItem.ItemType)
                {
                    case ItemType.Hppotion1:
                    case ItemType.Hppotion2:
                        temp.SetPlayerHp(consume.healAmount);
                        Debug.Log("체력 회복");
                        break;
                    case ItemType.Mppotion1:
                    case ItemType.Mppotion2:
                        temp.SetPlayerMp(consume.MpAmount);
                        Debug.Log("마나 회복");
                        break;
                }
            }


            existingSlot.Count--;
            existingSlot.UpdateUI();
            if (existingSlot.Count == 0)
            {
                existingSlot.ClearSlot();
            }
            Debug.Log("소비템 사용");
        }
    }


    private void OnEnable()
    {
        if (TxtGold != null)
        {
            UpdateIncome();
        }
        
    }
    
    



    //돈 초기화
    public void UpdateIncome()
    {
       UIManager.Instance.TxtGold.text= UIManager.Instance.Income.ToString();
    }

    //선택한 하나 지우기
    public void UpdateInventoryUI(int soldItemId)
    {
        foreach (var slot in Slots)
        {
            if (slot.CurrentItem != null && slot.CurrentItem.id == soldItemId)
            {
                Debug.Log($"🛑 슬롯에서 {soldItemId} 제거!");
                slot.ClearSlot();
                break; // 한 개만 제거 후 종료
            }
        }
    }
    


    
}

using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class DoubleClick : MonoBehaviour
{
    public Button targetButton;
    private float clickTime = 0f;
    private float clickInterval = 0.5f;
    private int clickCount = 0;
    
    [SerializeField] GameObject ShopUI;
    
    
    public enum InventoryType { Inventory, Shop,Equip,Misc } // 현재 UI 타입 구분
    public enum ItemType { Equipment, Consumable, Misc }// 아이템 타입 구분

    [Header("UI 타입")]
    public InventoryType currentInventoryType;

    
 
    
    void OnEnable()
    {
        if (targetButton != null)
        {
            return;
        }
        targetButton = GetComponent<Button>();
        targetButton.onClick.AddListener(OnButtonClick);
    }

    void OnButtonClick()
    {
        clickCount++;

        if (clickCount == 1)
        {
            clickTime = Time.time;
        }
        else if (clickCount == 2)
        {
            if (Time.time - clickTime <= clickInterval)
            {
                HandleDoubleClick();
            }
            clickCount = 0;
        }

        if (Time.time - clickTime > clickInterval)
        {
            clickCount = 0;
        }
    }

    void HandleDoubleClick()
    {
        Slot inventoyItemSlot = transform.GetComponentInChildren<Slot>();
        if (currentInventoryType == InventoryType.Inventory)
        {
            
            
            if (inventoyItemSlot == null)
            {
                Debug.Log("없어요");
                return;
            }
            if (inventoyItemSlot.CurrentItem == null)
            {
                Debug.LogWarning("현재 슬롯에 아이템이 없습니다!");
                return;
            }
            
            //슬롯안에 들어있는 아이템 카테고리가 장비라면
            if (inventoyItemSlot.CurrentItem.category == Item.ItemCategory.Equipment)
            {
                
                //플레이어 레벨을 확인 그리고 limitjob를 확인 렙이 낮으면 장착 못하게 하기  
                if (inventoyItemSlot.CurrentItem is Equipment eq)
                {
                    if (PlayerInformation.playerStatInfo.Level < eq.limitLevel)
                    {
                        Debug.Log("렙이 낮아서 착용 할수 없어요");
                        UIManager.Instance.warningGroup.SetActive(true);
                        UIManager.Instance.PlaySoundDlgNotice();
                        return;
                    }
                }
                //수정 구문
                UIManager.Instance.EquipItem2(inventoyItemSlot.CurrentItem ,inventoyItemSlot);
                
            }
            //슬롯안에 들어있는 아이템 카테고리가 소비라면 
            else if (inventoyItemSlot.CurrentItem.category == Item.ItemCategory.Consumable)
            {
                UIManager.Instance.UseItem(inventoyItemSlot.CurrentItem);
            }
            
        }
        else if (currentInventoryType == InventoryType.Shop)
        {
           var showUI= transform.parent.GetComponent<ShowUI>();
           
           var category= showUI.CheckID();
           
           
           //부모오브젝트를 가져와서  있다면 
           if (showUI.ownerType == Info.OwnerType.Shop)
           {
               if (category == Item.ItemCategory.Equipment )
               {
                   showUI.BuyItem();
               }
               else
               {
                   showUI.buyGroups.SetActive(true);
                   UIManager.Instance.PlaySoundDlgNotice();
               }
           }
           else if (showUI.ownerType == Info.OwnerType.Player)
           {
               if (category == Item.ItemCategory.Equipment )
               {
                   showUI.SellItem();
               }
               else
               {
                   showUI.sellGroups.SetActive(true);
                   UIManager.Instance.PlaySoundDlgNotice();
               }
           }
        }
        //장비 벗기
        else if (currentInventoryType == InventoryType.Equip)
        {
            EquipSlot currentEquipSlot = transform.GetComponentInChildren<EquipSlot>();
            if (currentEquipSlot != null)
            {
                UIManager.Instance.AddItem(currentEquipSlot.CurrentItem);
                if (currentEquipSlot.CurrentItem is Equipment eq)
                {
                    if (PlayerInformation.equipmentStat == null)
                        PlayerInformation.equipmentStat = new PlayerStatInfo(); // null일 경우 초기화
                    
                    
                    var equipmentstat = PlayerInformation.equipmentStat; // 기존 객체 사용

                    equipmentstat.AttackPower -= eq.attackPower;
                    equipmentstat.MagicPower -= eq.magicPower;
                    equipmentstat.Defense -= eq.defensePower;

                    Debug.Log("eq의 어택파워 :" + eq.attackPower);
                    Debug.Log("eq의 방어력 :" + eq.defensePower);
                    Debug.Log("equipmentstat.AttackPower: " + equipmentstat.AttackPower);
                    Debug.Log("equipmentstat.Defense: " + equipmentstat.Defense);
                    Debug.Log("equipmentstat.MagicPower: " + equipmentstat.MagicPower);
                    Debug.Log("장비 벗기 완료");
                }
                Color color = currentEquipSlot._image.color;
                color.a = 0f; // 알파 값 0 (완전 투명)
                currentEquipSlot._image.color = color;
                currentEquipSlot.CurrentItem = null;
                var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
                playerObj.GetComponent<PlayerInformation>().CalculateStat();
            }
        }else if (currentInventoryType == InventoryType.Misc)
        {
            if (ShopUI ==null)
            {
                Debug.Log("상점이 없거나, currentInventoryType 확인하세요");
                return;
            }
            ShopUI.SetActive(true);
        }
      
    }
}

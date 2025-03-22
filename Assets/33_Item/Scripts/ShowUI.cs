
using System.Linq;
using Google.Protobuf.Protocol;
using TMPro;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShowUI : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    public Info UIPrefab;
    public GameObject TooltipGroup;
    
    // public PlayerInventory ClientInventroy;
    public TextMeshProUGUI TxtCurrentMeso;
    public TextMeshProUGUI Tooltip;
    //쓸데없는 곳 Raycast를 막기 위한 함수,툴팁을 위한것
    public CanvasGroup tooltipCanvas;
    public Info.OwnerType ownerType;
    public Image PlayerIcon;

    public GameObject sellGroups;
    public GameObject buyGroups;
    public TMP_InputField inputField;
    
    
    
    void Start()
    {
        
        // 마우스 이벤트  여러번 방지
        if (Tooltip != null)
        {
            
            tooltipCanvas = TooltipGroup.GetComponent<CanvasGroup>();
            if (tooltipCanvas != null)
            {
                tooltipCanvas.blocksRaycasts = false; 
            }
        }
    }

  
    
    public void InitUI()
    {
        // 아이템 로딩 완료 이벤트를 구독
        if (ItemManager.Instance != null)
        {
            // 수정 : 구독이 아닌 함수실행으로 
            // ItemManager.Instance.OnItemsLoaded += UpdateUI;
            
            UpdateUI();
            
        }
        else
        {
            Debug.LogWarning("ItemManager.Instance가 null입니다.");
        }
    }
    
    private void OnEnable()
    {
        InitUI();
        ItemManager.Instance.OnItemsLoaded += UpdateUI;
    }
    private void OnDisable()
    {
        if (ItemManager.Instance != null)
        {
            ItemManager.Instance.OnItemsLoaded -= UpdateUI;
        }
    }

    public Item.ItemCategory? CheckID()
    {
        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                int id = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                return CheckID(id); // 아이템 ID로 카테고리 반환
            }
        }
        return null; // 클릭된 아이템이 없으면 null 반환
    }

    public Item.ItemCategory? CheckID(int ID)
    {
        Debug.Log("아이템:"+ItemManager.Instance.ItemList
            .FirstOrDefault(item => item.id == ID)?.category);
        return ItemManager.Instance.ItemList
            .FirstOrDefault(item => item.id == ID)?.category;
    }

    #region 최초 상점 UI 갱신
    public void UpdateUI()
    {
        // 기존 UI를 모두 삭제하고 새로 생성
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject); // 기존 아이템 UI 삭제
        }
        if (ownerType == Info.OwnerType.Shop)
        {
          
            foreach (var itemData in ItemManager.Instance.ItemList
                         .OrderBy(item => item.category != Item.ItemCategory.Consumable ? 1 : 0) // 1) Consumable 먼저
                         .ThenBy(item => item is Equipment eq && eq.parts == Equipment.Parts.Weapon ? 0 : 1) // 2) 무기 먼저
                         .ThenBy(item => item is Equipment eq ? (int)eq.parts : int.MaxValue) // 3) 방어구 (Head → Body → Foot 순)
                         .ThenBy(item => item is Equipment eq ? eq.buyprice : int.MaxValue)) // 4) 장비만 가격 오름차순 정렬
            {
                if (itemData.ItemType == ItemType.Gold) // 골드는 제외
                    continue;

                var newItemUI = Instantiate(UIPrefab, transform);
                newItemUI.SetInfo(itemData, ownerType);
            }
        }
       
        if (ownerType == Info.OwnerType.Player)
        {
            // var Player = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            //게임 오브젝트 가져오기
            var player =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;

            if (player ==null)
            {
                return;
            }
            //아이콘 보이게하기
            if (PlayerIcon != null)
            {
                var playerImage = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id)
                    .GetComponentInChildren<SpriteRenderer>();
                PlayerIcon.sprite = playerImage.sprite;
            }
            
            
            if (TxtCurrentMeso != null)
            {
                 TxtCurrentMeso.text = UIManager.Instance.Income.ToString();
                
            }
            foreach (var itemData in UIManager.Instance.InventorySlots)
            { 
                var newItemUI = Instantiate(UIPrefab, transform); 
                newItemUI.SetInfo(itemData.CurrentItem, ownerType,UIManager.Instance.InventorySlots);
            }
          
        }
     
    }
    #endregion

    #region 아이템 구매 함수
    /// <summary>
    /// 아이템 구매 및 UI갱신 같이
    /// </summary>
    public void BuyItem(int amount)
    {
        //인벤토리 가져오기 수정
        // ClientInventroy=  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
       
        if (amount == 0)
        {
            UIManager.Instance.warningGroup.SetActive(true);
            UIManager.Instance.warningText.text = "한 개 이상 지정해 주세요";
            UIManager.Instance.PlaySoundDlgNotice();
            Debug.Log("돈 없다. 돈모아와라");
            return;
        }
        
        Debug.Log("구매할 갯수" +amount);
        if (UIManager.Instance.ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }
        //아이템 -1 번지로 초기화 기본값으로 --> 아무것도 없는 상태를 말함
        int itemId = -1;

        
        //클릭된 오브젝트의 아이템 아이디 값을 가져오는 구문
        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                break;
            }
        }
        
        if (itemId == -1)
        {
            return;
        }
        /*
         * 1.아이템아이디를 아이템리스트에서 맞는걸 찾아서 할당
         * 2.플레이어의 메소와 비교해서 구매 가능 여부를 확인
         * P.S 플레이어 스탯 및 플레이어의 메소 파라미터가  연결이 안되 테스트용 PlayerTestIncome을 만들어서 사용
         */
        var playerInventory =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
        Item itemToAdd = ItemManager.Instance.ItemList.Find(item => item.id == itemId);
       
        //있을때 실행 
        if (itemToAdd != null)
        {
            if (UIManager.Instance.Income< (itemToAdd.buyprice)*amount)
            {
                //아마 팝업 UI를 뜨게 할예정
                UIManager.Instance.warningGroup.SetActive(true);
                UIManager.Instance.warningText.text = "돈이 모잘라요";
                UIManager.Instance.PlaySoundDlgNotice();
                Debug.Log("돈 없다. 돈모아와라");
                return;
            }
            
            
            if (itemToAdd.category == Item.ItemCategory.Equipment)
            {
                if (amount == 1)
                {
                    UIManager.Instance.Income -= itemToAdd.buyprice;
                    // ClientInventroy.AddItem(itemToAdd); // ✅ 개수 체크 및 추가
                    UIManager.Instance.AddItem(itemToAdd,amount);
                    playerInventory.UpdateIncome();
                }
                else
                {
                    //여기도 알람참?
                    UIManager.Instance.warningGroup.SetActive(true);
                    UIManager.Instance.warningText.text = "무기는 하나만 구매 가능합니다.";
                    UIManager.Instance.PlaySoundDlgNotice();
                    Debug.Log("무기는 하나만 구매 가능");
                }
            }
            else
            {
                UIManager.Instance.Income -= (itemToAdd.buyprice) * amount;
                // ClientInventroy.AddItem(itemToAdd); // ✅ 개수 체크 및 추가
                UIManager.Instance.AddItem(itemToAdd,amount);
                playerInventory.UpdateIncome();
            }
        }
        else
        {
            Debug.LogWarning("⚠ 아이템을 찾을 수 없습니다.");
            return;
        }

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
        // ClientInventroy.ShowInventory();
    }
    #endregion

    #region 아이템 판매 함수
    /// <summary>
    /// 아이템 판매 및 UI갱신 같이
    /// </summary>
    /*
     * 구조는 구매와 동일 대신 빼기만 사용
     */
    public void SellItem(int amount)
    {
        if (amount == 0)
        {
            UIManager.Instance.warningGroup.SetActive(true);
            UIManager.Instance.warningText.text = "한 개 이상 지정해 주세요";
            UIManager.Instance.PlaySoundDlgNotice();
            Debug.Log("돈 없다. 돈모아와라");
            return;
        }
        if (UIManager.Instance.ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }

        int itemId = -1;

        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                Debug.Log($"🎯 선택된 아이템 ID: {itemId}");
                break;
            }
        }

        if (itemId == -1)
        {
            Debug.LogWarning("⚠ 아이템이 선택되지 않았거나 ID를 찾을 수 없습니다.");
            return;
        }

        // 현재 인벤토리에서 해당 아이템을 보유한 슬롯 찾기
        Slot existingSlot = UIManager.Instance.InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == itemId);

        var playerInventory =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
      
        
        if (existingSlot != null)
        {

            if (existingSlot.CurrentItem.category == Item.ItemCategory.Equipment)
            {
                if (amount == 1)
                {
                    existingSlot.Count--;
                    UIManager.Instance.Income += existingSlot.CurrentItem.sellprice;
                }
                else
                {
                    UIManager.Instance.warningGroup.SetActive(true);
                    UIManager.Instance.warningText.text = "무기는 하나만 판매 가능합니다.";
                    UIManager.Instance.PlaySoundDlgNotice();
                }
            }
            else
            {
                if (amount > existingSlot.Count )
                {
                    UIManager.Instance.warningGroup.SetActive(true);
                    UIManager.Instance.warningText.text = "팔려는 갯수가 더큽니다.";
                    UIManager.Instance.PlaySoundDlgNotice();
                    return;
                }
                else
                {
                    // ✅ 아이템 개수 감소
                    existingSlot.Count -= amount;
                    // 골드 증가
                    UIManager.Instance.Income += (existingSlot.CurrentItem.sellprice) * amount;
                }
            }
            if (existingSlot.Count == 0)
            {
                // ✅ 개수가 0이면 슬롯 초기화 (이미지도 원래대로)
                Debug.Log($"🗑 {existingSlot.CurrentItem.itemName} 개수 0개 -> 슬롯 초기화");
                existingSlot.ClearSlot();
            }
            else
            {
                // ✅ 개수가 남아 있으면 UI 업데이트
                existingSlot.UpdateUI();
                Debug.Log($"📉 {existingSlot.CurrentItem.itemName} 판매 완료, 남은 개수: {existingSlot.Count}");
            }
            playerInventory.UpdateIncome();
        }
        else
        {
            Debug.LogWarning("⚠ 판매할 아이템이 없습니다.");
            return;
        }

        // 🔥 인벤토리 UI 업데이트
        playerInventory.UpdateInventoryUI(itemId);

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
    }
    #endregion

    #region 장비 판매
    public void SellItem()
    {
        
        if (UIManager.Instance.ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }

        int itemId = -1;

        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                Debug.Log($"🎯 선택된 아이템 ID: {itemId}");
                break;
            }
        }

        if (itemId == -1)
        {
            Debug.LogWarning("⚠ 아이템이 선택되지 않았거나 ID를 찾을 수 없습니다.");
            return;
        }

        // 현재 인벤토리에서 해당 아이템을 보유한 슬롯 찾기
        Slot existingSlot = UIManager.Instance.InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == itemId);

        var playerInventory =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
        if (existingSlot != null)
        {
            // ✅ 아이템 개수 감소
            existingSlot.Count--;
            // 골드 증가
            UIManager.Instance.Income += existingSlot.CurrentItem.sellprice;

            if (existingSlot.Count <= 0)
            {
                // ✅ 개수가 0이면 슬롯 초기화 (이미지도 원래대로)
                Debug.Log($"🗑 {existingSlot.CurrentItem.itemName} 개수 0개 -> 슬롯 초기화");
                existingSlot.ClearSlot();
            }
            else
            {
                // ✅ 개수가 남아 있으면 UI 업데이트
                existingSlot.UpdateUI();
                Debug.Log($"📉 {existingSlot.CurrentItem.itemName} 판매 완료, 남은 개수: {existingSlot.Count}");
            }

            
            playerInventory.UpdateIncome();
        }
        else
        {
            Debug.LogWarning("⚠ 판매할 아이템이 없습니다.");
            return;
        }

        // 🔥 인벤토리 UI 업데이트
        playerInventory.UpdateInventoryUI(itemId);

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
    }
    

    #endregion

    #region 장비 전용
     public void BuyItem()
    {
        //인벤토리 가져오기 수정
        // ClientInventroy=  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
        
        if (UIManager.Instance.ClientInventroy == null)
        {
            Debug.LogError("❌ ClientInventroy가 설정되지 않았습니다!");
            return;
        }
        //아이템 -1 번지로 초기화 기본값으로 --> 아무것도 없는 상태를 말함
        int itemId = -1;

        
        //클릭된 오브젝트의 아이템 아이디 값을 가져오는 구문
        foreach (Transform childTransform in transform)
        {
            var component = childTransform.GetComponent<ChangeImageOnClick>();
            if (component != null && component.CurrentClickedGameObject != null)
            {
                itemId = component.CurrentClickedGameObject.GetComponent<Info>().ID;
                break;
            }
        }
        
        if (itemId == -1)
        {
            return;
        }
        /*
         * 1.아이템아이디를 아이템리스트에서 맞는걸 찾아서 할당
         * 2.플레이어의 메소와 비교해서 구매 가능 여부를 확인
         * P.S 플레이어 스탯 및 플레이어의 메소 파라미터가  연결이 안되 테스트용 PlayerTestIncome을 만들어서 사용
         */
        var playerInventory =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<YHSMyPlayerController>().playerInventory;
        Item itemToAdd = ItemManager.Instance.ItemList.Find(item => item.id == itemId);
        if (itemToAdd != null)
        {
            if (UIManager.Instance.Income< itemToAdd.buyprice)
            {
                //아마 팝업 UI를 뜨게 할예정
                Debug.Log("돈 없다. 돈모아와라");
                return;
            }
            UIManager.Instance.Income -= itemToAdd.buyprice;
            // ClientInventroy.AddItem(itemToAdd); // ✅ 개수 체크 및 추가
            UIManager.Instance.AddItem(itemToAdd);
            playerInventory.UpdateIncome();
        }
        else
        {
            Debug.LogWarning("⚠ 아이템을 찾을 수 없습니다.");
            return;
        }

        // 🔥 OnItemsLoaded 이벤트 강제 트리거
        Debug.Log("OnItemsLoaded 강제 트리거");
        ItemManager.Instance.TriggerOnItemsLoaded();
        // ClientInventroy.ShowInventory();

    }
    #endregion
    
    
    
    
    
    
    
    
    //마우스가 가르킨 오브젝트의 정보에 맞게 툴팁 설명 해주기
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject hoveredObject = eventData.pointerEnter;
        Info hoveredInfo;
        if (hoveredObject.TryGetComponent<Info>(out hoveredInfo))
        {
            foreach (var itemData in ItemManager.Instance.ItemList)
            {
                if (itemData.id == hoveredInfo.ID)
                {
                    if (Tooltip != null)
                    {
                        TooltipGroup.SetActive(true);
                        if (itemData is Equipment eq)
                        {
                            Tooltip.text = "착용레벨:"+ eq.limitLevel + "\n"+
                                           "공격력:"+ eq.attackPower + "\n"+
                                           "마법공격력:"+ eq.magicPower + "\n"+
                                           "방어력:"+ eq.defensePower + "\n"+
                                           eq.description +"\n"+"더블 클릭으로 장착이 가능하다.";
                        }
                        else
                        {
                            Tooltip.text = itemData.description +"\n"+"더블 클릭으로 사용이 가능하다";
                        }

                        TooltipGroup.transform.position = eventData.position;
                    }
                }
            }
        }
        else
        {
            // 부모 오브젝트가 없거나, Info 컴포넌트가 없음
            Debug.Log("부모 오브젝트가 없거나 Info 컴포넌트가 없음");
        }
    }
    //마우스가 가르킨 오브젝트 없으면끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (Tooltip != null)
        {
            TooltipGroup.SetActive(false);
        }
    }

    public void BtnBuyClickCheckID()
    {
        var  itemCategory =CheckID();
        if (itemCategory == Item.ItemCategory.Equipment)
        {
            BuyItem();
        }else if (itemCategory == Item.ItemCategory.Consumable)
        {
            buyGroups.SetActive(true);
        }
    }
    
    public void BtnSellClickCheckID()
    {
        var  itemCategory =CheckID();
        if (itemCategory == Item.ItemCategory.Equipment)
        {
            SellItem();
        }else if (itemCategory == Item.ItemCategory.Consumable)
        {
            sellGroups.SetActive(true);
        }
    }
}
using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Protocol;
using UnityEngine;
using TMPro;


public class UIManager : MonoBehaviour 
{
    public GameObject invenotory;
    public GameObject Equipment;
    public GameObject BuffList;
    
    private static UIManager _instance;
    
    public List<Slot> InventorySlots = new List<Slot>();
    
    public List<EquipSlot> EquipSlots = new List<EquipSlot>();
    
    public PlayerInventory ClientInventroy;
    public TextMeshProUGUI TxtGold;
    public static UIManager Instance { get { return _instance; } }

    public Dictionary<int,Item> InventoryItems = new Dictionary<int,Item>();


    public AudioSource audioSource;
    
    public List<AudioClip> audioClips;
    
    [Header("툴팁")]
    public GameObject tooltipGroup;
    
    public TextMeshProUGUI tooltipText;
    public CanvasGroup tooltipCanvas;

    [Header("경고")]
    public GameObject warningGroup;
    public TextMeshProUGUI warningText;

    [Header("첫 초기화용")]
    public bool hasInitialized  = false;
   
    
    //플레이어가 가지고 있는 돈을 확인
    public int Income = 1000;
    
    
    
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
        audioSource = GetComponent<AudioSource>();
    }
    
    
    public void EquipSetWindowActive()
    {
        if (Equipment.activeSelf == true)
        {
            Equipment.SetActive(false);
        }
        else
        {
            Equipment.SetActive(true);
        }
    }
    
    public void invenSetWindowActive()
    {
        if (invenotory.activeSelf == true)
        {
            invenotory.SetActive(false);
        }
        else
        {
            invenotory.SetActive(true);
        }
    }

    #region 사운드 실행 함수
    public void PlaySoundOpen()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.MenuUp)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }

    public void PlaySoundClose()
    {
        
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.MenuDown)
            {
               
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundBtnClick()
    {
        
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.BtMouseClick)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundPickupItem()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.PickUpItem)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    public void PlaySoundDlgNotice()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.DlgNotice)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    
    public void PlaySoundUsePotion()
    {
        foreach (var VARIABLE in audioClips)
        {
            if (VARIABLE.name ==DefineSoundName.UsePotion)
            {
                audioSource.PlayOneShot(VARIABLE);
            }
        }
    }
    #endregion

    #region 처음 맵에 들어갈때 UI들 연동해주는 함수
    public void ConnectPlayer()
    {
        invenotory.SetActive(true);
        BuffList.SetActive(true);

        var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        InventorySlots = new List<Slot>(invenotory.GetComponentsInChildren<Slot>());
        EquipSlots = new List<EquipSlot>(Equipment.GetComponentsInChildren<EquipSlot>());
        ClientInventroy = playerObj.GetComponent<YHSMyPlayerController>().playerInventory;
        invenotory.SetActive(false);
        Equipment.SetActive(false);
    }

    public void ConnectEquipment()
    {
        EquipSlots = new List<EquipSlot>(Equipment.GetComponentsInChildren<EquipSlot>());
        
        Equipment.SetActive(false);
    }
    #endregion

    #region 장비 장착
    public void EquipItem2(Item newItem, Slot slot)
    {
        // ✅ 장비인지 확인
        if (newItem.category != Item.ItemCategory.Equipment)
        {
            Debug.Log("⚠ 장비 아이템이 아닙니다!");
            return;
        }

        if (newItem is Equipment equipment)
        {
            // ✅ Cnone 타입이면 모든 직업이 장착 가능
             if (equipment.classType == ClassType.Cnone)
            {
                Debug.Log($"🛡 {newItem.itemName}은(는) 모든 직업이 장착할 수 있습니다!");
            }
            else
            {
                // ✅ 직업별 무기 제한 적용
                if (equipment.classType != PlayerInformation.playerStatInfo.ClassType)
                {
                    Debug.Log($"❌ {PlayerInformation.playerStatInfo.ClassType}는 {equipment.classType} 전용 아이템을 장착할 수 없습니다!");
                    warningText.text =
                        $"❌ {PlayerInformation.playerStatInfo.ClassType}는 {equipment.classType} 전용 아이템을 장착할 수 없습니다!";
                    Instance.warningGroup.SetActive(true);
                    Instance.PlaySoundDlgNotice();
                    return;
                }
            }
        }
        
        // ✅ 부위에 맞는 장비 슬롯 찾기
        if (newItem is Equipment eq1)
        {
            EquipSlot equipSlot = EquipSlots.FirstOrDefault(slot => slot.CurrentPart == eq1.parts);
        
            if (equipSlot == null)
            {
                Debug.Log("⚠ 해당 장비를 장착할 슬롯이 없습니다.");
                return;
            }

            // ✅ 기존 장착된 아이템 확인 후 인벤토리로 이동
            if (equipSlot.CurrentItem != null)
            {
                Debug.Log($"🔄 {equipSlot.CurrentItem.itemName} → {newItem.itemName} 장비 교체!");
                SwapEquipment(slot, equipSlot);
            }
            else
            {
                UpdateItemSlot(slot);
                Debug.Log($"✅ {newItem.itemName} 장착 완료!");
            }

            // ✅ 새 장비 장착
            equipSlot.SetItem(newItem);
        }

        // ✅ 장비 능력치 적용
        if (newItem is Equipment eq)
        {
            var playerObj =ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            ApplyEquipmentStats(eq);
            playerObj.GetComponent<PlayerInformation>().CalculateStat();
        }
    }
    #endregion

    #region 장비창에 장착후 인벤토리에서 제거
    void UpdateItemSlot(Slot slot)
    {
        if (slot.Count > 1)
        {
            slot.Count--;
            slot.CountText.text = slot.Count.ToString();
        }
        else
        {
            // 인벤토리에서 제거
            slot._image.sprite = null;
            Color color = slot._image.color;
            color.a = 0f; // 투명 처리
            slot._image.color = color;
            slot.Count = 0;
            slot.CurrentItem = null;
            Debug.Log("🗑 아이템이 하나뿐이어서 삭제됨");
        }
    }
    #endregion

    #region 아이템 스왑
    void SwapEquipment(Slot inventorySlot, EquipSlot equipSlot)
    {
        //수정 해야 될꺼같긴함
        Item oldItem = equipSlot.CurrentItem; // 기존 장비 저장

        // ✅ 기존 장비를 인벤토리로 이동
        if (oldItem != null)
        {
            inventorySlot.SetItem(oldItem);

            if (oldItem is Equipment equipment)
            {
                // 장착 해제에 따른 적용스탯 갱신
                PlayerInformation.equipmentStat.AttackPower -= equipment.attackPower;
                PlayerInformation.equipmentStat.MagicPower -= equipment.magicPower;
                PlayerInformation.equipmentStat.Defense -= equipment.defensePower;
            }
        }

        // ✅ 장착 슬롯 초기화
        equipSlot.CurrentItem = null;   
    }
    #endregion
    
    #region 클래스 타입에 따라 능력치 설정하는 함수
    void ApplyEquipmentStats(Equipment eq)
    {
        if (PlayerInformation.equipmentStat == null)
            PlayerInformation.equipmentStat = new PlayerStatInfo(); // null 체크 후 초기화
        

       
        var equipmentStat = PlayerInformation.equipmentStat; // 기존 객체 사용
        
   
        equipmentStat.Defense += eq.defensePower;
        switch (PlayerInformation.playerStatInfo.ClassType)
        {
            case ClassType.Archer:
                equipmentStat.AttackPower += eq.attackPower; // 궁수 보너스
                break;
            case ClassType.Magician:
                equipmentStat.MagicPower += eq.magicPower; // 마법사 보너스
                break;
            case ClassType.Warrior:
                equipmentStat.AttackPower += eq.attackPower; // 전사 보너스
                break;
        }
        Debug.Log($"⚔ 장비 장착 완료 - 공격력: {equipmentStat.AttackPower}, 방어력: {equipmentStat.Defense}, 마법력: {equipmentStat.MagicPower}");
    }
    #endregion


    #region 초기 기본값  생성
    public void InitItem()
    {
        if (hasInitialized)
        {
            Debug.Log("이미 한번 초기화 했음");
            return;
        }
        Equipment.SetActive(true);
        
        EquipSlot weaponItem = EquipSlots.Find(slot => slot.name == "WeaponItem");
        if (weaponItem != null)
        {
            Debug.Log("WeaponItem 발견: " + weaponItem.name);

            ClassType playerClass = PlayerInformation.playerStatInfo.ClassType; // 플레이어 직업 가져오기
            bool isUpdated = false; // 아이템이 설정되었는지 체크
            foreach (Item VARIABLE in ItemManager.Instance.ItemList)
            {
                if (VARIABLE is Equipment equipment && equipment.classType == playerClass) // 플레이어 직업과 같은 장비만 선택
                {
                    switch (playerClass)
                    {
                        case ClassType.Archer:
                            if (VARIABLE.ItemType == ItemType.Arrow1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("아쳐 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                        case ClassType.Magician:
                            if (VARIABLE.ItemType == ItemType.Staff1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("매지션 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                        case ClassType.Warrior:
                            if (VARIABLE.ItemType == ItemType.Sword1)
                            {
                                updateDefaultWeapon(weaponItem, VARIABLE);
                                Debug.Log("워리어 장비 설정: " + VARIABLE.itemName);
                                isUpdated = true;
                            }
                            break;
                    }

                    if (isUpdated) // 장비가 설정되었으면 루프 종료
                    {
                        var playerObj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
                        playerObj.GetComponent<PlayerInformation>().CalculateStat();

                        break;
                    }
                }
            }

            Equipment.SetActive(false);
        }
        else
        {
            Debug.Log("WeaponItem 못찾음 ");
        }
    }
   

    public void InitMpPoitions()
    {
        if (hasInitialized)
        {
            Debug.Log("이미 한번 초기화 했음");
            return;
        }
        var slot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
        foreach (var item in ItemManager.Instance.ItemList)
        {
            if (item.ItemType == ItemType.Mppotion1)
            {
                slot.SetItem(item);
                slot.Count = 10;
                slot.UpdateUI();
            }
        }
    }
    
    public void InitHpPoitions()
    {
        if (hasInitialized)
        {
            Debug.Log("이미 한번 초기화 했음");
            return;
        }
        var slot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
        foreach (var item in ItemManager.Instance.ItemList)
        {
            if (item.ItemType == ItemType.Hppotion1)
            {
                slot.SetItem(item);
                slot.Count = 10;
                slot.UpdateUI();
            }
        }
    }
    #endregion

    #region UI갱신 + 정보 넣기(updateDefaultWeapon)
    //조건문의 아이템 타입과 같은 아이템 스크립트만 가져옴 
    void updateDefaultWeapon( EquipSlot weaponItem,Item VARIABLE )
    {
        weaponItem.CurrentItem = VARIABLE;
        weaponItem._image.sprite = VARIABLE.IconSprite;
        Color color = weaponItem._image.color;
        color.a = 1f;  // 
        weaponItem._image.color = color;
                            
        var equipmentstat = PlayerInformation.equipmentStat;
        if (weaponItem.CurrentItem is Equipment eq)
        {
            equipmentstat.AttackPower = eq.attackPower;
            equipmentstat.Defense = eq.defensePower;
            equipmentstat.MagicPower = eq.magicPower;
            Debug.Log("초기값 갱신");
            Debug.Log(equipmentstat.AttackPower);
            Debug.Log(equipmentstat.MagicPower);
            
        }
    }
    #endregion

    #region 아이템 사용
    public void UseItem(Item newItem)
    {
        if (newItem == null) { return; }

        if (newItem.category == Item.ItemCategory.Equipment)
        {
            Debug.Log("소비 아님");
            return;
        }
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot !=null)
        {
            /*
             * 사용 했을 시 사용 효과 적용
             */
            PlayerInformation temp = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id).GetComponent<PlayerInformation>();
            
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
                    case ItemType.Superpotion1:
                        temp.SetPlayerHp(PlayerInformation.playerStatInfo.MaxHp/2);
                        temp.SetPlayerMp(PlayerInformation.playerStatInfo.MaxMp/2);
                        break;
                    case ItemType.Superpotion2:
                        temp.SetPlayerHp(PlayerInformation.playerStatInfo.MaxHp);
                        temp.SetPlayerMp(PlayerInformation.playerStatInfo.MaxMp);
                        break;
                }
                PlaySoundUsePotion();
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
    #endregion

    #region 아이템 추가
    public void AddItem(Item newItem,int amount)
    {
        if (newItem == null)
        {
            return;
        }
        // 기존에 있는 아이템인지 확인
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot ==null )
        {
            Debug.Log("아이템 타입은"+newItem.ItemType);
        }
        
        if (newItem.ItemType == ItemType.Gold)
        {
            Income += 10;
            TxtGold.text = Income.ToString();
            Debug.Log($"🟡 골드 획득! 현재 보유 골드: {Income}");
            return;  // 인벤토리에 추가되지 않도록 여기서 함수 종료
        }
        
        
        if (existingSlot != null)
        {
            
            // 🔥 이미 존재하는 아이템이고 소비 가능한거면 개수 증가 시키기
            if (existingSlot.CurrentItem is Consumable consume)
            {
               
                existingSlot.Count += amount;
                existingSlot.UpdateUI();
                Debug.Log($"🟢 {newItem.itemName} 개수 증가: {existingSlot.Count}");
            }
            // 🔥 이미 존재하는 아이템이고 장비면 다른 슬롯에 넣기 
            else if (existingSlot.CurrentItem is Equipment equip)
            {
                Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
                if (emptySlot != null)
                {
                    emptySlot.Count++;
                    emptySlot.SetItem(newItem);
                    emptySlot.UpdateUI();
                    Debug.Log($"🟢 {newItem.itemName} 새로운 슬롯에 추가됨");
                }
                else
                {
                    Debug.LogWarning("❌ 인벤토리에 빈 슬롯이 없습니다!");
                }
            }
        }
        else
        {
            // 🔥 새로운 아이템이면 빈 슬롯에 추가
            Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
            if (emptySlot != null)
            {
                emptySlot.Count += amount;
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
    
    public void AddItem(Item newItem)
    {
        if (newItem == null)
        {
            return;
        }
        // 기존에 있는 아이템인지 확인
        Slot existingSlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem != null && slot.CurrentItem.id == newItem.id);
        if (existingSlot ==null )
        {
            Debug.Log("아이템 타입은"+newItem.ItemType);
        }
        
        if (newItem.ItemType == ItemType.Gold)
        {
            Income += 10;
            TxtGold.text = Income.ToString();
            Debug.Log($"🟡 골드 획득! 현재 보유 골드: {Income}");
            return;  // 인벤토리에 추가되지 않도록 여기서 함수 종료
        }
        
        
        if (existingSlot != null)
        {
            
            // 🔥 이미 존재하는 아이템이고 소비 가능한거면 개수 증가 시키기
            if (existingSlot.CurrentItem is Consumable consume)
            {
               
                existingSlot.Count++;
                existingSlot.UpdateUI();
                Debug.Log($"🟢 {newItem.itemName} 개수 증가: {existingSlot.Count}");
            }
            // 🔥 이미 존재하는 아이템이고 장비면 다른 슬롯에 넣기 
            else if (existingSlot.CurrentItem is Equipment equip)
            {
                Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
                if (emptySlot != null)
                {
                    emptySlot.Count++;
                    emptySlot.SetItem(newItem);
                    emptySlot.UpdateUI();
                    Debug.Log($"🟢 {newItem.itemName} 새로운 슬롯에 추가됨");
                }
                else
                {
                    Debug.LogWarning("❌ 인벤토리에 빈 슬롯이 없습니다!");
                }
            }
        }
        else
        {
            // 🔥 새로운 아이템이면 빈 슬롯에 추가
            Slot emptySlot = InventorySlots.FirstOrDefault(slot => slot.CurrentItem == null);
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
    #endregion

    #region 타입별 정렬

    public void TypeCheck()
    {
        foreach (var inven in InventorySlots)
        {
            if (inven.CurrentItem is Equipment eq)
            {
                Debug.Log(eq.itemName);
            }
        }
    }

    #endregion

}

using Google.Protobuf.Protocol;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    PlayerInformation playerInfo;   // 테스트용

    private GameObject player;
    public float pickupRange = 0.7f;  // 줍기 범위
    public LayerMask itemLayer;   
    
    private void Start()
    {
        playerInfo = GetComponent<PlayerInformation>();
        player =  ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
        itemLayer = 1 << LayerMask.NameToLayer("Item");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            MinimapManager.Instance.OnOffMinimap();
        }
        #region 알파벳 키 (qwerty)
        //if (Input.GetKeyDown(KeyCode.A))
        //{
        //    // 버프 스킬
        //    // MyPlayerController의 MovePlayer()에 위치합니다.
        //}
        if (Input.GetKeyDown(KeyCode.S))
        {
            //playerInfo.PrintStatInfo();
            UIManager.Instance.PlaySoundOpen();
            StatWindowManager.Instance.SetWindowActive();
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            if (!UIManager.Instance.invenotory.activeSelf)
            {
                UIManager.Instance.PlaySoundOpen();
            }
            UIManager.Instance.invenSetWindowActive();
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!UIManager.Instance.Equipment.activeSelf)
            {
                UIManager.Instance.PlaySoundOpen();
            }
            UIManager.Instance.EquipSetWindowActive();
            
        }       
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Debug.Log("GetItem");
            ObjectManager.Instance.PickupNearbyItems2();
        }
    
        #endregion

        #region 편집 키 (insert, delete 류)
        if (Input.GetKeyDown(KeyCode.Insert))
        {
            //하얀포션
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Hppotion2)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Delete))
        {
            //마나엘릭서
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Mppotion2)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.Home))
        {
            //엘릭서
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Superpotion1)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.End))
        {
            //파워엘릭서
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가 
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Superpotion2)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        //체력포션1
        if (Input.GetKeyDown(KeyCode.PageUp))
        {
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가 
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Hppotion1)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.PageDown))
        {
            var Obj = ObjectManager.Instance.FindById(ObjectManager.Instance.MyPlayer.Id);
            Debug.Log(Obj.GetComponent<PlayerController>().isDead);
            if (Obj.GetComponent<PlayerController>().isDead)
            {
                return;
            }
            
            foreach (var VARIABLE in UIManager.Instance.InventorySlots)
            {
                //없을것을 대비한 널 체크 추가
                if (VARIABLE.CurrentItem != null && VARIABLE.CurrentItem.ItemType == ItemType.Mppotion1)
                {
                    UIManager.Instance.UseItem(VARIABLE.CurrentItem);
                    break;
                }
            }
        }
        #endregion

        #region 테스트용 임시 단축키
        if (Input.GetKey(KeyCode.LeftShift))
        {
            // LeftShit 누른 상태로

            if (Input.GetKeyDown(KeyCode.H))
            {
                // "H"eal
                // 캐릭터 HP/MP 전체 회복
                playerInfo.SetPlayerHp(PlayerInformation.playerStatInfo.MaxHp);
                playerInfo.SetPlayerMp(PlayerInformation.playerStatInfo.MaxMp);
            }
            if (Input.GetKeyDown(KeyCode.R))
            {
                // "R"eturn
                // 마을로 귀환 (보스 맵에서는 사용하지 마세요, 무슨 일이 일어날지 모름)
                GetComponent<YHSMyPlayerController>().SendPlayerToVillage();
            }
            if (Input.GetKeyDown(KeyCode.T))
            {
                // "T"housand
                // 1000메소 획득
                UIManager.Instance.Income += 1000;
                UIManager.Instance.TxtGold.text = UIManager.Instance.Income.ToString();
            }
            if (Input.GetKeyDown(KeyCode.U))
            {
                // "U"p
                // 1레벨 업
                playerInfo.SetPlayerExp(PlayerInformation.playerStatInfo.TotalExp);
            }
            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                // "["
                // 최대 체력의 10% 깎기
                playerInfo.SetPlayerHp(-PlayerInformation.playerStatInfo.MaxHp / 10);
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                // "]"
                // 최대 마나의 10% 깎기
                playerInfo.SetPlayerMp(-PlayerInformation.playerStatInfo.MaxMp / 10);
            }
        }
        #endregion
    }
}

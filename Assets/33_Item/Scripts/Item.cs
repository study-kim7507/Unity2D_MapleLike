using Google.Protobuf.Protocol;
using UnityEngine;



[System.Serializable]
public abstract class Item : ScriptableObject
{
    public enum ItemCategory
    {
        Consumable, // 소모품 (포션, 음식)
        Equipment   // 장비 (검, 갑옷)
    }
    /*
     * 포션은 1000번부터 시작
     * 장비는 5000번부터 시작
     */
    public int id;
    public string itemName;
    public string description;
    public int sellprice;
    public int buyprice;
    public ItemCategory category; // Consumable or Equipment
    public ItemType ItemType; //서버에 관한 아이템 타입
    public Sprite IconSprite;
    public string iconAddress; // Addressables에서 사용할 아이콘 주소
}
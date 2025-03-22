
using UnityEngine;

[System.Serializable]
public class ItemData 
{
    public int id;
    public string itemName;
    public string description;
    public int sellprice;
    public int buyprice;
    public string itemType; // Consumable or Equipment
    public string itemCategory; // Consumable or Equipment
    public string iconAddress;
    public Sprite icon;
    
    //Equipment
    public int limitLevel;
    public string classType;
    public string parts;
    public int magicPower;
    public int attackPower;// 공격력 증가
    public int defensePower; // 방어력 증가
    
    //Consumable
    public int healAmount; // Hp 회복
    public int MpAmount; // Mp 회복
}


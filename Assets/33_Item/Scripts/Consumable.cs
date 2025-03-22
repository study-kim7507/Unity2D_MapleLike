using UnityEngine;

[CreateAssetMenu(fileName = "Consumable", menuName = "Scriptable Objects/Consumable")]
public class Consumable : Item
{
    public int healAmount; // Hp 회복
    public int MpAmount; // Mp 회복
    
    public Consumable()
    {
        category = ItemCategory.Consumable; // 소모품으로 설정
    }
}

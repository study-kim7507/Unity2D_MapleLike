using Google.Protobuf.Protocol;
using UnityEngine;


[CreateAssetMenu(fileName = "Equipment ", menuName = "Scriptable Objects/Equipment ")]
public class Equipment  : Item
{
   
    public enum Parts
    {
        Head,    // 머리
        Body,    // 몸통
        Foot,      // 다리
        Weapon  //무기
    }

    public int limitLevel;
    public ClassType classType;
    public Parts parts;
    //보너스 스텟
    public int attackPower;
    public int magicPower;
    public int defensePower; // 방어력 증가
    
    
    public Equipment()
    {
        category = ItemCategory.Equipment; // 장비로 설정
    }
}

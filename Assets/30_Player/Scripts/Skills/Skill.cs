using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "Skill", menuName = "Scriptable Objects/Skill")]
public class Skill : ScriptableObject
{
    public string skillName;
    public int maxLevel;
    public int damage;
    public int addDamage;
    public int manaCost;
    public int addManaCost;
    public int hitCount;
    public int coolTime;
    public int duration;

    public Skill(string skillName, int maxLevel, int damage, int manaCost, int hitCount, int coolTime, int duration)
    {
        this.skillName = skillName;
        this.maxLevel = maxLevel;
        this.damage = damage;
        this.manaCost = manaCost;
        this.hitCount = hitCount;
        this.coolTime = coolTime;
        this.duration = duration;
    }

    public int Damage(int skillLevel)
    {
        return skillLevel > 0 && skillLevel <= maxLevel ? damage + addDamage * (skillLevel - 1) : damage;
    }

    public int Cost(int skillLevel)
    {
        return skillLevel > 0 && skillLevel <= maxLevel ? manaCost + addManaCost * (skillLevel - 1) : manaCost;
    }
}

public class SkillData
{
    public Skill skill;
    public int level;

    public SkillData(Skill skill, int level)
    {
        this.skill = skill;
        this.level = level;
    }

    public int GetDamage()
    {
        return skill.Damage(level);
    }

    public int GetManaCost()
    {
        return skill.Cost(level);
    }
}

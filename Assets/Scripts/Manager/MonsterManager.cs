using System.Collections.Generic;
using System.Text.RegularExpressions;
using Google.Protobuf.Protocol;
using UnityEngine;

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance { get { return _instance; } }

    [SerializeField] private BossMonsterFactory _bossMonsterFactory;
    [SerializeField] private NormalMonsterFactory _normalMonsterFactory;
    
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
    }

    public GameObject CreateMonster(MonsterInfo info)
    {
        string monsterName = Regex.Replace(info.Name, @"_[0-9]+$", "");

        GameObject monster = _normalMonsterFactory.CreateMonster(monsterName);
        if (monster == null)
        {
            monster = _bossMonsterFactory.CreateMonster(monsterName);
        }

        // 둘 다 실패하면 null 반환
        if (monster == null)
        {
            Debug.LogWarning($"Monster creation failed: {monsterName}");
            return null;
        }
        
        MonsterController mc = monster.GetComponent<MonsterController>();
        mc.UpdateInfo(info);

        return monster;
    }
}

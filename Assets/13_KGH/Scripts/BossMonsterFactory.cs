using System.Collections.Generic;
using UnityEngine;

public class BossMonsterFactory : MonsterFactory
{
    [SerializeField] private List<StringGameObjectPair> bossMonsterPrefabsList = new List<StringGameObjectPair>();
    private Dictionary<string, GameObject> bossMonsterPrefabsDictionary = new Dictionary<string, GameObject>();
 
    private void Awake()
    {
        // 리스트 → 딕셔너리 변환
        foreach (var pair in bossMonsterPrefabsList)
        {
            if (!bossMonsterPrefabsDictionary.ContainsKey(pair.name))
                bossMonsterPrefabsDictionary.Add(pair.name, pair.prefab);
            else
                Debug.LogWarning($"Duplicate boss monster key: {pair.name}");
        }
    }
    public override GameObject CreateMonster(string monsterName)
    {
        if (!bossMonsterPrefabsDictionary.TryGetValue(monsterName, out GameObject prefab))
        {
            Debug.LogWarning($"Normal monster prefab not found: {monsterName}");
            return null;
        }

        return Instantiate(prefab);
    }
}

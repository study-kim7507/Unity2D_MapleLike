using System.Collections.Generic;
using UnityEngine;

public class NormalMonsterFactory : MonsterFactory
{
    [SerializeField] private List<StringGameObjectPair> normalMonsterPrefabsList = new List<StringGameObjectPair>();
    private Dictionary<string, GameObject> normalMonsterPrefabsDictionary = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // 리스트 → 딕셔너리 변환
        foreach (var pair in normalMonsterPrefabsList)
        {
            if (!normalMonsterPrefabsDictionary.ContainsKey(pair.name))
                normalMonsterPrefabsDictionary.Add(pair.name, pair.prefab);
            else
                Debug.LogWarning($"Duplicate normal monster key: {pair.name}");
        }
    }
    public override GameObject CreateMonster(string monsterName)
    {
        if (!normalMonsterPrefabsDictionary.TryGetValue(monsterName, out GameObject prefab))
        {
            Debug.LogWarning($"Normal monster prefab not found: {monsterName}");
            return null;
        }

        return Instantiate(prefab);
    }
}
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StringGameObjectPair
{
    public string name;
    public GameObject prefab;
}

public class MonsterManager : MonoBehaviour
{
    private static MonsterManager _instance;
    public static MonsterManager Instance { get { return _instance; } }

    public List<StringGameObjectPair> monsterPrefabs = new List<StringGameObjectPair>();

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

    public GameObject GetMonsterPrefab(string name)
    {
        foreach (var pair in monsterPrefabs)
        {
            if (pair.name.Equals(name, System.StringComparison.OrdinalIgnoreCase))
            {
                return pair.prefab;
            }
        }

        Debug.LogWarning("Monster prefab not found for name: " + name);
        return null; // 몬스터 프리팹을 찾지 못한 경우 null 반환
    }
}

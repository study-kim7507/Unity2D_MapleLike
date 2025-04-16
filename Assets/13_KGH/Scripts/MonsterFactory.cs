using UnityEngine;

[System.Serializable]
public class StringGameObjectPair
{
    public string name;
    public GameObject prefab;
}

public abstract class MonsterFactory : MonoBehaviour
{
    public abstract GameObject CreateMonster(string monsterName);
}


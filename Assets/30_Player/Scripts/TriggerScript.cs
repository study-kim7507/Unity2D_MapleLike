using System.Collections.Generic;
using UnityEngine;

public class TriggerScript : MonoBehaviour
{
    public Vector2 player;
    public int TriggerType = 0;
    private List<MonsterController> Monsters;
    public List<PlayerController> Players;

    private void Awake()
    {
        Monsters = new List<MonsterController>();
        Players = new List<PlayerController>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Monster"))
        {
            Monsters.Add(collision.GetComponent<MonsterController>());
        }
        if (collision.gameObject.layer == LayerMask.NameToLayer("MyPlayer") && TriggerType == 1)
        {
            Players.Add(collision.GetComponent<PlayerController>());
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //if (collision.CompareTag("Monster"))
        //{
        //    Monsters.Remove(collision.GetComponent<MonsterController>());
        //}
        //if (collision.gameObject.layer == LayerMask.NameToLayer("MyPlayer") && TriggerType == 1)
        //{
        //    Players.Remove(collision.GetComponent<PlayerController>());
        //}
    }

    /// <summary>
    /// 스킬이 발동됐을 때, 스킬 범위 내에 플레이어로부터 가장 가까운 몬스터를 찾는다.
    /// </summary>
    private MonsterController FindTarget()
    {
        Vector2 playerPos = player;
        if (Monsters.Count == 0)
        {
            return null;
        }
        MonsterController nearestMon = Monsters[0];

        for (int i = 0; i < Monsters.Count; i++)
        {
            if (Vector2.Distance(playerPos, Monsters[i].transform.position) < 
                Vector2.Distance(playerPos, nearestMon.transform.position)) 
            {
                nearestMon = Monsters[i];
            }
        }

        //NormalMonsterController monster = nearestMon.GetComponent<MonsterController>() as NormalMonsterController;
        //monster.SetState(Google.Protobuf.Protocol.MonsterState.MStun);

        return nearestMon;
    }

    public YHSMyPlayerController GetPlayer()
    {
        foreach(PlayerController player in Players)
        {
            if (player is YHSMyPlayerController client)
            {
                return client;
            }
        }
        return null;
    }

    public MonsterController GetTarget()
    {
        return FindTarget();
    }

    public List<MonsterController> GetAllTargets()
    {
        return Monsters;
    }
}

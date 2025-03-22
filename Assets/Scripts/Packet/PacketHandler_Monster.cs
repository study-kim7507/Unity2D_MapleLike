using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

// 몬스터 관련 패킷 핸들러
// 기환님이 작업하시는 곳
public partial class PacketHandler
{
    private int lastHitPlayerId;
    public static void S_MonsterSpawnHandler(PacketSession session, IMessage packet)
    {
        S_MonsterSpawn spawnPacket = packet as S_MonsterSpawn;

        SceneLoadManager.Instance.ExecuteOrQueue(() =>
        {
            foreach (MonsterInfo obj in spawnPacket.MonsterInfos)
            {
                ObjectManager.Instance.AddMonster(obj);
            }
        });
    }

    public static void S_MonsterMoveHandler(PacketSession session, IMessage packet)
    {
        S_MonsterMove movePacket = packet as S_MonsterMove;

        GameObject go = ObjectManager.Instance.FindById(movePacket.MonsterId);
        if (go == null)
            return;

        {
            BaseController bc = go.GetComponent<BaseController>();
            if (bc == null)
                return;

            bc.SetDestination(movePacket.DestinationX, movePacket.DestinationY);
        }

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
        {
            nmc.SetDirection(movePacket.IsRight);
            if (movePacket.State != MonsterState.MDead && movePacket.State != MonsterState.MStun)
                nmc.SetState(movePacket.State);
        }
        
        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();  
        if (bmc != null)
        {
            bmc.SetDirection(movePacket.IsRight);
            if (movePacket.State != MonsterState.MDead && movePacket.State != MonsterState.MStun)
                bmc.SetState(movePacket.State);
        }
    }

    public static void S_MonsterDespawnHandler(PacketSession session, IMessage packet)
    {
        S_MonsterDespawn despawnPacket = packet as S_MonsterDespawn;

        SceneLoadManager.Instance.ExecuteOrQueue(() =>
        {
            foreach (int monsterId in despawnPacket.MonsterIds)
            {
                ObjectManager.Instance.Remove(monsterId);
            }
        });
    }

    public static void S_MonsterSkillHandler(PacketSession session, IMessage packet)
    {
        S_MonsterSkill skillPacket = packet as S_MonsterSkill;

        GameObject go = ObjectManager.Instance.FindById(skillPacket.MonsterId);
        if (go == null)
            return;

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
        {
            nmc.SetNormalMonsterSkillType(skillPacket.SkillType);
            nmc.SetState(MonsterState.MSkill);
        }

        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();
        if (bmc != null)
        {
            bmc.SetBossSkillType(skillPacket.SkillType);
            bmc.SetState(MonsterState.MSkill);
        }
    }

    public static void S_HitMonsterHandler(PacketSession session, IMessage packet)
    {
        S_HitMonster hitPacket = packet as S_HitMonster;

        GameObject go = ObjectManager.Instance.FindById(hitPacket.MonsterId);
        if (go == null)
            return;

        MonsterController mc = go.GetComponent<MonsterController>();
        if (mc != null)
        {
            mc.SetCurrentHp(hitPacket.MonsterCurrentHp);
            mc.lastHitPlayerId = hitPacket.PlayerId;
        }

        // 일반몬스터의 경우
        NormalMonsterController nmc = go.GetComponent<NormalMonsterController>();
        if (nmc != null)
        {
            nmc.SetState(MonsterState.MStun, hitPacket.Damages.Count);
            nmc.UpdateHPBarGauge();
        }
            
        // 보스몬스터의 경우
        BossMonsterController bmc = go.GetComponent<BossMonsterController>();
        if (bmc != null)
        {
            bmc.SetState(MonsterState.MStun, hitPacket.Damages.Count);
            bmc.UpdateHPBarGauge();
        }

        // 데미지 출력
        Transform target = go.transform;
        SpawnManager.Instance.SpawnDamage(hitPacket.Damages, target, 0);
        if (hitPacket.PlayerId != PlayerInformation.playerInfo.PlayerId)
        {
            Transform player = ObjectManager.Instance.FindById(hitPacket.PlayerId).transform;
            GameObject hitEffect = player.GetComponentInChildren<BaseClass>().HitObject;
            GameObject hitEffectObject = SpawnManager.Instantiate(hitEffect, target.GetComponent<BoxCollider2D>().bounds.center, Quaternion.identity);
            hitEffectObject.GetComponent<SpriteRenderer>().flipX = (player.position.x > target.position.x);
            hitEffectObject.GetComponent<Animator>().SetTrigger("Hit");
            SpawnManager.Destroy(hitEffectObject, 0.45f);
        }
    }

    public static void S_BossRegisterDenyHandler(PacketSession session, IMessage packet)
    {
        S_BossRegisterDeny bossRegisterDenyPacket = packet as S_BossRegisterDeny;

        // 플레이어 하위에 부착된 보스 UI 오브젝트의 EnterFailedUI active
        if (BossRoomEnterUI.Instance != null)
            BossRoomEnterUI.Instance.ActiveEnterFaileUIdPanel();
    }

    public static void S_BossWaitingHandler(PacketSession session, IMessage packet)
    {
        S_BossWaiting bossWatingPacket = packet as S_BossWaiting;
        int currentWaitingCount = bossWatingPacket.WaitingCount;

        // 플레이어 하위에 부착된 보스 UI 오브젝트의 PartyMatchingUI Update
        if (BossRoomEnterUI.Instance != null)
            BossRoomEnterUI.Instance.UpdatePartyMatchingUIPanel(currentWaitingCount);
    }

    public static void S_GameClearHandler(PacketSession session, IMessage packet)
    {
        S_GameClear gameClearPacket = packet as S_GameClear;

        BossClearAndExitUI.Instance.BossClear();
    }
}

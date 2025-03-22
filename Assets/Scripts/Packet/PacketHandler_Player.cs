using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

// 플레이어 관련 패킷 핸들러
// 아마 기태님 현승님 작업하실 곳.
// 충돌이 우려되면 또 파셜로 나누면 됩니다.
public partial class PacketHandler
{
    public static void S_EnterGameHandler(PacketSession session, IMessage packet)
    {
        S_EnterGame enterPacket = packet as S_EnterGame;

        // 씬 로드
        //string temp = $"12_YHS/1_SnailPark";
        //SceneLoadManager.Instance.LoadScene(temp);
        string sceneName = ((MapName)enterPacket.MapId).ToString();
        SceneLoadManager.Instance.LoadScene(sceneName);

        SceneLoadManager.Instance.ExecuteOrQueue(() =>
        {
            ObjectManager.Instance.AddPlayer(enterPacket.PlayerInfo, myPlayer: true);
        });
    }

    /// <summary>
    /// 내가 위치한 맵에 다른 플레이어가 생성될 때 처리하는 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_PlayerSpawnHandler(PacketSession session, IMessage packet)
    {
        S_PlayerSpawn spawnPacket = packet as S_PlayerSpawn;
        foreach (PlayerInfo obj in spawnPacket.PlayerInfos)
        {
            SceneLoadManager.Instance.ExecuteOrQueue(() =>
            {
                ObjectManager.Instance.AddPlayer(obj, myPlayer: false);
            });
        }
    }

    /// <summary>
    /// 나를 제외한 다른 플레이어 이동 및 애니메이션 동기화 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet)
    {
        S_PlayerMove movePacket = packet as S_PlayerMove;

        GameObject go = ObjectManager.Instance.FindById(movePacket.PlayerId);
        if (go == null)
            return;

        // 내 플레이어는 클라이언트에서 자체 처리하므로 동기화 제외
        YHSMyPlayerController mpc = go.GetComponent<YHSMyPlayerController>();
        if (mpc != null)
            return;

        PlayerController pc = go.GetComponent<PlayerController>();
        if (pc == null)
            return;

        pc.SetDestination(movePacket.PositionX, movePacket.PositionY);
        pc.SetPlayerDirection(movePacket.IsRight);
        pc.SetPlayerState(movePacket.State);
    }

    /// <summary>
    /// 내가 위치한 맵에서 다른 플레이어가 사라졌을 때 처리하는 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_PlayerDespawnHandler(PacketSession session, IMessage packet)
    {
        S_PlayerDespawn despawnPacket = packet as S_PlayerDespawn;
        foreach (int id in despawnPacket.PlayerIds)
        {
            ObjectManager.Instance.Remove(id);
            GameObject.Destroy(GameObject.Find(id.ToString()));     // 생성할 때 오브젝트명이 플레이어ID이기 때문에 가능한 로직이다.
        }
    }

    public static void S_PlayerSkillHandler(PacketSession session, IMessage packet)
    {
        // 현재 S_PlayerMoveHandler()와 기능 통합
    }

    /// <summary>
    /// 플레이어들의 피격 데미지스킨 동기화를 위한 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_PlayerDamagedHandler(PacketSession session, IMessage packet)
    {
        // 플레이어의 Hit(Stun) 상태로의 전환은 S_PlayerMoveHandler()와 기능 통합, 나중에 분리 가능할지도?
        // 패킷에서 damage를 수신하므로 파티원간 현재 체력 공유 구현도 가능하다.
        S_PlayerDamaged playerDamagedPacket = packet as S_PlayerDamaged;

        GameObject targetObject = ObjectManager.Instance.FindById(playerDamagedPacket.PlayerId);

        if (targetObject == null)
        {
            return;
        }

        SpawnManager.Instance.SpawnDamage(
            new List<int>() { playerDamagedPacket.Damage },
            targetObject.transform,
            1
            );
    }

    public static void S_PlayerDieHandler(PacketSession session, IMessage packet)
    {
        // 현재 S_PlayerMoveHandler()와 기능 통합
    }

    /// <summary>
    /// 자신이 위치한 맵에서 자신이 나갔을 때 처리하는 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet)
    {
        S_LeaveGame leavePacket = packet as S_LeaveGame;
        ObjectManager.Instance.Clear();

        // C_ChangeMap을 다시 송신하거나 메인 화면으로 넘어가도록 해야?
    }

    /// <summary>
    /// 자신이 몬스터를 처치하고 경험치를 획득했을 때 처리하는 메서드
    /// </summary>
    /// <param name="session"></param>
    /// <param name="packet"></param>
    public static void S_GetExpHandler(PacketSession session, IMessage packet)
    {
        S_GetExp getExpPacket = packet as S_GetExp;

        if (getExpPacket.PlayerIds != ObjectManager.Instance.MyPlayer.Id)
        {
            return;
        }

        GameObject go = ObjectManager.Instance.FindById(getExpPacket.PlayerIds);

        PlayerInformation playerInformation = go.GetComponent<PlayerInformation>();
        playerInformation.SetPlayerExp(getExpPacket.Exp);
    }
}

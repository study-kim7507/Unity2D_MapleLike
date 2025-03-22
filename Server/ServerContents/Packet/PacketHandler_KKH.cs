using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Room;
using ServerContents.Session;
using ServerCore;
using System.Collections.Generic;
using System.Numerics;

public partial class PacketHandler
{
    public static void C_HitMonsterHandler(PacketSession session, IMessage packet)
    {
        C_HitMonster hitPacket = packet as C_HitMonster;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        List<int> damageAmounts = new List<int>();
        foreach (int playerAttackPower in hitPacket.PlayerAttackPowers)
            damageAmounts.Add(playerAttackPower);
        room.Push(room.MonsterHitAndSetTarget, player, hitPacket.MonsterId, damageAmounts);
    }
    public static void C_BossRegisterHandler(PacketSession session, IMessage packet)
    {
        C_BossRegister bossRegisterPacket = packet as C_BossRegister;  
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null) 
            return;

        // 보스 웨이팅 룸이 될 듯.
        GameRoom currentRoom = player.Room;
        if (currentRoom == null)
            return;

        GameRoom bossRoom = RoomManager.Instance.Find((int)MapName.BossRoom);
        if (bossRoom == null)
            return;

        // 현재 레이드를 진행 중인 파티가 없으며, 새로운 플레이어가 솔로 입장을 원하는 경우
        // 더하여, 다른 웨이팅 중인 플레이어가 없다는 경우도 포함
        if (bossRegisterPacket.BossEnterType == BossEnterType.Single &&
            bossRoom.GetPlayerCountInRoom() == 0 &&
            currentRoom._bossRoomWaitingPlayers.Count() == 0)
        {
            // MapChange 패킷을 보스 웨이팅 룸에 뿌리고, 해당 플레이어를 보스 룸으로 이동 시킴
            C_ChangeMap changeMapPacket = new C_ChangeMap();
            changeMapPacket.MapId = (int)MapName.BossRoom;

            currentRoom.Push(currentRoom.HandleChangeMap, player, changeMapPacket);
        }

        // 현재 레이드를 진행 중인 파티가 없으며, 새로운 플레이어가 파티 입장을 원하는 경우
        // 더하여, 다른 웨이팅 중인 플레이어가 없다는 경우도 포함
        else if (bossRegisterPacket.BossEnterType == BossEnterType.Multi &&
            bossRoom.GetPlayerCountInRoom() == 0 &&
            currentRoom._bossRoomWaitingPlayers.Count() <= 4)
        {
            if (!currentRoom._bossRoomWaitingPlayers.ContainsKey(player.Id))
                currentRoom._bossRoomWaitingPlayers.Add(player.Id, player);
            
            // 새로운 플레이어가 웨이팅에 참여 후, 입장 가능하게 되면 입장 시킴
            if (currentRoom._bossRoomWaitingPlayers.Count() == 5)
            {
                foreach (var waitingPlayer in currentRoom._bossRoomWaitingPlayers)
                {
                    C_ChangeMap changeMapPacket = new C_ChangeMap();
                    changeMapPacket.MapId = (int)MapName.BossRoom;

                    currentRoom.Push(currentRoom.HandleChangeMap, waitingPlayer.Value, changeMapPacket);
                }
            }
            else
            {
                currentRoom.Push(currentRoom.HandleBossWaiting);
            }
        }
        else
        { 
            currentRoom.Push(currentRoom.HandleBossEnterDenied, player);
        }
    }

    public static void C_BossCancleHandler(PacketSession session, IMessage packet)
    {
        C_BossCancle bossCanclePacket = packet as C_BossCancle;
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        // 보스 웨이팅 룸이 될 듯.
        GameRoom currentRoom = player.Room;
        if (currentRoom == null)
            return;

        // 키가 존재할 때만 제거
        if (currentRoom._bossRoomWaitingPlayers.ContainsKey(player.Id))
        {
            currentRoom._bossRoomWaitingPlayers.Remove(player.Id);
        }
        currentRoom.Push(currentRoom.HandleBossWaiting);
    }
}
using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerCore;

class PacketHandler
{
    public static void S_ConnectedHandler(PacketSession session, IMessage packet) { }
    public static void S_EnterGameHandler(PacketSession session, IMessage packet) { }
    public static void S_PlayerSpawnHandler(PacketSession session, IMessage packet) { }
    public static void S_MonsterSpawnHandler(PacketSession session, IMessage packet) { }
    public static void S_PlayerMoveHandler(PacketSession session, IMessage packet) { }
    public static void S_MonsterMoveHandler(PacketSession session, IMessage packet) { }
    public static void S_LeaveGameHandler(PacketSession session, IMessage packet) { }
    public static void S_PlayerDespawnHandler(PacketSession session, IMessage packet) { }
    public static void S_MonsterDespawnHandler(PacketSession session, IMessage packet) { }
    public static void S_ItemSpawnHandler(PacketSession session, IMessage packet) { }
    public static void S_PlayerSkillHandler(PacketSession session, IMessage packet) { }
    public static void S_MonsterSkillHandler(PacketSession session, IMessage packet) { }
    public static void S_HitMonsterHandler(PacketSession session, IMessage packet) { }
    public static void S_PlayerDamagedHandler(PacketSession session, IMessage packet) { }
    public static void S_BossRegisterDenyHandler(PacketSession session, IMessage packet) { }
    public static void S_BossWaitingHandler(PacketSession session, IMessage packet) { }
    public static void S_GameClearHandler(PacketSession session, IMessage packet) { }
    public static void S_GetExpHandler(PacketSession session, IMessage packet) { }
    public static void S_LootItemHandler(PacketSession session, IMessage packet) { }
    public static void S_ItemDespawnHandler(PacketSession session, IMessage packet) { }
}
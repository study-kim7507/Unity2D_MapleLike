using System;
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
    public static void C_LootItemHandler(PacketSession session, IMessage packet)
    {
        C_LootItem pkt = packet as C_LootItem;
        if (pkt !=null)
        {
            Console.WriteLine("Loot Item");
            Console.WriteLine(pkt.ItemId);
        }
        
        ClientSession clientSession = session as ClientSession;

        Player player = clientSession.MyPlayer;
        if (player == null)
            return;

        GameRoom room = player.Room;
        if (room == null)
            return;

        room.Push(room.ItemRooting, player, pkt.ItemId);
    }
}
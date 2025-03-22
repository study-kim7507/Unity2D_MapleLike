using System.Net;
using System;
using UnityEngine;
using ServerCore;
using Google.Protobuf.Protocol;
using Google.Protobuf;

public class ServerSession : PacketSession
{
    public void Send(IMessage packet)
    {
        string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
        MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
        ushort size = (ushort)packet.CalculateSize();
        byte[] sendBuffer = new byte[size + 4]; // 4 : 헤더(2 바이트) + 패킷종류(2 바이트)
        Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort)); // ushort : 헤더(2 바이트)
        Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));      // ushort : 패킷종류(2 바이트)
        Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);                                // size : 헤더 패킷을 제외한 패킷 데이터 크기 
        Send(new ArraySegment<byte>(sendBuffer));
    }

    public override void OnConnected(EndPoint endPoint)
    {
        Console.WriteLine($"client {endPoint} is connected to the server. Here is Client");

        // 서버와 동시에 커스텀 핸들러를 초기화 해줌으로써
        // 유니티 메인쓰레드에서의 패킷 처리가 가능하게 유도
        PacketManager.Instance.CustomHandler = (s, m, i) =>
        {
            // CustomHandler는 Action 이므로
            // 지금 당장 Push를 하는게 아니고
            // Push 작업을 Action으로 등록하는 것임에 유의
            PacketQueue.Instance.Push(i, m);
        };
    }

    public override void OnDisconnected(EndPoint endPoint)
    {
        Console.WriteLine($"client {endPoint} is disconnected from the server. Here is Client");
    }

    public override void OnRecvPacket(ArraySegment<byte> buffer)
    {
        Console.WriteLine("Receive message from server");
        PacketManager.Instance.OnRecvPacket(this, buffer);
    }

    public override void OnSend(int numOfBytes)
    {
        Console.WriteLine("Send message to server");
    }
}

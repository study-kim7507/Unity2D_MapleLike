using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Room;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Session
{
    public class ClientSession : PacketSession
    {
        public Player MyPlayer { get; set; }

        public int SessionId { get; set; }

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

            //Console.WriteLine($"Send {msgName} message to client");
        }

        // 패킷 모아보내기 - 전송을 위해 IMeesage 타입을 ArraySegment 타입으로 변환
        // 로직상 스레드세이프
        public void Send(List<IMessage> packetList)
        {
            if (packetList.Count == 0)
                return;

            List<ArraySegment<byte>> _packetList = new List<ArraySegment<byte>>();

            foreach (IMessage packet in packetList)
            {
                string msgName = packet.Descriptor.Name.Replace("_", string.Empty);
                MsgId msgId = (MsgId)Enum.Parse(typeof(MsgId), msgName);
                ushort size = (ushort)packet.CalculateSize();
                byte[] sendBuffer = new byte[size + 4];                                                  // 4 : 헤더(2 바이트) + 패킷종류(2 바이트)
                Array.Copy(BitConverter.GetBytes((ushort)(size + 4)), 0, sendBuffer, 0, sizeof(ushort)); // ushort : 헤더(2 바이트)
                Array.Copy(BitConverter.GetBytes((ushort)msgId), 0, sendBuffer, 2, sizeof(ushort));      // ushort : 패킷종류(2 바이트)
                Array.Copy(packet.ToByteArray(), 0, sendBuffer, 4, size);                                // size : 헤더 패킷을 제외한 패킷 데이터 크기 
                _packetList.Add(sendBuffer);
            }
            Send(_packetList);
        }

        public override void OnConnected(EndPoint endPoint)
        {
            Console.WriteLine( $"client {endPoint} is connected to the server. Here is server" );

            S_Connected connectedPkt = new S_Connected();
            Send(connectedPkt);

            //MyPlayer = ObjectManager.Instance.Add<Player>();
            //{
            //    MyPlayer.Info.Name = $"Player_{MyPlayer.Info.PlayerId}";
            //    MyPlayer.Session = this;
            //}

            //Console.WriteLine($"{endPoint} Object Added in Dic, Send Enter packet To Server...");

            //GameRoom room = RoomManager.Instance.Find(1);
            //if(room ==null)
            //{
            //    Console.WriteLine( $"N번 방이 존재하지 않습니다. 클라이언트 접속 종료" );
            //    Disconnect();
            //    return;
            //}

            //room.Push(room.PlayerEnterGame, MyPlayer, 0);
        }

        public override void OnDisconnected(EndPoint endPoint)
        {
            Console.WriteLine($"client {endPoint} is disconnected from the server. Here is server");

            if(MyPlayer != null)
            {                
                GameRoom room = RoomManager.Instance.Find(MyPlayer.Room.RoomId);
                if (room == null)
                {
                    Console.WriteLine( $"\"{MyPlayer.Room.RoomId}\"룸을 찾지 못했습니다. 로직 에러 (고의적 크래쉬..)" );
                }
                room.Push(room.LeavePlayer, MyPlayer.Info.PlayerId);
            }

            SessionManager.Instance.Remove(this);
        }

        public override void OnRecvPacket(ArraySegment<byte> buffer)
        {
            //Console.WriteLine("Receive message from client");
            PacketManager.Instance.OnRecvPacket(this, buffer);
        }

        public override void OnSend(int numOfBytes)
        {
            //Console.WriteLine("Send message to client");
        }
    }
}

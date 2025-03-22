using System;
using System.Collections.Generic;
using Google.Protobuf.Protocol;
using ServerContents.Room;
using ServerContents.Session;
using ServerCore;
using System.Net;
using System.Threading;

namespace ServerContents
{
    class Program
    {
        static Listener _listener = new Listener();
        static List<GameRoom> rooms = new List<GameRoom>();

        static void Main(string[] args)
        {
            foreach (MapName map in Enum.GetValues(typeof(MapName)))
            {
                rooms.Add(RoomManager.Instance.Add((int)map));
            }

            Console.WriteLine("Server Start!");
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            string ipAddressString = "127.0.0.1";
            IPAddress ipAddr = IPAddress.Parse(ipAddressString);
            //IPAddress ipAddr = ipHost.AddressList[0]; 
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // Func.Invoke() (Listener의 _sessionFactory.Invoke();) 에 의해 SessionManager.Instance.Generate() 가 N번 생성됨
            // Delegate인 SessionManager.Instance.Generate()는 여기서 당장 실행 되진 않고 인자로써 넘겨준다.
            _listener.Init(endPoint, () => { return SessionManager.Instance.Generate(); });

            Console.WriteLine($"This Server IP is {endPoint.Address.ToString()}");
            Console.WriteLine("Server Listening...");

            try
            {
                while (true)
                {
                    // TODO. 방을 나누고 각각의 스레드로 배분한다.
                    foreach (var room in rooms)
                    {
                        room.Push(room.Flush);
                    }

                    Thread.Sleep(1);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"예외 발생: {ex.Message}");
                Console.ReadKey();
            }
        }
    }
}

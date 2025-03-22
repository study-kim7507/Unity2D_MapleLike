using DummyClient.Session;
using ServerCore;
using System.Net;

namespace DummyClient
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Connector _connector = new Connector();
            ServerSession _session = new ServerSession();

            // DNS (Domain Name System)
            string host = Dns.GetHostName();
            IPHostEntry ipHost = Dns.GetHostEntry(host);
            string ipAddressString = "127.0.0.1";
            IPAddress ipAddr = IPAddress.Parse(ipAddressString);
            //IPAddress ipAddr = ipHost.AddressList[0];
            IPEndPoint endPoint = new IPEndPoint(ipAddr, 7777);

            // Func.Invoke() (Connector의 _sessionFactory.Invoke();) 에 의해 SessionManager.Instance.Generate() 가 N번 생성됨
            // Delegate인 SessionManager.Instance.Generate()는 여기서 당장 실행 되진 않고 인자로써 넘겨준다.
            _connector.Connect(endPoint, () => {return SessionManager.Instance.Generate(); }, 100); // 더미 클라이언트 N개 접속

            while (true)
            {
                SessionManager.Instance.SendForEach();
                Thread.Sleep(10);  // 250 : 4프레임, 100 : 약 10프레임,  33 : 약 30프레임
            }
        }
    }
}

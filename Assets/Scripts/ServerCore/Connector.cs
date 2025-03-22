using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ServerCore
{
    public class Connector
    {
        Func<Session> _sessionFactory;

        // count : count개의 더미클라이언트(부하 테스트용)
        public void Connect(IPEndPoint endPoint, Func<Session> sessionFactory, int count = 1)
        {
            for (int i = 0; i < count; i++)
            {
                Socket socket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                // Func는 델리게이트 이므로, sessionFactory 라는 것을 추가 
                // 나중에 _sessionFactory.Invoke()따위로 +=된 함수들을 순차적 호출 
                _sessionFactory = sessionFactory;

                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                // SocketAsyncEventArgs.Completed는 비동기 작업의 완료를 알리는 이벤트
                args.Completed += OnConnectCompleted;
                args.RemoteEndPoint = endPoint;
                // socket을 RegisterConnect() 인자로 넣어주기 위해 UserToken 사용
                // Listener.cs와 다르게 Socket을 전역변수로 받지 않는 이유는
                // Listener는 유일하지만 Connector는 여러 클라이언트와 연결을 처리하기 때문에
                // 인자로 넘겨줌으로써 개별 세션에서 관리됨
                args.UserToken = socket; 

                RegisterConnect(args);
            }
        }

        void RegisterConnect(SocketAsyncEventArgs args)
        {
            Socket socket = args.UserToken as Socket;
            if (socket == null)
                return;

            bool pending = socket.ConnectAsync(args);
            // true : 보류 / false : 동기적으로 완료(바로 완료된 경우)
            // 동기적으로 바로 완료되었으면 OnAcceptCompleted()를 즉시 호출 
            if (pending == false)
                OnConnectCompleted(null, args);
        }

        void OnConnectCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.ConnectSocket);
                session.OnConnected(args.RemoteEndPoint);
            }
            else
            {
                Console.WriteLine($"OnConnectCompleted Fail: {args.SocketError}");
            }
        }
    }
}

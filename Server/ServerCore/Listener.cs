using System.Net.Sockets;
using System.Net;

namespace ServerCore
{
    public class Listener
    {
        Socket _listenSocket;
        Func<Session> _sessionFactory;

        public void Init(IPEndPoint endPoint, Func<Session> sessionFactory, int register = 5, int backlog = 100)
        {
            _listenSocket = new Socket(endPoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            // Func는 델리게이트 이므로, sessionFactory 라는 것을 추가 
            // 나중에 _sessionFactory.Invoke()따위로 +=된 함수들을 순차적 호출 
            _sessionFactory += sessionFactory;

            _listenSocket.Bind(endPoint);           
            _listenSocket.Listen(backlog); // backlog : 최대 대기수

            // 최대 register개에서 Connect 요청 처리
            for (int i = 0; i < register; i++)
            {
                SocketAsyncEventArgs args = new SocketAsyncEventArgs();
                // SocketAsyncEventArgs.Completed는 비동기 작업의 완료를 알리는 이벤트
                args.Completed += new EventHandler<SocketAsyncEventArgs>(OnAcceptCompleted); 
                RegisterAccept(args);
            }
        }

        void RegisterAccept(SocketAsyncEventArgs args)
        {
            // 남아 있는 기존 연결을 초기화
            args.AcceptSocket = null;

            bool pending = _listenSocket.AcceptAsync(args);
            // true : 보류 / false : 동기적으로 완료(바로 완료된 경우)
            // 동기적으로 바로 완료되었으면 OnAcceptCompleted()를 즉시 호출 
            if (pending == false)
                OnAcceptCompleted(null, args);
        }

        // 호출되는 경우는 두 가지
        // 1. _listenSocket.AcceptAsync(args)가 true일 때 나중에 args.Completed로 등록된 이벤트로 비동기적으로 호출
        // 2. _listenSocket.AcceptAsync(args)가 false(동기적으로 완료)면 직접 호출
        void OnAcceptCompleted(object sender, SocketAsyncEventArgs args)
        {
            if (args.SocketError == SocketError.Success)
            {
                Session session = _sessionFactory.Invoke();
                session.Start(args.AcceptSocket);
                session.OnConnected(args.AcceptSocket.RemoteEndPoint);
            }
            else
                Console.WriteLine(args.SocketError.ToString());

            RegisterAccept(args);
        }
    }
}

using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace DummyClient.Session
{
    // Sessionmanager 클래스는 더미클라이언트에만 있는 개념으로,
    // 실제 클라이언트(유니티)에서는 단일 클라이언트 세션이 서버에 매칭되기 때문에 유니티에선 해당 클래스 파일이 없음.
    class SessionManager
    {
        static SessionManager _session = new SessionManager();
        public static SessionManager Instance { get { return _session; } }

        int _sessionId = 0; // TODO. 나중에 삭제하거나 서버로부터 아이디 부여받을 것 
        // 서버와 연결된 모든 세션을 딕셔너리로 관리
        Dictionary<int, ServerSession> _sessions = new Dictionary<int, ServerSession>();
        object _lock = new object();

        // 생성된 세션에 아이디를 부여하고 관리 대상 딕셔너리에 넣음
        public ServerSession Generate()
        {
            lock (_lock)
            {
                int sessionId = ++_sessionId;

                ServerSession session = new ServerSession();
                _sessions.Add(sessionId, session);

                Console.WriteLine($"Client ID {sessionId} is Connected to ###. Here is Client");

                return session;
            }
        }


        // 더미클라이언트 부하 테스트용 브로드캐스트
        public void SendForEach()
        {
            lock (_lock)
            {
                foreach (var session in _sessions)
                {
                    C_PlayerMove dummyMovePacket = new C_PlayerMove();
                    session.Value.Send(dummyMovePacket);
                }
            }
        }
    }
}

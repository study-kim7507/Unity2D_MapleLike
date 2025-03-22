using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    // 방을 관리한다.
    public class RoomManager
    {
        public static RoomManager Instance { get; } = new RoomManager();

        object _lock = new object();
        // 모든 생성된 방을 딕셔너리로 관리
        Dictionary<int, GameRoom> _rooms = new Dictionary<int, GameRoom>();

        // 생성된 방에 아이디를 부여하고 관리 대상 딕셔너리에 넣음
        public GameRoom Add(int roomId)
        {
            GameRoom gameRoom = new GameRoom();
            gameRoom.Push(gameRoom.Init);

            lock (_lock)
            {
                gameRoom.RoomId = roomId;
                _rooms.Add(roomId, gameRoom);
            }

            return gameRoom;
        }

        // 관리 대상에서(딕셔너리) 삭제
        public bool Remove(int roomId)
        {
            lock (_lock)
            {
                return _rooms.Remove(roomId);
            }
        }

        // 관리 대상에서(딕셔너리) 찾기
        public GameRoom Find(int roomId)
        {
            lock (_lock)
            {
                GameRoom room = null;
                if (_rooms.TryGetValue(roomId, out room))
                    return room;

                return null;
            }
        }
    }
}
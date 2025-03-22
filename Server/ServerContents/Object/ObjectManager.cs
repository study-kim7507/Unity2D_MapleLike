using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    public class ObjectManager
    {
        public static ObjectManager Instance { get; } = new ObjectManager();

        object _lock = new object();

        // 서버와 연결된 모든 오브젝트를 딕셔너리로 관리 (여러 GameRoom의 객체를 총괄)
        int _id = 0;
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, NormalMonster> _normalMonsters = new Dictionary<int, NormalMonster>();
        Dictionary<int, BossMonster> _bossMonsters = new Dictionary<int, BossMonster>();
        Dictionary<int, Item> _items = new Dictionary<int, Item>(); 
        // Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>(); 후순위

        // 생성된 오브젝트에 아이디를 부여하고 관리 대상 딕셔너리에 넣음
        public T Add<T>() where T : GameObject, new()
        {
            T gameObject = new T();

            lock (_lock)
            {
                gameObject.Id = GenerateId(gameObject.ObjectType);

                if (gameObject.ObjectType == GameObjectType.Player)
                {
                    _players.Add(gameObject.Id, gameObject as Player);
                }
                else if (gameObject.ObjectType == GameObjectType.Normalmonster)
                {
                    _normalMonsters.Add(gameObject.Id, gameObject as NormalMonster);
                }
                else if (gameObject.ObjectType == GameObjectType.Bossmonster)
                {
                    _bossMonsters.Add(gameObject.Id, gameObject as BossMonster);
                }
                else if (gameObject.ObjectType == GameObjectType.Item)
                {
                    _items.Add(gameObject.Id, gameObject as Item);
                }
            }

            return gameObject;
        }

        int GenerateId(GameObjectType type)
        {
            lock (_lock)
            {
                return ((int)type << 24) | (_id++);
            }
        }

        // 관리 대상에서(딕셔너리) 삭제
        public bool Remove(int objectId)
        {
            lock (_lock)
            {
                return _players.Remove(objectId);
            }
        }

        public static GameObjectType GetObjectTypeById(int id)
        {
            int type = (id >> 24) & 0x7F;
            return (GameObjectType)type;
        }

        // 관리 대상에서(딕셔너리) 찾기
		public Player Find(int objectId)
		{
			GameObjectType objectType = GetObjectTypeById(objectId);

			lock (_lock)
			{
				if (objectType == GameObjectType.Player)
				{
					Player player = null;
					if (_players.TryGetValue(objectId, out player))
						return player;
				}
			}

			return null;
		}
    }
}

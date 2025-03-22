using Google.Protobuf.Protocol;
using ServerContents.Room;
using ServerContents.Session;

namespace ServerContents.Object
{
    // GameObject : 서버와 관련된 모든 게임 오브젝트
    // ex. 플레이어, 몬스터, 아이템 등 동기화가 필요한 오브젝트
    public class GameObject
    {
        public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;
        public int Id
        {
            get
            {
                if (this is Player player) return player.Info.PlayerId;
                if (this is Monster monster) return monster.Info.MonsterId;
                if (this is Item item) return item.Info.ItemId;
                return 0;
            }
            set
            {
                if (this is Player player) player.Info.PlayerId = value;
                else if (this is Monster monster) monster.Info.MonsterId = value;
                else if (this is Item item) item.Info.ItemId = value;
            }
        }

        public GameRoom Room { get; set; }

        public virtual void Update()
        {

        }
    }
}

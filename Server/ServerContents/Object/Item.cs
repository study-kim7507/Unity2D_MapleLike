using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Object
{
    class Item : GameObject
    {
        public ItemInfo Info { get; set; } = new ItemInfo(); // TODO 현재는 더미지만 추후에는 서버가 아이템의 정보를 갖고 있게 한다.

        public Item()
        {
            ObjectType = GameObjectType.Item;
        }
    }
}

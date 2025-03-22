using Google.Protobuf.Protocol;
using ServerContents.Object;
using ServerContents.Session;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerContents.Object
{
    public class Player : GameObject
    {
        public ClientSession Session { get; set; }
        public PlayerInfo Info { get; set; } = new PlayerInfo();
        public PlayerStatInfo Stat { get; private set; } = new PlayerStatInfo();

        public float Speed
        {
            get { return Stat.Speed; }
            set { Stat.Speed = value; }
        }

        public int Hp
        {
            get { return Stat.Hp; }
            set { Stat.Hp = Math.Clamp(value, 0, Stat.MaxHp); }
        }

        public Player()
        {
            ObjectType = GameObjectType.Player;
            Info.StatInfo = Stat;
        }
    }
}

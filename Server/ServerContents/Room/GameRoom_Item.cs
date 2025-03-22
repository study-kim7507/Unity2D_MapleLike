using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using System;


namespace ServerContents.Room
{
    // 아이템과 관련된 로직을 관리한다.
    // 경원님 작업하는 곳.
    public partial class GameRoom : JobSerializer
    {
        static Random random = new Random();
        private Dictionary<ItemType, double> dropRates;

        public void ItemEnterGame(Player player, float posX, float posY)
        {
            lock (_lock)
            {
                if (player == null)
                    return;

                Item item = ObjectManager.Instance.Add<Item>();
                _items.Add(item.Id, item);
                item.Room = this;

                // 아이템이 게임룸에 입장한 사실을 모든 클라이언트에게 전송
                S_ItemSpawn spawnPacket = new S_ItemSpawn();
                spawnPacket.ItemType = GetRandomItem();
                Console.WriteLine(spawnPacket.ItemType);
                spawnPacket.CanOnlyOwnerLootTime = 10000; // 10초
                spawnPacket.LifeTime = 60000; // 30초
                spawnPacket.ItemInfo = item.Info;
                spawnPacket.ItemInfo.OwnerId = player.Id;
                spawnPacket.ItemInfo.PositionX = posX;
                spawnPacket.ItemInfo.PositionY = posY;

                Broadcast(spawnPacket);

                // 아이템 N초 후에 사라질 것을 예약함
                S_ItemDespawn deSpawnPacket = new S_ItemDespawn();
                deSpawnPacket.ItemId = item.Id;
                PushAfter(spawnPacket.CanOnlyOwnerLootTime, SetItemRootAnyOne, item.Id); // ex) 15초 후
                PushAfter(spawnPacket.LifeTime, LeaveItem, deSpawnPacket.ItemId); // ex) 30초 후
            }
        }

        // 아이템을 관리 대상에서(딕셔너리) 삭제
        // 경우 1. 최대 N초가 지나 맵에저 자연스럽게 사라질 때
        public void LeaveItem(int itemId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(itemId);

            Item item = null;

            // 이미 이 맵에 해당 아이템이 없다면 함수 종료
            if (_items.Remove(itemId, out item) == false)
                return;

            item.Room = null;

            // 아이템이 게임룸에서 퇴장한 사실을 모든 클라이언트에게 전송
            {
                S_ItemDespawn deSpawnPacket = new S_ItemDespawn();
                // TODO 아이템에 관한 정보 설정
                deSpawnPacket.ItemId = itemId;
                Broadcast(deSpawnPacket);
            }
        }

        public void SetItemRootAnyOne(int itemId)
        {
            Item item = null;

            // 이미 이 맵에 해당 아이템이 없다면 함수 종료
            if (_items.TryGetValue(itemId, out item) == false)
                return;

            item.Info.CanRootAnyOne = true;
        }

        public void ItemRooting(Player player, int itemId)
        {
            if (player == null) return;

            Item item = null;
            if (_items.TryGetValue(itemId, out item) == false)
                return;

            // 모두에게 소유권이 있거나, 소유권이 있는 사람이 아이템 루팅을 요청할 때
            if ((item.Info.CanRootAnyOne) || (!item.Info.CanRootAnyOne && item.Info.OwnerId == player.Id))
            {
                if (_items.Remove(itemId, out item) == false)
                {
                    return;
                }

                item.Room = null;

                S_LootItem pkt = new S_LootItem();
                pkt.ItemId = itemId;
                pkt.PlayerId = player.Id;
                Broadcast(pkt);
            }
        }

        void SetDropItemRates()
        {
            dropRates = new Dictionary<ItemType, double>()
            {
                { ItemType.Gold, 100.0 },

                { ItemType.Hppotion1, 5.0 },
                { ItemType.Hppotion2, 2.5 },
                { ItemType.Mppotion1, 5.0 },
                { ItemType.Mppotion2, 2.5 },
                { ItemType.Superpotion1, 2.0 },
                { ItemType.Superpotion2, 1.0 },

                { ItemType.Helmet1, 0.2 },
                { ItemType.Helmet2, 0.1 },
                { ItemType.Armor1, 0.2 },
                { ItemType.Armor2, 0.1 },
                { ItemType.Boots1, 0.2 },
                { ItemType.Boots2, 0.1 },

                { ItemType.Sword1, 0.2 },
                { ItemType.Sword2, 0.1 },
                { ItemType.Sword3, 0.05 },
                { ItemType.Staff1, 0.2 },
                { ItemType.Staff2, 0.1 },
                { ItemType.Staff3, 0.05 },
                { ItemType.Arrow1, 0.2 },
                { ItemType.Arrow2, 0.1 },
                { ItemType.Arrow3, 0.05 }
            };

            double totalBaseRate = dropRates.Values.Sum();

            dropRates = dropRates.ToDictionary(
                kvp => kvp.Key,
                kvp => (kvp.Value / totalBaseRate) * 100.0 // 정규화하여 총합이 100이 되도록 조정
            );

            CheckItemDropRates();

        }

        void CheckItemDropRates()
        {
            double sum = 0;
            foreach (var p in dropRates)
            {
                sum += p.Value;
            }
            Console.WriteLine($"Item Rrop Rates sum (After normalization) : {sum:F3}");
        }

        public ItemType GetRandomItem()
        {
            double roll = random.NextDouble() * 100; // 0.0 ~ 99.9999...

            double cumulative = 0.0;

            foreach (var kvp in dropRates)
            {
                cumulative += kvp.Value;
                if (roll < cumulative)
                    return kvp.Key;
            }

            return 0; // Dafult Gold (이론상 도달하지 않음)
        }
    }
}

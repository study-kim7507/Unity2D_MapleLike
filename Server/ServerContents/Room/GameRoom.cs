using Google.Protobuf;
using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using ServerContents.Session;
using ServerCore;
using System;
using System.Collections.Generic;
using System.Threading;
using static System.Runtime.InteropServices.JavaScript.JSType;

// 메이플스토리의 1개의 맵이라고 보면 됩니다.
namespace ServerContents.Room
{
    // 컨텐츠 로직과 플레이어와 대한 로직을 관리한다.
    public partial class GameRoom : JobSerializer
    {
        public int RoomId { get; set; }

        // 게임룸과 연결된 모든 오브젝트를 딕셔너리로 관리 (ObjectManager에서 생성된 객체 중 이 GameRoom에 있는 객체 관리)
        Dictionary<int, Player> _players = new Dictionary<int, Player>();
        Dictionary<int, NormalMonster> _normalMonsters = new Dictionary<int, NormalMonster>();
        Dictionary<int, BossMonster> _bossMonsters = new Dictionary<int, BossMonster>();
        Dictionary<int, Item> _items = new Dictionary<int, Item>();
        // Dictionary<int, Projectile> _projectiles = new Dictionary<int, Projectile>(); 후순위

        // 패킷 모아보내기용 List
        List<IMessage> _pendingList = new List<IMessage>();

        // TODO : 락 삭제
        object _lock = new object();

        public GameRoom()
        {
#if DEBUG
            PrintProceecPacket();
#endif
        }

        public void Init()
        {
            MonsterManager.Instance.LoadAllData();
            PlayerSpawnPositionManager.Instance.LoadAllData();
            SetDropItemRates();
            //TestItemRate();

            // Nms초당 에 한 번 서버에 귀속적인 로직을 실행한다.
            // ex. 몬스터의 업데이트
            Update(100);
        }

        void Update(int waitms)
        {
            MonsterUpdate();
            UpdateBossWaitingCount();
            BossRoomUpdate();

            PushAfter(waitms, Update, waitms);
        }


        public void PlayerEnterGame(GameObject gameObject, int prevMapId = 0)
        {
            lock (_lock)
            {
                if (gameObject == null)
                    return;

                GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

                Player player = gameObject as Player;
                _players.Add(gameObject.Id, player);
                player.Room = this;

                // 게임룸에 있는 모든 객체를 입장한 본인에게 전송
                {
                    // S_Enter : 자기 자신의 캐릭터 
                    S_EnterGame enterPacket = new S_EnterGame();
                    enterPacket.MapId = RoomId;
                    enterPacket.PlayerInfo = player.Info;
                    enterPacket.PlayerInfo.PositionX = enterPacket.SpawnPointX = PlayerSpawnPositionManager.Instance.GetPlayerSpawnPosition(prevMapId, RoomId).X;
                    enterPacket.PlayerInfo.PositionY = enterPacket.SpawnPointY = PlayerSpawnPositionManager.Instance.GetPlayerSpawnPosition(prevMapId, RoomId).Y;
                    player.Session.Send(enterPacket);

                    // S_Spawn : 다른 사람의 캐릭터
                    S_PlayerSpawn playersSpawnPacket = new S_PlayerSpawn();
                    foreach (Player p in _players.Values)
                    {
                        if (player != p)
                            playersSpawnPacket.PlayerInfos.Add(p.Info);
                    }

                    // 현재 있는 map의 몬스터 동기화
                    S_MonsterSpawn monstersSpawnPacket = new S_MonsterSpawn();
                    foreach (NormalMonster m in _normalMonsters.Values)
                        monstersSpawnPacket.MonsterInfos.Add(m.Info);
                    foreach (BossMonster m in _bossMonsters.Values)
                        monstersSpawnPacket.MonsterInfos.Add(m.Info);

                    player.Session.Send(playersSpawnPacket);
                    player.Session.Send(monstersSpawnPacket);
                }

                // 게임룸에 입장한 사실을 다른 클라이언트에게 전송
                {
                    S_PlayerSpawn spawnPacket = new S_PlayerSpawn();
                    spawnPacket.PlayerInfos.Add((gameObject as Player)?.Info);
                    foreach (Player p in _players.Values)
                    {
                        if (p.Id != gameObject.Id)
                            p.Session.Send(spawnPacket);
                    }
                    
                }
                //죽인사람을 알기 위해  테스트용 신경 x 
                //나온 위치 
                // ItemEnterGame(player,0,0);
            }
        }

        // 플레이어를 관리 대상에서(딕셔너리) 삭제
        public void LeavePlayer(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Player)
            {
                Player player = null;
                if (_players.Remove(objectId, out player) == false)
                    return;

                player.Room = null;

                // 본인한테 정보 전송
                {
                    S_LeaveGame leavePacket = new S_LeaveGame();
                    player.Session.Send(leavePacket);
                }
            }

            // 타인한테 정보 전송
            {
                S_PlayerDespawn despawnPacket = new S_PlayerDespawn();
                despawnPacket.PlayerIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    if (p.Id != objectId)
                        p.Session.Send(despawnPacket);
                }
            }
        }

        public void Flush()
        {
            if (_pendingList.Count == 0) return;
            foreach (var s in _players)
            {
                SendPacketPlus();
                s.Value.Session.Send(_pendingList);
            }
            _pendingList.Clear();
        }

        // 플레이어의 위치 동기화 
        public void HandleMove(Player player, C_PlayerMove movePacket)
        {
            if (player == null)
                return;

            // 맵 이동 따위로 딕셔너리에서 유효하지 않은 플레이어라면 리턴.
            if (!_players.ContainsKey(player.Info.PlayerId))
                return;

            // 서버에서 관리하기 위해 데이터 반영
            _players[player.Info.PlayerId].Info.PositionX = movePacket.PositionX;
            _players[player.Info.PlayerId].Info.PositionY = movePacket.PositionY;
            _players[player.Info.PlayerId].Info.CreatureState = movePacket.State;

            // 다른 플레이어한테도 알려준다
            S_PlayerMove resMovePacket = new S_PlayerMove();
            resMovePacket.State = movePacket.State;
            resMovePacket.PlayerId = player.Info.PlayerId;
            resMovePacket.PositionX = movePacket.PositionX;
            resMovePacket.PositionY = movePacket.PositionY;
            resMovePacket.IsRight = movePacket.IsRight;
            RecvPacketPlus();
            Broadcast(resMovePacket);
        }

        /// <summary>
        /// 이 플레이어가 스킬을 사용했다고 '알리기만' 한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillPacket"></param>
        public void HandleSkill(Player player, C_PlayerSkill skillPacket)
        {
            if (player == null)
                return;

            // TODO 실제로 플레이어가 스킬 사용이 가능한 상태인지 검증
            S_PlayerSkill resPkt = new S_PlayerSkill();
            resPkt.Skillid = player.Id;
            resPkt.SkillType = skillPacket.SkillType;
            Broadcast(resPkt);
        }

        /// <summary>
        /// 이 플레이어가 공격 받았다고 '알리기만' 한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="skillPacket"></param> 
        public void HandleDamaged(Player player, C_PlayerDamaged damagePacket)
        {
            if (player == null)
                return;

            S_PlayerDamaged resPkt = new S_PlayerDamaged();
            resPkt.PlayerId = player.Id;
            resPkt.Damage = damagePacket.Damage;
            Broadcast(resPkt);
        }

        /// <summary>
        /// 이 플레이어가 죽었다고 '알리기만' 한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="diePacket"></param>
        public void HandleDie(Player player, C_PlayerDie diePacket)
        {
            if (player == null)
                return;

            // TODO 패킷 만들고
            //S_PlayerDie resPkt = new S_PlayerDie();
            //resPkt.PlayerId = player.Id;
            //Broadcast(resPkt);
        }

        /// <summary>
        /// 플레이어의 맵 이동 요청을 처리한다.
        /// </summary>
        /// <param name="player"></param>
        /// <param name="diePacket"></param>
        public void HandleChangeMap(Player player, C_ChangeMap changeMapPacket)
        {
            if (player == null)
                return;

            // 1. 해당하는 맵이 실존하는지 검색
            GameRoom room = RoomManager.Instance.Find(changeMapPacket.MapId);
            if (room == null)
            {
                Console.WriteLine($"{changeMapPacket.MapId}번 방이 존재하지 않습니다. 서버 오류");
                return;
            }

            // 2. 이미 유저가 해당 맵에 있으면 리턴
            if (room._players.ContainsKey(player.Info.PlayerId))
            {
                Console.WriteLine($"{player.Info.PlayerId}유저는 이미 {changeMapPacket.MapId}맵에 존재합니다.");
                return;
            }

            // 3. 나가려는 플레이어 데이터를 날려준다.
            LeavePlayer(player.Id);

            room.Push(room.PlayerEnterGame, player, RoomId);
        }

        // 게임룸에 있는 다른 클라이언트에게 알림
        public void Broadcast(IMessage packet)
        {
            _pendingList.Add(packet);
        }
    }
}

using Google.Protobuf.Protocol;
using ServerContents.Job;
using ServerContents.Object;
using ServerContents.Session;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ServerContents.Room
{
    // 몬스터와 관련된 로직을 관리한다.
    // 기환님 작업하는 곳.
    public partial class GameRoom : JobSerializer
    { 
        #region 몬스터 관련
        void MonsterUpdate()
        {
            MonsterManager.Instance.MonsterSpawn(RoomId);

            foreach (NormalMonster normalMonster in _normalMonsters.Values)
            {
                normalMonster.Update();

                _normalMonsters[normalMonster.Id].Info = normalMonster.Info;
                _normalMonsters[normalMonster.Id].Stat = normalMonster.Stat;
            }

            foreach (BossMonster bossMonster in _bossMonsters.Values)
            {
                if (_players.Count() > 0)
                {
                    bossMonster.Update();
                    _bossMonsters[bossMonster.Id].Info = bossMonster.Info;
                    _bossMonsters[bossMonster.Id].Stat = bossMonster.Stat;
                }
            }
        }

         
        void BossRoomUpdate()
        {
            // 보스맵인 경우
            if (RoomId != (int)MapName.BossRoom)
                return;

            if (_players.Count > 0) return;
            else
            {
                foreach (int normalMonsterId in  _normalMonsters.Keys)
                {
                    LeaveMonster(normalMonsterId);
                    MonsterManager.Instance.MonsterDespawn(normalMonsterId);
                }
                    
                foreach (int bossMonsterId in _bossMonsters.Keys)
                {
                    LeaveMonster(bossMonsterId);
                    MonsterManager.Instance.MonsterDespawn(bossMonsterId);
                }
                MonsterManager.Instance.canSpawnBoss = true;   
            }
        }

        public void MonsterEnterGame(GameObject gameObject)
        {
            lock (_lock)
            {
                if (gameObject == null)
                    return;

                GameObjectType type = ObjectManager.GetObjectTypeById(gameObject.Id);

                if (type == GameObjectType.Normalmonster)
                {
                    NormalMonster monster = gameObject as NormalMonster;
                    _normalMonsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                }
                else if (type == GameObjectType.Bossmonster)
                {
                    BossMonster monster = gameObject as BossMonster;
                    _bossMonsters.Add(gameObject.Id, monster);
                    monster.Room = this;
                }

                // 게임룸에 존재하는 모든 클라이언트에게 전달
                {
                    S_MonsterSpawn spawnPacket = new S_MonsterSpawn();
                    spawnPacket.MonsterInfos.Add((gameObject as Monster)?.Info);
                    foreach (Player p in _players.Values)
                    {
                        p.Session.Send(spawnPacket);
                    }
                }
            }
        }

        // 몬스터를 관리 대상에서(딕셔너리) 삭제
        public void LeaveMonster(int objectId)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(objectId);

            if (type == GameObjectType.Normalmonster)
            {
                NormalMonster normalMonster = null;
                if (_normalMonsters.Remove(objectId, out normalMonster) != false)
                {
                    normalMonster.Room = null;
                }
            }
            else if (type == GameObjectType.Bossmonster)
            {
                BossMonster bossMonster = null;
                if (_bossMonsters.Remove(objectId, out bossMonster) == false)
                {
                    bossMonster.Room = null;
                }
            }

            // 게임룸에 존재하는 모든 클라이언트에게 전달
            {
                S_MonsterDespawn despawnPacket = new S_MonsterDespawn();
                despawnPacket.MonsterIds.Add(objectId);
                foreach (Player p in _players.Values)
                {
                    p.Session.Send(despawnPacket);
                }
            }
        }

        public void MonsterHitAndSetTarget(Player player, int monsterId, List<int> damageAmounts)
        {
            Monster monster = null;

            if (_normalMonsters.TryGetValue(monsterId, out var normalMonster))
                monster = normalMonster;

            else if (_bossMonsters.TryGetValue(monsterId, out var bossMonster))
                monster = bossMonster;

            if (monster != null)
            {
                monster.SetTarget(player);
                monster.TakeDamage(player.Id, damageAmounts);
            }
            else
            {
                Console.WriteLine("YMH : 몬스터 타게팅 실패. 해당 몬스터를 찾지 못했습니다(서버 로직 오류)");
            }
        }

        // 타겟 업데이트에서 현재 타겟으로 지정된 플레이어가 룸에 있는지 여부를 확인하기 위한 함수
        public bool IsPlayerInRoom(int id)
        {
            return _players.ContainsKey(id);
        }

        // 보스 소환에 있어서 보스룸에 아무런 플레이어가 없을 때만 소환되도록 현재 룸에 있는 플레이어의 수를 확인하기 위함.
        public int GetPlayerCountInRoom()
        {
            return _players.Count;
        }

        // 보스몬스터의 일반몬스터 소환 스킬 사용에 있어서 현재 룸에 있는 일반몬스터의 수를 파악하기 위함.
        public int GetNormalMonsterCountInRoom()
        {
            return _normalMonsters.Count;
        }

        public void RemoveMonster(int id)
        {
            GameObjectType type = ObjectManager.GetObjectTypeById(id);

            if (type == GameObjectType.Normalmonster)
            {
                NormalMonster normalMonster = null;
                if (_normalMonsters.Remove(id, out normalMonster) != false)
                {
                    normalMonster.Room = null;
                }
            }
            else if (type == GameObjectType.Bossmonster)
            {
                BossMonster bossMonster = null;
                if (_bossMonsters.Remove(id, out bossMonster) == false)
                {
                    bossMonster.Room = null;
                }
            }
        }

        #endregion

        public void GameClear()
        {
            if (RoomId != (int)MapName.BossRoom)
                return;

            foreach(var normalMonster in _normalMonsters)
            {
                LeaveMonster(normalMonster.Key);
            }

            foreach (Player p in _players.Values)
            {
                S_GameClear gameClearPacket = new S_GameClear();
                p.Session.Send(gameClearPacket);
            }
        }

        #region 보스룸 웨이팅 관련

        // 현재 보스룸 입장 대기열에 있는 플레이어 아이디 저장 및 현재 레이드 진행 중인지 여부를 저장
        public Dictionary<int, Player> _bossRoomWaitingPlayers = new Dictionary<int, Player>();

        public void UpdateBossWaitingCount()
        {
            foreach (var playerId in _bossRoomWaitingPlayers.Keys)
            {
                if (_players.ContainsKey(playerId) == false)
                {
                    _bossRoomWaitingPlayers.Remove(playerId);
                    HandleBossWaiting();
                }
            }
        }

        public void HandleBossWaiting()
        {
            foreach (var player in _bossRoomWaitingPlayers.Values)
            {
                S_BossWaiting bossWaitingPacket = new S_BossWaiting();
                bossWaitingPacket.WaitingCount = _bossRoomWaitingPlayers.Count;
                player.Session.Send(bossWaitingPacket);
            }
        }

        public void HandleBossEnterDenied(Player player)
        {
            S_BossRegisterDeny bossRegisterDenyPacket = new S_BossRegisterDeny();
            player.Session.Send(bossRegisterDenyPacket);
        }

        #endregion
    }
}

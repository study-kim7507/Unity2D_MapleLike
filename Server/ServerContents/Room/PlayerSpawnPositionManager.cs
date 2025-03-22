using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    // JSON 구조에 맞는 클래스 정의
    #region 플레이어 스폰 포지션 데이터 관련
    public class PlayerSpawnPositionDatabase
    {
        public List<PlayerSpawnPositionEntry> playerSpawnPositionDatabase { get; set; }
    }

    public class PlayerSpawnPositionEntry
    {
        public int currentMapId { get; set; }
        public List<NextMap> nextMaps { get; set; }
    }

    public class NextMap
    {
        public int nextMapId { get; set; }
        public SpawnPosition spawnPosition { get; set; }
    }

    public class SpawnPosition
    {
        public double X { get; set; }
        public double Y { get; set; }
    }
    #endregion

    public class PlayerSpawnPositionManager
    {
        public static PlayerSpawnPositionManager Instance { get; } = new PlayerSpawnPositionManager();
        private bool isAlreadyInitialize = false;

        private PlayerSpawnPositionDatabase? playerSpawnPositionDatabase;

        // [[currnetMapId, nextMapId], spawnPosition]
        Dictionary<Tuple<int, int>, Vector2> playerSpawnPositions = new Dictionary<Tuple<int, int>, Vector2>();

        public Vector2 GetPlayerSpawnPosition(int currentMapId, int nextMapId)
        {
            var key = Tuple.Create(currentMapId, nextMapId);

            if (playerSpawnPositions.TryGetValue(key, out var spawnPosition))
            {
                return spawnPosition;
            }

            // 없는 경우
            return Vector2.Zero;
        }

        public void LoadAllData()
        {
            if (!isAlreadyInitialize)
            {
                // AWS 
                string path = AppDomain.CurrentDomain.BaseDirectory + "PlayerSpawnPositions.Json";

                // Local
                if (!File.Exists(path))
                {
                    string projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
                    path = Path.Combine(projectRoot, "Room", "Data", "PlayerSpawnPositions.Json");
                }

                if (File.Exists(path))
                {
                    string json = File.ReadAllText(path);
                    playerSpawnPositionDatabase = JsonSerializer.Deserialize<PlayerSpawnPositionDatabase>(json);
                    SaveDataAsDictionary(playerSpawnPositionDatabase);
                }
                else
                {
                    Console.WriteLine("PlayerSpawnPositions.Json 파일이 존재하지 않습니다.");
                    Environment.Exit(0); 
                    return;
                }
                isAlreadyInitialize = true;
            }
        }

        private void SaveDataAsDictionary(PlayerSpawnPositionDatabase playerSpawnPositionDataBase)
        {
            if (playerSpawnPositionDataBase == null || playerSpawnPositionDataBase.playerSpawnPositionDatabase == null)
            {
                Console.WriteLine("데이터베이스가 비어있거나 유효하지 않습니다.");
                return;
            }

            foreach (var entry in playerSpawnPositionDataBase.playerSpawnPositionDatabase)
            {
                int currentMapId = entry.currentMapId;

                foreach (var nextMap in entry.nextMaps)
                {
                    int nextMapId = nextMap.nextMapId;
                    var spawnPosition = nextMap.spawnPosition;

                    // Vector2로 변환
                    Vector2 position = new Vector2((float)spawnPosition.X, (float)spawnPosition.Y);

                    // Tuple을 키로 사용하여 딕셔너리에 추가
                    var key = Tuple.Create(currentMapId, nextMapId);
                    playerSpawnPositions[key] = position; // 이미 존재하는 키일 경우 값을 업데이트.
                }
            }
        }
    }
}

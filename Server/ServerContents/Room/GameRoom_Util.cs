using Google.Protobuf.Protocol;
using ServerContents.Job;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerContents.Room
{
    // 유틸 관련 기능
    // 개략적 성능 확인 기능
    // 기타 기능
    public partial class GameRoom : JobSerializer
    {
        #region 패킷 송수신 개수 개략적 확인용
        public int recvPacketCount = 0;
        public int sendPacketCount = 0;
        public void RecvPacketPlus()
        {
            recvPacketCount++;
        }

        public void SendPacketPlus()
        {
            sendPacketCount++;
        }

        private async void PrintProceecPacket()
        {
            while (true)
            {
                if (RoomId != (int)MapName.Tutorial) break;
                Console.WriteLine($"# {Enum.GetName(typeof(MapName), RoomId),-15} | 총 {recvPacketCount + sendPacketCount,5}개 | recv : {recvPacketCount,5}개 | send : {sendPacketCount,5}개 처리 (1초)");
                recvPacketCount = 0;
                sendPacketCount = 0;

                await Task.Delay(1000); // 1초 대기 (비동기적으로 실행)
            }
        }

        #endregion 패킷 송수신 개수 개략적 확인용

        void TestItemRate(int testCount = 100000)
        {
            Dictionary<ItemType, int> dropResults = new Dictionary<ItemType, int>();

            foreach (var item in dropRates.Keys)
            {
                dropResults[item] = 0;
            }

            for (int i = 0; i < testCount; i++)
            {
                ItemType droppedItem = GetRandomItem();
                dropResults[droppedItem]++;
            }

            Console.WriteLine($"총 테스트 횟수: {testCount}");
            Console.WriteLine("==== 아이템 드롭 분포 ====");
            double totalPercentage = 0.0;
            foreach (var kvp in dropResults.OrderByDescending(kv => kv.Value))
            {
                double percentage = (kvp.Value / (double)testCount) * 100.0;
                totalPercentage += percentage;
                Console.WriteLine($"{kvp.Key}: {kvp.Value}회 ({percentage:F3}%)");
            }

            Console.WriteLine($"확률 총합: {totalPercentage:F3}%");
        }
    }
}

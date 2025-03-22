using NUnit.Framework.Constraints;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class QuestInfos
{
    private List<List<Dictionary<string, Tuple<int, int>>>> questDetail = new List<List<Dictionary<string, Tuple<int, int>>>>
    {
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                {"달팽이", new Tuple<int, int>(0, 5) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "슬라임", new Tuple<int, int>(0, 10) },
                { "스텀프", new Tuple<int, int>(0, 10) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "주황버섯", new Tuple<int, int>(0, 10) },
                { "리본돼지", new Tuple<int, int>(0, 10) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "페어리", new Tuple<int, int>(0, 10) },
                { "아이언호그", new Tuple<int, int>(0, 10) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "머쉬맘", new Tuple<int, int>(0, 3) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "정식기사", new Tuple<int, int>(0, 20) }
            }
        },
        new List<Dictionary<string, Tuple<int, int>>>
        {
            new Dictionary<string, Tuple<int, int>>
            {
                { "시그너스", new Tuple<int, int>(0, 1) }
            }
        },
    };

    private List<string> mapName = new List<string>
    {
        "달팽이동산",
        "헤네시스 사냥터",
        "헤네시스 사냥터",
        "남의 집",
        "남의 집",
        "비밀정원",
        "시그너스의 전당"
    };

    private List<int> rewardExpAmounts = new List<int>
    {
        50, 150, 300, 500, 1000, 1500, 2000
    };


    public List<Dictionary<string, Tuple<int, int>>> GetQuestDetailDescriptionText(int playerLevel)
    {
        int questNum = 0;
        if (playerLevel >= 1 && playerLevel <= 5) questNum = 0;
        if (playerLevel >= 6 && playerLevel <= 10) questNum = 1;
        if (playerLevel >= 11 && playerLevel <= 13) questNum = 2;
        if (playerLevel >= 14 && playerLevel <= 15) questNum = 3;
        if (playerLevel >= 16 && playerLevel <= 19) questNum = 4;
        if (playerLevel >= 20 && playerLevel <= 24) questNum = 5;
        else if (playerLevel >= 25) questNum = 6;

        return questDetail[questNum];
    }

    public int GetRewardExpAmount(int playerLevel)
    {
        int questNum = 0;
        if (playerLevel >= 1 && playerLevel <= 5) questNum = 0;
        if (playerLevel >= 6 && playerLevel <= 10) questNum = 1;
        if (playerLevel >= 11 && playerLevel <= 13) questNum = 2;
        if (playerLevel >= 14 && playerLevel <= 15) questNum = 3;
        if (playerLevel >= 16 && playerLevel <= 19) questNum = 4;
        if (playerLevel >= 20 && playerLevel <= 24) questNum = 5;
        else if (playerLevel >= 25) questNum = 6;

        return rewardExpAmounts[questNum]; 
    }

    public string GetQuestMap(int playerLevel)
    {
        int questNum = 0;
        if (playerLevel >= 1 && playerLevel <= 5) questNum = 0;
        if (playerLevel >= 6 && playerLevel <= 10) questNum = 1;
        if (playerLevel >= 11 && playerLevel <= 13) questNum = 2;
        if (playerLevel >= 14 && playerLevel <= 15) questNum = 3;
        if (playerLevel >= 16 && playerLevel <= 19) questNum = 4;
        if (playerLevel >= 20 && playerLevel <= 24) questNum = 5;
        else if (playerLevel >= 25) questNum = 6;

        return mapName[questNum];
    }
}

public class Quest
{
    private QuestInfos questInfos = new QuestInfos();
    
    public bool isClearQuest = false;
    private int currentPlayerLevel = 0;

    private List<string> questDescriptionText = new List<string>
    { 
        "초보 모험가처럼 보이는 군.\n전설의 모험가인 마이 내가 다음 과제를 제공해줄테니\n해결하면서 같이 성장해보자구.\n\n다음의 몬스터를 처치하고 나에게 돌아와.",
        "아직 내가 부여한 과제를 완료하지 못했군.\n아래 남은 과제를 확인하고 완수하고 오게.\n\n다음의 몬스터를 처치하고 나에게 돌아와.",
        "내가 부여한 과제를 모두 완수했군.\n고생했네, 보상으로 성장의 발판이 될 수 있는 \n경험치를 제공해주겠네.\n"
    };
    private string mapName;

    private List<Dictionary<string, Tuple<int, int>>> questDetail = new List<Dictionary<string, Tuple<int, int>>>();
    
    public int rewardExpAmount = 0;

    public Quest GetNewQuest()
    {
        currentPlayerLevel = PlayerInformation.playerStatInfo.Level;
        
        questDetail = questInfos.GetQuestDetailDescriptionText(currentPlayerLevel);
        rewardExpAmount = questInfos.GetRewardExpAmount(currentPlayerLevel);
        mapName = questInfos.GetQuestMap(currentPlayerLevel);

        return this;
    }

    public string GetQuestDescriptionText(int num)
    {
        return questDescriptionText[num] + "\n\n" + GetQuestDetailDescriptionText() + "\n" + "수행 장소 : " + mapName;
    }

    private string GetQuestDetailDescriptionText()
    {
        string result = "";
        foreach (var dict in questDetail)
        {
            foreach (var item in dict)
            {
                result += $"<color=red>{item.Key} {item.Value.Item1} / {item.Value.Item2}</color>\n";
            }
        }

        return result;
    }

    public void UpdateQuest(string monsterName)
    {
        if (QuestManager.Instance.currentQuest != null && QuestManager.Instance.currentQuest != this)
            return;

        string curr = "";

        if (isClearQuest)
        {
            return;
        }

        // 몬스터 이름 파싱 (name_id 형태에서 name만 추출)
        string[] parts = monsterName.Split('_'); // 언더스코어로 분리
        string currMonsterName = parts[0]; // 첫 번째 부분이 몬스터 이름

        // 마지막 문자가 A, B, C, D, E인지 확인하고 제거
        if (currMonsterName.Length > 0 && "ABCDE".Contains(currMonsterName[^1]))
        {
            currMonsterName = currMonsterName[..^1]; // 마지막 문자 제거
        }

        foreach (var dict in questDetail)
        {
            if (dict.ContainsKey(currMonsterName))
            {
                var currentTuple = dict[currMonsterName];

                int updatedItem1 = Math.Clamp(currentTuple.Item1 + 1, 0, currentTuple.Item2);
                var updatedTuple = new Tuple<int, int>(updatedItem1, currentTuple.Item2);
                dict[currMonsterName] = updatedTuple;

                curr = "[퀘스트] " + currMonsterName + " " + updatedTuple.Item1.ToString() + " / " + updatedTuple.Item2.ToString() + " 처치";
                break;
            }
        }

        bool clear = true;

        foreach (var dict in questDetail)
        {
            foreach(var item in dict)
            {
                var currTuple = item.Value;
                if (currTuple.Item1 != currTuple.Item2)
                {
                    clear = false;
                    break;
                }
            }
            if (!clear) break;
        }


        if (clear)
        {
            curr = "퀘스트를 완료하였습니다.\n퀘스트 수령 NPC에게 돌아가 완료하십시요.";
            isClearQuest = clear;
        }
        QuestManager.Instance.ShowProgress(curr);
    }
}

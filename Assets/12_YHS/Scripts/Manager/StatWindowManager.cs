using Google.Protobuf.Protocol;
using TMPro;
using UnityEngine;

public class StatWindowManager : MonoBehaviour
{
    public GameObject statWindowPanel;

    [SerializeField] TMP_Text levelValue;
    [SerializeField] TMP_Text classTypeValue;
    [SerializeField] TMP_Text currentHpValue;
    [SerializeField] TMP_Text maxHpValue;
    [SerializeField] TMP_Text currentMpValue;
    [SerializeField] TMP_Text maxMpValue;
    [SerializeField] TMP_Text expValue;
    [SerializeField] TMP_Text expPercentageValue;
    [SerializeField] TMP_Text attackPowerValue;
    [SerializeField] TMP_Text magicPowerValue;
    [SerializeField] TMP_Text defenceValue;

    [SerializeField] TMP_Text strValue;
    [SerializeField] TMP_Text dexValue;
    [SerializeField] TMP_Text intValue;
    [SerializeField] TMP_Text lukValue;

    private static StatWindowManager instance;
    public static StatWindowManager Instance { get { return instance; } }

    private void Awake()
    {
        if (instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void InitStatWindow(PlayerInformation playerInformation)
    {
        playerInformation.UpdateStatWindowAction += UpdateStatWindow;
    }

    private void UpdateStatWindow()
    {
        PlayerStatInfo statInfo = PlayerInformation.playerStatInfo;
        levelValue.text = statInfo.Level.ToString();

        switch (statInfo.ClassType)
        {
            case ClassType.Warrior:
                classTypeValue.text = "전사";
                break;
            case ClassType.Magician:
                classTypeValue.text = "마법사";
                break;
            case ClassType.Archer:
                classTypeValue.text = "궁수";
                break;
            default:
                classTypeValue.text = "초보자";
                break;

        }

        currentHpValue.text = statInfo.Hp.ToString();
        maxHpValue.text = statInfo.MaxHp.ToString();
        currentMpValue.text = statInfo.Mp.ToString();
        maxMpValue.text = statInfo.MaxMp.ToString();
        expValue.text = statInfo.CurrentExp.ToString();
        expPercentageValue.text = ((float)statInfo.CurrentExp / statInfo.TotalExp * 100f).ToString("F2");
        attackPowerValue.text = statInfo.AttackPower.ToString();
        magicPowerValue.text = statInfo.MagicPower.ToString();
        defenceValue.text = statInfo.Defense.ToString();

        AbilityPoint ap = PlayerInformation.playerAp;
        strValue.text = ap.Ap[0].ToString();
        dexValue.text = ap.Ap[1].ToString();
        intValue.text = ap.Ap[2].ToString();
        lukValue.text = ap.Ap[3].ToString();
    }

    public void SetWindowActive()
    {
        if (statWindowPanel.activeSelf == true)
        {
            statWindowPanel.SetActive(false);
        }
        else
        {
            UpdateStatWindow();
            statWindowPanel.SetActive(true);
        }
    }

    public void SetWindowActive(bool active)
    {
        if (active == true)
        {
            UpdateStatWindow();
        }

        statWindowPanel.SetActive(active);
    }
}

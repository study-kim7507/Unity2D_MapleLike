using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class BossRoomEnterUI : MonoBehaviour
{
    public static BossRoomEnterUI Instance { get; private set; }

    [SerializeField] GameObject partyMatchingUIPanel;
    [SerializeField] GameObject enterFailedUIPanel;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else Destroy(gameObject);
    }

    private void SoloEnterButtonPressed()
    {
        C_BossRegister bossRegisterPacket = new C_BossRegister();
        bossRegisterPacket.BossEnterType = BossEnterType.Single;
        NetworkManager.Instance.Send(bossRegisterPacket);
    }

    private void PartyEnterButtonPressed()
    {
        // 서버로 보스 입장 대기열에 들어감을 알림
        C_BossRegister bossRegisterPacket = new C_BossRegister();
        bossRegisterPacket.BossEnterType = BossEnterType.Multi;
        NetworkManager.Instance.Send(bossRegisterPacket);
    }
    
    private void MatchCancelButtonPressed()
    {
        // 서버로 매치 취소 패킷 전송
        C_BossCancle bossCanclePacket = new C_BossCancle();
        NetworkManager.Instance.Send(bossCanclePacket);

        // 파티 매칭 패널 비활성화
        partyMatchingUIPanel.SetActive(false);
    }

    private void DeActiveEnterFailedUIPanel()
    {
        enterFailedUIPanel.SetActive(false);
    }

    public void EntirePanelCloseButtonPressed()
    {
        Destroy(gameObject);    
    }

    public void UpdatePartyMatchingUIPanel(int currentWaitingCount)
    {
        partyMatchingUIPanel.SetActive(true);
        GetComponentInChildren<WaitingCountDisplayer>().UpdateCountImage(currentWaitingCount);
    }

    public void ActiveEnterFaileUIdPanel()
    {
        enterFailedUIPanel.SetActive(true);
    }

   
}

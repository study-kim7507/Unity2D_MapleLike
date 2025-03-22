using UnityEngine;

public class BossRoomPortal : MonoBehaviour
{
    [SerializeField] GameObject bossRoomEnterUIPrefab;

    public void BossRoomEnterUIActive(GameObject player)
    {
        // 플레이어 아래에 이미 UI 오브젝트가 있는지 확인
        Transform existingUI = player.transform.Find(bossRoomEnterUIPrefab.name);

        if (existingUI == null)
        {
            // UI 오브젝트가 없으면 인스턴스화하고 활성화
            GameObject uiObject = Instantiate(bossRoomEnterUIPrefab, player.transform);
            uiObject.SetActive(true);
        }
        else
        {
            // UI 오브젝트가 이미 존재하면 활성화만 처리
            existingUI.gameObject.SetActive(true);
        }
    }
}

using Google.Protobuf.Protocol;
using Google.Protobuf.WellKnownTypes;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class StartSceneHelper : MonoBehaviour
{
    public GameObject BasePanel;
    public GameObject FadeOutPanel;
    float fadeOutDuration = 2.0f;

    void OnEnable()
    {
        PacketHandler.OnActivateStartScenePanel += ActivatePanel;
    }

    void OnDisable()
    {
        PacketHandler.OnActivateStartScenePanel -= ActivatePanel;
    }

    void ActivatePanel()
    {
        BasePanel.SetActive(true);
    }

    public void OnClickClassChoiceBtn(int param=0)
    {
        if (param == 0) return;
        FadeOutPanel.SetActive(true);
        GetComponent<AudioSource>()?.Play();
        StartCoroutine(FadeInAndSendPacket(param));
    }

    private IEnumerator FadeInAndSendPacket(int param)
    {
        Image panelImage = FadeOutPanel.GetComponent<Image>();

        float elapsedTime = 0f;
        Color panelColor = panelImage.color;

        while (elapsedTime < fadeOutDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeOutDuration);
            panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1);

        C_ClassChoice pkt = new C_ClassChoice();
        pkt.ClassType = (ClassType)param;
        NetworkManager.Instance.Send(pkt);
    }
}

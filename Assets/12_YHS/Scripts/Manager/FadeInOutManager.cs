using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeInOutManager : MonoBehaviour
{
    [SerializeField] GameObject fadeInOutPanel;
    [SerializeField] float fadeInOutDuration = 0.5f;

    private static FadeInOutManager instance;
    public static FadeInOutManager Instance { get { return instance; } }

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

    /// <summary>
    /// 검은 화면으로 전환하는 메서드
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeOut()
    {
        Image panelImage = fadeInOutPanel.GetComponent<Image>();
        Color panelColor = panelImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInOutDuration)
        {
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeInOutDuration);
            panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            elapsedTime += Time.deltaTime;
            Debug.Log(elapsedTime);
            yield return null;
        }

        panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 1);
    }

    /// <summary>
    /// 검은 화면에서 탈출하는 메서드
    /// </summary>
    /// <returns></returns>
    public IEnumerator FadeIn()
    {
        Image panelImage = fadeInOutPanel.GetComponent<Image>();
        Color panelColor = panelImage.color;
        float elapsedTime = 0f;

        while (elapsedTime < fadeInOutDuration)
        {
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeInOutDuration);
            panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, alpha);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        panelImage.color = new Color(panelColor.r, panelColor.g, panelColor.b, 0);
    }
}

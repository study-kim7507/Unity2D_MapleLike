using NUnit.Framework.Constraints;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
public class QuestManager : MonoBehaviour
{
    public static QuestManager Instance { get; private set; }
    
    public Quest currentQuest = null;
    
    [HideInInspector] public bool isProgressingQuest = false;

    [SerializeField] private GameObject newQuestPanelPrefab;
    [SerializeField] private GameObject clearQusetPanelPrefab;
    [SerializeField] private GameObject progressingQuestPanelPrefab;
    [SerializeField] private GameObject showProgessPanelPrefab;
    private Coroutine showProgressPanelCoroutine = null;

    private AudioSource audioSource;
    [SerializeField] AudioClip onOffAudioClip;
    [SerializeField] AudioClip questClearAudioClip;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = GetComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddNewQuest()
    {
        // 퀘스트 수락 관련 패널 보여주기
        currentQuest = new Quest();
        if (currentQuest != null)
            currentQuest = currentQuest.GetNewQuest();

        audioSource.PlayOneShot(onOffAudioClip);
        newQuestPanelPrefab.SetActive(true);
        newQuestPanelPrefab.GetComponentInChildren<TextMeshProUGUI>().text = currentQuest.GetQuestDescriptionText(0);
    }

    public void ShowProgress(string str)
    {
        showProgessPanelPrefab.SetActive(true);
        showProgessPanelPrefab.GetComponent<TextMeshProUGUI>().text = str;

        if (showProgressPanelCoroutine != null)
            StopCoroutine(showProgressPanelCoroutine);
        showProgressPanelCoroutine = StartCoroutine(ShowProgressCoroutine());
    }

    private IEnumerator ShowProgressCoroutine()
    {
        // TextMeshProUGUI 컴포넌트를 가져옵니다.
        var textMeshPro = showProgessPanelPrefab.GetComponent<TextMeshProUGUI>();

        // Alpha 값을 1로 설정합니다.
        Color startColor = new Color(textMeshPro.color.r, textMeshPro.color.g, textMeshPro.color.b, 1);
        Color endColor = new Color(startColor.r, startColor.g, startColor.b, 0); // 끝 색상 (Alpha 0)

        float duration = 3.0f; // 지속 시간
        float elapsedTime = 0f; // 경과 시간

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime; // 경과 시간 업데이트
            textMeshPro.color = Color.Lerp(startColor, endColor, elapsedTime / duration); // 색상 보간
            yield return null; // 다음 프레임까지 대기
        }

        textMeshPro.color = endColor; // 최종 색상 설정
        showProgessPanelPrefab.SetActive(false);
    }

    public void CheckClearQuest()
    {
        if (currentQuest != null)
        {
            if (currentQuest.isClearQuest)
            {
                audioSource.PlayOneShot(onOffAudioClip);
                clearQusetPanelPrefab.SetActive(true);
                clearQusetPanelPrefab.GetComponentInChildren<TextMeshProUGUI>().text = currentQuest.GetQuestDescriptionText(2);
            }
            else if (isProgressingQuest)
            {
                audioSource.PlayOneShot(onOffAudioClip);
                progressingQuestPanelPrefab.SetActive(true);
                progressingQuestPanelPrefab.GetComponentInChildren<TextMeshProUGUI>().text = currentQuest.GetQuestDescriptionText(1);
            }
        }
    }

    public void QuestAcceptButtonPressed()
    {
        audioSource.PlayOneShot(onOffAudioClip);
        newQuestPanelPrefab.SetActive(false);
        isProgressingQuest = true;
    }

    public void QuestRefuseButtonPressed()
    {
        audioSource.PlayOneShot(onOffAudioClip);
        newQuestPanelPrefab.SetActive(false);
        currentQuest = null;
    }

    public void QuestClearButtonPressed()
    {
        audioSource.PlayOneShot(questClearAudioClip);
        isProgressingQuest = false;
        clearQusetPanelPrefab.SetActive(false);

        int rewardExpAmount = currentQuest.rewardExpAmount;
        currentQuest = null;

        int playerId = ObjectManager.Instance.MyPlayer.Id;
        GameObject go = ObjectManager.Instance.FindById(playerId);
        PlayerInformation playerInformation = go.GetComponent<PlayerInformation>();

        playerInformation.SetPlayerExp(rewardExpAmount);
        UIManager.Instance.Income += 500;
        UIManager.Instance.TxtGold.text = UIManager.Instance.Income.ToString();
    }

    public void QuestGiveupButtonPressed()
    {
        audioSource.PlayOneShot(onOffAudioClip);
        progressingQuestPanelPrefab.SetActive(false);
        isProgressingQuest = false;
        currentQuest = null;
    }

    public void CloseButtonPressed()
    {
        audioSource.PlayOneShot(onOffAudioClip);
        progressingQuestPanelPrefab.SetActive(false);
    }
}

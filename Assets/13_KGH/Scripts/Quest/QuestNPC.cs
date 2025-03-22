using Google.Protobuf.Protocol;
using UnityEngine;
using UnityEngine.UI;

public class QuestNPC : MonoBehaviour
{
    private Button targetButton;

    private float clickTime = 0f;
    private float clickInterval = 0.5f;
    private int clickCount = 0;

    [SerializeField] private GameObject questProgressNotifier;
    private Animator questProgressNotifierAnimator;

    void OnEnable()
    {
        if (targetButton != null)
        {
            return;
        }
        targetButton = GetComponent<Button>();
        targetButton.onClick.AddListener(OnButtonClick);

        if (questProgressNotifier != null)
            questProgressNotifierAnimator = questProgressNotifier.GetComponent<Animator>();
    }

    private void Update()
    {
        if (QuestManager.Instance.currentQuest == null && !QuestManager.Instance.isProgressingQuest)
        {
            AnimatorStateInfo animatorStateInfo = questProgressNotifierAnimator.GetCurrentAnimatorStateInfo(0);
            if (!animatorStateInfo.IsName("NotStart"))
                questProgressNotifierAnimator.SetTrigger("NotStart");
        }
            
        else if (QuestManager.Instance.currentQuest != null && QuestManager.Instance.isProgressingQuest && !QuestManager.Instance.currentQuest.isClearQuest)
        {
            AnimatorStateInfo animatorStateInfo = questProgressNotifierAnimator.GetCurrentAnimatorStateInfo(0);
            if (!animatorStateInfo.IsName("InProgress"))
                questProgressNotifierAnimator.SetTrigger("InProgress");
        }
            
        else if (QuestManager.Instance.currentQuest != null && QuestManager.Instance.isProgressingQuest && QuestManager.Instance.currentQuest.isClearQuest)
        {
            AnimatorStateInfo animatorStateInfo = questProgressNotifierAnimator.GetCurrentAnimatorStateInfo(0);
            if (!animatorStateInfo.IsName("Complete"))
                questProgressNotifierAnimator.SetTrigger("Complete");
        }
    }

    void OnButtonClick()
    {
        clickCount++;

        if (clickCount == 1)
        {
            clickTime = Time.time;
        }
        else if (clickCount == 2)
        {
            if (Time.time - clickTime <= clickInterval)
            {
                HandleDoubleClick();
            }
            clickCount = 0;
        }

        if (Time.time - clickTime > clickInterval)
        {
            clickCount = 0;
        }
    }

    void HandleDoubleClick()
    {
        // 새로운 퀘스트
        if (QuestManager.Instance.currentQuest == null)
        {
            // 레벨에 따른 퀘스트 제공
            QuestManager.Instance.AddNewQuest();
        }
        // 이미 퀘스트 진행 중
        else if (QuestManager.Instance.currentQuest != null)
        {
            // 완료 여부에 따른 패널
            QuestManager.Instance.CheckClearQuest();
        }
    }
}

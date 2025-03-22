using System.Collections;
using TMPro;
using UnityEngine;

public class BuffIconAnimation : MonoBehaviour
{
    [SerializeField] private Animator coolTimeIconAnimator;
    [SerializeField] private TMP_Text timerText;
    public string skillName;
    public float duration;

    void Start()
    {
        PlayAnimation("CoolDown", duration);
    }

    private void PlayAnimation(string trigger, float duration)
    {
        float clipLength = coolTimeIconAnimator.runtimeAnimatorController.animationClips[0].length;
        coolTimeIconAnimator.speed = clipLength / duration;
        coolTimeIconAnimator.Play(trigger);
        StartCoroutine(SetTimer());
    }

    IEnumerator SetTimer()
    {
        float timer = duration;
        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerText.text = ((int)timer).ToString();
            yield return null;
        }

        Destroy(gameObject);
    }

    private void OnDestroy()
    {
        if (BuffManager.instance.BuffList.Contains(gameObject))
        {
            BuffManager.instance.BuffList.Remove(gameObject);
        }
    }
}

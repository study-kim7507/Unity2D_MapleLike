using UnityEngine;
using UnityEngine.UI;

public class SmallNoticeManager : MonoBehaviour
{
    private static SmallNoticeManager instance;
    public static SmallNoticeManager Instance { get { return instance; } }

    [SerializeField] Transform smallNoticeContentTransform;
    [SerializeField] ScrollRect smallNoticeScrollRect;

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

    public void SpawnSmallNotice(string notice)
    {
        SpawnManager.Instance.SpawnSmallNotice(notice, smallNoticeContentTransform);
    }
}

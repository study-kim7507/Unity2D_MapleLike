using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SoundManager : MonoBehaviour
{
    private static SoundManager instance;
    public static SoundManager Instance { get { return instance; } }

    private AudioSource audioSource;

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

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.volume = 0.5f;
    }

    #region 소리 출력
    /// <summary>
    /// 1회 출력하는 메서드
    /// </summary>
    /// <param name="assetKey"></param>
    public void PlaySoundOneShot(string assetKey)
    {
        AsyncOperationHandle<AudioClip> loadHandler;
        loadHandler = Addressables.LoadAssetAsync<AudioClip>(assetKey);

        loadHandler.Completed += handle =>
        {
            if (loadHandler.Status == AsyncOperationStatus.Succeeded)
            {
                audioSource.PlayOneShot(loadHandler.Result);
            }
            else
            {
                Debug.LogError("Addressable Load Failed: " + assetKey);
            }
        };
    }
    #endregion
}

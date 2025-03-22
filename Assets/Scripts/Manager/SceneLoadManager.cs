using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class SceneLoadManager : MonoBehaviour
{
    public static SceneLoadManager Instance { get; private set; }

    public bool IsSceneLoading { get; private set; } = false; // 씬이 로딩 중인지 여부
    private Queue<System.Action> pendingActions = new Queue<System.Action>(); // 씬 로드 대기 작업 큐

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 씬이 로드될 때 이곳을 실행한다.
    // ex) 대표적으로 S_EnterGameHandler()를 실행 할 때 
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }

    private IEnumerator LoadSceneAsync(string sceneName)
    {
        IsSceneLoading = true;
        // 여기서 씬이 로드가 될때까지 대기한다.
        AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
        yield return new WaitUntil(() => asyncOp.isDone);
        IsSceneLoading = false;

        // 씬 로드가 완료 됐다면, 대기중이던 Action들을 처리한다.
        // ex) AddPlayer(), AddMonster()
        while (pendingActions.Count > 0)
        {
            pendingActions.Dequeue().Invoke();
        }
    }

    // 씬이 로딩 중이면 대기열 큐에 추가, 아니면 즉시 실행
    public void ExecuteOrQueue(System.Action action)
    {
        if (IsSceneLoading)
        {
            pendingActions.Enqueue(action);
        }
        else
        {
            action.Invoke();
        }
    }
}
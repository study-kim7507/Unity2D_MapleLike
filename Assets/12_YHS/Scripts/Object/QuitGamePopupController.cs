using System.Collections;
using UnityEngine;

public class QuitGamePopupController : MonoBehaviour
{
    public void SetPopupActive()
    {
        gameObject.SetActive(true);
    }

    public void SetPopupInactive()
    {
        gameObject.SetActive(false);
    }

    private IEnumerator FadeOutAndQuitGame()
    {
        yield return StartCoroutine(FadeInOutManager.Instance.FadeOut());
    }

    public void QuitGame()
    {
        FadeOutAndQuitGame();
        
        Application.Quit();
    }
}

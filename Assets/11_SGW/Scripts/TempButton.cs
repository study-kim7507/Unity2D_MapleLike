using UnityEngine;
using UnityEngine.UI;

public class TempButton : MonoBehaviour
{
    public Button exitButton;

    void Start()
    {
        exitButton.onClick.AddListener(UIManager.Instance.PlaySoundBtnClick);
    }
}


using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WaitingCountDisplayer : MonoBehaviour
{
    [SerializeField] List<Image> personCountImage;

    [SerializeField] Sprite onSprite;
    [SerializeField] Sprite offSprite;
    
    public void UpdateCountImage(int currentWaitingCount)
    {
        for (int i = 0; i < personCountImage.Count; i++)
        {
            if (i < currentWaitingCount) personCountImage[i].sprite = onSprite;
            else personCountImage[i].sprite = offSprite;
        }
    }
}

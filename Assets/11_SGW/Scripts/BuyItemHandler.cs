using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuyItemHandler : MonoBehaviour
{
    public TMP_InputField inputField;  // InputField 참조
    public ShowUI showUI;              // ShowUI 스크립트 참조

    
    public void OnBuyButtonClick()
    {
        if (showUI == null)
        {
            Debug.LogError("ShowUI 스크립트가 연결되지 않았습니다!");
            return;
        }

        int quantity = 0; // 기본값 설정

        if (int.TryParse(inputField.text, out int result))
        {
            quantity = Mathf.Max(0, result); 
        }
        else
        {
            Debug.LogWarning("잘못된 입력! 기본값(1)로 구매");
        }

        showUI.BuyItem(quantity);
        
        gameObject.transform.parent.gameObject.SetActive(false);
    }
    
    public void OnSellButtonClick()
    {
        if (showUI == null)
        {
            Debug.LogError("ShowUI 스크립트가 연결되지 않았습니다!");
            return;
        }

        int quantity = 0; // 기본값 설정

        if (int.TryParse(inputField.text, out int result))
        {
            quantity = Mathf.Max(0, result); // 최소 1개 이상 구매하도록 설정
        }
        else
        {
            Debug.LogWarning("잘못된 입력! 기본값(1)로 구매");
        }

        showUI.SellItem(quantity);
        
        gameObject.transform.parent.gameObject.SetActive(false);
    }
}
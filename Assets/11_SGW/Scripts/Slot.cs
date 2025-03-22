using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Slot : MonoBehaviour
{
    public Image _image;
    public Item CurrentItem;
    public int Count;
    public TextMeshProUGUI CountText;
    private void Awake()
    {
        _image = GetComponent<Image>();
    }


    
    public void SetItem(Item item)
    {
        
        CurrentItem = item;
        _image.sprite = item.IconSprite; // ✅ 아이콘 설정
        Color color = _image.color;
        color.a = 1f; // 알파 값 1 (완전 불투명)
        _image.color = color;
        
        if (CountText != null)
        {
            if (Count > 1)
            {
                CountText.text = Count.ToString();
            }
            else
            {
                CountText.text = "";
            }
        }
      

    }
    
    public void UpdateUI()
    {
        if (CurrentItem != null)
        {
            CountText.text = Count > 1 ? Count.ToString() : ""; // 1개 이상이면 숫자 표시
        }
    }

    public void ClearSlot()
    {
        Color color = _image.color;
        if (Count < 1)
        {
            color.a = 0f; // ✅ 아이콘을 완전히 투명하게 설정
            _image.color = color;
            CurrentItem = null; // 아이템 제거
        }
        
    }

}

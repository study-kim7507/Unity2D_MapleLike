using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShowInfo : MonoBehaviour,IPointerEnterHandler, IPointerExitHandler
{
    private void Start()
    {
        // 마우스 이벤트  여러번 방지
        if (UIManager.Instance.tooltipText != null)
        {
            
            UIManager.Instance.tooltipCanvas = UIManager.Instance.tooltipGroup.GetComponent<CanvasGroup>();
            if (UIManager.Instance.tooltipCanvas != null)
            {
                UIManager.Instance.tooltipCanvas.blocksRaycasts = false; 
            }
        }
    }

    //마우스가 가르킨 오브젝트의 정보에 맞게 툴팁 설명 해주기
    public void OnPointerEnter(PointerEventData eventData)
    {
        GameObject hoveredObject = eventData.pointerEnter;
        
        Debug.Log("내가 본거"+hoveredObject);
        
        
        if (hoveredObject.TryGetComponent<EquipSlot>(out EquipSlot equipSlot))
        {
            foreach (var itemData in ItemManager.Instance.ItemList)
            {

                if (equipSlot.CurrentItem != null && itemData.id == equipSlot.CurrentItem.id)
                {
                    UIManager.Instance.tooltipGroup.SetActive(true);
                    UIManager.Instance.tooltipGroup.transform.position = equipSlot.transform.position+ new Vector3(180,0,0);
                    if (equipSlot.CurrentItem is Equipment eq)
                    {
                        UIManager.Instance.tooltipText.text = "착용레벨:"+ eq.limitLevel + "\n"+
                                                              "공격력:"+ eq.attackPower + "\n"+
                                                              "마법공격력:"+ eq.magicPower + "\n"+
                                                              "방어력:"+ eq.defensePower + "\n"+
                                                              eq.description +"\n"+"더블클릭으로 장착 및 해제가 가능하다";
                        return;
                    }
                    
                }
            }
        }
        else if (hoveredObject.TryGetComponent<Slot>(out Slot inventorySlot))
        {
            
            foreach (var itemData in ItemManager.Instance.ItemList)
            {
                if (inventorySlot.CurrentItem != null && itemData.id == inventorySlot.CurrentItem.id)
                {
                    UIManager.Instance.tooltipGroup.SetActive(true);
                    UIManager.Instance.tooltipGroup.transform.position = inventorySlot.transform.position+ new Vector3(180,0,0);
                    
                    if (inventorySlot.CurrentItem is Equipment eq)
                    {
                        UIManager.Instance.tooltipText.text ="착용레벨:"+ eq.limitLevel + "\n"+
                                                             "공격력:"+ eq.attackPower + "\n"+
                                                             "마법공격력:"+ eq.magicPower + "\n"+
                                                             "방어력:"+ eq.defensePower + "\n"+
                                                             eq.description +"\n"+"더블클릭으로 장착 및 해제가 가능하다.";
                        return;
                    }
                    else if (inventorySlot.CurrentItem is Consumable co)
                    {
                        UIManager.Instance.tooltipText.text = co.description +"\n"+"더블클릭으로 사용이 가능하다.";
                        return;
                    }
                   
                }
            }
        }
    }
    
    //마우스가 가르킨 오브젝트 없으면끄기
    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.Instance.tooltipText != null)
        {
            UIManager.Instance.tooltipGroup.SetActive(false);
        }
    }
}

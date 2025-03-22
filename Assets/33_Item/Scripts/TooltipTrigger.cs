using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject tooltip; // UI에서 연결할 텍스트 오브젝트 (비활성화 상태)
    
    
    private void Start()
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false); // 시작 시 숨김
        }
    }
    
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(true); // 마우스를 올리면 표시
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
        {
            tooltip.SetActive(false); // 마우스를 떼면 숨김
        }
    }
}

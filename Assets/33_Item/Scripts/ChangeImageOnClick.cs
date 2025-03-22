using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 이벤트 인터페이스 사용

public class ChangeImageOnClick : MonoBehaviour, IPointerClickHandler
{
    public Sprite newSprite;   // 변경할 스프라이트
    private Sprite originalSprite;  // 원래 스프라이트 저장
    private Image targetImage; // 이미지 컴포넌트
    public  GameObject CurrentClickedGameObject;

    private static ChangeImageOnClick lastClicked;  // 마지막 클릭된 오브젝트

    void Start()
    {
        if (targetImage != null)
        {
            originalSprite = targetImage.sprite; // 초기 이미지 저장
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        CurrentClickedGameObject = eventData.pointerCurrentRaycast.gameObject;
        targetImage = CurrentClickedGameObject.GetComponent<Image>();
        // 이전 클릭된 이미지가 있다면 원래 이미지로 변경
        if (lastClicked != null && lastClicked != this)
        {
            lastClicked.ResetImage();
        }

        // 현재 이미지 변경
        if (targetImage != null && newSprite != null)
        {
            targetImage.sprite = newSprite;
        }

        // 현재 오브젝트를 마지막 클릭된 것으로 설정
        lastClicked = this;
    }

    public void ResetImage()
    {
        if (targetImage != null)
        {
            CurrentClickedGameObject = null;
            targetImage.sprite = originalSprite;
        }
    }
}
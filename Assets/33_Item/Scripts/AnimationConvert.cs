using UnityEngine;

public class AnimationConvert : MonoBehaviour
{
   
   /// <summary>
   /// 애니메이션 이벤트로 사용되는 함수 
   /// </summary>
   public void IdleStart()
   {
      GetComponent<Animator>().SetTrigger("Idle");
   }
   
}

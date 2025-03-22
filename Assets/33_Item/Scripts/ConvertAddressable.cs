using UnityEngine;
using UnityEngine.AddressableAssets;


//어드레서블 상위 오브젝트에 할당하는 컴포넌트 OnDisable할때 메모리를 같이 해제해줌
public class ConvertAddressable : MonoBehaviour
{
    
    private void OnDisable()
    {
        Addressables.ReleaseInstance(this.gameObject);
    }
}

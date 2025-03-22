using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerCanvas : MonoBehaviour
{
    Transform playerTransform;

    private void Start()
    {
        playerTransform = GetComponentInParent<Transform>();
    }

    public void FlipCanvas(bool isRight)
    {
        Vector3 scale = transform.localScale;

        if (isRight == true)
        {
            scale.x = -1f;
        }
        else
        {
            scale.x = 1f;
        }
        transform.localScale = scale;
    }
}

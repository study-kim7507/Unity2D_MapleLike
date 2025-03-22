using UnityEngine;

public class LevelUpSpawner : MonoBehaviour
{
    PlayerController pc;
    Vector3 scale;

    private void Start()
    {
        pc = GetComponentInParent<PlayerController>();
        scale = transform.localScale;
    }

    private void Update()
    {
        // 플레이어 방향이 바뀌어도 스프라이트 방향 유지
        if (pc.isRight == false)
        {
            transform.localScale = scale;
        }
        else
        {
            Vector3 tempScale = scale;
            tempScale.x *= -1f;
            transform.localScale = tempScale;
        }
    }

    public void DestroyLevelUp()
    {
        Destroy(gameObject);
    }
}

using Google.Protobuf.Protocol;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class BaseController : MonoBehaviour
{
    public int Id { get; set; }

    protected Vector3 destinationPos = new Vector3();

    protected virtual void Update()
    {
        //SyncPos();
    }

    protected virtual void FixedUpdate()
    {
        SyncPos();
    }

    public void SyncPos()
    {
        // destinationPos : S_Move패킷을 통해 갱신된 목표 위치
        float distance = Vector3.Distance(transform.position, destinationPos);


        // 목표지점에 거의 다왔으면 목표 위치로 순간이동
        if (distance < 0.1f)
        {
            transform.position = destinationPos;
        }
        else
        {
            // 스르륵 이동
            float speed = Mathf.Clamp(distance * 2f, 20f, 30f); // 최소 N, 최대 M으로 이동속도 설정
            transform.position = Vector3.MoveTowards(transform.position, destinationPos, speed * Time.fixedDeltaTime);
        }
    }

    public void SetDestination(float x, float y)
    {
        destinationPos.x = x;
        destinationPos.y = y;
    }
}

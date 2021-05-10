using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamearMovement : MonoBehaviour
{
    public float move_speed = 1;

    // Update is called once per frame
    void Update()
    {
        // z 깊이 좌표는 항상 0 이다.
        Vector3 mouse_pos = Input.mousePosition;
        Vector3 move_pos = Vector3.zero;

        // 쿼터 뷰 형식의 카메라 이기 때문에
        // 로컬 포지션의 값을 변경시켜 준다.
        if (mouse_pos.x < 4)
        {
            move_pos -= transform.right * move_speed;
        }
        else if (mouse_pos.x > (Screen.width - 4))
        {
            move_pos += transform.right * move_speed;
        }

        if (mouse_pos.y < 4)
        {
            move_pos -= transform.up * move_speed;
        }
        else if (mouse_pos.y > (Screen.height - 4))
        {
            move_pos += transform.up * move_speed;
        }

        // 움직임 처리.
        if (move_pos != Vector3.zero)
        {
            transform.position += move_pos;
        }
    }
}

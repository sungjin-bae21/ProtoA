using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectSettingManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MouseSetting();
    }

    void MouseSetting()
    {
        Cursor.visible = true;
        // 독립형 빌드에서만 지원됨.
        Cursor.lockState = CursorLockMode.Confined;
    }
}

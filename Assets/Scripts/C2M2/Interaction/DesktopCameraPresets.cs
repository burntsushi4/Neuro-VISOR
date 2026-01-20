using UnityEngine;
using C2M2.Utils;
public class DesktopCameraPresets : MonoBehaviour
{
    public Transform preset1;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            transform.position = preset1.position;
            transform.rotation = preset1.rotation;
        }
    }
}

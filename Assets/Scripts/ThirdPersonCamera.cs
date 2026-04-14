using UnityEngine;

/// <summary>
/// ThirdPersonCamera — كاميرا تتبع اللاعب من الخلف والأعلى.
/// يتضاف تلقائياً عن طريق GameBootstrap.
/// </summary>
public class ThirdPersonCamera : MonoBehaviour
{
    public Transform target;
    public float distance    = 8f;
    public float height      = 5f;
    public float smoothSpeed = 8f;
    public float mouseSensX  = 3f;
    public float mouseSensY  = 2f;

    private float _yaw;
    private float _pitch = 20f;

    void LateUpdate()
    {
        if (target == null) return;

        // Mouse look
        _yaw   += Input.GetAxis("Mouse X") * mouseSensX;
        _pitch  -= Input.GetAxis("Mouse Y") * mouseSensY;
        _pitch   = Mathf.Clamp(_pitch, 5f, 60f);

        Quaternion rot    = Quaternion.Euler(_pitch, _yaw, 0);
        Vector3    offset = rot * new Vector3(0, 0, -distance) + Vector3.up * height;
        Vector3    desiredPos = target.position + offset;

        transform.position = Vector3.Lerp(transform.position, desiredPos, smoothSpeed * Time.deltaTime);
        transform.LookAt(target.position + Vector3.up * 1f);
    }
}

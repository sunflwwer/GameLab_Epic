// CameraYProxy.cs
using UnityEngine;

public class CameraYProxy : MonoBehaviour
{
    [Header("Target to read Y from")]
    [SerializeField] private Transform target;   // Player 등

    [Header("Fixed X for camera when inside zone")]
    [SerializeField] private float fixedX = 0f;

    [Header("Optional: Z to use for camera")]
    [SerializeField] private float fixedZ = -10f;

    public void SetTarget(Transform t) => target = t;
    public void SetFixedX(float x) => fixedX = x;

    private void LateUpdate()
    {
        if (!target) return;
        Vector3 p = transform.position;
        p.x = fixedX;
        p.y = target.position.y;
        p.z = fixedZ;
        transform.position = p;
    }
}
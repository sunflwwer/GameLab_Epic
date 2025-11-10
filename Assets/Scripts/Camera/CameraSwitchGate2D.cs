// CameraSwitchGate2D.cs
using UnityEngine;
using Cinemachine;

public class CameraSwitchGate2D : MonoBehaviour
{
    public enum GateType { EnterZone, ExitZone }

    [Header("Gate Mode")]
    [SerializeField] private GateType mode = GateType.EnterZone;

    [Header("Cameras")]
    [SerializeField] private CinemachineVirtualCamera normalCam;   // VCam_A
    [SerializeField] private CinemachineVirtualCamera zoneCam;     // VCam_B

    [Header("Priorities")]
    [SerializeField] private int normalPriority = 10;
    [SerializeField] private int zonePriority = 20;

    [Header("Player Filter")]
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;

        if (mode == GateType.EnterZone)
        {
            if (normalCam) normalCam.Priority = normalPriority;
            if (zoneCam)   zoneCam.Priority   = zonePriority;
        }
        else // ExitZone
        {
            if (zoneCam)   zoneCam.Priority   = normalPriority;
            if (normalCam) normalCam.Priority = zonePriority;
        }
    }
}
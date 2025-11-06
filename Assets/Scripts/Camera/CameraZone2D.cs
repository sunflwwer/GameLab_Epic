using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CameraZone2D : MonoBehaviour
{
    public enum ZoneMode { FullFollow, YOnly }

    [Header("Zone")]
    [SerializeField] private ZoneMode zoneMode = ZoneMode.YOnly;
    [SerializeField] private bool setFixedX = false;      // true면 zoneFixedX 사용
    [SerializeField] private float zoneFixedX = 0f;       // 이 존에서 강제로 사용할 X

    [Header("Refs")]
    [SerializeField] private CameraFollowProxy proxy;
    [SerializeField] private string playerTag = "Player";

    private void Reset()
    {
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag)) return;
        if (proxy == null) proxy = FindObjectOfType<CameraFollowProxy>();
        if (proxy == null) return;

        if (zoneMode == ZoneMode.FullFollow)
        {
            proxy.SetMode(CameraFollowProxy.Mode.FullFollow);
        }
        else
        {
            if (setFixedX)
                proxy.SetYOnlyWithFixedX(zoneFixedX);
            else
                proxy.SetMode(CameraFollowProxy.Mode.YOnly); // 들어갈 순간 플레이어 X 고정
        }
    }
}
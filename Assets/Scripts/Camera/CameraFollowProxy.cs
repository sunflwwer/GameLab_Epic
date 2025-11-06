using UnityEngine;

public class CameraFollowProxy : MonoBehaviour
{
    public enum Mode { FullFollow, YOnly }

    [Header("Targets")]
    [SerializeField] private Transform player;   // 플레이어 Transform
    [SerializeField] private Mode mode = Mode.FullFollow;

    [Header("YOnly 옵션")]
    [Tooltip("Y만 추적일 때 고정할 X 값. lockXToPlayerAtEnter=true면 무시됨.")]
    [SerializeField] private float fixedX = 0f;
    [Tooltip("지하로 들어갈 때 '그 순간의 플레이어 X'로 고정할지 여부")]
    [SerializeField] private bool lockXToPlayerAtEnter = true;

    private float lockedX;        // YOnly 모드일 때 사용할 X
    private bool initialized;

    private void Awake()
    {
        if (player == null)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        lockedX = fixedX;
        initialized = true;
    }

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 pos = transform.position;

        if (mode == Mode.FullFollow)
        {
            pos.x = player.position.x;
            pos.y = player.position.y;
        }
        else // YOnly
        {
            pos.x = lockedX;
            pos.y = player.position.y;
        }

        pos.z = transform.position.z; // z는 유지(카메라 거리)
        transform.position = pos;
    }

    // ----- 외부에서 모드 전환 -----
    public void SetMode(Mode newMode)
    {
        if (mode == newMode) return;

        if (newMode == Mode.YOnly)
        {
            // 들어가는 순간의 X를 고정하거나, 설정된 fixedX를 사용
            lockedX = lockXToPlayerAtEnter && player != null ? player.position.x : fixedX;
        }
        mode = newMode;
    }

    // 존에서 X 고정값을 지정하고 싶을 때 호출
    public void SetYOnlyWithFixedX(float x)
    {
        mode = Mode.YOnly;
        lockedX = x;
    }

    public void SetPlayer(Transform t)
    {
        player = t;
    }
}
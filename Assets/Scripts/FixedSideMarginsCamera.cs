using UnityEngine;

/// <summary>
/// 카메라의 Viewport Rect를 고정 비율로 줄여
/// 좌우에 항상 동일한 여백(필러박스)을 만든다.
/// UI(Canvas Overlay)는 영향 받지 않는다.
/// </summary>
[ExecuteAlways]
[RequireComponent(typeof(Camera))]
public class FixedSideMarginsCamera : MonoBehaviour
{
    [Range(0f, 0.45f)]
    [Tooltip("좌우 여백 비율(스크린 가로 대비). 0.1이면 좌우 각각 10% 여백.")]
    public float sideMargin = 0.1f;

    private Camera _cam;
    private float _lastMargin;
    private int _lastW, _lastH;

    private void OnEnable()
    {
        _cam = GetComponent<Camera>();
        Apply();
    }

    private void OnValidate() => Apply();

    private void Update()
    {
        // 창 크기나 해상도 변화 감지 시 재적용
        if (Screen.width != _lastW || Screen.height != _lastH || !Mathf.Approximately(_lastMargin, sideMargin))
        {
            Apply();
        }
    }

    private void Apply()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        if (_cam == null) return;

        _lastW = Screen.width;
        _lastH = Screen.height;
        _lastMargin = sideMargin;

        float x = Mathf.Clamp01(sideMargin);
        float width = Mathf.Clamp01(1f - (x * 2f));

        // 좌우 여백: x만큼, y는 풀 높이
        _cam.rect = new Rect(x, 0f, width, 1f);

        // 여백 색을 쓰려면 Solid Color 권장
        if (_cam.clearFlags != CameraClearFlags.SolidColor)
            _cam.clearFlags = CameraClearFlags.SolidColor;
    }
}
// FixedSideMarginsCamera.cs
using UnityEngine;
#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HIGH_DEFINITION
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
#endif

[DisallowMultipleComponent]
[RequireComponent(typeof(Camera))]
public class FixedSideMarginsCamera : MonoBehaviour
{
    [Range(0f, 0.45f)]
    [Tooltip("좌우 여백 비율(스크린 가로 대비). 0.1이면 좌우 각각 10% 여백.")]
    public float sideMargin = 0.1f;

    [Tooltip("URP 카메라 스택 사용 시 Base 카메라에만 적용하도록 강제")]
    public bool urpBaseOnly = true;

    private Camera _cam;

    private void OnEnable()
    {
        _cam = GetComponent<Camera>();
        EnsureSolidColor(_cam);

#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HIGH_DEFINITION
        RenderPipelineManager.beginCameraRendering += OnBeginCameraRendering;
#else
        Camera.onPreCull += OnPreCullCamera;
#endif
    }

    private void OnDisable()
    {
#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HIGH_DEFINITION
        RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
#else
        Camera.onPreCull -= OnPreCullCamera;
#endif
        // 필요 시 비활성화될 때 원상복구
        if (_cam) _cam.rect = new Rect(0, 0, 1, 1);
    }

    private void OnValidate()
    {
        if (_cam == null) _cam = GetComponent<Camera>();
        Apply(_cam);
    }

#if UNITY_RENDER_PIPELINES_UNIVERSAL || UNITY_RENDER_PIPELINES_HIGH_DEFINITION
    private void OnBeginCameraRendering(ScriptableRenderContext ctx, Camera cam)
    {
        if (cam != _cam) return;

        if (urpBaseOnly && TryGetURPData(cam, out var urpData))
        {
            // Base 카메라에만 적용, Overlay는 풀 rect
            if (urpData.renderType == CameraRenderType.Base) Apply(cam);
            else cam.rect = new Rect(0, 0, 1, 1);
        }
        else
        {
            Apply(cam);
        }
    }

    private static bool TryGetURPData(Camera cam, out UniversalAdditionalCameraData data)
    {
        data = cam.GetComponent<UniversalAdditionalCameraData>();
        return data != null;
    }
#else
    private void OnPreCullCamera(Camera cam)
    {
        if (cam != _cam) return;
        Apply(cam);
    }
#endif

    private void Apply(Camera cam)
    {
        if (cam == null) return;

        float x = Mathf.Clamp01(sideMargin);
        float width = Mathf.Clamp01(1f - (x * 2f));
        cam.rect = new Rect(x, 0f, width, 1f);
    }

    private static void EnsureSolidColor(Camera cam)
    {
        if (cam == null) return;
        if (cam.clearFlags != CameraClearFlags.SolidColor)
            cam.clearFlags = CameraClearFlags.SolidColor;
    }
}

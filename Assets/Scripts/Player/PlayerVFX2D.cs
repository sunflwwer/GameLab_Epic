using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플레이어 전용 VFX:
/// - 피격 플래시(Animator 없이 동작)
/// - 무적 시간(i-frame) 점멸 표시
/// - 사망 연출(팝 + 옵션 페이드) ※ 플레이어는 파괴하지 않음
/// - SpriteRenderer가 다른 오브젝트에 있어도 동작(참조로 연결 또는 자동 탐색)
/// </summary>
public class PlayerVFX2D : MonoBehaviour
{
    [Header("Target Renderers")]
    [Tooltip("플레이어 시각 요소에 해당하는 SpriteRenderer들(몸 파츠가 여러 개면 모두 넣기). 비우면 하위에서 자동 탐색.")]
    [SerializeField] private List<SpriteRenderer> renderers;

    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new Color(1f, 0.4f, 0.4f, 1f);
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashBlinkCount = 1;

    [Header("I-Frame Blink")]
    [SerializeField] private bool useIFrameBlink = true;
    [SerializeField] private float blinkInterval = 0.08f;   // 점멸 주기
    [SerializeField] private bool blinkByAlpha = true;      // true: 알파 깜빡임, false: enabled on/off

    [Header("Death Pop (연출만)")]
    [SerializeField] private float popScale = 1.2f;
    [SerializeField] private float popDuration = 0.1f;
    [SerializeField] private bool fadeOutOnDeath = false;   // 플레이어는 기본 false 권장
    [SerializeField] private float fadeDuration = 0.1f;

    [Header("Optional Disable During Death")]
    [SerializeField] private bool disableColliderOnDeath = false;
    [SerializeField] private bool disableRigidbodyOnDeath = false;

    // 내부 상태
    private Color[] originalColors;
    private Vector3 originalScale;
    private Coroutine flashCo;
    private Coroutine blinkCo;

    private void Awake()
    {
        // 타겟 미지정이면 자식들에서 자동 수집
        if (renderers == null || renderers.Count == 0)
        {
            renderers = new List<SpriteRenderer>(GetComponentsInChildren<SpriteRenderer>(includeInactive: true));
        }

        // 기본값 백업
        originalScale = transform.localScale;

        if (renderers == null) renderers = new List<SpriteRenderer>();
        originalColors = new Color[renderers.Count];
        for (int i = 0; i < renderers.Count; i++)
        {
            if (renderers[i] != null) originalColors[i] = renderers[i].color;
        }
    }

    // ---------- Public API ----------

    /// <summary>피격 시 한 번 번쩍</summary>
    public void PlayHitFlash()
    {
        if (!isActiveAndEnabled) return;
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(HitFlashRoutine());
    }

    /// <summary>히트 플래시 총 시간(= flashDuration * flashBlinkCount)</summary>
    public float TotalFlashTime => Mathf.Max(0.01f, flashDuration * Mathf.Max(1, flashBlinkCount));

    /// <summary>i-frame을 'delay' 뒤에 시작(히트 플래시가 끝난 뒤 시작시키는 용도)</summary>
    public void StartIFrameBlink(float duration, float delay = 0f)
    {
        if (!useIFrameBlink || duration <= 0f) return;
        if (blinkCo != null) StopCoroutine(blinkCo);
        blinkCo = StartCoroutine(IFrameBlinkRoutine(duration, delay));
    }

    /// <summary>
    /// 사망 연출(팝 + 옵션 페이드). 플레이어는 파괴하지 않음.
    /// maxDurationFromHealth: 외부(Health)에서 주는 최대 연출 시간
    /// </summary>
    public void PlayDeathPop(float maxDurationFromHealth)
    {
        float popTime = Mathf.Min(popDuration, Mathf.Max(0.02f, maxDurationFromHealth * 0.6f));
        float fadeTime = fadeOutOnDeath ? Mathf.Min(fadeDuration, Mathf.Max(0.02f, maxDurationFromHealth - popTime)) : 0f;
        StartCoroutine(DeathRoutine(popTime, fadeTime));
    }

    /// <summary>외형 초기화(색/스케일/알파/점멸 강제 해제)</summary>
    public void ResetVisual()
    {
        if (flashCo != null) { StopCoroutine(flashCo); flashCo = null; }
        if (blinkCo != null) { StopCoroutine(blinkCo); blinkCo = null; }
        transform.localScale = originalScale;

        // 색/활성 상태 복원
        for (int i = 0; i < renderers.Count; i++)
        {
            var r = renderers[i];
            if (!r) continue;
            if (originalColors != null && i < originalColors.Length && originalColors[i] != default)
                r.color = new Color(originalColors[i].r, originalColors[i].g, originalColors[i].b, 1f);
            else
                r.color = new Color(1f, 1f, 1f, 1f);
            r.enabled = true;
        }
    }

    // ---------- Routines ----------

    private IEnumerator HitFlashRoutine()
    {
        for (int i = 0; i < Mathf.Max(1, flashBlinkCount); i++)
        {
            // 히트색
            for (int j = 0; j < renderers.Count; j++)
            {
                if (!renderers[j]) continue;
                renderers[j].color = hitColor;
            }
            yield return new WaitForSecondsRealtime(flashDuration * 0.5f);

            // 원래색
            for (int j = 0; j < renderers.Count; j++)
            {
                if (!renderers[j]) continue;
                // 원래색 배열이 없거나 범위를 넘어가면 현재 색상의 RGB를 유지하고 알파만 1로
                Color c = (originalColors != null && j < originalColors.Length && originalColors[j] != default)
                          ? originalColors[j]
                          : renderers[j].color;
                renderers[j].color = new Color(c.r, c.g, c.b, 1f);
            }
            yield return new WaitForSecondsRealtime(flashDuration * 0.5f);
        }
        flashCo = null;
    }

    private IEnumerator IFrameBlinkRoutine(float duration, float delay)
    {
        if (delay > 0f)
            yield return new WaitForSecondsRealtime(delay);

        float t = 0f;
        bool toggle = false;

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;   // 히트스톱 무시
            toggle = !toggle;

            if (blinkByAlpha)
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    var r = renderers[i];
                    if (!r) continue;
                    var c = r.color;
                    c.a = toggle ? 0.35f : 1f;
                    r.color = c;
                }
            }
            else
            {
                for (int i = 0; i < renderers.Count; i++)
                {
                    var r = renderers[i];
                    if (!r) continue;
                    r.enabled = toggle;
                }
            }

            yield return new WaitForSecondsRealtime(blinkInterval);
        }

        // 종료 시 정상화
        if (blinkByAlpha)
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var r = renderers[i];
                if (!r) continue;
                var c = r.color; c.a = 1f; r.color = c;
            }
        }
        else
        {
            for (int i = 0; i < renderers.Count; i++)
            {
                var r = renderers[i];
                if (!r) continue;
                r.enabled = true;
            }
        }

        blinkCo = null;
    }

    private IEnumerator DeathRoutine(float popTime, float fadeTime)
    {
        if (disableColliderOnDeath)
        {
            var col = GetComponentInParent<Collider2D>();
            if (col) col.enabled = false;
        }
        if (disableRigidbodyOnDeath)
        {
            var rb = GetComponentInParent<Rigidbody2D>();
            if (rb)
            {
                rb.linearVelocity = Vector2.zero;
                rb.isKinematic = true;
                rb.simulated = false;
            }
        }

        // 1) 팝
        float t = 0f;
        Vector3 startScale = transform.localScale;
        while (t < popTime)
        {
            t += Time.unscaledDeltaTime; // 히트스톱 무시
            float k = Mathf.Clamp01(t / popTime);
            transform.localScale = Vector3.LerpUnclamped(startScale, startScale * popScale, k);
            yield return null;
        }

        // 2) 페이드(옵션)
        if (fadeOutOnDeath && renderers.Count > 0)
        {
            t = 0f;
            // 현재 색상 기준으로 알파만 보간
            Color[] start = new Color[renderers.Count];
            for (int i = 0; i < renderers.Count; i++)
                if (renderers[i]) start[i] = renderers[i].color;

            while (t < fadeTime)
            {
                t += Time.unscaledDeltaTime; // 히트스톱 무시
                float k = Mathf.Clamp01(t / fadeTime);
                for (int i = 0; i < renderers.Count; i++)
                {
                    var r = renderers[i];
                    if (!r) continue;
                    var c = start[i];
                    r.color = new Color(c.r, c.g, c.b, Mathf.Lerp(c.a, 0f, k));
                }
                yield return null;
            }
        }
    }
}

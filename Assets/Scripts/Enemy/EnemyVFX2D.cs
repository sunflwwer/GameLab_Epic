using System.Collections;
using UnityEngine;

/// <summary>
/// 적 피격/사망 시 비주얼 연출 전담.
/// - Animator 없어도 동작
/// - SpriteRenderer.color 플래시, 스케일 팝
/// </summary>
[RequireComponent(typeof(SpriteRenderer))]
public class EnemyVFX2D : MonoBehaviour
{
    [Header("Hit Flash")]
    [SerializeField] private Color hitColor = new Color(1f, 0.25f, 0.25f, 1f);
    [SerializeField] private float flashDuration = 0.08f;
    [SerializeField] private int flashBlinkCount = 1; // 1이면 한번 번쩍

    [Header("Death Pop")]
    [SerializeField] private float popScale = 1.25f;     // 죽을 때 잠깐 커졌다가
    [SerializeField] private float popDuration = 0.08f;  // 이 시간 동안 scale 보간
    [SerializeField] private bool fadeOutOnDeath = true; // 투명도 페이드아웃 할지
    [SerializeField] private float fadeDuration = 0.08f; // 페이드 시간

    [Header("Optional")]
    [SerializeField] private bool disableColliderOnDeath = true;
    [SerializeField] private bool disableRigidbodyOnDeath = true;

    private SpriteRenderer sr;
    private Color originalColor;
    private Vector3 originalScale;
    private Coroutine flashCo;

    private void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        originalColor = sr.color;
        originalScale = transform.localScale;
    }

    public void PlayHitFlash()
    {
        if (!isActiveAndEnabled) return;
        if (flashCo != null) StopCoroutine(flashCo);
        flashCo = StartCoroutine(HitFlashRoutine());
    }

    public void PlayDeathPop(float maxDurationFromHealth)
    {
        if (!isActiveAndEnabled) return;
        // Health에서 주는 deathDelay 안에 연출을 끝내기 위해, 너무 길면 잘라줌
        float popTime = Mathf.Min(popDuration, Mathf.Max(0.01f, maxDurationFromHealth * 0.6f));
        float fadeTime = fadeOutOnDeath ? Mathf.Min(fadeDuration, Mathf.Max(0.01f, maxDurationFromHealth - popTime)) : 0f;
        StartCoroutine(DeathRoutine(popTime, fadeTime));
    }

    private IEnumerator HitFlashRoutine()
    {
        // 깜빡임 횟수만큼 원색↔히트색 왕복
        for (int i = 0; i < Mathf.Max(1, flashBlinkCount); i++)
        {
            sr.color = hitColor;
            yield return new WaitForSeconds(flashDuration * 0.5f);
            sr.color = originalColor;
            yield return new WaitForSeconds(flashDuration * 0.5f);
        }
        flashCo = null;
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

        // 1) 팝(스케일 업)
        float t = 0f;
        while (t < popTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / popTime);
            transform.localScale = Vector3.Lerp(originalScale, originalScale * popScale, k);
            yield return null;
        }

        // 2) 페이드아웃(옵션)
        if (fadeOutOnDeath)
        {
            Color c0 = sr.color;
            t = 0f;
            while (t < fadeTime)
            {
                t += Time.deltaTime;
                float k = Mathf.Clamp01(t / fadeTime);
                sr.color = new Color(c0.r, c0.g, c0.b, Mathf.Lerp(c0.a, 0f, k));
                yield return null;
            }
        }
    }
}

using System;                        // ← 이벤트용
using GMTK.PlatformerToolkit;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerHealth2D : MonoBehaviour, IHittable
{
    [Header("HP")]
    [SerializeField] private int maxHp = 5;
    [SerializeField] private int currentHp;

    // 외부(UI)에서 읽을 수 있게 프로퍼티 제공
    public int CurrentHp => currentHp;
    public int MaxHp => maxHp;

    // HP 변경 알림 이벤트
    public event Action<int, int> OnHealthChanged;

    [Header("Invincible")]
    [SerializeField] private float iFrame = 0.25f;
    private float lastHitTime = -999f;

    [Header("VFX (플레이어 전용)")]
    [SerializeField] private PlayerVFX2D vfx;

    private characterHurt hurt; // 리스폰 루틴 연동용

    private void Awake()
    {
        currentHp = maxHp;
        if (!vfx) vfx = GetComponent<PlayerVFX2D>();
        hurt = GetComponent<GMTK.PlatformerToolkit.characterHurt>();
        RaiseHealthChanged();    // 초기값 브로드캐스트
    }

    public void RestoreFull()
    {
        currentHp = maxHp;
        RaiseHealthChanged();
        // 리스폰 직후 외형 초기화(선택)
        vfx?.ResetVisual();
    }

    public void TakeDamage(int amount = 1)
    {
        if (Time.time - lastHitTime < iFrame) return; 
        lastHitTime = Time.time;

        if (vfx)
        {
            vfx.PlayHitFlash();

            // 히트 플래시가 끝난 뒤에 I-Frame 점멸 시작
            float delay = vfx.TotalFlashTime;          // = flashDuration * flashBlinkCount
            vfx.StartIFrameBlink(iFrame, delay);
        }

        currentHp = Mathf.Max(0, currentHp - Mathf.Max(1, amount));
        RaiseHealthChanged();

        if (currentHp <= 0)
        {
            if (vfx) vfx.PlayDeathPop(0.2f);
            if (hurt) hurt.hurtRoutine();
        }
    }


    // ===== IHittable 구현 =====
    public void Hit(int damage, Vector3 hitPoint)
    {
        TakeDamage(damage);
    }

    // 이벤트/외부 UI 갱신 트리거
    private void RaiseHealthChanged()
    {
        OnHealthChanged?.Invoke(currentHp, maxHp);
    }
}
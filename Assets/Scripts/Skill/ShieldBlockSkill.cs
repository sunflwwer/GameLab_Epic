using GMTK.PlatformerToolkit;
using UnityEngine;

public class ShieldBlockSkill : SkillBase
{
    [Header("Target")]
    [SerializeField] private Transform shield;

    [Header("Hit Area")]
    [SerializeField] private float range   = 1.0f;
    [SerializeField] private float offsetX = -0.7f;
    [SerializeField] private float offsetY = 0.0f;

    [Header("Damage to Enemies")]
    [SerializeField] private bool canDamageEnemies = false; // ★ 기본 false: 방패는 공격하지 않음
    [SerializeField] private int damage = 1;                // (true일 때만 사용)
    [SerializeField] private string enemyTag = "Enemy";

    [Header("FX (옵션)")]
    [SerializeField] private GameObject hitVfx;
    [SerializeField] private float vfxLifetime = 0.25f;
    [SerializeField] private AudioSource sfx;

    [Header("Bounce On Hit")]
    [SerializeField] private bool bounceOnHit = true;
    [SerializeField] private float bounceAmount = 6f;
    [SerializeField] private LayerMask groundLayer;

    [Header("Block Bullets")]
    [SerializeField] private bool blockEnemyBullets = true;
    [SerializeField] private LayerMask bulletLayer = 0;

    [Header("Hold Options")]
    [SerializeField] private bool holdToBlock = true;
    [SerializeField] private float checkInterval = 0.04f;

    [Header("Bounce Control")]
    [SerializeField] private bool   bounceOncePerHold   = true;
    [SerializeField] private float  minBounceInterval   = 0.12f;
    [SerializeField] private bool   requireFallingToBounce = true;
    private bool  bouncedThisHold = false;
    private float lastBounceTime  = -999f;

    // 패링(총알 차단) 성공 효과
    [Header("Parry (Bullet Block) Effects")]
    [SerializeField] private bool enableParryBounce = true;
    [SerializeField] private float parryBounceAmount = 3f;
    [SerializeField] private bool  parryBypassCooldown = true;

    [SerializeField] private bool enableParryLightBurst = true;
    [SerializeField] private Light2DPulse lightPulse;
    [SerializeField] private float parryLightOuter = 10f;
    [SerializeField] private float parryLightInner = 1.5f;

    private bool  isBlocking;
    private float nextCheck;

    protected override void OnActivate(Transform caster)
    {
        if (!holdToBlock)
        {
            DoBlockTick();
            return;
        }
        StartBlocking();
    }

    public void StartBlocking()
    {
        if (isBlocking) return;
        isBlocking = true;
        nextCheck = 0f;
        bouncedThisHold = false;
        lastBounceTime  = -999f;
        if (sfx) sfx.Play();
    }

    public void StopBlocking()
    {
        if (!isBlocking) return;
        isBlocking = false;
        bouncedThisHold = false;
    }

    private void Update()
    {
        if (!isBlocking) return;
        if (Time.time >= nextCheck)
        {
            nextCheck = Time.time + checkInterval;
            DoBlockTick();
        }
    }

    private void DoBlockTick()
    {
        Transform caster = transform.root ? transform.root : transform;
        float dir = Mathf.Sign(caster.localScale.x == 0 ? 1 : caster.localScale.x);
        Vector2 center = (Vector2)caster.position + new Vector2(offsetX * dir, offsetY);

        var hits = Physics2D.OverlapCircleAll(center, range, ~0);

        bool hitGround      = Physics2D.OverlapCircle(center, range, groundLayer);
        bool blockedBullet  = false;
        bool hitEnemyForBounce = false; // canDamageEnemies=true일 때만 의미

        foreach (var h in hits)
        {
            if (!h) continue;

            // 1) 적 총알 차단(패링)
            if (blockEnemyBullets)
            {
                if (bulletLayer.value != 0)
                {
                    int hitMask = 1 << h.gameObject.layer;
                    if ((bulletLayer.value & hitMask) != 0)
                    {
                        var bullet = h.GetComponentInParent<EnemyBullet2D>();
                        if (bullet != null)
                        {
                            Destroy(bullet.gameObject);
                            blockedBullet = true;
                            continue;
                        }
                    }
                }
                else
                {
                    var bullet = h.GetComponentInParent<EnemyBullet2D>();
                    if (bullet != null)
                    {
                        Destroy(bullet.gameObject);
                        blockedBullet = true;
                        continue;
                    }
                }
            }

            // 2) 적 데미지: 토글 off면 완전히 스킵
            if (canDamageEnemies)
            {
                var root = h.attachedRigidbody ? h.attachedRigidbody.transform.root : h.transform.root;
                if (root && root.CompareTag(enemyTag))
                {
                    var hp = h.GetComponentInParent<EnemyHealth2D>();
                    if (hp != null)
                    {
                        hp.TakeDamage(damage);
                        hitEnemyForBounce = true;
                    }
                }
            }
        }

        // ---- 기본 바운스: 적 히트(옵션) 또는 지면 접촉 ----
        // ---- 기본 바운스: 적 히트(옵션) 또는 지면 접촉 ----
        bool wantBounce = bounceOnHit && (hitEnemyForBounce || hitGround);

// 패링 성공 프레임이면 기본 바운스 끄기(중복 방지)
        if (blockedBullet) wantBounce = false;

        if (wantBounce)
        {
            if (requireFallingToBounce)
            {
                var rb = caster.GetComponent<Rigidbody2D>();
                if (rb && rb.linearVelocity.y > 0f) wantBounce = false; // ← 오타 수정
            }
            if (Time.time - lastBounceTime < minBounceInterval) wantBounce = false;
            if (bounceOncePerHold && bouncedThisHold)          wantBounce = false;

            if (wantBounce)
            {
                var jump = caster.GetComponent<GMTK.PlatformerToolkit.characterJump>();
                if (jump != null)
                {
                    jump.bounceUp(bounceAmount);
                    lastBounceTime  = Time.time;
                    bouncedThisHold = true;
                }
            }
        }


        // ---- 패링 성공 효과(총알 차단) ----
        if (blockedBullet)
        {
            ApplyParryEffects(caster);
        }

        // VFX
        if (hitVfx)
        {
            var v = Instantiate(hitVfx, center, Quaternion.identity);
            Destroy(v, vfxLifetime);
        }
    }

    private void ApplyParryEffects(Transform caster)
    {
        // 1) 중앙 조율자에 '패링 성공' 보고 → 콤보 윈도우 내면 고점프
        var combo = caster.GetComponent<PlayerComboJump>();
        if (combo != null)
        {
            combo.ReportShieldParry();
        }
        else if (enableParryBounce) // 하위호환: 기존 개별 튕김
        {
            var jump = caster.GetComponent<GMTK.PlatformerToolkit.characterJump>();
            if (jump != null) jump.bounceUp(parryBounceAmount);
        }

        // 2) 빛 반경 확장(기존)
        if (enableParryLightBurst && lightPulse)
        {
            lightPulse.PulseTo(parryLightOuter, parryLightInner);
        }
    }


#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Transform caster = transform.root != null ? transform.root : transform;
        float dir = Mathf.Sign(caster.localScale.x == 0 ? 1 : caster.localScale.x);
        Vector2 center = (Vector2)caster.position + new Vector2(offsetX * dir, offsetY);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, range);
    }
#endif
}

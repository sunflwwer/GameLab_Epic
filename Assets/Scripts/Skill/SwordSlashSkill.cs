using GMTK.PlatformerToolkit;
using UnityEngine;

public class SwordSlashSkill : SkillBase
{
    [Header("Hit Area")]
    [SerializeField] private float range   = 1.0f;   // 원형 범위
    [SerializeField] private float offsetX = +0.7f;  // +면 오른쪽, -면 왼쪽
    [SerializeField] private float offsetY = 0.0f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private string enemyTag = "Enemy";

    [Header("FX (옵션)")]
    [SerializeField] private GameObject slashVfx;
    [SerializeField] private float vfxLifetime = 0.25f;
    [SerializeField] private AudioSource sfx;

    [Header("Bounce On Hit")]
    [SerializeField] private bool bounceOnHit = true;     // 적/바닥 히트 시 튕김
    [SerializeField] private float bounceAmount = 6f;     // 튕김 세기
    [SerializeField] private LayerMask groundLayer;       // 바닥 레이어 지정

    // facingSource 삭제됨

    protected override void OnActivate(Transform caster)
    {
        Debug.Log("[Sword] Attack input");

        // 방향 고정: 항상 오른쪽(+1). 항상 왼쪽을 원하면 -1f 로 바꾸세요.
        const float dir = +1f;

        // 판정 중심
        Vector2 center = (Vector2)caster.position + new Vector2(offsetX * dir, offsetY);

        // 히트 판정(태그만)
        var hits = Physics2D.OverlapCircleAll(center, range);
        bool hitEnemy = false;
        int hitCount = 0;

        foreach (var h in hits)
        {
            if (!h) continue;

            var root = h.attachedRigidbody ? h.attachedRigidbody.transform.root : h.transform.root;
            if (!root || !root.CompareTag(enemyTag)) continue;

            var hp = h.GetComponentInParent<EnemyHealth2D>();
            if (hp == null) continue;

            hp.TakeDamage(damage);
            hitCount++;
            hitEnemy = true;
            Debug.Log($"[Sword] Hit {root.name} for {damage}");
        }

        if (hitCount == 0) Debug.Log("[Sword] No enemy in range");

        // 바닥 접촉 체크
        bool hitGround = Physics2D.OverlapCircle(center, range, groundLayer);

        // ▼ 튕김: 중앙 조율자에 보고
        if (bounceOnHit && (hitEnemy || hitGround))
        {
            var combo = caster.GetComponent<PlayerComboJump>();
            if (combo != null)
            {
                bool bounced = combo.ReportSwordHit();
                // 필요 시: bounced가 false면 fallback으로 예전 jump.bounceUp 호출 유지 가능
            }
            else
            {
                var jump = caster.GetComponent<GMTK.PlatformerToolkit.characterJump>();
                if (jump != null) jump.bounceUp(bounceAmount); // 임시 하위호환
            }
        }

        // 효과
        if (sfx) sfx.Play();
        if (slashVfx)
        {
            var v = Instantiate(slashVfx, center, Quaternion.identity);
            Destroy(v, vfxLifetime);
        }
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        // 기즈모도 동일하게 오른쪽 고정
        const float dir = +1f;
        Transform caster = transform.root ? transform.root : transform;
        Vector2 center = (Vector2)caster.position + new Vector2(offsetX * dir, offsetY);
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(center, range);
    }
#endif
}

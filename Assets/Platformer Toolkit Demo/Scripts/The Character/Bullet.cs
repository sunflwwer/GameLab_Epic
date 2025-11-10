using UnityEngine;

public class Bullet : MonoBehaviour {
    [Header("Behavior")]
    [SerializeField] private LayerMask groundLayer; // Ground(일반+부서짐 모두)
    [SerializeField] private float range = 10f;
    [SerializeField] private float lifetime = 3f;

    [Header("Damage")]
    [SerializeField] private int damage = 1;

    [Header("Breakable Settings")]
    [SerializeField] private string breakableTag = "BreakableGround";
    [SerializeField] private float destructRadius = 0f;

    private Vector3 startPos, lastPos;
    private Rigidbody2D rb;

    private void Awake() {
        startPos = transform.position;
        lastPos  = startPos;
        rb = GetComponent<Rigidbody2D>();
        if (rb) rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        Destroy(gameObject, lifetime);
        var col = GetComponent<Collider2D>();
        if (col) col.isTrigger = true;
    }

    private void Update() {
        if (Vector3.Distance(startPos, transform.position) >= range) {
            Destroy(gameObject);
            return;
        }
        lastPos = transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other) {
        // 0) 적 우선 처리 (원하면 순서 바꿔도 됨)
        var enemy = other.GetComponentInParent<EnemyHealth2D>();
        if (enemy != null) {
            enemy.TakeDamage(damage);
            Destroy(gameObject);
            return;
        }

        // 1) 부서지는 타일맵?
        bool isBreakable = other.CompareTag(breakableTag) ||
                           (other.transform.parent != null && other.transform.parent.CompareTag(breakableTag));

        if (isBreakable) {
            var destruct = other.GetComponentInParent<DestructibleTilemap>();
            if (destruct != null) {
                // 직전→현재 레이캐스트로 진짜 접점/노멀 확보
                Vector2 from = lastPos;
                Vector2 to   = transform.position;
                Vector2 dir  = (to - from).normalized;
                float dist   = Vector2.Distance(from, to) + 0.2f;

                RaycastHit2D hit = Physics2D.Raycast(from, dir, dist, groundLayer);
                Vector2 hitPoint = other.ClosestPoint(transform.position);
                Vector2 hitNormal = -dir; // 레이가 실패해도 대략적 노멀

                if (hit && (hit.collider == other || hit.collider.transform.IsChildOf(other.transform))) {
                    hitPoint  = hit.point;
                    hitNormal = hit.normal;
                }

                // 셀 경계 튕김 방지: 안쪽으로 살짝 밀기(셀 크기 10~20%)
                destruct.HitWorldRobust(hitPoint, hitNormal, damage, destructRadius);
            }
            Destroy(gameObject);
            return;
        }

        // 2) 일반 Ground면 총알만 제거
        int otherMask = 1 << other.gameObject.layer;
        if ((groundLayer.value & otherMask) != 0) {
            Destroy(gameObject);
            return;
        }
    }
}

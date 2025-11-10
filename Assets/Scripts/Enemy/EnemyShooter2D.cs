using UnityEngine;

/// <summary>
/// 플레이어가 사거리/시야 안에 들어오면 일정 간격으로 총알 발사.
/// </summary>
public class EnemyShooter2D : MonoBehaviour
{
    [Header("대상")]
    [SerializeField] private Transform player;           // 비워두면 Start에서 자동 탐색
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask obstacleMask;     // 시야를 가리는 레이어(벽/지형)

    [Header("발사 설정")]
    [SerializeField] private Transform firePoint;        // 총구
    [SerializeField] private GameObject bulletPrefab;
    [SerializeField] private float bulletSpeed = 12f;
    [SerializeField] private float fireInterval = 1.0f;
    [SerializeField] private float spreadAngle = 0f;     // 퍼짐(도)
    [SerializeField] private int burstCount = 1;         // 연발 수
    [SerializeField] private float burstGap = 0.08f;     // 연발 간격

    [Header("시야/사거리")]
    [SerializeField] private float detectRange = 8f;
    [SerializeField] private bool requireLineOfSight = true;

    [Header("회전/정렬(선택)")]
    [SerializeField] private bool rotateGunToAim = true;

    private float nextFireTime;

    private void Start()
    {
        if (!player)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) player = p.transform;
        }
    }

    private void Update()
    {
        if (!player || !firePoint || !bulletPrefab) return;

        Vector2 toPlayer = player.position - firePoint.position;
        float dist = toPlayer.magnitude;
        if (dist > detectRange) return;

        if (requireLineOfSight && !HasLineOfSight(toPlayer)) return;

        if (rotateGunToAim)
        {
            float z = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg;
            firePoint.rotation = Quaternion.Euler(0, 0, z);
        }

        if (Time.time >= nextFireTime)
        {
            nextFireTime = Time.time + fireInterval;
            StartCoroutine(FireBurst(toPlayer.normalized));
        }
    }

    private System.Collections.IEnumerator FireBurst(Vector2 dir)
    {
        for (int i = 0; i < burstCount; i++)
        {
            FireOne(ApplySpread(dir, spreadAngle));
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstGap);
        }
    }

    private void FireOne(Vector2 dir)
    {
        var go = Instantiate(bulletPrefab, firePoint.position, Quaternion.identity);
        var rb = go.GetComponent<Rigidbody2D>();
        if (rb)
        {
            rb.linearVelocity = dir * bulletSpeed;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
    }

    private bool HasLineOfSight(Vector2 toPlayer)
    {
        // 시야 레이: 장애물에 막히면 실패
        RaycastHit2D hit = Physics2D.Raycast(firePoint.position, toPlayer.normalized, toPlayer.magnitude, obstacleMask);
        return hit.collider == null;
    }

    private Vector2 ApplySpread(Vector2 dir, float angleDeg)
    {
        if (angleDeg <= 0f) return dir;
        float half = angleDeg * 0.5f;
        float rand = Random.Range(-half, half);
        float r = rand * Mathf.Deg2Rad;
        float cos = Mathf.Cos(r), sin = Mathf.Sin(r);
        return new Vector2(dir.x * cos - dir.y * sin, dir.x * sin + dir.y * cos).normalized;
    }
}

using UnityEngine;

/// <summary>
/// 직진 총알: 수명, 사거리, 충돌 시 플레이어에 대미지, 지형에 닿으면 파괴.
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class EnemyBullet2D : MonoBehaviour
{
    [Header("일반")]
    [SerializeField] private float range = 12f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private int damage = 1;

    private Vector3 spawnPos;

    private void Awake()
    {
        spawnPos = transform.position;
        Destroy(gameObject, lifetime);

        var col = GetComponent<Collider2D>();
        col.isTrigger = true;

        var rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f; // 필요시 조절
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
    }

    private void Update()
    {
        if (Vector3.Distance(spawnPos, transform.position) >= range)
            Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        int mask = 1 << other.gameObject.layer;

        // 플레이어 대미지 (원하는 방식으로 교체)
        var hp = other.GetComponentInParent<IHittable>();
        if (hp != null)
        {
            hp.Hit(damage, transform.position);
            Destroy(gameObject);
            return;
        }

        // 지형/벽 등에 닿으면 파괴
        if ((groundLayer.value & mask) != 0)
        {
            Destroy(gameObject);
            return;
        }
    }
}

/// <summary>
/// 피격 인터페이스(플레이어/오브젝트가 구현)
/// </summary>
public interface IHittable
{
    void Hit(int damage, Vector3 hitPoint);
}
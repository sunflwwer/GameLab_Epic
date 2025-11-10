using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[DisallowMultipleComponent]
public class SwordHitbox : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;
    public string enemyTag = "Enemy";

    [Header("Lifetime")]
    [SerializeField] private float autoDestroyAfter = 0.35f;

    private readonly HashSet<EnemyHealth2D> _alreadyHit = new();

    private void OnEnable()
    {
        if (autoDestroyAfter > 0f) Destroy(gameObject, autoDestroyAfter);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // 태그 필터
        if (!string.IsNullOrEmpty(enemyTag) && !other.CompareTag(enemyTag)) return;

        // 부모까지 찾아서 EnemyHealth2D 얻기
        var hp = other.GetComponentInParent<EnemyHealth2D>();
        if (hp == null) return;

        // 중복 타격 방지
        if (_alreadyHit.Contains(hp)) return;
        _alreadyHit.Add(hp);

        hp.TakeDamage(damage);
    }
}
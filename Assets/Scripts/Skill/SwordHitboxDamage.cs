using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwordHitboxDamage : MonoBehaviour
{
    [Header("Damage")]
    public int damage = 1;
    public string enemyTag = "Enemy";

    private readonly HashSet<EnemyHealth2D> _hitOnce = new();
    private Collider2D _col;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
        _col.isTrigger = true;
        var rb = GetComponent<Rigidbody2D>();
        if (!rb) { rb = gameObject.AddComponent<Rigidbody2D>(); rb.isKinematic = true; }
        rb.bodyType = RigidbodyType2D.Kinematic;
    }

    public void ResetHits()
    {
        _hitOnce.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!string.IsNullOrEmpty(enemyTag) && !other.CompareTag(enemyTag)) return;

        var hp = other.GetComponentInParent<EnemyHealth2D>();
        if (hp == null || _hitOnce.Contains(hp)) return;

        _hitOnce.Add(hp);
        hp.TakeDamage(damage);
    }
}
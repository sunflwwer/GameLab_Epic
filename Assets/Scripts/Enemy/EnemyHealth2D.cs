using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class EnemyHealth2D : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 1;
    [SerializeField] private bool destroyOnDeath = true;
    [SerializeField] private float deathDelay = 0.05f;

    [Header("FX (옵션)")]
    [SerializeField] private GameObject deathVfx;
    [SerializeField] private AudioSource deathSfx;

    [Header("Events")]
    public UnityEvent onDeath;
    
    [SerializeField] private string requiredTag = "Enemy";

    private int current;

    private EnemyVFX2D vfx;

    // 외부에서 체력 정보 읽기용
    public int CurrentHealth => current;
    public int MaxHealth => maxHealth;

    private void Awake()
    {
        current = maxHealth;

        // 콜라이더는 논-트리거 권장
        var col = GetComponent<Collider2D>() ?? GetComponentInChildren<Collider2D>();
        if (col) col.isTrigger = false;

        vfx = GetComponentInChildren<EnemyVFX2D>();

        // 태그 자동 보정(에디터/런타임 둘 다 가능, 태그가 프로젝트에 존재해야 함)
        if (!CompareTag(requiredTag))
        {
            try { gameObject.tag = requiredTag; } catch { /* 프로젝트에 태그 없으면 무시 */ }
        }
    }

    public void TakeDamage(int amount = 1)
    {
        if (current <= 0) return;
        current -= Mathf.Max(1, amount);

        Debug.Log($"[EnemyHealth2D] {name} took {amount}, hp={current}/{maxHealth}");

        if (current > 0 && vfx != null) vfx.PlayHitFlash();
        if (current <= 0)
        {
            if (deathSfx) deathSfx.Play();
            if (deathVfx) Instantiate(deathVfx, transform.position, Quaternion.identity);
            onDeath?.Invoke();

            if (vfx != null) vfx.PlayDeathPop(deathDelay);
            if (destroyOnDeath) Destroy(gameObject, deathDelay);
        }
    }

}
using UnityEngine;
using GMTK.PlatformerToolkit;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerStomp2D : MonoBehaviour
{
    [Header("판정 옵션")]
    [SerializeField, Range(0f, 1f)] private float topContactNormalY = 0.5f; // 위에서 밟았는지(법선 y)
    [SerializeField] private float requireDownSpeed = -0.5f;                // 최소 하강 속도

    [Header("효과")]
    [SerializeField] private int stompDamage = 1;        // 적에게 줄 데미지
    [SerializeField] private float bouncePower = 16f;    // 튕겨 오를 힘

    private Rigidbody2D rb;
    private characterJump jump;
    [SerializeField] private CharacterShoot shooter; 

    private void Awake()
    {
        rb   = GetComponent<Rigidbody2D>();
        jump = GetComponent<characterJump>();
        if (shooter == null) shooter = GetComponent<CharacterShoot>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 하강 중이었는지(너무 미세한 접촉 방지)
        if (rb.linearVelocity.y > requireDownSpeed)
            return;

        // 접촉 지점들 중 "위에서 밟음" 판정이 있는지 확인
        for (int i = 0; i < collision.contactCount; i++)
        {
            var contact = collision.GetContact(i);

            // 플레이어 기준 접촉 법선이 위(+Y)면, 위에서 내려와 닿은 것
            if (contact.normal.y >= topContactNormalY)
            {
                // 태그로 적 판정 (충돌한 콜라이더 또는 부모 쪽에 Enemy 태그가 있을 수 있음)
                Transform hitTr = collision.collider.transform;
                Transform enemyRoot = FindTaggedEnemyRoot(hitTr);

                if (enemyRoot != null)
                {
                    // 데미지
                    var enemyHP = enemyRoot.GetComponent<EnemyHealth2D>();
                    if (enemyHP != null)
                        enemyHP.TakeDamage(stompDamage);

                    // 플레이어 튕김
                    if (jump != null) jump.bounceUp(bouncePower);
                    else
                    {
                        var v = rb.linearVelocity;
                        v.y = Mathf.Max(v.y, bouncePower);
                        rb.linearVelocity = v;
                    }
                    
                    // 밟기에 성공하면 즉시 리로드
                    if (shooter != null)
                        shooter.Reload();
                }

                // 한 번 처리했으면 종료
                return;
            }
        }
    }

    // 충돌 지점 트랜스폼에서 위로 올라가며 "Enemy" 태그를 가진 루트를 찾음
    private Transform FindTaggedEnemyRoot(Transform t)
    {
        while (t != null)
        {
            if (t.CompareTag("Enemy")) return t;
            t = t.parent;
        }
        return null;
    }
}

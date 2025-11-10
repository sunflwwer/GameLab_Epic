using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class EnemyPatrol2D : MonoBehaviour
{
    [Header("이동")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float patrolDistance = 4f;   // 시작점 기준 좌우 범위
    [SerializeField] private bool flipSpriteByScale = true;

    [Header("바닥/벽 감지")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform frontCheck;        // 벽 감지용
    [SerializeField] private Transform downCheck;         // 낭떠러지 감지용(발끝)
    [SerializeField] private float wallCheckDist = 0.2f;
    [SerializeField] private float groundCheckRadius = 0.15f; // 오버랩 원 반경(레이 대신)

    [Header("안전장치")]
    [SerializeField] private float flipCooldown = 0.2f;   // 플립 연타 방지
    [SerializeField] private float snapBackStep = 0.08f;  // 가장자리에서 한 걸음 뒤로
    [SerializeField] private float maxFallY = -50f;       // 안전망(너무 아래로 떨어지면 되돌림)

    private Rigidbody2D rb;
    private Vector2 startPos;
    private int dir = 1;          // 1: +X, -1: -X
    private float nextFlipAllowed; // 쿨다운 타임스탬프
    private float lastSafeX;       // 최근 안전한 바닥 위 X

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true; // Z 회전 고정
        startPos = transform.position;

        // 초기 안전 위치 기록
        lastSafeX = transform.position.x;

        // front/down 체크가 비어 있으면 자동 생성
        if (!frontCheck)
        {
            frontCheck = new GameObject("FrontCheck").transform;
            frontCheck.SetParent(transform);
            frontCheck.localPosition = new Vector3(0.5f, 0f, 0f);
        }
        if (!downCheck)
        {
            downCheck = new GameObject("DownCheck").transform;
            downCheck.SetParent(transform);
            downCheck.localPosition = new Vector3(0.4f, -0.5f, 0f);
        }
    }

    private void FixedUpdate()
    {
        // 이동
        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);

        // 낙하 안전망(디버깅용): 너무 떨어지면 시작점으로 복구
        if (transform.position.y < maxFallY)
        {
            rb.position = new Vector2(startPos.x, startPos.y);
            rb.linearVelocity = Vector2.zero;
            dir = 1;
            return;
        }

        // 최근 안전한 바닥 위치 갱신(발밑에 바닥 있을 때만)
        if (IsGroundUnderFoot())
            lastSafeX = transform.position.x;

        // 1) 순찰 거리 경계 도달 시 반전
        float offsetX = transform.position.x - startPos.x;
        if (Mathf.Abs(offsetX) >= patrolDistance)
            TryFlip(boundarySnap: true);

        // 2) 앞벽 또는 낭떠러지 감지 시 반전
        if (HitWallAhead() || NoGroundAhead())
            TryFlip(boundarySnap: false);

        // 시각 반전
        if (flipSpriteByScale)
            transform.localScale = new Vector3(dir > 0 ? 1 : -1, 1, 1);
    }

    private bool HitWallAhead()
    {
        if (!frontCheck) return false;
        Vector2 origin = frontCheck.position;
        Vector2 castDir = Vector2.right * dir;
        RaycastHit2D hit = Physics2D.Raycast(origin, castDir, wallCheckDist, groundLayer);
        return hit.collider != null;
    }

    // 발끝에서 원형 오버랩으로 발밑 확인(레이 대신; 지터에 강함)
    private bool IsGroundUnderFoot()
    {
        if (!downCheck) return false;
        Collider2D col = Physics2D.OverlapCircle(downCheck.position, groundCheckRadius, groundLayer);
        return col != null;
    }

    // 진행 방향 발끝 조금 앞 지점까지도 바닥이 없으면 낭떠러지로 판정
    private bool NoGroundAhead()
    {
        if (!downCheck) return false;
        Vector2 ahead = (Vector2)downCheck.position + new Vector2(groundCheckRadius * 0.75f * dir, 0f);
        Collider2D col = Physics2D.OverlapCircle(ahead, groundCheckRadius, groundLayer);
        return col == null;
    }

    private void TryFlip(bool boundarySnap)
    {
        if (Time.time < nextFlipAllowed) return;

        // 플립하기 전에 살짝 뒤로 스냅(가장자리에서 끼이는 것 방지)
        Vector2 p = rb.position;
        p.x = (boundarySnap ? Mathf.Clamp(p.x, startPos.x - patrolDistance, startPos.x + patrolDistance)
                            : lastSafeX) - dir * snapBackStep;
        rb.position = p;

        dir *= -1;
        nextFlipAllowed = Time.time + flipCooldown;
    }

#if UNITY_EDITOR
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        if (downCheck) Gizmos.DrawWireSphere(downCheck.position, groundCheckRadius);

        Gizmos.color = Color.yellow;
        if (frontCheck)
            Gizmos.DrawLine(frontCheck.position, frontCheck.position + Vector3.right * dir * wallCheckDist);

        Gizmos.color = Color.magenta;
        if (Application.isPlaying)
            Gizmos.DrawLine(new Vector3(startPos.x - patrolDistance, transform.position.y),
                            new Vector3(startPos.x + patrolDistance, transform.position.y));
    }
#endif
}

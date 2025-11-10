using System.Collections;
using UnityEngine;

public class PlayerSimpleWeapons : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private Transform sword; // 칼
    [SerializeField] private Transform spear; // 창(방패 역할)

    [Header("Motion")]
    [SerializeField] private float moveDistance = 5f;      // 로컬 -Y 이동 거리
    [SerializeField] private float downDuration = 0.12f;   // 내려가는 시간
    [SerializeField] private float upDuration = 0.12f;     // 올라오는 시간
    [SerializeField] private AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Motion (X)")]
    [SerializeField] private float swordXOffset = +1f; // 칼: +X
    [SerializeField] private float spearXOffset = -1f; // 창: -X

    private Vector3    swordOriginLocal;
    private Vector3    spearOriginLocal;
    private Quaternion spearOriginLocalRot; // 창(방패) 회전 원위치

    private Coroutine swordCo;
    private Coroutine spearCo;

    private bool swordHolding;
    private bool spearHolding;

    private void Awake()
    {
        if (sword) swordOriginLocal = sword.localPosition;
        if (spear)
        {
            spearOriginLocal    = spear.localPosition;
            spearOriginLocalRot = spear.localRotation;
        }
    }

    // --- 칼: 홀드 시작/종료(회전 없음) ---
    public void StartSwordHold()
    {
        if (!sword) return;
        swordHolding = true;
        if (swordCo != null) StopCoroutine(swordCo);
        swordCo = StartCoroutine(HoldDownAndBack_PosOnly(sword, () => swordHolding, swordOriginLocal, swordXOffset));
    }

    public void EndSwordHold()
    {
        swordHolding = false; // 코루틴이 감지하고 복귀 트윈
    }

    // --- 창/방패: 홀드 시작/종료(회전 포함) ---
    public void StartSpearHold()
    {
        if (!spear) return;
        spearHolding = true;
        if (spearCo != null) StopCoroutine(spearCo);

        // 목표 자세: z 회전 0도로 세우기 (로컬 회전 기준)
        Quaternion targetLocalRot = Quaternion.Euler(0f, 0f, 0f);

        spearCo = StartCoroutine(HoldDownAndBack_PosRot(
            spear,
            () => spearHolding,
            spearOriginLocal,
            spearOriginLocalRot,
            spearXOffset,
            targetLocalRot
        ));
    }

    public void EndSpearHold()
    {
        spearHolding = false; // 코루틴이 감지하고 복귀 트윈
    }

    // ▼ 위치만 Lerp (칼용)
    private IEnumerator HoldDownAndBack_PosOnly(Transform target, System.Func<bool> isHolding, Vector3 originLocal, float xDelta)
    {
        Vector3 downLocal = originLocal + new Vector3(xDelta, -moveDistance, 0f);

        // 내려가기
        yield return LerpLocalPos(target, originLocal, downLocal, downDuration);

        // 홀드 유지
        while (isHolding())
        {
            target.localPosition = downLocal; // 드리프트 방지
            yield return null;
        }

        // 원위치 복귀
        yield return LerpLocalPos(target, downLocal, originLocal, upDuration);
    }

    // ▼ 위치 + 회전 동시 Lerp (방패용)
    private IEnumerator HoldDownAndBack_PosRot(
        Transform target,
        System.Func<bool> isHolding,
        Vector3 originLocalPos,
        Quaternion originLocalRot,
        float xDelta,
        Quaternion targetLocalRot
    )
    {
        Vector3 downLocalPos = originLocalPos + new Vector3(xDelta, -moveDistance, 0f);

        // 내려가며 회전도 함께 0도로
        yield return LerpLocalPosRot(target, originLocalPos, downLocalPos, originLocalRot, targetLocalRot, downDuration);

        // 홀드 유지
        while (isHolding())
        {
            target.localPosition = downLocalPos;
            target.localRotation = targetLocalRot;
            yield return null;
        }

        // 원위치/원각도로 복귀
        yield return LerpLocalPosRot(target, downLocalPos, originLocalPos, targetLocalRot, originLocalRot, upDuration);
    }

    // 위치만 보간
    private IEnumerator LerpLocalPos(Transform t, Vector3 from, Vector3 to, float dur)
    {
        float t0 = 0f;
        while (t0 < dur)
        {
            t0 += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t0 / dur));
            t.localPosition = Vector3.LerpUnclamped(from, to, k);
            yield return null;
        }
        t.localPosition = to;
    }

    // 위치+회전 동시 보간 (로컬 기준)
    private IEnumerator LerpLocalPosRot(Transform t, Vector3 fromPos, Vector3 toPos, Quaternion fromRot, Quaternion toRot, float dur)
    {
        float t0 = 0f;
        while (t0 < dur)
        {
            t0 += Time.deltaTime;
            float k = ease.Evaluate(Mathf.Clamp01(t0 / dur));
            t.localPosition = Vector3.LerpUnclamped(fromPos, toPos, k);
            t.localRotation = Quaternion.Slerp(fromRot, toRot, k);
            yield return null;
        }
        t.localPosition = toPos;
        t.localRotation = toRot;
    }

    // 원위치 리셋(선택)
    public void ResetWeapons()
    {
        if (sword) sword.localPosition = swordOriginLocal;
        if (spear)
        {
            spear.localPosition = spearOriginLocal;
            spear.localRotation = spearOriginLocalRot;
        }
        swordHolding = spearHolding = false;
    }
}

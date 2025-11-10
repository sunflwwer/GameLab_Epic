// Light2DPulse.cs
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.Universal;

[DisallowMultipleComponent]
public class Light2DPulse : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Light2D light2D; // 비우면 자동할당

    [Header("Radius")]
    [SerializeField] private float baseOuter = 2f;     // 기본 Outer
    [SerializeField] private float baseInner = 0f;     // 기본 Inner(원하면 0)
    [SerializeField] private float expandedOuter = 5f; // 펄스 시 Outer
    [SerializeField] private float expandedInner = 1.5f; // 펄스 시 Inner

    [Header("Timing")]
    [SerializeField] private float expandDuration = 0.12f;
    [SerializeField] private float holdDuration   = 0.05f;
    [SerializeField] private float shrinkDuration = 0.20f;

    [Header("Ease")]
    [SerializeField] private AnimationCurve easeOut = AnimationCurve.EaseInOut(0,0,1,1);
    [SerializeField] private AnimationCurve easeIn  = AnimationCurve.EaseInOut(0,0,1,1);

    private Coroutine co;

    private void Reset()
    {
        light2D = GetComponent<Light2D>();
        if (light2D)
        {
            baseOuter = light2D.pointLightOuterRadius;
            baseInner = light2D.pointLightInnerRadius;
        }
    }

    private void Awake()
    {
        if (!light2D) light2D = GetComponent<Light2D>();
    }

    public void Pulse()
    {
        if (!light2D) return;
        if (co != null) StopCoroutine(co);
        co = StartCoroutine(CoPulse());
    }
    
    public void PulseTo(float targetOuter, float targetInner)
    {
        if (!light2D) return;
        expandedOuter = targetOuter;
        expandedInner = targetInner;
        Pulse();
    }

    private IEnumerator CoPulse()
    {
        // 확장
        float t = 0f;
        float startO = light2D.pointLightOuterRadius;
        float startI = light2D.pointLightInnerRadius;
        while (t < expandDuration)
        {
            t += Time.deltaTime;
            float k = easeOut.Evaluate(Mathf.Clamp01(t / expandDuration));
            light2D.pointLightOuterRadius = Mathf.Lerp(startO, expandedOuter, k);
            light2D.pointLightInnerRadius = Mathf.Lerp(startI, expandedInner, k);
            yield return null;
        }

        if (holdDuration > 0f) yield return new WaitForSeconds(holdDuration);

        // 축소
        t = 0f;
        startO = light2D.pointLightOuterRadius;
        startI = light2D.pointLightInnerRadius;
        while (t < shrinkDuration)
        {
            t += Time.deltaTime;
            float k = easeIn.Evaluate(Mathf.Clamp01(t / shrinkDuration));
            light2D.pointLightOuterRadius = Mathf.Lerp(startO, baseOuter, k);
            light2D.pointLightInnerRadius = Mathf.Lerp(startI, baseInner, k);
            yield return null;
        }

        light2D.pointLightOuterRadius = baseOuter;
        light2D.pointLightInnerRadius = baseInner;
        co = null;
    }
}

using UnityEngine;
using TMPro; // TextMeshPro 사용

public class SlowUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private bool useInteger = true;   // 정수로 표기할지 여부

    // 초기 세팅: characterJump.Awake에서 호출됨
    public void Init(float max, float current)
    {
        UpdateSlow(current, max);
    }

    // 게이지 갱신: characterJump.Update에서 호출됨
    public void UpdateSlow(float current, float max)
    {
        if (!label) return;

        if (useInteger)
        {
            int c = Mathf.Clamp(Mathf.RoundToInt(current), 0, Mathf.RoundToInt(max));
            int m = Mathf.RoundToInt(max);
            label.text = $"{c}/{m}";
        }
        else
        {
            // 소수 1자리 표기 원하면 useInteger=false
            float c = Mathf.Clamp(current, 0f, max);
            label.text = $"{c:0.0}/{max:0.0}";
        }
    }
}
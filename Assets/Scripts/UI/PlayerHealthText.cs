using TMPro;
using UnityEngine;

public class PlayerHealthText : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private PlayerHealth2D playerHealth;
    [SerializeField] private TMP_Text hpText;

    [Header("표시 형식")]
    [SerializeField] private bool useHearts = false;                    // 하트(♥)로 표시할지
    [SerializeField] private string numericFormat = "HP {0} / {1}";     // 예: HP 3 / 5

    private void Reset()
    {
        if (!hpText) hpText = GetComponent<TMP_Text>();
        if (!playerHealth) playerHealth = FindObjectOfType<PlayerHealth2D>();
    }

    private void OnEnable()
    {
        if (!hpText) hpText = GetComponent<TMP_Text>();
        if (!playerHealth) playerHealth = FindObjectOfType<PlayerHealth2D>();

        if (playerHealth != null)
        {
            playerHealth.OnHealthChanged += HandleHealthChanged;
            // 초기 표시
            HandleHealthChanged(playerHealth.CurrentHp, playerHealth.MaxHp);
        }
    }

    private void OnDisable()
    {
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }

    private void HandleHealthChanged(int current, int max)
    {
        if (!hpText) return;

        if (useHearts)
            hpText.text = BuildHearts(current, max);     // ♥♥♥··
        else
            hpText.text = string.Format(numericFormat, current, max);
    }

    private string BuildHearts(int current, int max)
    {
        current = Mathf.Clamp(current, 0, max);
        return new string('♥', current) + new string('·', Mathf.Max(0, max - current));
    }
}
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 머리 위에 체력바 표시
/// Canvas(World Space) + Image(Fill) 방식
/// 프리팹에 수동으로 연결해서 사용
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyHealth2D enemyHealth;
    [SerializeField] private Image fillImage;           // 체력바 Fill 이미지
    [SerializeField] private GameObject barContainer;   // 체력바 전체 오브젝트 (숨기기 가능)

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f); // 적 위 오프셋
    [SerializeField] private bool hideWhenFull = true;  // 풀피일 때 숨기기
    [SerializeField] private bool alwaysFaceCamera = true; // 카메라 바라보기

    [Header("Colors")]
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color halfColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    [SerializeField] private float colorThresholdHalf = 0.5f;
    [SerializeField] private float colorThresholdLow = 0.25f;

    private Transform target;      // 적의 Transform
    private Camera mainCam;
    private int currentHp;
    private int maxHp;

    private void Awake()
    {
        if (!enemyHealth) enemyHealth = GetComponentInParent<EnemyHealth2D>();
        mainCam = Camera.main;
    }

    private void Start()
    {
        if (enemyHealth != null)
        {
            target = enemyHealth.transform;
            maxHp = enemyHealth.MaxHealth;
            currentHp = enemyHealth.CurrentHealth;
            UpdateBar();
        }
        else
        {
            Debug.LogWarning("[EnemyHealthBar] EnemyHealth2D를 찾을 수 없습니다!");
            if (barContainer) barContainer.SetActive(false);
        }
    }

    private void LateUpdate()
    {
        if (target == null) return;

        // 위치 업데이트
        transform.position = target.position + offset;

        // 카메라 바라보기
        if (alwaysFaceCamera && mainCam != null)
        {
            transform.rotation = mainCam.transform.rotation;
        }

        // 체력 변경 감지 및 업데이트
        if (enemyHealth != null)
        {
            int newHp = enemyHealth.CurrentHealth;
            if (newHp != currentHp)
            {
                currentHp = newHp;
                UpdateBar();
            }
        }
    }

    private void UpdateBar()
    {
        if (fillImage == null) return;

        float ratio = maxHp > 0 ? (float)currentHp / maxHp : 0f;
        fillImage.fillAmount = ratio;

        // 색상 변경
        if (ratio > colorThresholdHalf)
            fillImage.color = fullColor;
        else if (ratio > colorThresholdLow)
            fillImage.color = halfColor;
        else
            fillImage.color = lowColor;

        // 풀피일 때 숨기기
        if (hideWhenFull && barContainer != null)
        {
            barContainer.SetActive(ratio < 1f);
        }
    }
}


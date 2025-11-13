using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 자동 생성용 간단한 적 체력바
/// EnemyHealthBarAutoSetup에서 사용
/// </summary>
public class SimpleEnemyHealthBar : MonoBehaviour
{
    private EnemyHealth2D enemyHealth;
    private Image fillImage;
    private GameObject barContainer;
    private Vector3 offset;
    private bool hideWhenFull;
    private bool alwaysFaceCamera;
    private Color fullColor;
    private Color halfColor;
    private Color lowColor;
    private float colorThresholdHalf = 0.5f;
    private float colorThresholdLow = 0.25f;

    private Transform target;
    private Camera mainCam;
    private int currentHp;
    private int maxHp;

    public void Setup(EnemyHealth2D enemy, Image fill, GameObject container, Vector3 off, 
                     bool hideFull, bool faceCamera, Color full, Color half, Color low)
    {
        enemyHealth = enemy;
        fillImage = fill;
        barContainer = container;
        offset = off;
        hideWhenFull = hideFull;
        alwaysFaceCamera = faceCamera;
        fullColor = full;
        halfColor = half;
        lowColor = low;

        target = enemyHealth.transform;
        mainCam = Camera.main;
        maxHp = enemyHealth.MaxHealth;
        currentHp = enemyHealth.CurrentHealth;
        
        UpdateBar();
    }

    private void LateUpdate()
    {
        if (target == null || enemyHealth == null) return;

        // 위치 업데이트
        transform.position = target.position + offset;

        // 카메라 바라보기
        if (alwaysFaceCamera && mainCam != null)
        {
            transform.rotation = mainCam.transform.rotation;
        }

        // 체력 변경 감지 및 업데이트
        int newHp = enemyHealth.CurrentHealth;
        if (newHp != currentHp)
        {
            currentHp = newHp;
            UpdateBar();
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


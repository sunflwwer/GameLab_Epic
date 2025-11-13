using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 적 오브젝트에 붙이면 자동으로 체력바 UI를 생성합니다.
/// EnemyHealth2D가 있는 오브젝트에 이 스크립트를 추가하면 끝!
/// </summary>
[RequireComponent(typeof(EnemyHealth2D))]
public class EnemyHealthBarAutoSetup : MonoBehaviour
{
    [Header("자동 생성 설정")]
    [SerializeField] private bool autoSetupOnStart = true;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.5f, 0f);
    
    [Header("체력바 크기")]
    [SerializeField] private float barWidth = 1f;
    [SerializeField] private float barHeight = 0.15f;
    
    [Header("색상")]
    [SerializeField] private Color backgroundColor = new Color(0, 0, 0, 0.8f);
    [SerializeField] private Color fullColor = Color.green;
    [SerializeField] private Color halfColor = Color.yellow;
    [SerializeField] private Color lowColor = Color.red;
    
    [Header("옵션")]
    [SerializeField] private bool hideWhenFull = true;
    [SerializeField] private bool alwaysFaceCamera = true;

    private void Start()
    {
        if (autoSetupOnStart)
        {
            SetupHealthBar();
        }
    }

    [ContextMenu("체력바 생성")]
    public void SetupHealthBar()
    {
        // 이미 체력바가 있는지 확인
        if (transform.Find("HealthBarCanvas") != null)
        {
            Debug.Log($"[{name}] 이미 체력바가 있습니다. 건너뜁니다.");
            return;
        }

        var enemyHealth = GetComponent<EnemyHealth2D>();
        if (enemyHealth == null)
        {
            Debug.LogError($"[{name}] EnemyHealth2D를 찾을 수 없습니다!");
            return;
        }

        // 1. Canvas 생성 (World Space)
        GameObject canvasObj = new GameObject("HealthBarCanvas");
        canvasObj.transform.SetParent(transform);
        canvasObj.transform.localPosition = Vector3.zero;
        canvasObj.transform.localRotation = Quaternion.identity;
        canvasObj.transform.localScale = Vector3.one;

        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10;

        canvasObj.AddComponent<GraphicRaycaster>();

        // Canvas 크기 설정
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(100, 20);
        canvasRect.localScale = new Vector3(0.01f, 0.01f, 0.01f);

        // 2. Background 생성
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(canvasObj.transform);
        bgObj.transform.localPosition = Vector3.zero;
        bgObj.transform.localRotation = Quaternion.identity;
        bgObj.transform.localScale = Vector3.one;

        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = backgroundColor;

        RectTransform bgRect = bgObj.GetComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;

        // 3. Fill 생성
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(bgObj.transform);
        fillObj.transform.localPosition = Vector3.zero;
        fillObj.transform.localRotation = Quaternion.identity;
        fillObj.transform.localScale = Vector3.one;

        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = fullColor;
        fillImage.type = Image.Type.Filled;
        fillImage.fillMethod = Image.FillMethod.Horizontal;
        fillImage.fillOrigin = (int)Image.OriginHorizontal.Left;
        fillImage.fillAmount = 1f;

        RectTransform fillRect = fillObj.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        // 4. SimpleEnemyHealthBar 스크립트 추가 (간단 버전)
        SimpleEnemyHealthBar healthBar = canvasObj.AddComponent<SimpleEnemyHealthBar>();
        healthBar.Setup(enemyHealth, fillImage, bgObj, offset, hideWhenFull, alwaysFaceCamera, 
                       fullColor, halfColor, lowColor);

        Debug.Log($"[{name}] 체력바가 성공적으로 생성되었습니다!");
    }

    [ContextMenu("체력바 삭제")]
    public void RemoveHealthBar()
    {
        Transform existingBar = transform.Find("HealthBarCanvas");
        if (existingBar != null)
        {
            if (Application.isPlaying)
                Destroy(existingBar.gameObject);
            else
                DestroyImmediate(existingBar.gameObject);
            
            Debug.Log($"[{name}] 체력바가 삭제되었습니다.");
        }
        else
        {
            Debug.Log($"[{name}] 삭제할 체력바가 없습니다.");
        }
    }
}

